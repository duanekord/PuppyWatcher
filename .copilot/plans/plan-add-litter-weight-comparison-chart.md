# 🎯 Add Litter Weight Comparison Chart

## Understanding
The user wants a multi-line weight comparison chart on the Litter Details page that shows all puppies' weights over time, with each puppy as a separate line colored by their collar color.

## Assumptions
- Chart.js v4 is already loaded globally (confirmed in App.razor line 22)
- The existing `weightChart.js` handles single-dataset charts; a new function is needed for multi-dataset
- Weight data for each puppy can be fetched via `PuppyApiClient.GetWeightEntriesAsync(puppyId)`
- Collar colors from `GetColorValue()` can serve as line colors
- The chart should use the same Days/Weeks toggle pattern as PuppyDetail.razor

## Approach
Add a `renderMulti` function to `weightChart.js` that accepts multiple datasets (one per puppy, each with a name, color, and data points). Then update `LitterDetail.razor` to inject `IJSRuntime`, load weight entries for all litter puppies, prepare the datasets, and render the chart after the component mounts. Include proper `IAsyncDisposable` with the `InvalidOperationException` catch pattern established earlier.

## Key Files
- `PuppyWeightWatcher.Web/wwwroot/js/weightChart.js` - add multi-dataset render function
- `PuppyWeightWatcher.Web/Components/Pages/LitterDetail.razor` - add chart UI and data loading

## Risks & Open Questions
- Puppies may have weight entries on different dates; need to build a unified date axis
- If no puppies have weight data, the chart section should be hidden gracefully

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-15 06:28:07

## 📝 Plan Steps
- ✅ **Add `renderMulti` and `destroyMulti` functions to weightChart.js for multi-dataset line charts**
- ✅ **Add IJSRuntime injection, IAsyncDisposable, and chart state fields to LitterDetail.razor**
- ✅ **Add weight data loading logic to LitterDetail.razor LoadData method**
- ✅ **Add chart HTML section (canvas, Days/Weeks toggle) to LitterDetail.razor markup**
- ✅ **Add RenderChart, SetTimeframe, OnAfterRenderAsync, DisposeAsync, and GetColorValue methods to LitterDetail.razor code block**
- ✅ **Build and verify compilation**

