# OxyPlot RTL Arabic Charts Guide

## Overview
This guide explains how to use OxyPlot for RTL Arabic charts in WPF, addressing the issues found in LiveCharts/LiveCharts2 with Arabic text rendering.

## Why OxyPlot?

### LiveCharts Issues with Arabic:
1. **Text Reversal**: Arabic labels appear reversed or mirrored
2. **Number Issues**: Numbers sometimes reversed incorrectly
3. **RTL Layout**: FlowDirection doesn't work properly
4. **PieChart**: Legends and category names don't display correctly

### OxyPlot Advantages:
1. **Native WPF Text Rendering**: Uses WPF's built-in text system which properly supports Arabic
2. **No Text Mirroring**: Arabic text renders correctly as-is
3. **RTL Support**: Works well with `FlowDirection="RightToLeft"`
4. **Numbers Stay Normal**: Western Arabic numerals (0-9) remain correct

## Quick Start

### 1. Install Package
```xml
<PackageReference Include="OxyPlot.Wpf" Version="2.2.0" />
```

### 2. Add XAML Namespace
```xml
xmlns:oxy="http://oxyplot.org/wpf"
```

### 3. Use in XAML
```xml
<Grid FlowDirection="RightToLeft">
    <oxy:PlotView Model="{Binding YourPlotModel}"
                  Background="White"
                  FlowDirection="RightToLeft"/>
</Grid>
```

## Complete Example

### ViewModel (C#)

```csharp
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

public PlotModel CreateLineChart()
{
    var model = new PlotModel
    {
        Title = "المبيعات اليومية",
        TitleFontSize = 16,
        TitleFontWeight = OxyPlot.FontWeights.Bold,
        TitleColor = OxyColor.Parse("#333333"),
        Background = OxyColors.White,
        PlotAreaBackground = OxyColors.White,
        TextColor = OxyColor.Parse("#333333")
    };

    // X-Axis (Categories - Arabic labels)
    var categoryAxis = new CategoryAxis
    {
        Position = AxisPosition.Bottom,
        Title = "الأيام",
        TitleFontSize = 14,
        TitleFontWeight = OxyPlot.FontWeights.Bold,
        AxislineColor = OxyColors.Gray,
        TextColor = OxyColor.Parse("#333333"),
        FontSize = 12,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.Parse("#E0E0E0"),
        IsZoomEnabled = false,
        IsPanEnabled = false
    };
    
    // Add Arabic labels
    categoryAxis.Labels.Add("السبت");
    categoryAxis.Labels.Add("الأحد");
    categoryAxis.Labels.Add("الإثنين");
    categoryAxis.Labels.Add("الثلاثاء");
    categoryAxis.Labels.Add("الأربعاء");
    categoryAxis.Labels.Add("الخميس");
    categoryAxis.Labels.Add("الجمعة");
    
    model.Axes.Add(categoryAxis);

    // Y-Axis (Values - Numbers stay Western Arabic)
    var valueAxis = new LinearAxis
    {
        Position = AxisPosition.Left,
        Title = "المبيعات (ل.س)",
        TitleFontSize = 14,
        TitleFontWeight = OxyPlot.FontWeights.Bold,
        AxislineColor = OxyColors.Gray,
        TextColor = OxyColor.Parse("#333333"),
        FontSize = 12,
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
    
    // Add data points
    lineSeries.Points.Add(new DataPoint(0, 15000));
    lineSeries.Points.Add(new DataPoint(1, 22000));
    lineSeries.Points.Add(new DataPoint(2, 18000));
    lineSeries.Points.Add(new DataPoint(3, 25000));
    lineSeries.Points.Add(new DataPoint(4, 20000));
    lineSeries.Points.Add(new DataPoint(5, 28000));
    lineSeries.Points.Add(new DataPoint(6, 32000));
    
    model.Series.Add(lineSeries);

    return model;
}
```

### XAML View

