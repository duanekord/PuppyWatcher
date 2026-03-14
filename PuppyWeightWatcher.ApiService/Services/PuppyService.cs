using Microsoft.EntityFrameworkCore;
using PuppyWeightWatcher.ApiService.Data;
using PuppyWeightWatcher.Shared.Models;

namespace PuppyWeightWatcher.ApiService.Services;

public interface IPuppyService
{
    Task<List<Puppy>> GetAllPuppiesAsync(string userId);
    Task<Puppy?> GetPuppyByIdAsync(Guid id, string userId);
    Task<Puppy> CreatePuppyAsync(Puppy puppy, string userId);
    Task<Puppy?> UpdatePuppyAsync(Guid id, Puppy puppy, string userId);
    Task<bool> DeletePuppyAsync(Guid id, string userId);
    Task<List<WeightEntry>> GetWeightEntriesAsync(Guid puppyId, string userId);
    Task<WeightEntry?> AddWeightEntryAsync(WeightEntry entry, string userId);
    Task<bool> DeleteWeightEntryAsync(Guid entryId, string userId);
    Task<WeightStatistics?> GetWeightStatisticsAsync(Guid puppyId, string userId);
    Task<ShotRecord?> AddShotRecordAsync(Guid puppyId, ShotRecord shotRecord, string userId);
    Task<bool> DeleteShotRecordAsync(Guid puppyId, Guid shotRecordId, string userId);
    Task<List<PuppyPhoto>> GetPhotosAsync(Guid puppyId, string userId);
    Task<PuppyPhoto?> GetPhotoAsync(Guid photoId, string userId);
    Task<PuppyPhoto?> AddPhotoAsync(PuppyPhoto photo, string userId);
    Task<bool> DeletePhotoAsync(Guid puppyId, Guid photoId, string userId);
    Task<bool> SetProfilePhotoAsync(Guid puppyId, Guid photoId, string userId);
    Task<Dictionary<Guid, PuppyPhoto>> GetProfilePhotosByPuppyIdsAsync(List<Guid> puppyIds);
    Task<List<Litter>> GetAllLittersAsync(string userId);
    Task<Litter?> GetLitterByIdAsync(Guid id, string userId);
    Task<Litter> CreateLitterAsync(Litter litter, string userId);
    Task<Litter?> UpdateLitterAsync(Guid id, Litter litter, string userId);
    Task<bool> DeleteLitterAsync(Guid id, string userId);
    Task<List<Puppy>> GetPuppiesByLitterIdAsync(Guid litterId, string userId);
    Task<bool> AddPuppyToLitterAsync(Guid litterId, Guid puppyId, string userId);
    Task<bool> AddPuppiesToLitterAsync(Guid litterId, List<Guid> puppyIds, string userId);
    Task<bool> RemovePuppyFromLitterAsync(Guid puppyId, string userId);
    // Litter member management
    Task<List<LitterMember>> GetLitterMembersAsync(Guid litterId, string userId);
    Task<LitterMember?> AddLitterMemberAsync(Guid litterId, string memberEmail, LitterRole role, string userId);
    Task<bool> UpdateLitterMemberRoleAsync(Guid litterId, Guid memberId, LitterRole role, string userId);
    Task<bool> RemoveLitterMemberAsync(Guid litterId, Guid memberId, string userId);
}

public class PuppyService(PuppyDbContext db) : IPuppyService
{
    // --- Helper methods for access control ---

