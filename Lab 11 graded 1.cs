using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace InteractiveSemanticAnalyzer
{
    class Program
    {
        static List<List<string>> SymbolTable = new List<List<string>>();
        static List<string> Tokens = new List<string>();
        static Regex variableRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");
        static int currentTokenIndex = 0;

        static void Main(string[] args)
        {
            InitializeSymbolTable();

            Console.WriteLine("Enter your source code line by line (type 'END' to finish):");

            string line;
            while ((line = Console.ReadLine()) != null && line.Trim() != "END")
            {
                var tokenized = Tokenize(line);
                Tokens.AddRange(tokenized);
            }

            Console.WriteLine("\nTokens:");
            foreach (var token in Tokens) Console.Write(token + " ");
            Console.WriteLine("\n\nParsing and Performing Syntax Directed Translation...\n");

            try
            {
                ParseProgram();
                Console.WriteLine("\n✅ Semantic Analysis Completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex.Message);
            }

            Console.ReadLine();
        }

        static void InitializeSymbolTable()
        {

            // You can pre-add default variables or keep it empty
        }

        static List<string> Tokenize(string line)
        {
            var tokens = new List<string>();
            var pattern = @"\d+(\.\d+)?|[A-Za-z_][A-Za-z0-9_]*|==|!=|<=|>=|[+\-*/=;(){}<>,]";
            foreach (Match match in Regex.Matches(line, pattern))
            {
                tokens.Add(match.Value);
            }
            return tokens;
        }

        static string Peek(int offset = 0)
        {
            if (currentTokenIndex + offset < Tokens.Count)
                return Tokens[currentTokenIndex + offset];
            return null;
        }

        static bool Match(string expected)
        {
            if (Peek() == expected)
            {
                currentTokenIndex++;
                return true;
            }
            throw new Exception($"Syntax Error: Expected '{expected}', found '{Peek()}'");
        }

        static void ParseProgram()
        {
            Match("int");
            Match("main");
            Match("(");
            Match(")");
            ParseBlock();
        }

        static void ParseBlock()
        {
            Match("{");
            while (Peek() != "}" && Peek() != null)
                ParseStatement();
            Match("}");
        }

        static void ParseStatement()
        {
            if (Peek() == "int")
                ParseDeclaration();
            else if (Peek() == "if")
                ParseIfStatement();
            else if (variableRegex.IsMatch(Peek()))
                ParseAssignment();
            else
                throw new Exception($"Unexpected token '{Peek()}' in statement.");
        }

        static void ParseDeclaration()
        {
            Match("int");
            string name = Peek();
            Match(name);
            Match(";");
            AddToSymbolTable(name, "int", "0");
        }

        static void ParseAssignment()
        {
            string name = Peek();
            Match(name);
            Match("=");
            int value = ParseExpression();
            Match(";");
            UpdateSymbolTable(name, value.ToString());
            Console.WriteLine($"[Semantic] {name} = {value}");
        }

        static void ParseIfStatement()
        {
            Match("if");
            Match("(");
            bool condition = ParseCondition();
            Match(")");
            if (condition)
                ParseBlock();
            else
                SkipBlock();
        }

        static bool ParseCondition()
        {
            int left = ParseExpression();
            string op = Peek();
            Match(op);
            int right = ParseExpression();

            return op switch
            {
                "==" => left == right,
                "!=" => left != right,
                ">" => left > right,
                "<" => left < right,
                ">=" => left >= right,
                "<=" => left <= right,
                _ => throw new Exception($"Unknown conditional operator '{op}'"),
            };
        }

        static int ParseExpression()
        {
            int result = ParseTerm();
            while (Peek() == "+" || Peek() == "-")
            {
                string op = Peek();
                Match(op);
                int right = ParseTerm();
                result = op == "+" ? result + right : result - right;
            }
            return result;
        }

        static int ParseTerm()
        {
            int result = ParseFactor();
            while (Peek() == "*" || Peek() == "/")
            {
                string op = Peek();
                Match(op);
                int right = ParseFactor();
                result = op == "*" ? result * right : result / right;
            }
            return result;
        }

        static int ParseFactor()
        {
            string token = Peek();
            if (token == "(")
            {
                Match("(");
                int value = ParseExpression();
                Match(")");
                return value;
            }
            else if (int.TryParse(token, out int num))
            {
                Match(token);
                return num;
            }
            else if (variableRegex.IsMatch(token))
            {
                Match(token);
                return GetSymbolValue(token);
            }
            else
            {
                throw new Exception($"Invalid token '{token}' in expression.");
            }
        }

        static void SkipBlock()
        {
            Match("{");
            int braceCount = 1;
            while (braceCount > 0 && currentTokenIndex < Tokens.Count)
            {
                if (Peek() == "{") braceCount++;
                else if (Peek() == "}") braceCount--;
                currentTokenIndex++;
            }
        }

        static void AddToSymbolTable(string name, string type, string value)
        {
            if (FindSymbol(name) == -1)
            {
                SymbolTable.Add(new List<string> { name, "id", type, value });
                Console.WriteLine($"[Declare] {name} as {type}");
            }
            else
            {
                throw new Exception($"Variable '{name}' already declared.");
            }
        }

        static void UpdateSymbolTable(string name, string value)
        {
            int index = FindSymbol(name);
            if (index != -1)
                SymbolTable[index][3] = value;
            else
                throw new Exception($"Variable '{name}' not declared.");
        }

        static int GetSymbolValue(string name)
        {
            int index = FindSymbol(name);
            if (index != -1)
                return int.Parse(SymbolTable[index][3]);
            throw new Exception($"Variable '{name}' not declared.");
        }

        static int FindSymbol(string name)
        {
            for (int i = 0; i < SymbolTable.Count; i++)
            {
                if (SymbolTable[i][0] == name)
                    return i;
            }
            return -1;
        }
    }
}
