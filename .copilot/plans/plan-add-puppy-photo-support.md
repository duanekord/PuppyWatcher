# 🎯 Add Puppy Photo Support

## Understanding
Add the ability to upload multiple photos per puppy, designate one as the "profile pic", show profile pics on the puppy list cards, and display a full photo gallery on the puppy detail page.

## Assumptions
- Photos stored as base64 data URLs in-memory (consistent with current in-memory storage pattern)
- Upload via multipart form data at the API, rendered as `<img>` data URIs on the frontend
- Each photo gets a caption, date taken, and an `IsProfilePhoto` flag
- Only one profile photo at a time per puppy
- Profile photo shown on list cards and at the top of the detail page

## Approach
Add a `PuppyPhoto` shared model, then extend `IPuppyService` / `PuppyService` with photo CRUD and set-profile-pic logic. Wire up multipart upload and GET/DELETE endpoints in Program.cs. Extend `PuppyApiClient` with photo methods using `MultipartFormDataContent`. Update `Puppies.razor` to show the profile pic thumbnail on each card. Add a Photos section to `PuppyDetail.razor` with upload form, gallery grid, set-as-profile and delete buttons.

## Key Files
- PuppyWeightWatcher.Shared/Models/PuppyPhoto.cs - new model
- PuppyWeightWatcher.Shared/Models/Puppy.cs - add ProfilePhotoId
- PuppyWeightWatcher.ApiService/Services/PuppyService.cs - photo service methods
- PuppyWeightWatcher.ApiService/Program.cs - photo API endpoints
- PuppyWeightWatcher.Web/PuppyApiClient.cs - photo client methods
- PuppyWeightWatcher.Web/Components/Pages/Puppies.razor - profile pic on cards
- PuppyWeightWatcher.Web/Components/Pages/PuppyDetail.razor - photo gallery + upload

## Risks & Open Questions
- Large photos stored in memory; acceptable for MVP but not for production
- No image resizing/thumbnailing; relies on CSS for display sizing

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-13 22:39:43

## 📝 Plan Steps
- ✅ **Create PuppyPhoto model in Shared project**
- ✅ **Add ProfilePhotoId property to the Puppy model**
- ✅ **Add photo methods to IPuppyService interface and PuppyService implementation**
- ✅ **Add photo API endpoints in ApiService Program.cs**
- ✅ **Add photo methods to PuppyApiClient in Web project**
- ✅ **Update Puppies.razor list page to show profile photo on cards**
- ✅ **Add photo gallery and upload section to PuppyDetail.razor**
- ✅ **Build and verify**

