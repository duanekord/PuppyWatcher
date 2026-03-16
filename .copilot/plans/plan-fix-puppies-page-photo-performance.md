# 🎯 Fix Puppies Page Photo Performance

## Understanding
The Puppies list page loads full Base64 photo data for every puppy's profile photo in a single API call, then embeds them as data URIs via the SignalR circuit. This causes massive data transfer and slow page loads.

## Assumptions
- Photos are stored as Base64 in the database (`PuppyPhoto.Base64Data`)
- The PuppyDetail page gallery can continue using Base64 data URIs since it loads one puppy at a time
- Only the Puppies list page needs optimization since it loads all photos at once

## Approach
Add a streaming photo endpoint to the Web app that proxies image requests to the API service. Then modify the profile photos API to return only metadata (IDs, content types) without Base64 data. Update the Puppies page to render `<img src="/api/photos/{id}">` tags that the browser fetches lazily. This eliminates the massive upfront data load and lets the browser cache images independently.

## Key Files
- `PuppyWeightWatcher.ApiService/Services/PuppyService.cs` - modify profile photos query to exclude Base64Data
- `PuppyWeightWatcher.ApiService/Program.cs` - add raw image serving endpoint
- `PuppyWeightWatcher.Web/Program.cs` - add photo proxy endpoint
- `PuppyWeightWatcher.Web/Components/Pages/Puppies.razor` - use img src URLs instead of data URIs

## Risks & Open Questions
- Need to ensure the proxy endpoint passes authentication/user context through

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-16 01:21:32

## 📝 Plan Steps
- ✅ **Add a raw image serving endpoint to the API service that returns bytes with correct content type**
- ✅ **Add a photo proxy endpoint to the Web app that forwards requests to the API service**
- ✅ **Modify GetProfilePhotosByPuppyIdsAsync to exclude Base64Data from the query projection**
- ✅ **Update the Puppies page card view to use img src URLs instead of data URIs**
- ✅ **Update the Puppies page to only store profile photo IDs and content types instead of full photo objects**
- ✅ **Build and verify compilation**

