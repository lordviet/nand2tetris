using HackAssembler.Contracts;

namespace HackAssembler.Implementations
{
    public class SymbolTable : ISymbolTable
    {
        private readonly IDictionary<string, int> entries;
        private int nextFreeAddress;

        public SymbolTable()
        {
            this.entries = InitializeSymbolTable();
            this.nextFreeAddress = Constants.DefaultFirstFreeAddress;
        } 

        public void AddEntry(string symbol, int address)
        {
            this.entries.Add(symbol, address);
        }

        public void AddEntry(string symbol)
        {
            while (this.entries.Values.Contains(this.nextFreeAddress))
            {
                this.nextFreeAddress++;
            }

            this.entries.Add(symbol, this.nextFreeAddress);
            this.nextFreeAddress++;
        }

        public bool Contains(string symbol)
        {
            return this.entries.ContainsKey(symbol);
        }

        public int GetAddress(string symbol)
        {
            return this.entries[symbol];
        }

        private static IDictionary<string, int> InitializeSymbolTable()
        {
            // Add default predefined symbols
            IDictionary<string, int> symbolMap = new Dictionary<string, int>()
            {
                ["SP"] = 0,
                ["LCL"] = 1,
                ["ARG"] = 2,
                ["THIS"] = 3,
                ["THAT"] = 4,
            };

            // Add default R0 - R15 symbols
            for (int address = 0; address < 16; address++)
            {
                symbolMap.Add($"R{address}", address);
            }

            // Add screen and keyboard symbols
            const int ScreenAddress = 16384;
            const int KeyboardAddress = 24576;

            symbolMap.Add("SCREEN", ScreenAddress);
            symbolMap.Add("KBD", KeyboardAddress);

            return symbolMap;
        }
    }
}

