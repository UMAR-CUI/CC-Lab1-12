using System;
using System.Collections.Generic;
using System.Linq;

namespace SLRParserDemo
{
    public enum TokenType { id, plus, star, lparen, rparen, dollar }
    public class Production
    {
        public string LHS { get; }
        public string[] RHS { get; }
        public Production(string lhs, params string[] rhs) { LHS = lhs; RHS = rhs; }
        public override string ToString() => $"{LHS} → {string.Join(" ", RHS)}";
    }

    public class SLRParser
    {
        private readonly List<Production> productions = new()
        {
            new Production("E'", "E"),     // 0: augmented
            new Production("E", "E", "+", "T"), // 1
            new Production("E", "T"),           // 2
            new Production("T", "T", "*", "F"), // 3
            new Production("T", "F"),           // 4
            new Production("F", "(", "E", ")"), // 5
            new Production("F", "id")           // 6
        };
        private readonly string[,] action = new string[12, 6]
        {
            // id   +    *    (    )    $
            { "S5", "",  "",  "S4", "",  ""   },     // 0
            { "","S6", "",  "",  "",  "acc"},        // 1
            { "", "R2","R2", "", "R2","R2"},         // 2
            { "", "R4","R4", "", "R4","R4"},         // 3
            { "S5", "",  "",  "S4", "",  ""   },     // 4
            { "", "R6","R6", "", "R6","R6"},         // 5
            { "S5", "",  "",  "S4", "",  ""   },     // 6
            { "S5", "",  "",  "S4", "",  ""   },     // 7
            { "", "S6","",  "",  "S11",""   },       // 8
            { "", "R1","S7", "", "R1","R1"},         // 9
            { "", "R3","R3", "", "R3","R3"},         //10
            { "", "R5","R5", "", "R5","R5"},         //11
        };

        private readonly int[,] gotoTable = new int[12, 3]
        {
            { 1, 2, 3 }, // 0
            { -1,-1,-1}, // 1
            { -1,-1,-1}, // 2
            { -1,-1,-1}, // 3
            { 8, 2, 3 }, // 4
            { -1,-1,-1}, // 5
            { -1, 9, 3 },// 6
            { -1,-1,10 },// 7
            { -1,-1,-1}, // 8
            { -1,-1,-1}, // 9
            { -1,-1,-1}, //10
            { -1,-1,-1}  //11
        };

        private readonly Dictionary<string, int> symbolToCol = new()
        {
            {"id",0}, {"+",1}, {"*",2}, {"(",3}, {")",4}, {"$",5}
        };
        private readonly Dictionary<string, int> gotoToCol = new()
        {
            {"E",0}, {"T",1}, {"F",2}
        };
        public List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (part == "id") tokens.Add("id");
                else if (part == "+") tokens.Add("+");
                else if (part == "*") tokens.Add("*");
                else if (part == "(") tokens.Add("(");
                else if (part == ")") tokens.Add(")");
                else throw new Exception($"Unknown token: {part}");
            }
            tokens.Add("$");
            return tokens;
        }
        public void Parse(string input)
        {
            var tokens = Tokenize(input);
            var stateStack = new Stack<int>();
            var symbolStack = new Stack<string>();

            stateStack.Push(0);
            int ip = 0;

            Console.WriteLine("{0,-20}{1,-30}{2,-30}{3}", "State Stack", "Symbol Stack", "Input", "Action");
            Console.WriteLine(new string('-', 100));

            while (true)
            {
                string currToken = tokens[ip];
                int state = stateStack.Peek();
                string act = action[state, symbolToCol[currToken]];

                Console.WriteLine("{0,-20}{1,-30}{2,-30}{3}",
                    string.Join(" ", stateStack.Reverse()),
                    string.Join(" ", symbolStack.Reverse()),
                    string.Join(" ", tokens.Skip(ip)),
                    act == "" ? "error" : act);

                if (act == "")
                {
                    Console.WriteLine("Parsing error!");
                    return;
                }
                else if (act[0] == 'S')
                {
                    int nextState = int.Parse(act.Substring(1));
                    symbolStack.Push(currToken);
                    stateStack.Push(nextState);
                    ip++;
                }
                else if (act[0] == 'R')
                {
                    int prodNum = int.Parse(act.Substring(1));
                    var prod = productions[prodNum];
                    for (int i = 0; i < prod.RHS.Length; i++)
                    {
                        symbolStack.Pop();
                        stateStack.Pop();
                    }
                    symbolStack.Push(prod.LHS);
                    int gotoState = gotoTable[stateStack.Peek(), gotoToCol[prod.LHS]];
                    stateStack.Push(gotoState);
                }
                else if (act == "acc")
                {
                    Console.WriteLine("{0,-20}{1,-30}{2,-30}{3}",
                        string.Join(" ", stateStack.Reverse()),
                        string.Join(" ", symbolStack.Reverse()),
                        string.Join(" ", tokens.Skip(ip)),
                        "accept");
                    Console.WriteLine("\nInput accepted!");
                    return;
                }
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Enter input (tokens separated by spaces, e.g., id + id * id):");
            string input = Console.ReadLine();
            var parser = new SLRParser();
            parser.Parse(input);
        }
    }
}
