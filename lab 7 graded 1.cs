using System;
using System.Collections.Generic;
using System.Linq;

namespace CompilerLab7
{
    public class Production
    {
        public string Lhs { get; }
        public List<string> Rhs { get; }
        public Production(string lhs, IEnumerable<string> rhs)
        {
            Lhs = lhs;
            Rhs = rhs.ToList();
        }
    }

    public class Grammar
    {
        public HashSet<string> NonTerminals { get; } = new();
        public HashSet<string> Terminals { get; } = new();
        public List<Production> Productions { get; } = new();

        public void AddProduction(string lhs, params string[] rhs)
        {
            Productions.Add(new Production(lhs, rhs));
            NonTerminals.Add(lhs);
            foreach (var sym in rhs)
                if (!char.IsUpper(sym[0]) && sym != "ε")
                    Terminals.Add(sym);
        }

        public bool IsNonTerminal(string symbol) => NonTerminals.Contains(symbol);
    }

    public class FirstFollowCalculator
    {
        public Dictionary<string, HashSet<string>> ComputeFirstSets(Grammar grammar)
        {
            var first = new Dictionary<string, HashSet<string>>();

            foreach (var t in grammar.Terminals)
                first[t] = new HashSet<string> { t };

            foreach (var nt in grammar.NonTerminals)
                first[nt] = new HashSet<string>();

            bool changed;
            do
            {
                changed = false;
                foreach (var prod in grammar.Productions)
                {
                    var lhsFirst = first[prod.Lhs];
                    int beforeCount = lhsFirst.Count;

                    if (prod.Rhs.Count == 0)
                    {
                        lhsFirst.Add("ε");
                    }
                    else
                    {
                        bool allNullable = true;
                        foreach (var symbol in prod.Rhs)
                        {
                            var symbolFirst = first.ContainsKey(symbol) ? first[symbol] : new HashSet<string> { symbol };
                            lhsFirst.UnionWith(symbolFirst.Where(s => s != "ε"));
                            if (!symbolFirst.Contains("ε"))
                            {
                                allNullable = false;
                                break;
                            }
                        }
                        if (allNullable) lhsFirst.Add("ε");
                    }

                    if (lhsFirst.Count > beforeCount)
                        changed = true;
                }
            } while (changed);

            return first;
        }

        public Dictionary<string, HashSet<string>> ComputeFollowSets(Grammar grammar, Dictionary<string, HashSet<string>> first)
        {
            var follow = new Dictionary<string, HashSet<string>>();
            foreach (var nt in grammar.NonTerminals)
                follow[nt] = new HashSet<string>();

            // Start symbol gets $
            follow[grammar.Productions[0].Lhs].Add("$");

            bool changed;
            do
            {
                changed = false;
                foreach (var prod in grammar.Productions)
                {
                    for (int i = 0; i < prod.Rhs.Count; i++)
                    {
                        var symbol = prod.Rhs[i];
                        if (grammar.IsNonTerminal(symbol))
                        {
                            var followSet = follow[symbol];
                            int beforeCount = followSet.Count;

                            var beta = prod.Rhs.Skip(i + 1).ToList();
                            var firstBeta = ComputeFirstSequence(beta, first);

                            followSet.UnionWith(firstBeta.Where(s => s != "ε"));

                            if (firstBeta.Contains("ε") || beta.Count == 0)
                                followSet.UnionWith(follow[prod.Lhs]);

                            if (followSet.Count > beforeCount)
                                changed = true;
                        }
                    }
                }
            } while (changed);

            return follow;
        }

        private HashSet<string> ComputeFirstSequence(List<string> symbols, Dictionary<string, HashSet<string>> first)
        {
            var result = new HashSet<string>();
            if (symbols.Count == 0)
            {
                result.Add("ε");
                return result;
            }

            foreach (var symbol in symbols)
            {
                if (!first.ContainsKey(symbol))
                {
                    result.Add(symbol);
                    break;
                }
                var symbolFirst = first[symbol];
                result.UnionWith(symbolFirst.Where(s => s != "ε"));
                if (!symbolFirst.Contains("ε"))
                    break;
            }

            if (symbols.All(s => first.ContainsKey(s) && first[s].Contains("ε")))
                result.Add("ε");

            return result;
        }
    }

    public class Program
    {
        public static void Main()
        {
            var grammar = new Grammar();
            // Grammar with 4 Non-Terminals and 4 Terminals
            // E → T E'
            // E' → + T E' | ε
            // T → F T'
            // T' → * F T' | ε
            // F → ( E ) | id

            grammar.AddProduction("E", "T", "E'");
            grammar.AddProduction("E'", "+", "T", "E'");
            grammar.AddProduction("E'", "ε");
            grammar.AddProduction("T", "F", "T'");
            grammar.AddProduction("T'", "*", "F", "T'");
            grammar.AddProduction("T'", "ε");
            grammar.AddProduction("F", "(", "E", ")");
            grammar.AddProduction("F", "id");

            var calculator = new FirstFollowCalculator();
            var firstSets = calculator.ComputeFirstSets(grammar);
            var followSets = calculator.ComputeFollowSets(grammar, firstSets);

            Console.WriteLine("FIRST Sets:");
            foreach (var kvp in firstSets)
                Console.WriteLine($"{kvp.Key} = {{ {string.Join(", ", kvp.Value)} }}");

            Console.WriteLine("\nFOLLOW Sets:");
            foreach (var kvp in followSets)
                Console.WriteLine($"{kvp.Key} = {{ {string.Join(", ", kvp.Value)} }}");
        }
    }
}
