using System;
using System.Text.RegularExpressions;

class RelationalOperatorDetector
{
    static void Main()
    {
        string code = "if (a >= b && c < d || e != f && g == h) { }";
        string pattern = @"(>=)|(<=)|(==)|(!=)|(<)|(>)";

        MatchCollection matches = Regex.Matches(code, pattern);

        Console.WriteLine("Relational Operators Found:");
        foreach (Match match in matches)
        {
            Console.WriteLine(match.Value);
        }
    }
}
