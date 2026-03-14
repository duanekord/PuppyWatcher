using Microsoft.EntityFrameworkCore;
using PuppyWeightWatcher.ApiService.Data;
using PuppyWeightWatcher.Shared.Models;

namespace PuppyWeightWatcher.ApiService.Services;

public interface IPuppyService
{
    Task<List<Puppy>> GetAllPuppiesAsync();
    Task<Puppy?> GetPuppyByIdAsync(Guid id);
    Task<Puppy> CreatePuppyAsync(Puppy puppy);
    Task<Puppy?> UpdatePuppyAsync(Guid id, Puppy puppy);
    Task<bool> DeletePuppyAsync(Guid id);
    Task<List<WeightEntry>> GetWeightEntriesAsync(Guid puppyId);
    Task<WeightEntry> AddWeightEntryAsync(WeightEntry entry);
    Task<bool> DeleteWeightEntryAsync(Guid entryId);
    Task<WeightStatistics> GetWeightStatisticsAsync(Guid puppyId);
    Task<ShotRecord> AddShotRecordAsync(Guid puppyId, ShotRecord shotRecord);
    Task<bool> DeleteShotRecordAsync(Guid puppyId, Guid shotRecordId);
    Task<List<PuppyPhoto>> GetPhotosAsync(Guid puppyId);
    Task<PuppyPhoto?> GetPhotoAsync(Guid photoId);
    Task<PuppyPhoto> AddPhotoAsync(PuppyPhoto photo);
    Task<bool> DeletePhotoAsync(Guid puppyId, Guid photoId);
    Task<bool> SetProfilePhotoAsync(Guid puppyId, Guid photoId);
    Task<Dictionary<Guid, PuppyPhoto>> GetProfilePhotosByPuppyIdsAsync(List<Guid> puppyIds);
    Task<List<Litter>> GetAllLittersAsync();
    Task<Litter?> GetLitterByIdAsync(Guid id);
    Task<Litter> CreateLitterAsync(Litter litter);
    Task<Litter?> UpdateLitterAsync(Guid id, Litter litter);
    Task<bool> DeleteLitterAsync(Guid id);
    Task<List<Puppy>> GetPuppiesByLitterIdAsync(Guid litterId);
    Task<bool> AddPuppyToLitterAsync(Guid litterId, Guid puppyId);
    Task<bool> RemovePuppyFromLitterAsync(Guid puppyId);
}

