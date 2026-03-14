using PuppyWeightWatcher.Shared.Models;
using PuppyWeightWatcher.ApiService.Data;
using PuppyWeightWatcher.ApiService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add SQL Server DbContext via Aspire integration with retry logic for Azure
builder.AddSqlServerDbContext<PuppyDbContext>("puppydb", configureDbContextOptions: options =>
{
    options.UseSqlServer(sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
    });
});

// Add response compression for faster API responses
builder.Services.AddResponseCompression();

// Register PuppyService as scoped (matches DbContext lifetime)
builder.Services.AddScoped<IPuppyService, PuppyService>();

var app = builder.Build();

// Apply pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PuppyDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
app.UseResponseCompression();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "Puppy Weight Watcher API is running. Use /puppies to get started.");

// Puppy endpoints
app.MapGet("/puppies", async (IPuppyService service) =>
{
    var puppies = await service.GetAllPuppiesAsync();
    return Results.Ok(puppies);
})
.WithName("GetAllPuppies");

app.MapGet("/puppies/{id:guid}", async (Guid id, IPuppyService service) =>
{
    var puppy = await service.GetPuppyByIdAsync(id);
    return puppy != null ? Results.Ok(puppy) : Results.NotFound();
})
.WithName("GetPuppyById");

app.MapPost("/puppies", async (Puppy puppy, IPuppyService service) =>
{
    var created = await service.CreatePuppyAsync(puppy);
    return Results.Created($"/puppies/{created.Id}", created);
})
.WithName("CreatePuppy");

app.MapPut("/puppies/{id:guid}", async (Guid id, Puppy puppy, IPuppyService service) =>
{
    var updated = await service.UpdatePuppyAsync(id, puppy);
    return updated != null ? Results.Ok(updated) : Results.NotFound();
})
.WithName("UpdatePuppy");

