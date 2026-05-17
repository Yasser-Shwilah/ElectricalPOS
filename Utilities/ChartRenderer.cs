using ElectricalPOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ElectricalPOS.Utilities
{
    /// <summary>
    /// Data point for charts
    /// </summary>
    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal Value2 { get; set; }
        public string Color { get; set; } = "#6366F1";
    }

    /// <summary>
    /// Utility class for rendering charts on Canvas
    /// </summary>
    public static class ChartRenderer
    {
        #region Line Chart

        public static void DrawLineChart(Canvas canvas, List<ChartDataPoint> dataPoints, string title = "")
        {
            if (dataPoints == null || dataPoints.Count == 0)
            {
                canvas.Children.Clear();
                return;
            }

            canvas.Children.Clear();

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            // Calculate margins
            double margin = 40;
            double chartWidth = width - 2 * margin;
            double chartHeight = height - 2 * margin - 40; // 40 for title

            // Draw title
            if (!string.IsNullOrEmpty(title))
            {
                DrawText(canvas, title, width / 2, margin / 2, 14, Colors.Black, true);
            }

            // Find min and max values
            decimal minValue = dataPoints.Min(p => p.Value);
            decimal maxValue = dataPoints.Max(p => p.Value);

            if (minValue == maxValue)
            {
                // All values are the same
                minValue = 0;
            }

            // Draw axes
            DrawLine(canvas, margin, height - margin, width - margin, height - margin, Colors.Gray, 2); // X axis
            DrawLine(canvas, margin, margin, margin, height - margin, Colors.Gray, 2); // Y axis

            // Draw data points
            double xStep = chartWidth / (dataPoints.Count - 1);
            double yScale = chartHeight / (double)(maxValue - minValue);

            Point[] points = new Point[dataPoints.Count];
            for (int i = 0; i < dataPoints.Count; i++)
            {
                double x = margin + i * xStep;
                double y = height - margin - ((double)(dataPoints[i].Value - minValue) * yScale);

                points[i] = new Point(x, y);

                // Draw point
                DrawCircle(canvas, x, y, 6, ParseColor(dataPoints[i].Color));
            }

            // Draw lines between points
            for (int i = 1; i < points.Length; i++)
            {
                DrawLine(canvas, points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, Colors.Blue, 2);
            }

            // Draw X axis labels
            for (int i = 0; i < dataPoints.Count; i++)
            {
                double x = margin + i * xStep;
                DrawText(canvas, dataPoints[i].Label, x, height - margin + 15, 10, Colors.Gray, true);
            }

            // Draw Y axis labels
            int ySteps = 5;
            decimal yStepValue = (maxValue - minValue) / ySteps;
            for (int i = 0; i <= ySteps; i++)
            {
                decimal value = minValue + i * yStepValue;
                double y = height - margin - (double)(i * chartHeight / ySteps);
                DrawText(canvas, value.ToString("N0"), margin - 5, y, 10, Colors.Gray, false, HorizontalAlignment.Right);
            }
        }

        #endregion

        #region Bar Chart

        public static void DrawBarChart(Canvas canvas, List<ChartDataPoint> dataPoints, string title = "", bool isHorizontal = false)
        {
            if (dataPoints == null || dataPoints.Count == 0)
            {
                canvas.Children.Clear();
                return;
            }

            canvas.Children.Clear();

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            double margin = 40;
            double chartWidth = width - 2 * margin;
            double chartHeight = height - 2 * margin - 40;

            if (!string.IsNullOrEmpty(title))
            {
                DrawText(canvas, title, width / 2, margin / 2, 14, Colors.Black, true);
            }

            decimal maxValue = dataPoints.Max(p => p.Value);
            double barWidth = chartWidth / dataPoints.Count * 0.6;
            double barSpacing = chartWidth / dataPoints.Count * 0.4;

            if (isHorizontal)
            {
                // Horizontal bar chart
                double yStep = chartHeight / dataPoints.Count;
                for (int i = 0; i < dataPoints.Count; i++)
                {
                    double barHeight = (double)(dataPoints[i].Value / maxValue) * chartHeight;
                    double x = margin;
                    double y = margin + i * yStep + (yStep - barWidth) / 2;

                    DrawRectangle(canvas, x, y, barHeight, barWidth, ParseColor(dataPoints[i].Color));
                    DrawText(canvas, dataPoints[i].Label, x + barHeight + 5, y + barWidth / 2, 10, Colors.Gray, false, HorizontalAlignment.Left);
                }
            }
            else
            {
                // Vertical bar chart
                double xStep = chartWidth / dataPoints.Count;
                for (int i = 0; i < dataPoints.Count; i++)
                {
                    double barHeight = (double)(dataPoints[i].Value / maxValue) * chartHeight;
                    double x = margin + i * xStep + (xStep - barWidth) / 2;
                    double y = height - margin - barHeight;

                    DrawRectangle(canvas, x, y, barWidth, barHeight, ParseColor(dataPoints[i].Color));
                    DrawText(canvas, dataPoints[i].Label, x + barWidth / 2, height - margin + 15, 10, Colors.Gray, true);
                }
            }
        }

        #endregion

        #region Pie Chart

        public static void DrawPieChart(Canvas canvas, List<ChartDataPoint> dataPoints, string title = "")
        {
            if (dataPoints == null || dataPoints.Count == 0)
            {
                canvas.Children.Clear();
                return;
            }

            canvas.Children.Clear();

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            double margin = 40;
            double chartWidth = width - 2 * margin;
            double chartHeight = height - 2 * margin - 40;

            if (!string.IsNullOrEmpty(title))
            {
                DrawText(canvas, title, width / 2, margin / 2, 14, Colors.Black, true);
            }

            decimal totalValue = dataPoints.Sum(p => p.Value);
            double centerX = width / 2;
            double centerY = height / 2;
            double radius = Math.Min(chartWidth, chartHeight) / 2 * 0.8;

            double currentAngle = 0;
            for (int i = 0; i < dataPoints.Count; i++)
            {
                double sliceAngle = (double)(dataPoints[i].Value / totalValue * 360);
                double startAngle = currentAngle;
                double endAngle = currentAngle + sliceAngle;

                DrawPieSlice(canvas, centerX, centerY, radius, startAngle, endAngle, ParseColor(dataPoints[i].Color));

                // Draw label
                double midAngle = (startAngle + endAngle) / 2;
                double labelX = centerX + Math.Cos(midAngle * Math.PI / 180) * radius * 0.7;
                double labelY = centerY + Math.Sin(midAngle * Math.PI / 180) * radius * 0.7;

                DrawText(canvas, dataPoints[i].Label, labelX, labelY, 10, Colors.Black, true);

                currentAngle = endAngle;
            }
        }

        #endregion

        #region Helper Methods

        private static Color ParseColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString))
                return Colors.Blue;

            try
            {
                // Handle hex colors like "#RRGGBB"
                if (colorString.StartsWith("#"))
                {
                    if (colorString.Length == 7) // #RRGGBB
                    {
                        byte r = Convert.ToByte(colorString.Substring(1, 2), 16);
                        byte g = Convert.ToByte(colorString.Substring(3, 2), 16);
                        byte b = Convert.ToByte(colorString.Substring(5, 2), 16);
                        return Color.FromRgb(r, g, b);
                    }
                    else if (colorString.Length == 9) // #AARRGGBB
                    {
                        byte a = Convert.ToByte(colorString.Substring(1, 2), 16);
                        byte r = Convert.ToByte(colorString.Substring(3, 2), 16);
                        byte g = Convert.ToByte(colorString.Substring(5, 2), 16);
                        byte b = Convert.ToByte(colorString.Substring(7, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                }
                // Handle known color names
                var colorProperty = typeof(Colors).GetProperty(colorString);
                if (colorProperty != null)
                {
                    return (Color)colorProperty.GetValue(null);
                }
            }
            catch
            {
                // Fall back to default color
            }
            return Colors.Blue;
        }

        private static void DrawLine(Canvas canvas, double x1, double y1, double x2, double y2, Color color, double thickness)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness
            };
            canvas.Children.Add(line);
        }

        private static void DrawRectangle(Canvas canvas, double x, double y, double width, double height, Color color)
        {
            Rectangle rect = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = new SolidColorBrush(color)
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            canvas.Children.Add(rect);
        }

        private static void DrawCircle(Canvas canvas, double x, double y, double radius, Color color)
        {
            Ellipse circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(color)
            };
            Canvas.SetLeft(circle, x - radius);
            Canvas.SetTop(circle, y - radius);
            canvas.Children.Add(circle);
        }

        private static void DrawPieSlice(Canvas canvas, double centerX, double centerY, double radius, double startAngle, double endAngle, Color color)
        {
            Path path = new Path();
            PathGeometry geometry = new PathGeometry();

            PathFigure figure = new PathFigure();
            figure.StartPoint = new Point(centerX, centerY);

            LineSegment line1 = new LineSegment();
            line1.Point = new Point(centerX + radius * Math.Cos(startAngle * Math.PI / 180), centerY + radius * Math.Sin(startAngle * Math.PI / 180));

            ArcSegment arc = new ArcSegment();
            arc.Point = new Point(centerX + radius * Math.Cos(endAngle * Math.PI / 180), centerY + radius * Math.Sin(endAngle * Math.PI / 180));
            arc.Size = new Size(radius, radius);
            arc.SweepDirection = SweepDirection.Clockwise;

            LineSegment line2 = new LineSegment();
            line2.Point = new Point(centerX, centerY);

            figure.Segments.Add(line1);
            figure.Segments.Add(arc);
            figure.Segments.Add(line2);

            geometry.Figures.Add(figure);
            path.Data = geometry;
            path.Fill = new SolidColorBrush(color);

            canvas.Children.Add(path);
        }

private static void DrawText(Canvas canvas, string text, double x, double y, double fontSize, Color color, bool centered = false, HorizontalAlignment alignment = HorizontalAlignment.Center)
{
    TextBlock textBlock = new TextBlock
    {
        Text = text,
        FontSize = fontSize,
        Foreground = new SolidColorBrush(color),
        FontFamily = new FontFamily("Segoe UI")
    };

    // Measure text size
    var formattedText = new FormattedText(
        text,
        System.Globalization.CultureInfo.CurrentCulture,
        FlowDirection.LeftToRight,
        new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
        textBlock.FontSize,
        textBlock.Foreground,
        VisualTreeHelper.GetDpi(canvas).PixelsPerDip);

    double textWidth = formattedText.Width;
    double textHeight = formattedText.Height;

    if (centered)
    {
        Canvas.SetLeft(textBlock, x - textWidth / 2);
        Canvas.SetTop(textBlock, y - textHeight / 2);
    }
    else
    {
        Canvas.SetLeft(textBlock, x);
        Canvas.SetTop(textBlock, y);
    }

    canvas.Children.Add(textBlock);
}

        #endregion
    }
}