namespace MtgSynergy.Application.DTOs;

public record SynergyContributionDto(
    string RuleName,
    double Score,
    string Reason,
    string Category
);

public record SynergyMatchDto(
    CardDto Card,
    double TotalScore,
    List<string> Reasons,
    List<SynergyContributionDto> Contributions
);

public record SynergyResponseDto(
    CardDto SourceCard,
    int TotalMatches,
    List<SynergyMatchDto> Synergies
);
