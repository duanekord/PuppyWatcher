# 🎯 Add List/Card View Toggle to Puppies Page

## Understanding
The user wants a toggle on the Puppies page to switch between the current card/grid view and a new list view. The toggle should be in the top-right area. The list view should show pertinent puppy info (collar color, name, breed, gender, age) with the last weight % change, but exclude shot info.

## Assumptions
- The toggle goes on the Puppies page itself (top-right, near the page header)
- Weight statistics need to be loaded per puppy to show the last % weight change (using `DayOverDayPercentChange` from `WeightStatistics`)
- The card view remains as-is; list view is a responsive table
- View preference is stored in component state (not persisted across sessions)

## Approach
Modify [Puppies.razor](PuppyWeightWatcher.Web/Components/Pages/Puppies.razor) to add a `viewMode` state variable, load `WeightStatistics` for each puppy alongside the existing data, add toggle buttons near the page header, and conditionally render either the card grid or a table list view. The list view will show a colored collar indicator, name, breed, gender, age, current weight, and last % weight change.

## Key Files
- PuppyWeightWatcher.Web/Components/Pages/Puppies.razor - all changes in this file

**Progress**: 75% [███████░░░]

**Last Updated**: 2026-03-15 03:33:49

## 📝 Plan Steps
- ✅ **Add view toggle state and weight statistics dictionary to @code block, and load statistics in LoadPuppies**
- ✅ **Add toggle buttons next to the page header and Add button**
- ✅ **Add list view table markup with conditional rendering between card and list views**
- ⏭️ **Build and verify compilation**

