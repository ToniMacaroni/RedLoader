namespace SonsSdk;

public static class ModReport
{
    internal static List<ModReportInfo> ModReports = new();
    
    public static void ReportMod(string modId, string message)
    {
        ModReports.Add(new ModReportInfo()
        {
            ModId = modId,
            Message = message
        });
    }

    public class ModReportInfo
    {
        public string ModId;
        public string Message;
    }
}