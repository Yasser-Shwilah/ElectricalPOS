# RTL (Right-to-Left) Arabic Support for OxyPlot Charts in WPF

This guide explains the complete solution for displaying Arabic text correctly in OxyPlot charts for Arabic RTL languages.

## Problem Summary

When displaying Arabic text in OxyPlot charts:
- Arabic text may appear reversed or mirrored in some cases
- Charts need proper RTL layout direction
- Numbers must remain as Western Arabic numerals (0-9)
- Labels and axis titles need proper rendering

## Solution Overview

The solution uses OxyPlot's native WPF text rendering combined with proper XAML configuration:

### 1. XAML Configuration (Essential)

The most important part is the XAML configuration with `FlowDirection`:

```xml
<UserControl FlowDirection="RightToLeft"
            FontFamily="{StaticResource ArabicFont}"
            xmlns:oxy="http://oxyplot.org/wpf">
    
    <Grid>
        <oxy:PlotView Model="{Binding YourPlotModel}"
                      Background="White"
                      FlowDirection="RightToLeft"/>
    </Grid>
</UserControl>
```

**Key Points:**
- `FlowDirection="RightToLeft"` on the UserControl
- `FlowDirection="RightToLeft"` on the PlotView
- FontFamily set to Arabic-supported font

### 2. ViewModel Configuration

#### Helper Methods

The `ArabicChartHelper` class provides methods for Arabic text handling:

```csharp
public static class ArabicChartHelper
{
    /// <summary>
    /// Prepares Arabic text for display in OxyPlot charts
    /// </summary>
    public static string FixArabicText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Trim();
    }

    /// <summary>
    /// Maps English day name to Arabic
    /// </summary>
    public static string ToArabicDayName(string englishDayName)
    {
        return englishDayName?.ToLower() switch
        {
            "saturday" => "السبت",
            "sunday" => "الأحد",
            "monday" => "الإثنين",
            "tuesday" => "الثلاثاء",
            "wednesday" => "الأربعاء",
            "thursday" => "الخميس",
            "friday" => "الجمعة",
            _ => englishDayName ?? ""
        };
    }

    /// <summary>
    /// Formats number with Western Arabic numerals (0-9)
    /// </summary>
    public static string FormatNumber(double value, string format = "N0")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Color palette suitable for Arabic charts
    /// </summary>
    public static OxyColor[] GetArabicChartColors()
    {
        return new[]
        {
            OxyColor.Parse("#6C63FF"),  // Purple
            OxyColor.Parse("#00D9C0"),  // Teal
            OxyColor.Parse("#FF6B6B"),  // Red
            OxyColor.Parse("#F59E0B"),  // Amber
            OxyColor.Parse("#10B981"), // Green
            OxyColor.Parse("#3B82F6"), // Blue
            OxyColor.Parse("#EC4899"), // Pink
            OxyColor.Parse("#8B5CF6"),  // Violet
            OxyColor.Parse("#14B8A6"), // Cyan
            OxyColor.Parse("#F97316")   // Orange
        };
    }
}
```

#### Creating Charts with Arabic Support

```csharp
private PlotModel CreateLineChartModel(string title, string xTitle, string yTitle)
{
    var model = new PlotModel
    {
        Title = title,  // OxyPlot uses WPF text rendering
        TitleFontSize = 16,
        TitleFontWeight = OxyPlot.FontWeights.Bold,
        TitleColor = OxyColor.Parse("#333333"),
        Background = OxyColors.White,
        PlotAreaBackground = OxyColors.White,
        TextColor = OxyColor.Parse("#333333"),
        DefaultColors = ArabicChartHelper.GetArabicChartColors()
    };

    // X-Axis (Categories - Arabic labels)
    var categoryAxis = new CategoryAxis
    {
        Position = AxisPosition.Bottom,
        Title = xTitle,
        TitleFontSize = 13,
        TitleFontWeight = OxyPlot.FontWeights.Bold,
        AxislineColor = OxyColors.Gray,
        TextColor = OxyColor.Parse("#333333"),
        FontSize = 11,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.Parse("#E0E0E0"),
        IsZoomEnabled = false,
        IsPanEnabled = false
    };
    model.Axes.Add(categoryAxis);

    // Y-Axis (Values - Numbers stay Western Arabic)
    var valueAxis = new LinearAxis
    {
        Position = AxisPosition.Left,
        Title = yTitle,
        TitleFontSize = 13,
        TitleFontWeight = OxyPlot.FontWeights.Bold,
        AxislineColor = OxyColors.Gray,
        TextColor = OxyColor.Parse("#333333"),
        FontSize = 11,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.Parse("#E0E0E0"),
        MinimumPadding = 0.1,
        MaximumPadding = 0.1,
        AbsoluteMinimum = 0,
        StringFormat = "N0"  // Uses Western Arabic numerals
    };
    model.Axes.Add(valueAxis);

    // Line Series
    var lineSeries = new LineSeries
    {
        Title = "المبيعات",
        Color = OxyColor.Parse("#6C63FF"),
        StrokeThickness = 3,
        MarkerType = MarkerType.Circle,
        MarkerSize = 6,
        MarkerStroke = OxyColor.Parse("#6C63FF"),
        MarkerFill = OxyColors.White,
        Smooth = true
    };
    model.Series.Add(lineSeries);

    return model;
}
```

