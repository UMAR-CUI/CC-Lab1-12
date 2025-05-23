using System;
using System.Text.RegularExpressions;

class LogicalOperatorDetector
{
    static void Main()
    {
        string code = "if (a == b && c != d || !e ^ f) { }";
        string pattern = @"(&&)|(\|\|)|(!=)|(!)|(==)|(\^)";

        MatchCollection matches = Regex.Matches(code, pattern);

        Console.WriteLine("Logical Operators Found:");
        foreach (Match match in matches)
        {
            Console.WriteLine(match.Value);
        }
    }
}
