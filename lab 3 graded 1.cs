using System;
using System.Text.RegularExpressions;

class FloatingPointNumberDetector
{
    static void Main()
    {
        string input = "12.34 0.567 123.4567 1.2 987654.3 45.678";
        string pattern = @"\b\d{1,5}\.\d{1}\b|\b\d{1,4}\.\d{1,2}\b|\b\d{1,3}\.\d{1,3}\b|\b\d{1,2}\.\d{1,4}\b|\b\d{1}\.\d{1,5}\b";

        MatchCollection matches = Regex.Matches(input, pattern);

        Console.WriteLine("Floating Point Numbers (max 6 digits) Found:");
        foreach (Match match in matches)
        {
            Console.WriteLine(match.Value);
        }
    }
}