#### Loading Data with Arabic Labels

```csharp
private async Task LoadSalesLast7DaysChartAsync()
{
    var last7Days = await _statisticsService.GetSalesLast7DaysAsync();
    
    var model = CreateLineChartModel("مبيعات آخر 7 أيام", "الأيام", "المبيعات (ل.س)");
    var categoryAxis = model.Axes.OfType<CategoryAxis>().FirstOrDefault();
    var lineSeries = model.Series.OfType<LineSeries>().FirstOrDefault();

    if (categoryAxis != null && lineSeries != null)
    {
        foreach (var day in last7Days)
        {
            // Convert English day names to Arabic
            categoryAxis.Labels.Add(ArabicChartHelper.ToArabicDayName(day.DayName));
            lineSeries.Points.Add(new DataPoint(0, (double)day.SalesAmount));
        }
    }

    SalesLast7DaysPlotModel = model;
}
```

## Chart Types

### 1. Line Chart
```csharp
var lineSeries = new LineSeries
{
    Title = "العنوان",
    Color = OxyColor.Parse("#6C63FF"),
    StrokeThickness = 3,
    Smooth = true,
    MarkerType = MarkerType.Circle
};
model.Series.Add(lineSeries);
```

### 2. Bar Chart
```csharp
var barSeries = new BarSeries
{
    FillColor = OxyColor.Parse("#00D9C0"),
    BarWidth = 0.8
};
barSeries.Items.Add(new BarItem { Value = 100 });
barSeries.Items.Add(new BarItem { Value = 200 });
model.Series.Add(barSeries);
```

### 3. Pie Chart
```csharp
var pieSeries = new PieSeries
{
    InsideLabelPosition = 0.5,
    AngleSpan = 360,
    StartAngle = 0,
    OutsideLabelFormat = "{1}: {2:P1}"
};
pieSeries.Slices.Add(new PieSlice("فئة 1", 35));
pieSeries.Slices.Add(new PieSlice("فئة 2", 45));
pieSeries.Slices.Add(new PieSlice("فئة 3", 20));
model.Series.Add(pieSeries);
```

## Key Points

1. **WPF Text Rendering**: OxyPlot uses WPF's built-in text system which supports Arabic correctly when `FlowDirection="RightToLeft"` is set in XAML.

2. **Western Arabic Numerals**: Use `CultureInfo.InvariantCulture` for number formatting to ensure numbers display as 0-9 (not Arabic-Indic ٠-٩).

3. **FlowDirection in XAML**: Must be set on both the UserControl AND the PlotView for proper RTL layout.

4. **Font Family**: Use Arabic-supported fonts like "Segoe UI", "Tahoma", or "Traditional Arabic".

5. **Day Name Translation**: Use `ArabicChartHelper.ToArabicDayName()` to convert English day names to Arabic.

## File Locations

- **ViewModel**: `ViewModels/StatisticsViewModel_OxyPlot.cs`
- **View**: `Views/StatisticsDashboardOxyPlotView.xaml`
- **Helper Class**: `ArabicChartHelper` (inside StatisticsViewModel_OxyPlot.cs)

## Troubleshooting

If Arabic text still appears incorrectly:
1. Ensure `FlowDirection="RightToLeft"` is set on UserControl
2. Ensure `FlowDirection="RightToLeft"` is set on PlotView
3. Verify FontFamily includes Arabic-supported fonts
4. Check that numbers use `CultureInfo.InvariantCulture`

If numbers appear as Arabic-Indic (٠-٩):
- Use `value.ToString("N0", CultureInfo.InvariantCulture)` instead of Arabic culture