```xml
<UserControl x:Class="YourApp.ArabicChartView"
             xmlns:oxy="http://oxyplot.org/wpf"
             FlowDirection="RightToLeft"
             FontFamily="Segoe UI">

    <Grid>
        <oxy:PlotView Model="{Binding SalesChartModel}"
                      Background="White"
                      FlowDirection="RightToLeft"
                      Margin="10"/>
    </Grid>
</UserControl>
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

## Arabic Helper Methods

### Creating Arabic Category Axis
```csharp
public static CategoryAxis CreateArabicCategoryAxis(string title)
{
    return new CategoryAxis
    {
        Position = AxisPosition.Bottom,
        Title = title,
        TitleFontSize = 14,
        TitleFontWeight = OxyPlot.FontWeights.Bold,
        AxislineColor = OxyColors.Gray,
        TextColor = OxyColor.Parse("#333333"),
        FontSize = 12,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.Parse("#E0E0E0")
    };
}
```

### Arabic Day Names
```csharp
public static string[] ArabicDayNames = new[]
{
    "السبت", "الأحد", "الإثنين", "الثلاثاء", 
    "الأربعاء", "الخميس", "الجمعة"
};
```

### Arabic Month Names
```csharp
public static string[] ArabicMonthNames = new[]
{
    "يناير", "فبراير", "مارس", "أبريل",
    "مايو", "يونيو", "يوليو", "أغسطس",
    "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
};
```

### Color Palette for Arabic Charts
```csharp
public static OxyColor[] ArabicChartColors = new[]
{
    OxyColor.Parse("#6C63FF"),  // Purple
    OxyColor.Parse("#00D9C0"),  // Teal
    OxyColor.Parse("#FF6B6B"),  // Red
    OxyColor.Parse("#F59E0B"),  // Amber
    OxyColor.Parse("#10B981"), // Green
    OxyColor.Parse("#3B82F6"), // Blue
    OxyColor.Parse("#EC4899"), // Pink
    OxyColor.Parse("#8B5CF6")  // Violet
};
```

## Number Formatting

### Western Arabic Numerals (Default)
```csharp
// Uses 0,1,2,3,4,5,6,7,8,9 (correct for Arabic text)
axis.StringFormat = "N0";  // 1,234
axis.StringFormat = "C0";  // $1,234
```

### Arabic-Indic Numerals (Optional)
```csharp
// Uses ٠١٢٣٤٥٦٧٨٩ (if needed)
var arabicCulture = new CultureInfo("ar-SA");
axis.StringFormat = "N0";
// Then apply culture formatting when displaying
```

## Common Issues & Solutions

### Issue: Numbers display as Arabic-Indic (٠-٩)
**Solution**: Use `CultureInfo.InvariantCulture` for formatting:
```csharp
value.ToString("N0", CultureInfo.InvariantCulture)
```

### Issue: Text looks mirrored
**Solution**: OxyPlot handles this automatically with WPF text rendering

### Issue: Chart not showing in RTL
**Solution**: Ensure both parent Grid and PlotView have:
```xml
FlowDirection="RightToLeft"
```

### Issue: Tooltips showing English
**Solution**: Set tracker format string:
```csharp
lineSeries.TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:N0}";
```

## File Locations

- **ViewModel**: `ViewModels/StatisticsViewModel_OxyPlot.cs`
- **View**: `Views/StatisticsDashboardOxyPlotView.xaml`
- **Code-behind**: `Views/StatisticsDashboardOxyPlotView.xaml.cs`

## Integration with Existing App

To integrate this into your existing app:

1. Build the project to ensure OxyPlot is restored
2. Add the new view to your navigation
3. Update MainWindow.xaml to include the new view
4. Or replace existing StatisticsDashboardView with the OxyPlot version

## Performance Tips

1. **PlotModel is immutable**: Create new models, don't modify existing
2. **Use async loading**: Load data asynchronously, then update models
3. **Set IsZoomEnabled=false** for simpler interaction
4. **Use StringFormat** for number formatting instead of custom labelers
