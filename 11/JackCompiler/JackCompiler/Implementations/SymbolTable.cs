using JackCompiler.Contracts;
using JackCompiler.Enums;
using JackCompiler.Models;

namespace JackCompiler.Implementations
{
    public class SymbolTable : ISymbolTable
    {
        public Dictionary<string, SymbolTableEntry> classScope;
        public Dictionary<string, SymbolTableEntry> subroutineScope;

        public SymbolTable()
        {
            this.classScope = new Dictionary<string, SymbolTableEntry>();
            this.subroutineScope = new Dictionary<string, SymbolTableEntry>();
        }

        public void StartSubroutine()
        {
            this.subroutineScope.Clear();
        }

        public void Define(string name, string type, IdentifierKind kind)
        {
            switch (kind)
            {
                case IdentifierKind.Static:
                case IdentifierKind.Field:
                    classScope.Add(name, ConstructSymbolTableEntry(type, kind, this.VarCount(kind)));
                    break;
                case IdentifierKind.Argument:
                case IdentifierKind.Var:
                    subroutineScope.Add(name, ConstructSymbolTableEntry(type, kind, this.VarCount(kind)));
                    break;
                default:
                    break;
            };
        }

        public int VarCount(IdentifierKind kind)
        {
            return kind switch
            {
                IdentifierKind.Static or IdentifierKind.Field => this.classScope.Count(kvp => kvp.Value.Kind == kind),
                IdentifierKind.Argument or IdentifierKind.Var => this.subroutineScope.Count(kvp => kvp.Value.Kind == kind),
                _ => throw new Exception($"Identifier kind is not recognized - {kind}"),
            };
            ;
        }

        public IdentifierKind KindOf(string name)
        {
            return this.GetIdentifierProperty(name, prop => prop.Kind, defaultReturnValue: IdentifierKind.None);
        }

        public string? TypeOf(string name)
        {
            return this.GetIdentifierProperty(name, prop => prop.Type, defaultReturnValue: null);
        }

        public int IndexOf(string name)
        {
            return this.GetIdentifierProperty(name, prop => prop.Index, defaultReturnValue: -1);
        }

        private T? GetIdentifierProperty<T>(string name, Func<SymbolTableEntry, T> propertySelector, T? defaultReturnValue)
        {
            if (this.subroutineScope.ContainsKey(name))
            {
                return propertySelector(subroutineScope[name]);
            }

            if (this.classScope.ContainsKey(name))
            {
                return propertySelector(classScope[name]);
            }

            return defaultReturnValue;
        }

        private static SymbolTableEntry ConstructSymbolTableEntry(string type, IdentifierKind kind, int index)
        {
            return new SymbolTableEntry
            {
                Type = type,
                Kind = kind,
                Index = index
            };
        }
    }
}

