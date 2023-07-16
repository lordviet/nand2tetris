using HackAssembler.Contracts;

namespace HackAssembler.Implementations
{
    public class MnemonicsConverter : IMnemonicsConverter
    {
        private readonly IDictionary<string, string> destinationMap;
        private readonly IDictionary<string, string> computationMap;
        private readonly IDictionary<string, string> jumpMap;

        private readonly string threeZeroBits = "000";

        public MnemonicsConverter()
        {
            this.destinationMap = new Dictionary<string, string>
            {
                ["M"] = "001",
                ["D"] = "010",
                ["MD"] = "011",
                ["A"] = "100",
                ["AM"] = "101",
                ["AD"] = "110",
                ["AMD"] = "111"
            };
            this.computationMap = new Dictionary<string, string>
            {
                ["0"] = "101010",
                ["1"] = "111111",
                ["-1"] = "111010",
                ["D"] = "001100",
                ["A"] = "110000",
                ["M"] = "110000",
                ["!D"] = "001101",
                ["!A"] = "110001",
                ["!M"] = "110001",
                ["-D"] = "001111",
                ["-A"] = "110011",
                ["-M"] = "110011",
                ["D+1"] = "011111",
                ["A+1"] = "110111",
                ["M+1"] = "110111",
                ["D-1"] = "001110",
                ["A-1"] = "110010",
                ["M-1"] = "110010",
                ["D+A"] = "000010",
                ["D+M"] = "000010",
                ["D-A"] = "010011",
                ["D-M"] = "010011",
                ["A-D"] = "000111",
                ["M-D"] = "000111",
                ["D&A"] = "000000",
                ["D&M"] = "000000",
                ["D|A"] = "010101",
                ["D|M"] = "010101",
            };
            this.jumpMap = new Dictionary<string, string>
            {
                ["JGT"] = "001",
                ["JEQ"] = "010",
                ["JGE"] = "011",
                ["JLT"] = "100",
                ["JNE"] = "101",
                ["JLE"] = "110",
                ["JMP"] = "111",
            };
        }

        public string Destination(string? mnemonic)
        {
            return this.ConvertMnemonic(mnemonic, this.destinationMap);
        }

        public string Computation(string mnemonic)
        {
            return this.ConvertMnemonic(mnemonic, this.computationMap);
        }

        public string Jump(string? mnemonic)
        {
            return this.ConvertMnemonic(mnemonic, this.jumpMap);
        }

        private string ConvertMnemonic(string? mnemonic, IDictionary<string, string> mnemonicsMap)
        {
            if (mnemonic is null)
            {
                return threeZeroBits;
            }

            if (!mnemonicsMap.ContainsKey(mnemonic))
            {
                throw new NotSupportedException($"Mnemonics map does not support mnemonic - '{mnemonic}'");
            }

            return mnemonicsMap[mnemonic];
        }

    }
}

