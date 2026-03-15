# 🎯 Add Weight Line Graph to Puppy Detail Page

## Understanding
The user wants a line graph on the puppy detail page that visualizes each puppy's weight over time, with the ability to toggle between day and week timeframes. The existing `PuppyDetail.razor` already has weight history data loaded via `PuppyApiClient.GetWeightEntriesAsync()`.

## Assumptions
- Chart will be placed on the `PuppyDetail.razor` page between the Weight Statistics card and the Weight History table
- Using Chart.js (v4) via JS interop as no charting library exists in the project
- Timeframe toggle will offer "Days" (individual entries) and "Weeks" (averaged by week) views
- The existing `weights` list (already loaded in `PuppyDetail.razor`) provides all necessary data — no new API endpoints needed
- The page uses `@rendermode InteractiveServer` so JS interop is available

## Approach
We'll add Chart.js via CDN in [App.razor](PuppyWeightWatcher.Web/Components/App.razor), create a small JS interop module [weightChart.js](PuppyWeightWatcher.Web/wwwroot/js/weightChart.js) that wraps Chart.js rendering, and add the chart UI directly to [PuppyDetail.razor](PuppyWeightWatcher.Web/Components/Pages/PuppyDetail.razor) with a timeframe selector. The chart will render using `IJSRuntime` to invoke the JS module after the component renders with data.

## Key Files
- PuppyWeightWatcher.Web/Components/App.razor - add Chart.js CDN script
- PuppyWeightWatcher.Web/wwwroot/js/weightChart.js - JS interop for Chart.js
- PuppyWeightWatcher.Web/Components/Pages/PuppyDetail.razor - add chart section and timeframe toggle

## Risks & Open Questions
- Chart.js CDN dependency adds an external resource; could be bundled locally if preferred
- Week averaging groups entries by ISO week; puppies with sparse data may show gaps

**Progress**: 80% [████████░░]

**Last Updated**: 2026-03-15 01:51:33

## 📝 Plan Steps
- ✅ **Add Chart.js CDN script reference to App.razor**
- ✅ **Create wwwroot/js/weightChart.js with chart rendering interop functions**
- ✅ **Add chart UI section with timeframe toggle to PuppyDetail.razor markup**
- ⏭️ **Add chart-related C# code (timeframe state, data preparation, JS interop calls) to PuppyDetail.razor @code block**
- ✅ **Build and verify compilation**

