using JackCompiler.Contracts;
using JackCompiler.Enums;
using JackCompiler.Models;

namespace JackCompiler.Implementations
{
    public class SymbolTable : ISymbolTable
    {
        public Dictionary<string, SymbolTableEntry> classScope;
        public Dictionary<string, SymbolTableEntry> subroutineScope;

        //public int classScopeIndex;
        //public int subroutineScopeIndex;

        public SymbolTable()
        {
            this.classScope = new Dictionary<string, SymbolTableEntry>();
            this.subroutineScope = new Dictionary<string, SymbolTableEntry>();

            //this.classScopeIndex = 0;
            //this.subroutineScopeIndex = 0;
        }

        public void StartSubroutine()
        {
            //this.subroutineScopeIndex = 0;
            this.subroutineScope.Clear();
        }

        public void Define(string name, string type, IdentifierKind kind)
        {
            // TODO: Fix index incrementation since it's not correct, use index method
            switch (kind)
            {
                case IdentifierKind.Static:
                case IdentifierKind.Field:
                    classScope.Add(name, this.ConstructSymbolTableEntry(type, kind, this.VarCount(kind)));
                    break;
                case IdentifierKind.Argument:
                case IdentifierKind.Var:
                    subroutineScope.Add(name, this.ConstructSymbolTableEntry(type, kind, this.VarCount(kind)));
                    break;
                default:
                    // TODO: Handle this one
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

        // TODO: These three can be abstracted away, idea
        //private T GetIdentifierProperty<T>(string name, Func<SymbolTableEntry, T> propertySelector, T? defaultReturnValue = null)
        //{
        //    if (subroutineScope.ContainsKey(name))
        //    {
        //        return propertySelector(subroutineScope[name]);
        //    }

        //    if (classScope.ContainsKey(name))
        //    {
        //        return propertySelector(classScope[name]);
        //    }

        //    throw new Exception($"Neither class, nor subroutine scope contains {name}");
        //}

        //public IdentifierKind KindOf(string name)
        //{
        //    return GetIdentifierProperty(name, x => x.Kind);
        //}


        public IdentifierKind KindOf(string name)
        {
            if (subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].Kind;
            }

            if (classScope.ContainsKey(name))
            {
                return classScope[name].Kind;
            }

            return IdentifierKind.None;
        }

        public string TypeOf(string name)
        {
            if (subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].Type;
            }

            if (classScope.ContainsKey(name))
            {
                return classScope[name].Type;
            }

            throw new Exception($"Neither class, nor subroutine scope contains {name}");
        }

        public int IndexOf(string name)
        {
            if (subroutineScope.ContainsKey(name))
            {
                return subroutineScope[name].Index;
            }

            if (classScope.ContainsKey(name))
            {
                return classScope[name].Index;
            }

            return -1;

            //throw new Exception($"Neither class, nor subroutine scope contains {name}");
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