    private async Task<List<Guid>> GetAccessibleLitterIdsAsync(string userId)
    {
        return await db.LitterMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.LitterId)
            .ToListAsync();
    }

    private async Task<LitterRole?> GetLitterRoleAsync(Guid litterId, string userId)
    {
        return await db.LitterMembers
            .AsNoTracking()
            .Where(m => m.LitterId == litterId && m.UserId == userId)
            .Select(m => (LitterRole?)m.Role)
            .FirstOrDefaultAsync();
    }

    private async Task<(bool canAccess, bool canEdit, bool canDelete, LitterRole? role)> GetPuppyPermissionsAsync(Guid puppyId, string userId)
    {
        var puppy = await db.Puppies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == puppyId);
        if (puppy == null)
            return (false, false, false, null);

        // Standalone puppy — only owner has access
        if (puppy.LitterId == null)
        {
            var isOwner = puppy.OwnerId == userId;
            return (isOwner, isOwner, isOwner, null);
        }

        // Puppy in a litter — access determined by litter membership
        var role = await GetLitterRoleAsync(puppy.LitterId.Value, userId);
        if (role == null)
            return (false, false, false, null);

        return role.Value switch
        {
            LitterRole.Owner => (true, true, true, role),
            LitterRole.CoOwner => (true, true, false, role),
            LitterRole.Viewer => (true, false, false, role),
            _ => (false, false, false, role)
        };
    }

    // --- Puppy methods ---

    public async Task<List<Puppy>> GetAllPuppiesAsync(string userId)
    {
        var accessibleLitterIds = await GetAccessibleLitterIdsAsync(userId);

        var puppies = await db.Puppies
            .AsNoTracking()
            .Where(p =>
                (p.LitterId != null && accessibleLitterIds.Contains(p.LitterId.Value)) ||
                (p.LitterId == null && p.OwnerId == userId))
            .ToListAsync();

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

    public async Task<Puppy?> GetPuppyByIdAsync(Guid id, string userId)
    {
        var (canAccess, _, _, _) = await GetPuppyPermissionsAsync(id, userId);
        if (!canAccess)
            return null;

        var puppy = await db.Puppies.FindAsync(id);
        if (puppy != null)
        {
            puppy.ShotRecords = await db.ShotRecords
                .Where(s => s.PuppyId == id)
                .ToListAsync();
        }
        return puppy;
    }

    public async Task<Puppy> CreatePuppyAsync(Puppy puppy, string userId)
    {
        puppy.Id = Guid.NewGuid();
        puppy.OwnerId = userId;
        puppy.CreatedAt = DateTime.UtcNow;
        db.Puppies.Add(puppy);
        await db.SaveChangesAsync();
        return puppy;
    }

    public async Task<Puppy?> UpdatePuppyAsync(Guid id, Puppy puppy, string userId)
    {
        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(id, userId);
        if (!canAccess || !canEdit)
            return null;

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

    public async Task<bool> DeletePuppyAsync(Guid id, string userId)
    {
        var (canAccess, _, canDelete, _) = await GetPuppyPermissionsAsync(id, userId);
        if (!canAccess || !canDelete)
            return false;

        var puppy = await db.Puppies.FindAsync(id);
        if (puppy == null)
            return false;

        db.Puppies.Remove(puppy);
        await db.SaveChangesAsync();
        return true;
    }

    // --- Weight entry methods ---

    public async Task<List<WeightEntry>> GetWeightEntriesAsync(Guid puppyId, string userId)
    {
        var (canAccess, _, _, _) = await GetPuppyPermissionsAsync(puppyId, userId);
        if (!canAccess)
            return [];

        return await db.WeightEntries
            .AsNoTracking()
            .Where(w => w.PuppyId == puppyId)
            .OrderBy(w => w.Date)
            .ToListAsync();
    }

    public async Task<WeightEntry?> AddWeightEntryAsync(WeightEntry entry, string userId)
    {
        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(entry.PuppyId, userId);
        if (!canAccess || !canEdit)
            return null;

        entry.Id = Guid.NewGuid();
        entry.CreatedAt = DateTime.UtcNow;
        db.WeightEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteWeightEntryAsync(Guid entryId, string userId)
    {
        var entry = await db.WeightEntries.FindAsync(entryId);
        if (entry == null)
            return false;

        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(entry.PuppyId, userId);
        if (!canAccess || !canEdit)
            return false;

        db.WeightEntries.Remove(entry);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<WeightStatistics?> GetWeightStatisticsAsync(Guid puppyId, string userId)
    {
        var (canAccess, _, _, _) = await GetPuppyPermissionsAsync(puppyId, userId);
        if (!canAccess)
            return null;

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

    // --- Shot record methods ---

    public async Task<ShotRecord?> AddShotRecordAsync(Guid puppyId, ShotRecord shotRecord, string userId)
    {
        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(puppyId, userId);
        if (!canAccess || !canEdit)
            return null;

        var puppy = await db.Puppies.FindAsync(puppyId);
        if (puppy == null)
            return null;

        shotRecord.Id = Guid.NewGuid();
        shotRecord.PuppyId = puppyId;
        db.ShotRecords.Add(shotRecord);

        puppy.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return shotRecord;
    }

    public async Task<bool> DeleteShotRecordAsync(Guid puppyId, Guid shotRecordId, string userId)
    {
        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(puppyId, userId);
        if (!canAccess || !canEdit)
            return false;

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

    // --- Photo methods ---

    public async Task<List<PuppyPhoto>> GetPhotosAsync(Guid puppyId, string userId)
    {
        var (canAccess, _, _, _) = await GetPuppyPermissionsAsync(puppyId, userId);
        if (!canAccess)
            return [];

        return await db.PuppyPhotos
            .AsNoTracking()
            .Where(p => p.PuppyId == puppyId)
            .OrderByDescending(p => p.DateTaken)
            .ToListAsync();
    }

    public async Task<PuppyPhoto?> GetPhotoAsync(Guid photoId, string userId)
    {
        var photo = await db.PuppyPhotos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == photoId);
        if (photo == null)
            return null;

        var (canAccess, _, _, _) = await GetPuppyPermissionsAsync(photo.PuppyId, userId);
        return canAccess ? photo : null;
    }

    public async Task<PuppyPhoto?> AddPhotoAsync(PuppyPhoto photo, string userId)
    {
        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(photo.PuppyId, userId);
        if (!canAccess || !canEdit)
            return null;

        var puppy = await db.Puppies.FindAsync(photo.PuppyId);
        if (puppy == null)
            return null;

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

    public async Task<bool> DeletePhotoAsync(Guid puppyId, Guid photoId, string userId)
    {
        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(puppyId, userId);
        if (!canAccess || !canEdit)
            return false;

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

    public async Task<bool> SetProfilePhotoAsync(Guid puppyId, Guid photoId, string userId)
    {
        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(puppyId, userId);
        if (!canAccess || !canEdit)
            return false;

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

    // --- Litter methods ---

    public async Task<List<Litter>> GetAllLittersAsync(string userId)
    {
        var memberLitterIds = await db.LitterMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .ToListAsync();

        var litterIds = memberLitterIds.Select(m => m.LitterId).ToList();

        var litters = await db.Litters
            .AsNoTracking()
            .Where(l => litterIds.Contains(l.Id))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var puppyCounts = await db.Puppies
            .AsNoTracking()
            .Where(p => p.LitterId != null && litterIds.Contains(p.LitterId.Value))
            .GroupBy(p => p.LitterId!.Value)
            .Select(g => new { LitterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.LitterId, g => g.Count);

        var rolesByLitter = memberLitterIds.ToDictionary(m => m.LitterId, m => m.Role);

        foreach (var litter in litters)
        {
            litter.PuppyCount = puppyCounts.GetValueOrDefault(litter.Id, 0);
            litter.UserRole = rolesByLitter.GetValueOrDefault(litter.Id);
        }
        return litters;
    }

    public async Task<Litter?> GetLitterByIdAsync(Guid id, string userId)
    {
        var role = await GetLitterRoleAsync(id, userId);
        if (role == null)
            return null;

        var litter = await db.Litters.FindAsync(id);
        if (litter != null)
        {
            litter.PuppyCount = await db.Puppies.CountAsync(p => p.LitterId == id);
            litter.UserRole = role;
        }
        return litter;
    }

    public async Task<Litter> CreateLitterAsync(Litter litter, string userId)
    {
        litter.Id = Guid.NewGuid();
        litter.CreatedAt = DateTime.UtcNow;
        db.Litters.Add(litter);

        // Automatically add creator as Owner
        db.LitterMembers.Add(new LitterMember
        {
            Id = Guid.NewGuid(),
            LitterId = litter.Id,
            UserId = userId,
            Role = LitterRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        litter.UserRole = LitterRole.Owner;
        return litter;
    }

    public async Task<Litter?> UpdateLitterAsync(Guid id, Litter litter, string userId)
    {
        var role = await GetLitterRoleAsync(id, userId);
        if (role == null || role == LitterRole.Viewer)
            return null;

        var existing = await db.Litters.FindAsync(id);
        if (existing == null)
            return null;

        existing.Name = litter.Name;
        existing.DateOfBirth = litter.DateOfBirth;
        existing.Breed = litter.Breed;
        existing.Notes = litter.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        var puppies = await db.Puppies
            .Where(p => p.LitterId == id)
            .ToListAsync();

        foreach (var puppy in puppies)
        {
            puppy.DateOfBirth = litter.DateOfBirth;
            puppy.Breed = litter.Breed;
            puppy.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        existing.UserRole = role;
        return existing;
    }

    public async Task<bool> DeleteLitterAsync(Guid id, string userId)
    {
        var role = await GetLitterRoleAsync(id, userId);
        if (role != LitterRole.Owner)
            return false;

        var litter = await db.Litters.FindAsync(id);
        if (litter == null)
            return false;

        db.Litters.Remove(litter);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Puppy>> GetPuppiesByLitterIdAsync(Guid litterId, string userId)
    {
        var role = await GetLitterRoleAsync(litterId, userId);
        if (role == null)
            return [];

        return await db.Puppies
            .AsNoTracking()
            .Where(p => p.LitterId == litterId)
            .ToListAsync();
    }

    public async Task<bool> AddPuppyToLitterAsync(Guid litterId, Guid puppyId, string userId)
    {
        var role = await GetLitterRoleAsync(litterId, userId);
        if (role == null || role == LitterRole.Viewer)
            return false;

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

    public async Task<bool> AddPuppiesToLitterAsync(Guid litterId, List<Guid> puppyIds, string userId)
    {
        var role = await GetLitterRoleAsync(litterId, userId);
        if (role == null || role == LitterRole.Viewer)
            return false;

        var litter = await db.Litters.FindAsync(litterId);
        if (litter == null)
            return false;

        var puppies = await db.Puppies
            .Where(p => puppyIds.Contains(p.Id))
            .ToListAsync();

        foreach (var puppy in puppies)
        {
            puppy.LitterId = litterId;
            puppy.DateOfBirth = litter.DateOfBirth;
            if (!string.IsNullOrEmpty(litter.Breed))
                puppy.Breed = litter.Breed;
            puppy.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePuppyFromLitterAsync(Guid puppyId, string userId)
    {
        var (canAccess, canEdit, _, _) = await GetPuppyPermissionsAsync(puppyId, userId);
        if (!canAccess || !canEdit)
            return false;

        var puppy = await db.Puppies.FindAsync(puppyId);
        if (puppy == null)
            return false;

        puppy.LitterId = null;
        puppy.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    // --- Litter member management ---

    public async Task<List<LitterMember>> GetLitterMembersAsync(Guid litterId, string userId)
    {
        var role = await GetLitterRoleAsync(litterId, userId);
        if (role == null)
            return [];

        return await db.LitterMembers
            .AsNoTracking()
            .Where(m => m.LitterId == litterId)
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<LitterMember?> AddLitterMemberAsync(Guid litterId, string memberEmail, LitterRole role, string userId)
    {
        var callerRole = await GetLitterRoleAsync(litterId, userId);

        // Only Owner can add CoOwners; Owner and CoOwner can add Viewers
        if (callerRole == null)
            return null;
        if (role == LitterRole.CoOwner && callerRole != LitterRole.Owner)
            return null;
        if (role == LitterRole.Viewer && callerRole < LitterRole.CoOwner)
            return null;
        if (role == LitterRole.Owner)
            return null; // Cannot add another owner

        // Check if already a member
        var existing = await db.LitterMembers
            .FirstOrDefaultAsync(m => m.LitterId == litterId && m.UserId == memberEmail);
        if (existing != null)
            return null;

        var member = new LitterMember
        {
            Id = Guid.NewGuid(),
            LitterId = litterId,
            UserId = memberEmail,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        db.LitterMembers.Add(member);
        await db.SaveChangesAsync();
        return member;
    }

    public async Task<bool> UpdateLitterMemberRoleAsync(Guid litterId, Guid memberId, LitterRole role, string userId)
    {
        var callerRole = await GetLitterRoleAsync(litterId, userId);
        if (callerRole != LitterRole.Owner)
            return false;

        if (role == LitterRole.Owner)
            return false; // Cannot make another owner

        var member = await db.LitterMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.LitterId == litterId);
        if (member == null)
            return false;

        if (member.Role == LitterRole.Owner)
            return false; // Cannot change the owner's role

        member.Role = role;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveLitterMemberAsync(Guid litterId, Guid memberId, string userId)
    {
        var callerRole = await GetLitterRoleAsync(litterId, userId);
        if (callerRole == null)
            return false;

        var member = await db.LitterMembers
            .FirstOrDefaultAsync(m => m.Id == memberId && m.LitterId == litterId);
        if (member == null)
            return false;

        // Cannot remove the owner
        if (member.Role == LitterRole.Owner)
            return false;

        // CoOwner can only remove Viewers
        if (callerRole == LitterRole.CoOwner && member.Role != LitterRole.Viewer)
            return false;

        // Viewers cannot remove anyone
        if (callerRole == LitterRole.Viewer)
            return false;

        db.LitterMembers.Remove(member);
        await db.SaveChangesAsync();
        return true;
    }
}
