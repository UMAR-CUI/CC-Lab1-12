using System;
using System.Text.RegularExpressions;

class Program {
    static void Main() {
        string document = "The moon moves through the night. Tigers and monkeys live in the jungle.";
        string pattern = @"\b[tT|mM]\w*";

        Console.WriteLine("Words starting with 't' or 'm':");
        foreach (Match match in Regex.Matches(document, pattern)) {
            Console.WriteLine(match.Value);
        }
    }
}
