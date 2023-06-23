using System.ComponentModel.DataAnnotations;

namespace AvaloniaTaskManagerPerformance.App.Models;
//from here: https://learn.microsoft.com/en-us/previous-versions/windows/desktop/mttmprov/msft-mtmemorysummary
public enum FormFactorEnum
{
    Undefined,
    Other,
    Unknown,
    SIMM,
    SIP,
    Chip,
    DIP,
    ZIP,
    Proprietary,
    DIMM,
    TSOP,
    Row,
    RIMM,
    SODIMM,
    SRIMM,
    [Display(Name="FB-DIMM")]
    FBDIMM
}