app.MapDelete("/puppies/{id:guid}", async (Guid id, IPuppyService service) =>
{
    var deleted = await service.DeletePuppyAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeletePuppy");

// Weight entry endpoints
app.MapGet("/puppies/{puppyId:guid}/weights", async (Guid puppyId, IPuppyService service) =>
{
    var entries = await service.GetWeightEntriesAsync(puppyId);
    return Results.Ok(entries);
})
.WithName("GetWeightEntries");

app.MapPost("/puppies/{puppyId:guid}/weights", async (Guid puppyId, WeightEntry entry, IPuppyService service) =>
{
    entry.PuppyId = puppyId;
    var created = await service.AddWeightEntryAsync(entry);
    return Results.Created($"/puppies/{puppyId}/weights/{created.Id}", created);
})
.WithName("AddWeightEntry");

app.MapDelete("/weights/{entryId:guid}", async (Guid entryId, IPuppyService service) =>
{
    var deleted = await service.DeleteWeightEntryAsync(entryId);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteWeightEntry");

// Weight statistics endpoint
app.MapGet("/puppies/{puppyId:guid}/statistics", async (Guid puppyId, IPuppyService service) =>
{
    var stats = await service.GetWeightStatisticsAsync(puppyId);
    return Results.Ok(stats);
})
.WithName("GetWeightStatistics");

// Shot record endpoints
app.MapPost("/puppies/{puppyId:guid}/shots", async (Guid puppyId, ShotRecord shotRecord, IPuppyService service) =>
{
    try
    {
        var created = await service.AddShotRecordAsync(puppyId, shotRecord);
        return Results.Created($"/puppies/{puppyId}/shots/{created.Id}", created);
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
})
.WithName("AddShotRecord");

app.MapDelete("/puppies/{puppyId:guid}/shots/{shotRecordId:guid}", async (Guid puppyId, Guid shotRecordId, IPuppyService service) =>
{
    var deleted = await service.DeleteShotRecordAsync(puppyId, shotRecordId);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteShotRecord");

// Photo endpoints
app.MapPost("/puppies/profile-photos", async (List<Guid> puppyIds, IPuppyService service) =>
{
    var photos = await service.GetProfilePhotosByPuppyIdsAsync(puppyIds);
    return Results.Ok(photos);
})
.WithName("GetProfilePhotos");

app.MapGet("/puppies/{puppyId:guid}/photos", async (Guid puppyId, IPuppyService service) =>
{
    var photos = await service.GetPhotosAsync(puppyId);
    return Results.Ok(photos);
})
.WithName("GetPhotos");

app.MapGet("/photos/{photoId:guid}", async (Guid photoId, IPuppyService service) =>
{
    var photo = await service.GetPhotoAsync(photoId);
    return photo != null ? Results.Ok(photo) : Results.NotFound();
})
.WithName("GetPhoto");

app.MapPost("/puppies/{puppyId:guid}/photos", async (Guid puppyId, HttpRequest request, IPuppyService service) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("Expected multipart form data.");

    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file == null || file.Length == 0)
        return Results.BadRequest("No file uploaded.");

    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);
    var base64 = Convert.ToBase64String(memoryStream.ToArray());

    var caption = form["caption"].FirstOrDefault();
    var dateTakenStr = form["dateTaken"].FirstOrDefault();
    var dateTaken = DateTime.TryParse(dateTakenStr, out var dt) ? dt : DateTime.Today;

    var photo = new PuppyPhoto
    {
        PuppyId = puppyId,
        FileName = file.FileName,
        ContentType = file.ContentType ?? "image/jpeg",
        Base64Data = base64,
        Caption = caption,
        DateTaken = dateTaken
    };

    try
    {
        var created = await service.AddPhotoAsync(photo);
        return Results.Created($"/photos/{created.Id}", created);
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
})
.WithName("UploadPhoto")
.DisableAntiforgery();

app.MapDelete("/puppies/{puppyId:guid}/photos/{photoId:guid}", async (Guid puppyId, Guid photoId, IPuppyService service) =>
{
    var deleted = await service.DeletePhotoAsync(puppyId, photoId);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeletePhoto");

app.MapPut("/puppies/{puppyId:guid}/photos/{photoId:guid}/profile", async (Guid puppyId, Guid photoId, IPuppyService service) =>
{
    var result = await service.SetProfilePhotoAsync(puppyId, photoId);
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("SetProfilePhoto");

// Litter endpoints
app.MapGet("/litters", async (IPuppyService service) =>
{
    var litters = await service.GetAllLittersAsync();
    return Results.Ok(litters);
})
.WithName("GetAllLitters");

app.MapGet("/litters/{id:guid}", async (Guid id, IPuppyService service) =>
{
    var litter = await service.GetLitterByIdAsync(id);
    return litter != null ? Results.Ok(litter) : Results.NotFound();
})
.WithName("GetLitterById");

app.MapPost("/litters", async (Litter litter, IPuppyService service) =>
{
    var created = await service.CreateLitterAsync(litter);
    return Results.Created($"/litters/{created.Id}", created);
})
.WithName("CreateLitter");

app.MapPut("/litters/{id:guid}", async (Guid id, Litter litter, IPuppyService service) =>
{
    var updated = await service.UpdateLitterAsync(id, litter);
    return updated != null ? Results.Ok(updated) : Results.NotFound();
})
.WithName("UpdateLitter");

app.MapDelete("/litters/{id:guid}", async (Guid id, IPuppyService service) =>
{
    var deleted = await service.DeleteLitterAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteLitter");

app.MapGet("/litters/{litterId:guid}/puppies", async (Guid litterId, IPuppyService service) =>
{
    var puppies = await service.GetPuppiesByLitterIdAsync(litterId);
    return Results.Ok(puppies);
})
.WithName("GetPuppiesByLitter");

app.MapPut("/litters/{litterId:guid}/puppies/{puppyId:guid}", async (Guid litterId, Guid puppyId, IPuppyService service) =>
{
    var result = await service.AddPuppyToLitterAsync(litterId, puppyId);
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("AddPuppyToLitter");

app.MapDelete("/litters/puppies/{puppyId:guid}", async (Guid puppyId, IPuppyService service) =>
{
    var result = await service.RemovePuppyFromLitterAsync(puppyId);
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("RemovePuppyFromLitter");

app.MapDefaultEndpoints();

app.Run();
