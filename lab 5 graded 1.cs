using System;
using System.Collections.Generic;
using System.Linq;

namespace CompilerLabs
{
    public enum SymbolType
    {
        Variable,
        Function,
        Constant,
        Keyword,
        Operator,
        Class,
        Method,
        Parameter
    }
    public enum DataType
    {
        Int,
        Float,
        String,
        Bool,
        Char,
        Void,
        Unknown
    }

    public class SymbolEntry
    {
        public string Name { get; set; }
        public SymbolType Type { get; set; }
        public DataType DataType { get; set; }
        public object Value { get; set; }
        public int Scope { get; set; }
        public int LineNumber { get; set; }
        public bool IsInitialized { get; set; }
        public List<DataType> ParameterTypes { get; set; } // For functions

        public SymbolEntry()
        {
            ParameterTypes = new List<DataType>();
        }

        public override string ToString()
        {
            return $"Name: {Name}, Type: {Type}, DataType: {DataType}, Scope: {Scope}, Line: {LineNumber}, Initialized: {IsInitialized}";
        }
    }
    public class HashSymbolTable
    {
        private const int DefaultSize = 101;
        private List<SymbolEntry>[] hashTable;
        private int size;
        private int currentScope;
        private Stack<int> scopeStack;

        public HashSymbolTable(int tableSize = DefaultSize)
        {
            size = tableSize;
            hashTable = new List<SymbolEntry>[size];
            for (int i = 0; i < size; i++)
            {
                hashTable[i] = new List<SymbolEntry>();
            }
            currentScope = 0;
            scopeStack = new Stack<int>();
        }
        private int HashFunction(string key)
        {
            const int prime = 31;
            int hash = 0;
            int pow = 1;

            foreach (char c in key)
            {
                hash = (hash + (c * pow)) % size;
                pow = (pow * prime) % size;
            }

            return Math.Abs(hash);
        }

        public bool Insert(string name, SymbolType symbolType, DataType dataType,
                          object value = null, int lineNumber = 0,
                          List<DataType> paramTypes = null)
        {
            int index = HashFunction(name);
            var existingSymbol = hashTable[index].FirstOrDefault(s =>
                s.Name == name && s.Scope == currentScope);

            if (existingSymbol != null)
            {
                Console.WriteLine($"Error: Symbol '{name}' already declared in current scope {currentScope}");
                return false;
            }

            var newSymbol = new SymbolEntry
            {
                Name = name,
                Type = symbolType,
                DataType = dataType,
                Value = value,
                Scope = currentScope,
                LineNumber = lineNumber,
                IsInitialized = value != null,
                ParameterTypes = paramTypes ?? new List<DataType>()
            };

            hashTable[index].Add(newSymbol);
            Console.WriteLine($"Inserted: {newSymbol}");
            return true;
        }
        public SymbolEntry Lookup(string name)
        {
            int index = HashFunction(name);
            for (int scope = currentScope; scope >= 0; scope--)
            {
                var symbol = hashTable[index].FirstOrDefault(s =>
                    s.Name == name && s.Scope == scope);

                if (symbol != null)
                {
                    return symbol;
                }
            }

            return null;
        }

        public bool Update(string name, object newValue)
        {
            var symbol = Lookup(name);
            if (symbol != null)
            {
                symbol.Value = newValue;
                symbol.IsInitialized = true;
                Console.WriteLine($"Updated: {symbol}");
                return true;
            }

            Console.WriteLine($"Error: Symbol '{name}' not found for update");
            return false;
        }

        public void EnterScope()
        {
            scopeStack.Push(currentScope);
            currentScope++;
            Console.WriteLine($"Entered scope: {currentScope}");
        }

        public void ExitScope()
        {
            Console.WriteLine($"Exiting scope: {currentScope}");
            for (int i = 0; i < size; i++)
            {
                hashTable[i].RemoveAll(s => s.Scope == currentScope);
            }

            if (scopeStack.Count > 0)
            {
                currentScope = scopeStack.Pop();
            }
            else
            {
                currentScope = 0;
            }

            Console.WriteLine($"Current scope: {currentScope}");
        }
        public void DisplayTable()
        {
            Console.WriteLine("\n=== Symbol Table Contents ===");
            for (int i = 0; i < size; i++)
            {
                if (hashTable[i].Count > 0)
                {
                    Console.WriteLine($"Hash Index {i}:");
                    foreach (var symbol in hashTable[i])
                    {
                        Console.WriteLine($"  {symbol}");
                    }
                }
            }
            Console.WriteLine("=============================\n");
        }
        public void DisplayStatistics()
        {
            int usedSlots = 0;
            int totalCollisions = 0;
            int maxChainLength = 0;

            for (int i = 0; i < size; i++)
            {
                if (hashTable[i].Count > 0)
                {
                    usedSlots++;
                    if (hashTable[i].Count > 1)
                    {
                        totalCollisions += hashTable[i].Count - 1;
                    }
                    maxChainLength = Math.Max(maxChainLength, hashTable[i].Count);
                }
            }

            Console.WriteLine($"\n=== Hash Table Statistics ===");
            Console.WriteLine($"Table Size: {size}");
            Console.WriteLine($"Used Slots: {usedSlots}");
            Console.WriteLine($"Load Factor: {(double)usedSlots / size:F2}");
            Console.WriteLine($"Total Collisions: {totalCollisions}");
            Console.WriteLine($"Max Chain Length: {maxChainLength}");
            Console.WriteLine($"=============================\n");
        }
    }
    public class Lab5Demo
    {
        public static void RunDemo()
        {
            var symbolTable = new HashSymbolTable();

            Console.WriteLine("=== Lab 5: Hash-based Symbol Table Demo ===\n");
            symbolTable.Insert("globalVar", SymbolType.Variable, DataType.Int, 42, 1);
            symbolTable.Insert("PI", SymbolType.Constant, DataType.Float, 3.14159, 2);
            symbolTable.Insert("main", SymbolType.Function, DataType.Void, null, 3,
                              new List<DataType> { DataType.Int, DataType.String });
            symbolTable.EnterScope();
            symbolTable.Insert("localVar", SymbolType.Variable, DataType.String, "hello", 5);
            symbolTable.Insert("counter", SymbolType.Variable, DataType.Int, null, 6);
            symbolTable.EnterScope();
            symbolTable.Insert("tempVar", SymbolType.Variable, DataType.Bool, true, 8);
            symbolTable.Insert("counter", SymbolType.Variable, DataType.Float, 3.5, 9);

            symbolTable.DisplayTable();
            Console.WriteLine("=== Lookup Tests ===");
            var result = symbolTable.Lookup("counter");
            Console.WriteLine($"Lookup 'counter': {(result != null ? result.ToString() : "Not found")}");

            result = symbolTable.Lookup("globalVar");
            Console.WriteLine($"Lookup 'globalVar': {(result != null ? result.ToString() : "Not found")}");

            result = symbolTable.Lookup("nonExistent");
            Console.WriteLine($"Lookup 'nonExistent': {(result != null ? result.ToString() : "Not found")}");
            Console.WriteLine("\n=== Update Tests ===");
            symbolTable.Update("counter", 10.5);
            symbolTable.Update("globalVar", 100);
            symbolTable.ExitScope();
            symbolTable.DisplayTable();

            symbolTable.ExitScope();
            symbolTable.DisplayTable();

            symbolTable.DisplayStatistics();
        }
    }
}
public class Program
{
    public static void Main()
    {
        CompilerLabs.Lab5Demo.RunDemo();
    }
}