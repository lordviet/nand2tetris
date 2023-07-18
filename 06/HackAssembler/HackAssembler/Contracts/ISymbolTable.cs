namespace HackAssembler.Contracts
{
    public interface ISymbolTable
	{
        void AddEntry(string symbol, int address);

        void AddEntry(string symbol);

        bool Contains(string symbol);

        int GetAddress(string symbol);
    }
}

