using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LexicalAnalyzerV1
{
    class Token
    {
        public string Type;
        public string Value;
        public int Line;
        public int Column;

        public Token(string type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }
    }

    class Symbol
    {
        public string Name;
        public string Type;
        public string Scope;
        public int Size;
        public int Line;
        public int Column;
        public string Kind;

        public Symbol(string name, string type, string scope, int size, int line, int column, string kind)
        {
            Name = name;
            Type = type;
            Scope = scope;
            Size = size;
            Line = line;
            Column = column;
            Kind = kind;
        }
    }

    class SymbolTable
    {
        private List<Symbol> table = new List<Symbol>();

        public void Add(string name, string type, string scope, int size, int line, int col, string kind)
        {
            if (Contains(name, scope))
            {
                Console.WriteLine($"ERROR: Redeclaration of variable '{name}' at line {line}, column {col}");
                throw new Exception();
            }

            table.Add(new Symbol(name, type, scope, size, line, col, kind));
        }

        public bool Contains(string name, string scope)
        {
            foreach (var sym in table)
            {
                if (sym.Name == name && sym.Scope == scope)
                    return true;
            }
            return false;
        }

        public void Display()
        {
            Console.WriteLine("\n--- SYMBOL TABLE ---");
            Console.WriteLine("Name\tType\tScope\tSize\tLine\tCol\tKind");
            foreach (var sym in table)
            {
                Console.WriteLine($"{sym.Name}\t{sym.Type}\t{sym.Scope}\t{sym.Size}\t{sym.Line}\t{sym.Column}\t{sym.Kind}");
            }
        }
    }

    class Program
    {
        static List<string> keywordList = new List<string> { "int", "float", "while", "main", "if", "else", "new" };
        static Regex operators_Reg = new Regex(@"^[-*+/><&|=]$");
        static Regex Special_Reg = new Regex(@"^[.,'\[\]{}();:?]$");

        static List<Token> tokens = new List<Token>();
        static SymbolTable symTable = new SymbolTable();
        static int currentToken = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("Enter code (end with an empty line):");
            string userInput = "";
            string line;

            while ((line = Console.ReadLine()) != null && line != "")
            {
                userInput += line + "\n";
            }

            TokenizeAndPrint(userInput);
            ParseTokens();
        }

        static void TokenizeAndPrint(string input)
        {
            tokens.Clear();
            int line_num = 1;
            int col_num = 1;
            int i = 0;

            while (i < input.Length)
            {
                char c = input[i];

                if (c == '\n')
                {
                    line_num++;
                    col_num = 1;
                    i++;
                    continue;
                }
                if (char.IsWhiteSpace(c))
                {
                    col_num++;
                    i++;
                    continue;
                }

                // Identifier or keyword
                if (char.IsLetter(c) || c == '_')
                {
                    int start = i;
                    int startCol = col_num;
                    while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                    {
                        i++; col_num++;
                    }
                    string word = input.Substring(start, i - start);
                    if (keywordList.Contains(word))
                    {
                        Console.WriteLine($"< keyword, {word} >");
                        tokens.Add(new Token("keyword", word, line_num, startCol));
                    }
                    else
                    {
                        Console.WriteLine($"< id, {word} >");
                        tokens.Add(new Token("id", word, line_num, startCol));
                    }
                    continue;
                }

                // Digit
                if (char.IsDigit(c))
                {
                    int start = i;
                    int startCol = col_num;
                    while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.'))
                    {
                        i++; col_num++;
                    }
                    string num = input.Substring(start, i - start);
                    Console.WriteLine($"< digit, {num} >");
                    tokens.Add(new Token("digit", num, line_num, startCol));
                    continue;
                }

                // Operator
                if (operators_Reg.IsMatch(c.ToString()))
                {
                    Console.WriteLine($"< op, {c} >");
                    tokens.Add(new Token("op", c.ToString(), line_num, col_num));
                    i++; col_num++;
                    continue;
                }

                // Punctuation
                if (Special_Reg.IsMatch(c.ToString()))
                {
                    Console.WriteLine($"< punc, {c} >");
                    tokens.Add(new Token("punc", c.ToString(), line_num, col_num));
                    i++; col_num++;
                    continue;
                }

                // Unknown
                Console.WriteLine($"ERROR: {c} at line {line_num}");
                tokens.Add(new Token("error", c.ToString(), line_num, col_num));
                i++; col_num++;
            }
        }

        static Token Peek() => currentToken < tokens.Count ? tokens[currentToken] : null;
        static Token Next()
        {
            if (currentToken < tokens.Count) return tokens[currentToken++];
            return null;
        }

        static void Expect(string type, string value = null, string expected = null)
        {
            var t = Peek();
            if (t == null || t.Type != type || (value != null && t.Value != value))
            {
                string found = t == null ? "EOF" : t.Value;
                string errMsg = $"ERROR: {found} at line {(t?.Line ?? tokens[tokens.Count - 1].Line)}, column {(t?.Column ?? tokens[tokens.Count - 1].Column)}; Expected {expected ?? type.ToUpper()}";
                Console.WriteLine(errMsg);
                throw new Exception();
            }
            Next();
        }

        static void ParseTokens()
        {
            currentToken = 0;
            try
            {
                while (currentToken < tokens.Count)
                {
                    ParseStatement();
                }

                symTable.Display();
            }
            catch
            {
                // Error already printed
            }
        }

        static void ParseStatement()
        {
            var t = Peek();
            if (t == null) return;

            if (t.Type == "keyword" && t.Value == "int")
            {
                Next();
                var idToken = Peek();
                Expect("id", null, "IDENTIFIER");

                symTable.Add(idToken.Value, "int", "global", 4, idToken.Line, idToken.Column, "variable");

                Expect("op", "=", "EQUALS SIGN");
                ParseExpression();
                Expect("punc", ";", "SEMICOLON");
            }
            else if (t.Type == "id")
            {
                var idToken = Peek();
                if (!symTable.Contains(idToken.Value, "global"))
                {
                    Console.WriteLine($"ERROR: Undeclared variable '{idToken.Value}' at line {idToken.Line}, column {idToken.Column}");
                    throw new Exception();
                }

                Next();
                if (Peek() != null && Peek().Type == "op" && Peek().Value == "=")
                {
                    Next();
                    ParseExpression();
                    Expect("punc", ";", "SEMICOLON");
                }
                else
                {
                    var next = Peek();
                    string errMsg = $"ERROR: {next?.Value} at line {next?.Line}, column {next?.Column}; Expected '='";
                    Console.WriteLine(errMsg);
                    throw new Exception();
                }
            }
            else
            {
                string errMsg = $"ERROR: {t.Value} at line {t.Line}, column {t.Column}; Unexpected token";
                Console.WriteLine(errMsg);
                throw new Exception();
            }
        }

        static void ParseExpression()
        {
            var t = Peek();
            if (t != null && (t.Type == "id" || t.Type == "digit"))
            {
                if (t.Type == "id" && !symTable.Contains(t.Value, "global"))
                {
                    Console.WriteLine($"ERROR: Undeclared variable '{t.Value}' at line {t.Line}, column {t.Column}");
                    throw new Exception();
                }

                Next();
                if (Peek() != null && Peek().Type == "op")
                {
                    Next();
                    ParseExpression();
                }
            }
            else
            {
                string errMsg = $"ERROR: {t?.Value ?? "EOF"} at line {t?.Line ?? tokens[tokens.Count - 1].Line}, column {t?.Column ?? tokens[tokens.Count - 1].Column}; Expected EXPRESSION";
                Console.WriteLine(errMsg);
                throw new Exception();
            }
        }
    }
}
