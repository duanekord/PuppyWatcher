# 🎯 Add Inline Collar Color Change on Puppy Detail Page

## Understanding
The user wants to change a puppy's collar color while preserving all associated data (weights, photos, shots, litter membership). The backend already supports this correctly via `UpdatePuppyAsync` which only updates specific fields on the existing entity. The edit page also already allows changing the color, but the user wants a more direct way. I'll add an inline color-change UI on the puppy detail page.

## Assumptions
- The backend `UpdatePuppyAsync` correctly preserves `Id`, `OwnerId`, `LitterId`, `ProfilePhotoId`, and all related records — only field-level updates are applied
- The existing `PuppyApiClient.UpdatePuppyAsync` sends a PUT with the full puppy object, and the backend only copies the editable fields
- The inline color change should use the same common colors already defined in `AddEditPuppy.razor`
- After saving, the page should refresh to show the new color in the header and info card

## Approach
Add an inline editing mode for the collar color on the `PuppyDetail.razor` page. When the user clicks a "Change Color" button next to the collar color text, it will show a color picker panel with the common color buttons and a text input. A "Save" button will call `UpdatePuppyAsync` with the existing puppy data but the new collar color. The page header color will update immediately. This keeps all data intact since we're sending the same puppy object with only the `CollarColor` changed.

## Key Files
- PuppyWeightWatcher.Web/Components/Pages/PuppyDetail.razor - add inline color editing UI and save logic

## Risks & Open Questions
- None significant — the backend update logic is already proven safe for field-level updates

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-30 02:05:05

## 📝 Plan Steps
- ✅ **Add state variables for inline color editing in PuppyDetail.razor @code section**
- ✅ **Replace the static collar color display with an inline editable section in the markup**
- ✅ **Add the SaveCollarColor and color helper methods in the @code section**
- ✅ **Build and verify no compilation errors**

