using BiseHyderabad.Core.DTOs;

namespace BiseHyderabad.Core.Helpers;

public static class WindowPhaseHelper
{
    public static string ResolvePhase(DateTime now, TimelineDto timeline)
    {
        if (!timeline.PortalOpen || !timeline.NormalStart.HasValue || !timeline.NormalEnd.HasValue)
        {
            return "closed";
        }

        if (now >= timeline.NormalStart.Value && now <= timeline.NormalEnd.Value)
        {
            return "normal";
        }

        if (timeline.GraceEnabled
            && timeline.GraceEnd.HasValue
            && now > timeline.NormalEnd.Value
            && now <= timeline.GraceEnd.Value)
        {
            return "grace";
        }

        return "closed";
    }

    public static string ResolvePhaseLabel(string phase) => phase switch
    {
        "normal" => "Open (Normal Fee)",
        "grace" => "Grace Period (Late Fee Applies)",
        _ => "Closed"
    };
}
