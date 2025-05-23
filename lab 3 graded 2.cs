using System;
using System.Text.RegularExpressions;

class Program {
    static void Main() {
        string[] inputs = { "8e4", "5e-2", "6e9", "e9", "99e" };
        string pattern = @"^\d+e-?\d+$";

        Console.WriteLine("Matched Numbers:");
        foreach (var input in inputs) {
            if (Regex.IsMatch(input, pattern)) {
                Console.WriteLine($"✔ {input}");
            }
        }
    }
}
