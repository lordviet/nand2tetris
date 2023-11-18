using JackCompiler.Contracts;
using JackCompiler.Enums;
using JackCompiler.Models;

namespace JackCompiler.Implementations
{
    public class SymbolTable : ISymbolTable
    {
        public Dictionary<string, SymbolTableEntry> classScope;
        public Dictionary<string, SymbolTableEntry> subroutineScope;

        public int classScopeIndex = 0;
        public int subroutineScopeIndex = 0;

        public SymbolTable()
        {
            this.classScope = new Dictionary<string, SymbolTableEntry>();
            this.subroutineScope = new Dictionary<string, SymbolTableEntry>();
        }

        public void StartSubroutine()
        {
            this.subroutineScopeIndex = 0;
            this.subroutineScope.Clear();
        }

        public void Define(string name, string type, IdentifierKind kind)
        {
            switch (kind)
            {
                case IdentifierKind.Static:
                case IdentifierKind.Field:
                    classScope.Add(name, this.ConstructSymbolTableEntry(type, kind, this.classScopeIndex));
                    classScopeIndex++;
                    break;
                case IdentifierKind.Argument:
                case IdentifierKind.Var:
                    subroutineScope.Add(name, this.ConstructSymbolTableEntry(type, kind, this.subroutineScopeIndex));
                    subroutineScopeIndex++;
                    break;
                default:
                    // TODO: Handle this one
                    break;
            };
        }

        public int VarCount(IdentifierKind kind)
        {
            throw new NotImplementedException();
        }

        // TODO: These three can be abstracted away
        public IdentifierKind KindOf(string name)
        {
            if (classScope.ContainsKey(name))
            {
                return classScope[name].Kind;
            }

            if (subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].Kind;
            }

            return IdentifierKind.None;
        }

        public string TypeOf(string name)
        {
            if (classScope.ContainsKey(name))
            {
                return classScope[name].Type;
            }

            if (subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].Type;
            }

            throw new Exception($"Neither class, nor subroutine scope contains {name}");
        }

        public int IndexOf(string name)
        {
            if (classScope.ContainsKey(name))
            {
                return classScope[name].Index;
            }

            if (subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].Index;
            }

            throw new Exception($"Neither class, nor subroutine scope contains {name}");
        }

        private SymbolTableEntry ConstructSymbolTableEntry(string type, IdentifierKind kind, int index)
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