public class PuppyService(PuppyDbContext db) : IPuppyService
{
    public async Task<List<Puppy>> GetAllPuppiesAsync()
    {
        var puppies = await db.Puppies.AsNoTracking().ToListAsync();
        var puppyIds = puppies.Select(p => p.Id).ToList();
        var allShotRecords = await db.ShotRecords
            .AsNoTracking()
            .Where(s => puppyIds.Contains(s.PuppyId))
            .ToListAsync();
        var shotRecordsByPuppy = allShotRecords.GroupBy(s => s.PuppyId)
            .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var puppy in puppies)
        {
            puppy.ShotRecords = shotRecordsByPuppy.GetValueOrDefault(puppy.Id, []);
        }
        return puppies;
    }

    public async Task<Puppy?> GetPuppyByIdAsync(Guid id)
    {
        var puppy = await db.Puppies.FindAsync(id);
        if (puppy != null)
        {
            puppy.ShotRecords = await db.ShotRecords
                .Where(s => s.PuppyId == id)
                .ToListAsync();
        }
        return puppy;
    }

    public async Task<Puppy> CreatePuppyAsync(Puppy puppy)
    {
        puppy.Id = Guid.NewGuid();
        puppy.CreatedAt = DateTime.UtcNow;
        db.Puppies.Add(puppy);
        await db.SaveChangesAsync();
        return puppy;
    }

    public async Task<Puppy?> UpdatePuppyAsync(Guid id, Puppy puppy)
    {
        var existingPuppy = await db.Puppies.FindAsync(id);
        if (existingPuppy == null)
            return null;

        existingPuppy.CollarColor = puppy.CollarColor;
        existingPuppy.Name = puppy.Name;
        existingPuppy.DateOfBirth = puppy.DateOfBirth;
        existingPuppy.Breed = puppy.Breed;
        existingPuppy.Gender = puppy.Gender;
        existingPuppy.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existingPuppy;
    }

    public async Task<bool> DeletePuppyAsync(Guid id)
    {
        var puppy = await db.Puppies.FindAsync(id);
        if (puppy == null)
            return false;

        db.Puppies.Remove(puppy);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<WeightEntry>> GetWeightEntriesAsync(Guid puppyId)
    {
        return await db.WeightEntries
            .AsNoTracking()
            .Where(w => w.PuppyId == puppyId)
            .OrderBy(w => w.Date)
            .ToListAsync();
    }

    public async Task<WeightEntry> AddWeightEntryAsync(WeightEntry entry)
    {
        entry.Id = Guid.NewGuid();
        entry.CreatedAt = DateTime.UtcNow;
        db.WeightEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteWeightEntryAsync(Guid entryId)
    {
        var entry = await db.WeightEntries.FindAsync(entryId);
        if (entry == null)
            return false;

        db.WeightEntries.Remove(entry);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<WeightStatistics> GetWeightStatisticsAsync(Guid puppyId)
    {
        var entries = await db.WeightEntries
            .AsNoTracking()
            .Where(w => w.PuppyId == puppyId)
            .OrderBy(w => w.Date)
            .ToListAsync();

        var stats = new WeightStatistics
        {
            PuppyId = puppyId,
            TotalWeightEntries = entries.Count
        };

        if (entries.Count == 0)
            return stats;

        var latestEntry = entries[^1];
        stats.CurrentWeight = latestEntry.WeightValue;
        stats.CurrentWeightUnit = latestEntry.Unit;
        stats.LastWeightDate = latestEntry.Date;
        stats.FirstWeightDate = entries[0].Date;

        stats.AverageWeight = entries.Average(e => e.WeightValue);

        var previousDay = entries
            .Where(e => e.Date < latestEntry.Date)
            .OrderByDescending(e => e.Date)
            .FirstOrDefault();

        if (previousDay != null)
        {
            stats.PreviousDayWeight = previousDay.WeightValue;
            stats.DayOverDayChange = latestEntry.WeightValue - previousDay.WeightValue;
            stats.DayOverDayPercentChange = (stats.DayOverDayChange / previousDay.WeightValue) * 100;
        }

        var weekAgoDate = latestEntry.Date.AddDays(-7);
        var previousWeek = entries
            .Where(e => e.Date <= weekAgoDate)
            .OrderByDescending(e => e.Date)
            .FirstOrDefault();

        if (previousWeek != null)
        {
            stats.PreviousWeekWeight = previousWeek.WeightValue;
            stats.WeekOverWeekChange = latestEntry.WeightValue - previousWeek.WeightValue;
            stats.WeekOverWeekPercentChange = (stats.WeekOverWeekChange / previousWeek.WeightValue) * 100;
        }

        if (entries.Count > 1)
        {
            stats.TotalWeightGain = latestEntry.WeightValue - entries[0].WeightValue;
            stats.TotalPercentGain = (stats.TotalWeightGain / entries[0].WeightValue) * 100;
        }

        return stats;
    }

    public async Task<ShotRecord> AddShotRecordAsync(Guid puppyId, ShotRecord shotRecord)
    {
        var puppy = await db.Puppies.FindAsync(puppyId);
        if (puppy == null)
            throw new InvalidOperationException($"Puppy with ID {puppyId} not found");

        shotRecord.Id = Guid.NewGuid();
        shotRecord.PuppyId = puppyId;
        db.ShotRecords.Add(shotRecord);

        puppy.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return shotRecord;
    }

    public async Task<bool> DeleteShotRecordAsync(Guid puppyId, Guid shotRecordId)
    {
        var shotRecord = await db.ShotRecords.FirstOrDefaultAsync(s => s.Id == shotRecordId && s.PuppyId == puppyId);
        if (shotRecord == null)
            return false;

        db.ShotRecords.Remove(shotRecord);

        var puppy = await db.Puppies.FindAsync(puppyId);
        if (puppy != null)
            puppy.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<PuppyPhoto>> GetPhotosAsync(Guid puppyId)
    {
        return await db.PuppyPhotos
            .AsNoTracking()
            .Where(p => p.PuppyId == puppyId)
            .OrderByDescending(p => p.DateTaken)
            .ToListAsync();
    }

    public async Task<PuppyPhoto?> GetPhotoAsync(Guid photoId)
    {
        return await db.PuppyPhotos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == photoId);
    }

    public async Task<PuppyPhoto> AddPhotoAsync(PuppyPhoto photo)
    {
        var puppy = await db.Puppies.FindAsync(photo.PuppyId);
        if (puppy == null)
            throw new InvalidOperationException($"Puppy with ID {photo.PuppyId} not found");

        photo.Id = Guid.NewGuid();
        photo.CreatedAt = DateTime.UtcNow;

        var hasPhotos = await db.PuppyPhotos.AnyAsync(p => p.PuppyId == photo.PuppyId);
        if (!hasPhotos)
        {
            photo.IsProfilePhoto = true;
            puppy.ProfilePhotoId = photo.Id;
        }

        db.PuppyPhotos.Add(photo);
        puppy.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return photo;
    }

    public async Task<bool> DeletePhotoAsync(Guid puppyId, Guid photoId)
    {
        var photo = await db.PuppyPhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.PuppyId == puppyId);
        if (photo == null)
            return false;

        db.PuppyPhotos.Remove(photo);

        var puppy = await db.Puppies.FindAsync(puppyId);
        if (puppy != null && puppy.ProfilePhotoId == photoId)
        {
            var nextPhoto = await db.PuppyPhotos
                .Where(p => p.PuppyId == puppyId && p.Id != photoId)
                .OrderByDescending(p => p.DateTaken)
                .FirstOrDefaultAsync();

            if (nextPhoto != null)
            {
                nextPhoto.IsProfilePhoto = true;
                puppy.ProfilePhotoId = nextPhoto.Id;
            }
            else
            {
                puppy.ProfilePhotoId = null;
            }
        }

        if (puppy != null)
            puppy.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetProfilePhotoAsync(Guid puppyId, Guid photoId)
    {
        var puppy = await db.Puppies.FindAsync(puppyId);
        if (puppy == null)
            return false;

        var photo = await db.PuppyPhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.PuppyId == puppyId);
        if (photo == null)
            return false;

        var currentPhotos = await db.PuppyPhotos.Where(p => p.PuppyId == puppyId).ToListAsync();
        foreach (var p in currentPhotos)
            p.IsProfilePhoto = false;

        photo.IsProfilePhoto = true;
        puppy.ProfilePhotoId = photoId;
        puppy.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<Guid, PuppyPhoto>> GetProfilePhotosByPuppyIdsAsync(List<Guid> puppyIds)
    {
        var profilePhotos = await db.PuppyPhotos
            .AsNoTracking()
            .Where(p => puppyIds.Contains(p.PuppyId) && p.IsProfilePhoto)
            .ToListAsync();
        return profilePhotos.ToDictionary(p => p.PuppyId);
    }

    public async Task<List<Litter>> GetAllLittersAsync()
    {
        var litters = await db.Litters.AsNoTracking().OrderByDescending(l => l.CreatedAt).ToListAsync();
        var litterIds = litters.Select(l => l.Id).ToList();
        var puppyCounts = await db.Puppies
            .AsNoTracking()
            .Where(p => p.LitterId != null && litterIds.Contains(p.LitterId.Value))
            .GroupBy(p => p.LitterId!.Value)
            .Select(g => new { LitterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.LitterId, g => g.Count);
        foreach (var litter in litters)
        {
            litter.PuppyCount = puppyCounts.GetValueOrDefault(litter.Id, 0);
        }
        return litters;
    }

    public async Task<Litter?> GetLitterByIdAsync(Guid id)
    {
        var litter = await db.Litters.FindAsync(id);
        if (litter != null)
        {
            litter.PuppyCount = await db.Puppies.CountAsync(p => p.LitterId == id);
        }
        return litter;
    }

    public async Task<Litter> CreateLitterAsync(Litter litter)
    {
        litter.Id = Guid.NewGuid();
        litter.CreatedAt = DateTime.UtcNow;
        db.Litters.Add(litter);
        await db.SaveChangesAsync();
        return litter;
    }

    public async Task<Litter?> UpdateLitterAsync(Guid id, Litter litter)
    {
        var existing = await db.Litters.FindAsync(id);
        if (existing == null)
            return null;

        existing.Name = litter.Name;
        existing.DateOfBirth = litter.DateOfBirth;
        existing.Breed = litter.Breed;
        existing.Notes = litter.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteLitterAsync(Guid id)
    {
        var litter = await db.Litters.FindAsync(id);
        if (litter == null)
            return false;

        db.Litters.Remove(litter);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Puppy>> GetPuppiesByLitterIdAsync(Guid litterId)
    {
        return await db.Puppies
            .AsNoTracking()
            .Where(p => p.LitterId == litterId)
            .ToListAsync();
    }

    public async Task<bool> AddPuppyToLitterAsync(Guid litterId, Guid puppyId)
    {
        var litter = await db.Litters.FindAsync(litterId);
        if (litter == null)
            return false;

        var puppy = await db.Puppies.FindAsync(puppyId);
        if (puppy == null)
            return false;

        puppy.LitterId = litterId;
        puppy.DateOfBirth = litter.DateOfBirth;
        if (!string.IsNullOrEmpty(litter.Breed))
            puppy.Breed = litter.Breed;
        puppy.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePuppyFromLitterAsync(Guid puppyId)
    {
        var puppy = await db.Puppies.FindAsync(puppyId);
        if (puppy == null)
            return false;

        puppy.LitterId = null;
        puppy.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }
}
