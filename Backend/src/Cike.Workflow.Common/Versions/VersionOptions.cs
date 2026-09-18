using System.Globalization;

namespace Cike.Workflow.Common.Versions;

public struct VersionOptions
{
    public static readonly VersionOptions Latest = new() { IsLatest = true };

    public static readonly VersionOptions Published = new() { IsPublished = true };

    public static readonly VersionOptions LatestOrPublished = new() { IsLatestOrPublished = true };

    public static readonly VersionOptions LatestAndPublished = new() { IsLatestAndPublished = true };

    public static readonly VersionOptions Draft = new() { IsDraft = true };

    public static readonly VersionOptions All = new() { AllVersions = true };

    public static VersionOptions SpecificVersion(int version) => new() { Version = version };

    public static VersionOptions FromString(string value) =>
        value switch
        {
            "AllVersions" => All,
            "Draft" => Draft,
            "Latest" => Latest,
            "Published" => Published,
            "LatestOrPublished" => LatestOrPublished,
            "LatestAndPublished" => LatestAndPublished,
            _ => SpecificVersion(int.Parse(value, CultureInfo.InvariantCulture))
        };

    public static bool TryParse(string value, out VersionOptions versionOptions)
    {
        versionOptions = FromString(value);
        return true;
    }

    public bool IsLatest { get; private set; }

    public bool IsLatestOrPublished { get; private set; }

    public bool IsLatestAndPublished { get; private set; }

    public bool IsPublished { get; private set; }

    public int Version { get; private set; }

    public bool IsDraft { get; private set; }

    public bool AllVersions { get; private set; }

    public override string ToString() => AllVersions ? "AllVersions" : IsDraft ? "Draft" : IsLatest ? "Latest" : IsPublished ? "Published" : IsLatestOrPublished ? "LatestOrPublished" : IsLatestAndPublished ? "LatestAndPublished" : Version.ToString(CultureInfo.InvariantCulture);
}
