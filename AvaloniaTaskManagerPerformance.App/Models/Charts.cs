using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace AvaloniaTaskManagerPerformance.App.Models;

public class Charts 
{
    //Don't know why, but if i put Series here it doesn't update in view
    public List<ISeries> SeriesPreview { get; set; }
    public static List<Axis> XAxis => new List<Axis>
    {
        new Axis
        {
            MaxLimit = 62,
            MinLimit = 2,
            MinStep = 1,
            IsVisible = false,
        }
    };
    public static List<Axis> YAxis => new List<Axis>
    {
        new Axis
        {
            MaxLimit = 100,
            MinLimit = 0,
            MinStep = 10,
            IsVisible = false
        }
    };
}