using System;
using System.Collections.Generic;

namespace CompilerLab8
{
    public enum State
    {
        Start,
        Identifier,
        Invalid
    }

    public class VariableDFA
    {
        private static readonly HashSet<State> AcceptingStates = new() { State.Identifier };

        private static readonly Dictionary<(State, char), State> Transitions = new()
        {
            {(State.Start, '_'), State.Identifier},
            {(State.Start, 'a'), State.Identifier},
            {(State.Start, 'b'), State.Identifier},
            {(State.Start, 'c'), State.Identifier},
            {(State.Start, 'd'), State.Identifier},
            {(State.Start, 'e'), State.Identifier},
            {(State.Start, 'f'), State.Identifier},
            {(State.Start, 'g'), State.Identifier},
            {(State.Start, 'h'), State.Identifier},
            {(State.Start, 'i'), State.Identifier},
            {(State.Start, 'j'), State.Identifier},
            {(State.Start, 'k'), State.Identifier},
            {(State.Start, 'l'), State.Identifier},
            {(State.Start, 'm'), State.Identifier},
            {(State.Start, 'n'), State.Identifier},
            {(State.Start, 'o'), State.Identifier},
            {(State.Start, 'p'), State.Identifier},
            {(State.Start, 'q'), State.Identifier},
            {(State.Start, 'r'), State.Identifier},
            {(State.Start, 's'), State.Identifier},
            {(State.Start, 't'), State.Identifier},
            {(State.Start, 'u'), State.Identifier},
            {(State.Start, 'v'), State.Identifier},
            {(State.Start, 'w'), State.Identifier},
            {(State.Start, 'x'), State.Identifier},
            {(State.Start, 'y'), State.Identifier},
            {(State.Start, 'z'), State.Identifier},
            // uppercase letters
            {(State.Start, 'A'), State.Identifier},
            {(State.Start, 'B'), State.Identifier},
            {(State.Start, 'C'), State.Identifier},
            {(State.Start, 'D'), State.Identifier},
            {(State.Start, 'E'), State.Identifier},
            {(State.Start, 'F'), State.Identifier},
            {(State.Start, 'G'), State.Identifier},
            {(State.Start, 'H'), State.Identifier},
            {(State.Start, 'I'), State.Identifier},
            {(State.Start, 'J'), State.Identifier},
            {(State.Start, 'K'), State.Identifier},
            {(State.Start, 'L'), State.Identifier},
            {(State.Start, 'M'), State.Identifier},
            {(State.Start, 'N'), State.Identifier},
            {(State.Start, 'O'), State.Identifier},
            {(State.Start, 'P'), State.Identifier},
            {(State.Start, 'Q'), State.Identifier},
            {(State.Start, 'R'), State.Identifier},
            {(State.Start, 'S'), State.Identifier},
            {(State.Start, 'T'), State.Identifier},
            {(State.Start, 'U'), State.Identifier},
            {(State.Start, 'V'), State.Identifier},
            {(State.Start, 'W'), State.Identifier},
            {(State.Start, 'X'), State.Identifier},
            {(State.Start, 'Y'), State.Identifier},
            {(State.Start, 'Z'), State.Identifier},
        };

        public bool ValidateVariable(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            State currentState = State.Start;

            foreach (char c in input)
            {
                currentState = NextState(currentState, c);
                if (currentState == State.Invalid)
                    return false;
            }

            return AcceptingStates.Contains(currentState);
        }

        private State NextState(State state, char c)
        {
            if (state == State.Start)
            {
                if (char.IsLetter(c) || c == '_')
                    return State.Identifier;
                return State.Invalid;
            }
            else if (state == State.Identifier)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    return State.Identifier;
                return State.Invalid;
            }
            return State.Invalid;
        }
    }

    public class Program
    {
        public static void Main()
        {
            var dfa = new VariableDFA();
            var testVars = new List<string>
            {
                "var1", "_var", "1var", "var_2", "var$", "VarName", "a", "Z9_", "_", "__var__"
            };

            Console.WriteLine("DFA Variable Validation:");
            foreach (var v in testVars)
            {
                Console.WriteLine($"{v}: {(dfa.ValidateVariable(v) ? "Valid" : "Invalid")}");
            }
        }
    }
}
