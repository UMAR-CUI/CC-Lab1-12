using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CompilerLab6
{
    public enum TokenType { Identifier, Number, Plus, Minus, Star, Slash, LParen, RParen, EOF }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public Token(TokenType type, string value) { Type = type; Value = value; }
        public override string ToString() => $"{Type}('{Value}')";
    }

    public static class Lexer
    {
        public static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            int pos = 0;
            while (pos < input.Length)
            {
                if (char.IsWhiteSpace(input[pos])) { pos++; continue; }
                if (char.IsLetter(input[pos]))
                {
                    int start = pos;
                    while (pos < input.Length && char.IsLetterOrDigit(input[pos])) pos++;
                    tokens.Add(new Token(TokenType.Identifier, input.Substring(start, pos - start)));
                }
                else if (char.IsDigit(input[pos]))
                {
                    int start = pos;
                    while (pos < input.Length && char.IsDigit(input[pos])) pos++;
                    tokens.Add(new Token(TokenType.Number, input.Substring(start, pos - start)));
                }
                else
                {
                    switch (input[pos])
                    {
                        case '+': tokens.Add(new Token(TokenType.Plus, "+")); pos++; break;
                        case '-': tokens.Add(new Token(TokenType.Minus, "-")); pos++; break;
                        case '*': tokens.Add(new Token(TokenType.Star, "*")); pos++; break;
                        case '/': tokens.Add(new Token(TokenType.Slash, "/")); pos++; break;
                        case '(': tokens.Add(new Token(TokenType.LParen, "(")); pos++; break;
                        case ')': tokens.Add(new Token(TokenType.RParen, ")")); pos++; break;
                        default:
                            throw new Exception($"Unknown character '{input[pos]}' at position {pos}");
                    }
                }
            }
            tokens.Add(new Token(TokenType.EOF, ""));
            return tokens;
        }
    }

    public abstract class AstNode { }
    public class NumberNode : AstNode { public string Value; public NumberNode(string v) { Value = v; } }
    public class IdentifierNode : AstNode { public string Name; public IdentifierNode(string n) { Name = n; } }
    public class BinaryOpNode : AstNode
    {
        public AstNode Left, Right;
        public string Op;
        public BinaryOpNode(AstNode l, AstNode r, string o) { Left = l; Right = r; Op = o; }
    }

    public class Parser
    {
        private List<Token> tokens;
        private int pos = 0;

        private Token Peek() => pos < tokens.Count ? tokens[pos] : null!;
        private Token Next() => tokens[pos++];
        private static readonly Dictionary<string, HashSet<TokenType>> FirstSets = new()
        {
            { "Expr", new HashSet<TokenType> { TokenType.Identifier, TokenType.Number, TokenType.LParen } },
            { "Term", new HashSet<TokenType> { TokenType.Identifier, TokenType.Number, TokenType.LParen } },
            { "Factor", new HashSet<TokenType> { TokenType.Identifier, TokenType.Number, TokenType.LParen } }
        };

        public AstNode Parse(string input)
        {
            tokens = Lexer.Tokenize(input);
            pos = 0;
            var expr = ParseExpr();
            if (Peek().Type != TokenType.EOF)
                throw new Exception("Unexpected tokens after expression");
            return expr;
        }

        private AstNode ParseExpr()
        {
            var node = ParseTerm();
            while (Peek().Type == TokenType.Plus || Peek().Type == TokenType.Minus)
            {
                var op = Next().Value;
                var right = ParseTerm();
                node = new BinaryOpNode(node, right, op);
            }
            return node;
        }

        private AstNode ParseTerm()
        {
            var node = ParseFactor();
            while (Peek().Type == TokenType.Star || Peek().Type == TokenType.Slash)
            {
                var op = Next().Value;
                var right = ParseFactor();
                node = new BinaryOpNode(node, right, op);
            }
            return node;
        }

        private AstNode ParseFactor()
        {
            var token = Peek();
            if (token.Type == TokenType.LParen)
            {
                Next();
                var node = ParseExpr();
                if (Peek().Type != TokenType.RParen)
                    throw new Exception("Expected ')'");
                Next();
                return node;
            }
            else if (token.Type == TokenType.Number)
            {
                Next();
                return new NumberNode(token.Value);
            }
            else if (token.Type == TokenType.Identifier)
            {
                Next();
                return new IdentifierNode(token.Value);
            }
            else
            {
                throw new Exception($"Unexpected token {token}");
            }
        }
    }

    public static class AstPrinter
    {
        public static void Print(AstNode node, int indent = 0)
        {
            string pad = new string(' ', indent);
            switch (node)
            {
                case NumberNode n: Console.WriteLine($"{pad}Number: {n.Value}"); break;
                case IdentifierNode id: Console.WriteLine($"{pad}Identifier: {id.Name}"); break;
                case BinaryOpNode bin:
                    Console.WriteLine($"{pad}BinaryOp: {bin.Op}");
                    Print(bin.Left, indent + 2);
                    Print(bin.Right, indent + 2);
                    break;
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            string input = "a + b * (c - 2) / d";
            var parser = new Parser();
            try
            {
                var ast = parser.Parse(input);
                Console.WriteLine("Parsed AST:");
                AstPrinter.Print(ast);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Parsing error: " + ex.Message);
            }
        }
    }
}
