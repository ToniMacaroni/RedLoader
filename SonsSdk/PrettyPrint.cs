namespace SonsSdk;

public class PrettyPrint
{
    private const string HeaderFooterChar = "=";
    private const string LineChar = "-";
    private const string SideBorderChar = "|";

    public static void Print(string header, IEnumerable<string> items, Action<string> loggingFunction)
    {
        var enumerable = items as string[] ?? items.ToArray();
        
        int maxWidth = Math.Max(header.Length, enumerable.Max(item => item.Length)) + 4;

        loggingFunction(HeaderFooterChar.PadRight(maxWidth, HeaderFooterChar[0]));
        PrintCentered(header, maxWidth, loggingFunction);
        loggingFunction(LineChar.PadRight(maxWidth, LineChar[0]));

        foreach (var item in enumerable)
        {
            PrintCentered(item, maxWidth, loggingFunction);
        }

        loggingFunction(LineChar.PadRight(maxWidth, LineChar[0]));
    }

    private static void PrintCentered(string text, int totalWidth, Action<string> loggingFunction)
    {
        int padding = totalWidth - text.Length - 2;
        int paddingLeft = padding / 2;
        int paddingRight = totalWidth - text.Length - paddingLeft - 2;
        loggingFunction($"{SideBorderChar} {text.PadLeft(text.Length + paddingLeft).PadRight(totalWidth - 2)} {SideBorderChar}");
    }
}