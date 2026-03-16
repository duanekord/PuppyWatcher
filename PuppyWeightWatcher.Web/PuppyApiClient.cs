using PuppyWeightWatcher.Shared.Models;

namespace PuppyWeightWatcher.Web;

public class PuppyApiClient(HttpClient httpClient)
{
    // Puppy operations
    public async Task<List<Puppy>> GetAllPuppiesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Puppy>>("/puppies", cancellationToken) ?? new List<Puppy>();
    }

    public async Task<Puppy?> GetPuppyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<Puppy>($"/puppies/{id}", cancellationToken);
    }

    public async Task<Puppy?> CreatePuppyAsync(Puppy puppy, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/puppies", puppy, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Puppy>(cancellationToken);
    }

    public async Task<Puppy?> UpdatePuppyAsync(Guid id, Puppy puppy, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/puppies/{id}", puppy, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Puppy>(cancellationToken);
    }

    public async Task<bool> DeletePuppyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/puppies/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    // Weight entry operations
    public async Task<List<WeightEntry>> GetWeightEntriesAsync(Guid puppyId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<WeightEntry>>($"/puppies/{puppyId}/weights", cancellationToken) ?? new List<WeightEntry>();
    }

    public async Task<WeightEntry?> AddWeightEntryAsync(Guid puppyId, WeightEntry entry, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/puppies/{puppyId}/weights", entry, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WeightEntry>(cancellationToken);
    }

    public async Task<bool> DeleteWeightEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/weights/{entryId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    // Statistics
    public async Task<WeightStatistics?> GetWeightStatisticsAsync(Guid puppyId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<WeightStatistics>($"/puppies/{puppyId}/statistics", cancellationToken);
    }

    // Shot record operations
    public async Task<ShotRecord?> AddShotRecordAsync(Guid puppyId, ShotRecord shotRecord, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/puppies/{puppyId}/shots", shotRecord, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShotRecord>(cancellationToken);
    }

    public async Task<bool> DeleteShotRecordAsync(Guid puppyId, Guid shotRecordId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/puppies/{puppyId}/shots/{shotRecordId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    // Photo operations
    public async Task<Dictionary<Guid, PuppyPhoto>> GetProfilePhotosAsync(List<Guid> puppyIds, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/puppies/profile-photos", puppyIds, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Dictionary<Guid, PuppyPhoto>>(cancellationToken) ?? new();
    }

    public async Task<List<PuppyPhoto>> GetPhotosAsync(Guid puppyId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<PuppyPhoto>>($"/puppies/{puppyId}/photos", cancellationToken) ?? new List<PuppyPhoto>();
    }

    public async Task<PuppyPhoto?> UploadPhotoAsync(Guid puppyId, Stream fileStream, string fileName, string contentType, string? caption, DateTime dateTaken, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(memoryStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        if (!string.IsNullOrWhiteSpace(caption))
            content.Add(new StringContent(caption), "caption");

        content.Add(new StringContent(dateTaken.ToString("o")), "dateTaken");

        var response = await httpClient.PostAsync($"/puppies/{puppyId}/photos", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PuppyPhoto>(cancellationToken);
    }

    public async Task<bool> DeletePhotoAsync(Guid puppyId, Guid photoId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/puppies/{puppyId}/photos/{photoId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetProfilePhotoAsync(Guid puppyId, Guid photoId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsync($"/puppies/{puppyId}/photos/{photoId}/profile", null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    // Litter operations
    public async Task<List<Litter>> GetAllLittersAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Litter>>("/litters", cancellationToken) ?? new List<Litter>();
    }

    public async Task<Litter?> GetLitterByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<Litter>($"/litters/{id}", cancellationToken);
    }

    public async Task<Litter?> CreateLitterAsync(Litter litter, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/litters", litter, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Litter>(cancellationToken);
    }

    public async Task<Litter?> UpdateLitterAsync(Guid id, Litter litter, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/litters/{id}", litter, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Litter>(cancellationToken);
    }

    public async Task<bool> DeleteLitterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/litters/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Puppy>> GetPuppiesByLitterIdAsync(Guid litterId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Puppy>>($"/litters/{litterId}/puppies", cancellationToken) ?? new List<Puppy>();
    }

    public async Task<bool> AddPuppyToLitterAsync(Guid litterId, Guid puppyId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsync($"/litters/{litterId}/puppies/{puppyId}", null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AddPuppiesToLitterAsync(Guid litterId, List<Guid> puppyIds, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/litters/{litterId}/puppies", puppyIds, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemovePuppyFromLitterAsync(Guid puppyId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/litters/puppies/{puppyId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    // Litter member operations
    public async Task<List<LitterMember>> GetLitterMembersAsync(Guid litterId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<LitterMember>>($"/litters/{litterId}/members", cancellationToken) ?? new List<LitterMember>();
    }

    public async Task<LitterMember?> AddLitterMemberAsync(Guid litterId, string email, LitterRole role, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/litters/{litterId}/members", new { Email = email, Role = role }, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<LitterMember>(cancellationToken);
    }

    public async Task<bool> UpdateLitterMemberRoleAsync(Guid litterId, Guid memberId, LitterRole role, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/litters/{litterId}/members/{memberId}", new { Role = role }, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveLitterMemberAsync(Guid litterId, Guid memberId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/litters/{litterId}/members/{memberId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
