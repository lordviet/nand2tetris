using JackCompiler.Enums;

namespace JackCompiler.Models
{
    public class SymbolTableEntry
    {
        public string Type { get; set; } = string.Empty;

        public IdentifierKind Kind { get; set; }

        public int Index { get; set; }
    }
}

