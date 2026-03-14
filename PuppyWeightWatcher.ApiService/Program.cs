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

// Apply pending EF Core migrations on startup with retry for container environments
var retryCount = 0;
const int maxRetries = 10;
while (true)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PuppyDbContext>();
        await db.Database.MigrateAsync();
        break;
    }
    catch (Exception ex) when (retryCount < maxRetries)
    {
        retryCount++;
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Migration");
        logger.LogWarning(ex, "Database migration attempt {Attempt}/{MaxRetries} failed. Retrying in {Delay}s...", retryCount, maxRetries, retryCount * 5);
        await Task.Delay(TimeSpan.FromSeconds(retryCount * 5));
    }
}

// Configure the HTTP request pipeline.
app.UseResponseCompression();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "Puppy Weight Watcher API is running. Use /puppies to get started.");

// Helper to extract userId from header
static string GetUserId(HttpContext ctx) => ctx.Request.Headers["X-User-Id"].FirstOrDefault() ?? "";

// Puppy endpoints
app.MapGet("/puppies", async (HttpContext ctx, IPuppyService service) =>
{
    var puppies = await service.GetAllPuppiesAsync(GetUserId(ctx));
    return Results.Ok(puppies);
})
.WithName("GetAllPuppies");

app.MapGet("/puppies/{id:guid}", async (Guid id, HttpContext ctx, IPuppyService service) =>
{
    var puppy = await service.GetPuppyByIdAsync(id, GetUserId(ctx));
    return puppy != null ? Results.Ok(puppy) : Results.NotFound();
})
.WithName("GetPuppyById");

app.MapPost("/puppies", async (Puppy puppy, HttpContext ctx, IPuppyService service) =>
{
    var created = await service.CreatePuppyAsync(puppy, GetUserId(ctx));
    return Results.Created($"/puppies/{created.Id}", created);
})
.WithName("CreatePuppy");

app.MapPut("/puppies/{id:guid}", async (Guid id, Puppy puppy, HttpContext ctx, IPuppyService service) =>
{
    var updated = await service.UpdatePuppyAsync(id, puppy, GetUserId(ctx));
    return updated != null ? Results.Ok(updated) : Results.NotFound();
})
.WithName("UpdatePuppy");

