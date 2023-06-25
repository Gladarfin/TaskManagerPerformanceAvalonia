using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;

public class SeriesHelper
{
    public List<ISeries> SetSeriesValues(SKColor fillColor, SKColor strokeColor, List<ObservablePoint> values)
    {
        return new List<ISeries>
        {
            new LineSeries<ObservablePoint>
            {
                Values = values,
                Fill = new SolidColorPaint(fillColor),
                Stroke = new SolidColorPaint(strokeColor){StrokeThickness = 1},
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0
            }
        };
    }
}