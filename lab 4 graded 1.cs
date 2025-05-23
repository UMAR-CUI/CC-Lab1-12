using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class LexicalAnalyzer
{
    static HashSet<string> keywords = new HashSet<string> { "int", "float", "if", "else", "print", "main" };
    static HashSet<char> operators = new HashSet<char> { '+', '-', '*', '/', '=', '>', '<' };
    static HashSet<char> delimiters = new HashSet<char> { ';', '(', ')', '{', '}' };

    static void Main()
    {
        string code = @"
            int main() {
                int a = 5;
                float b = 2.5;
                a = a + 1;
                if (a > b) {
                    print(a);
                } else {
                    print(b);
                }
            }
        ";

        List<(string Type, string Value)> tokens = TokenizeWithTwoBuffers(code, 32);

        foreach (var token in tokens)
        {
            Console.WriteLine($"<{token.Type}, {token.Value}>");
        }
    }

    static List<(string, string)> TokenizeWithTwoBuffers(string sourceCode, int bufferSize)
    {
        List<(string, string)> tokens = new();
        string fullCode = Regex.Replace(sourceCode, @"\s+", " ");
        int pos = 0;

        while (pos < fullCode.Length)
        {
            string buffer1 = fullCode.Substring(pos, Math.Min(bufferSize, fullCode.Length - pos));
            pos += bufferSize;
            string buffer2 = pos < fullCode.Length ? fullCode.Substring(pos, Math.Min(bufferSize, fullCode.Length - pos)) : "";

            string combined = buffer1 + buffer2;
            int i = 0;
            while (i < combined.Length)
            {
                char ch = combined[i];

                if (char.IsWhiteSpace(ch))
                {
                    i++;
                    continue;
                }

                if (char.IsLetter(ch) || ch == '_')
                {
                    string token = "";
                    while (i < combined.Length && (char.IsLetterOrDigit(combined[i]) || combined[i] == '_'))
                        token += combined[i++];
                    tokens.Add((keywords.Contains(token) ? "KEYWORD" : "IDENTIFIER", token));
                }
                else if (char.IsDigit(ch))
                {
                    string token = "";
                    while (i < combined.Length && (char.IsDigit(combined[i]) || combined[i] == '.'))
                        token += combined[i++];
                    tokens.Add(("NUMBER", token));
                }
                else if (operators.Contains(ch))
                {
                    if (i + 1 < combined.Length && (combined.Substring(i, 2) == ">=" || combined.Substring(i, 2) == "<=" || combined.Substring(i, 2) == "==" || combined.Substring(i, 2) == "!="))
                    {
                        tokens.Add(("OPERATOR", combined.Substring(i, 2)));
                        i += 2;
                    }
                    else
                    {
                        tokens.Add(("OPERATOR", ch.ToString()));
                        i++;
                    }
                }
                else if (delimiters.Contains(ch))
                {
                    tokens.Add(("DELIMITER", ch.ToString()));
                    i++;
                }
                else
                {
                    i++;
                }
            }
        }

        return tokens;
    }
}