app.MapDelete("/puppies/{id:guid}", async (Guid id, HttpContext ctx, IPuppyService service) =>
{
    var deleted = await service.DeletePuppyAsync(id, GetUserId(ctx));
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeletePuppy");

// Weight entry endpoints
app.MapGet("/puppies/{puppyId:guid}/weights", async (Guid puppyId, HttpContext ctx, IPuppyService service) =>
{
    var entries = await service.GetWeightEntriesAsync(puppyId, GetUserId(ctx));
    return Results.Ok(entries);
})
.WithName("GetWeightEntries");

app.MapPost("/puppies/{puppyId:guid}/weights", async (Guid puppyId, WeightEntry entry, HttpContext ctx, IPuppyService service) =>
{
    entry.PuppyId = puppyId;
    var created = await service.AddWeightEntryAsync(entry, GetUserId(ctx));
    return created != null ? Results.Created($"/puppies/{puppyId}/weights/{created.Id}", created) : Results.NotFound();
})
.WithName("AddWeightEntry");

app.MapDelete("/weights/{entryId:guid}", async (Guid entryId, HttpContext ctx, IPuppyService service) =>
{
    var deleted = await service.DeleteWeightEntryAsync(entryId, GetUserId(ctx));
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteWeightEntry");

// Weight statistics endpoint
app.MapGet("/puppies/{puppyId:guid}/statistics", async (Guid puppyId, HttpContext ctx, IPuppyService service) =>
{
    var stats = await service.GetWeightStatisticsAsync(puppyId, GetUserId(ctx));
    return stats != null ? Results.Ok(stats) : Results.NotFound();
})
.WithName("GetWeightStatistics");

// Shot record endpoints
app.MapPost("/puppies/{puppyId:guid}/shots", async (Guid puppyId, ShotRecord shotRecord, HttpContext ctx, IPuppyService service) =>
{
    var created = await service.AddShotRecordAsync(puppyId, shotRecord, GetUserId(ctx));
    return created != null ? Results.Created($"/puppies/{puppyId}/shots/{created.Id}", created) : Results.NotFound();
})
.WithName("AddShotRecord");

app.MapDelete("/puppies/{puppyId:guid}/shots/{shotRecordId:guid}", async (Guid puppyId, Guid shotRecordId, HttpContext ctx, IPuppyService service) =>
{
    var deleted = await service.DeleteShotRecordAsync(puppyId, shotRecordId, GetUserId(ctx));
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteShotRecord");

// Photo endpoints
app.MapPost("/puppies/profile-photos", async ([Microsoft.AspNetCore.Mvc.FromBody] List<Guid> puppyIds, IPuppyService service) =>
{
    var photos = await service.GetProfilePhotosByPuppyIdsAsync(puppyIds);
    return Results.Ok(photos);
})
.WithName("GetProfilePhotos");

app.MapGet("/puppies/{puppyId:guid}/photos", async (Guid puppyId, HttpContext ctx, IPuppyService service) =>
{
    var photos = await service.GetPhotosAsync(puppyId, GetUserId(ctx));
    return Results.Ok(photos);
})
.WithName("GetPhotos");

app.MapGet("/photos/{photoId:guid}", async (Guid photoId, HttpContext ctx, IPuppyService service) =>
{
    var photo = await service.GetPhotoAsync(photoId, GetUserId(ctx));
    return photo != null ? Results.Ok(photo) : Results.NotFound();
})
.WithName("GetPhoto");

app.MapPost("/puppies/{puppyId:guid}/photos", async (Guid puppyId, HttpRequest request, IPuppyService service) =>
{
    var userId = GetUserId(request.HttpContext);

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

    var created = await service.AddPhotoAsync(photo, userId);
    return created != null ? Results.Created($"/photos/{created.Id}", created) : Results.NotFound();
})
.WithName("UploadPhoto")
.DisableAntiforgery();

app.MapDelete("/puppies/{puppyId:guid}/photos/{photoId:guid}", async (Guid puppyId, Guid photoId, HttpContext ctx, IPuppyService service) =>
{
    var deleted = await service.DeletePhotoAsync(puppyId, photoId, GetUserId(ctx));
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeletePhoto");

app.MapPut("/puppies/{puppyId:guid}/photos/{photoId:guid}/profile", async (Guid puppyId, Guid photoId, HttpContext ctx, IPuppyService service) =>
{
    var result = await service.SetProfilePhotoAsync(puppyId, photoId, GetUserId(ctx));
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("SetProfilePhoto");

// Litter endpoints
app.MapGet("/litters", async (HttpContext ctx, IPuppyService service) =>
{
    var litters = await service.GetAllLittersAsync(GetUserId(ctx));
    return Results.Ok(litters);
})
.WithName("GetAllLitters");

app.MapGet("/litters/{id:guid}", async (Guid id, HttpContext ctx, IPuppyService service) =>
{
    var litter = await service.GetLitterByIdAsync(id, GetUserId(ctx));
    return litter != null ? Results.Ok(litter) : Results.NotFound();
})
.WithName("GetLitterById");

app.MapPost("/litters", async (Litter litter, HttpContext ctx, IPuppyService service) =>
{
    var created = await service.CreateLitterAsync(litter, GetUserId(ctx));
    return Results.Created($"/litters/{created.Id}", created);
})
.WithName("CreateLitter");

app.MapPut("/litters/{id:guid}", async (Guid id, Litter litter, HttpContext ctx, IPuppyService service) =>
{
    var updated = await service.UpdateLitterAsync(id, litter, GetUserId(ctx));
    return updated != null ? Results.Ok(updated) : Results.NotFound();
})
.WithName("UpdateLitter");

app.MapDelete("/litters/{id:guid}", async (Guid id, HttpContext ctx, IPuppyService service) =>
{
    var deleted = await service.DeleteLitterAsync(id, GetUserId(ctx));
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteLitter");

app.MapGet("/litters/{litterId:guid}/puppies", async (Guid litterId, HttpContext ctx, IPuppyService service) =>
{
    var puppies = await service.GetPuppiesByLitterIdAsync(litterId, GetUserId(ctx));
    return Results.Ok(puppies);
})
.WithName("GetPuppiesByLitter");

app.MapPut("/litters/{litterId:guid}/puppies/{puppyId:guid}", async (Guid litterId, Guid puppyId, HttpContext ctx, IPuppyService service) =>
{
    var result = await service.AddPuppyToLitterAsync(litterId, puppyId, GetUserId(ctx));
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("AddPuppyToLitter");

app.MapPost("/litters/{litterId:guid}/puppies", async (Guid litterId, [Microsoft.AspNetCore.Mvc.FromBody] List<Guid> puppyIds, HttpContext ctx, IPuppyService service) =>
{
    var result = await service.AddPuppiesToLitterAsync(litterId, puppyIds, GetUserId(ctx));
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("AddPuppiesToLitter");

app.MapDelete("/litters/puppies/{puppyId:guid}", async (Guid puppyId, HttpContext ctx, IPuppyService service) =>
{
    var result = await service.RemovePuppyFromLitterAsync(puppyId, GetUserId(ctx));
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("RemovePuppyFromLitter");

// Litter member management endpoints
app.MapGet("/litters/{litterId:guid}/members", async (Guid litterId, HttpContext ctx, IPuppyService service) =>
{
    var members = await service.GetLitterMembersAsync(litterId, GetUserId(ctx));
    return Results.Ok(members);
})
.WithName("GetLitterMembers");

app.MapPost("/litters/{litterId:guid}/members", async (Guid litterId, [Microsoft.AspNetCore.Mvc.FromBody] AddMemberRequest request, HttpContext ctx, IPuppyService service) =>
{
    var member = await service.AddLitterMemberAsync(litterId, request.Email, request.Role, GetUserId(ctx));
    return member != null ? Results.Created($"/litters/{litterId}/members/{member.Id}", member) : Results.BadRequest("Unable to add member. Check permissions and that user is not already a member.");
})
.WithName("AddLitterMember");

app.MapPut("/litters/{litterId:guid}/members/{memberId:guid}", async (Guid litterId, Guid memberId, [Microsoft.AspNetCore.Mvc.FromBody] UpdateMemberRoleRequest request, HttpContext ctx, IPuppyService service) =>
{
    var result = await service.UpdateLitterMemberRoleAsync(litterId, memberId, request.Role, GetUserId(ctx));
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("UpdateLitterMemberRole");

app.MapDelete("/litters/{litterId:guid}/members/{memberId:guid}", async (Guid litterId, Guid memberId, HttpContext ctx, IPuppyService service) =>
{
    var result = await service.RemoveLitterMemberAsync(litterId, memberId, GetUserId(ctx));
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("RemoveLitterMember");

app.MapDefaultEndpoints();

app.Run();

// Request models for member management
public record AddMemberRequest(string Email, LitterRole Role);
public record UpdateMemberRoleRequest(LitterRole Role);
