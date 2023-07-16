namespace HackAssembler.Contracts
{
    public interface IMnemonicsConverter
	{
		string Destination(string mnemonic);

		string Computation(string mnemonic);

		string Jump(string mnemonic);
	}
}

