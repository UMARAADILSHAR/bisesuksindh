using BiseSukkur.Core.Entities;

namespace BiseSukkur.Core.Helpers;

public static class EnrollmentCodeHelper
{
    public static string ResolveGroupCode(string group)
    {
        var normalized = (group ?? "").Trim().ToLower().Replace("_", "-").Replace(" ", "-");
        return normalized switch
        {
            "science" => "S",
            "pre-engineering" => "P",
            "pre-medical" => "M",
            "general" => "G",
            "general-regular" => "G",
            "general-private" => "G",
            "general-science" => "T",
            "arts" => "A",
            "humanities" => "H",
            "commerce" => "C",
            "computer-science" => "T",
            "home-economics" => "O",
            _ => normalized.Length > 0 ? normalized[..1].ToUpper() : "S"
        };
    }

    public static string ResolveDistrictCode(District? district)
    {
        if (district == null || string.IsNullOrWhiteSpace(district.ShortCode))
        {
            return "SK";
        }

        return district.ShortCode.Trim().ToUpper()[..Math.Min(2, district.ShortCode.Trim().Length)];
    }

    public static string FormatEnrollmentNumber(
        string yearSuffix,
        string groupCode,
        string districtCode,
        int zone,
        string schoolCode,
        int sequenceNumber)
    {
        var formattedSeq = sequenceNumber.ToString("D4");
        var paddedSchoolCode = schoolCode.PadLeft(3, '0');
        return $"E{yearSuffix}{groupCode}{districtCode}{zone}-{paddedSchoolCode}-{formattedSeq}";
    }
}
