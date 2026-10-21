using MtgSynergy.Core.Models;
using MtgSynergy.Core.Rules;

namespace MtgSynergy.Core.Models;

public class SynergyMatch
{
    public required Card Card { get; set; }
    public double TotalScore { get; set; }
    public List<string> Reasons { get; set; } = [];
    public List<SynergyContribution> Contributions { get; set; } = [];
}
