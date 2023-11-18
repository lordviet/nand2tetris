using JackCompiler.Enums;

namespace JackCompiler.Contracts
{
    public interface ISymbolTable
    {
        void StartSubroutine();

        void Define(string name, string type, IdentifierKind kind);

        int VarCount(IdentifierKind kind);

        IdentifierKind KindOf(string name);

        string TypeOf(string name);

        int IndexOf(string name);
    }
}

