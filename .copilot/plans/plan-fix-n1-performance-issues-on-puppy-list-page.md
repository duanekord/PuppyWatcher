# 🎯 Fix N+1 Performance Issues on Puppy List Page

## Understanding
The Puppies list page is slow because it makes N+1 HTTP calls (one per puppy to fetch all photos) and the API itself has N+1 database queries (one per puppy for shot records). The fix involves batching these into single queries/calls.

## Assumptions
- The `Puppy` model already has `ProfilePhotoId` which can be used to efficiently fetch only profile photos
- The `ShotRecords` navigation property on Puppy is ignored by EF (`entity.Ignore(p => p.ShotRecords)`) so we need to manually load them, but can do it in a single query
- The Blazor page only needs profile photos for the list view, not all photos

## Approach
Fix the N+1 patterns at both layers:

**API Service layer**: Replace the loop-based shot record loading in `GetAllPuppiesAsync()` with a single batched query. Add a new `GetProfilePhotosByPuppyIdsAsync()` method that fetches only profile photos in one query.

**API endpoint layer**: Add a new batch endpoint `POST /puppies/profile-photos` that accepts puppy IDs and returns only profile photos.

**Client layer**: Add `GetProfilePhotosAsync(List<Guid>)` to `PuppyApiClient`.

**Blazor page**: Replace the sequential per-puppy photo loading loop with a single batch call.

## Key Files
- PuppyWeightWatcher.ApiService/Services/PuppyService.cs - Fix N+1 DB query, add batch profile photo method
- PuppyWeightWatcher.ApiService/Program.cs - Add batch profile photo endpoint
- PuppyWeightWatcher.Web/PuppyApiClient.cs - Add batch client method
- PuppyWeightWatcher.Web/Components/Pages/Puppies.razor - Replace N+1 HTTP loop with single call

## Risks & Open Questions
- The `ShotRecords` property is ignored by EF, so Include() won't work directly; we use a single grouped query instead

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-14 00:19:18

## 📝 Plan Steps
- ✅ **Fix N+1 database query in PuppyService.GetAllPuppiesAsync by replacing the per-puppy loop with a single batched shot records query**
- ✅ **Add GetProfilePhotosByPuppyIdsAsync method to IPuppyService interface and PuppyService implementation**
- ✅ **Add batch profile photos API endpoint in Program.cs**
- ✅ **Add GetProfilePhotosAsync client method in PuppyApiClient.cs**
- ✅ **Update Puppies.razor LoadPuppies to use the batch profile photos endpoint instead of per-puppy loop**
- ✅ **Build and verify the changes compile successfully**

