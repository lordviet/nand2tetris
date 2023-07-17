using System.Text;
using HackAssembler.Enums;
using HackAssembler.Implementations;
using HackAssembler.Models;

namespace HackAssembler;

class Program
{
    static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine("Please specify a .asm filename as a parameter for the assembler.");
            return;
        }

        //string fileContents = File.ReadAllText(args[0]);

        string fileContents = File.ReadAllText("/Users/dmtodev/Desktop/Projects/nand2tetris/06/max/Max.asm");

        IDictionary<string, int> symbolTable = InitializeSymbolMap();

        Parser parser = new Parser(fileContents);

        // First pass
        IEnumerable<FileContentMeta> scrapedSymbols = parser.ScrapeLabelsWithLineNumbers();

        foreach (FileContentMeta symbol in scrapedSymbols)
        {
            symbolTable.Add(symbol.Content, symbol.LineNumber);
        }

        MnemonicsConverter converter = new MnemonicsConverter();

        StringBuilder sb = new StringBuilder();

        // Second pass
        while (parser.HasMoreCommands())
        {
            CommandType currentInstructionType = parser.CommandType();

            if (currentInstructionType == CommandType.A)
            {
                string symbol = parser.Symbol();

                string aInstructionBits = HandleAInstruction(symbol);

                sb.Append(aInstructionBits);
            }

            if (currentInstructionType == CommandType.C)
            {
                string? destination = parser.Destination();
                string computation = parser.Computation();
                string? jump = parser.Jump();

                string cInstructionBits = HandleCInstruction(converter, destination, computation, jump);

                sb.Append(cInstructionBits);
            }

            parser.Advance();
        }

        Console.WriteLine(sb.ToString());
    }

    private static string HandleAInstruction(string symbol)
    {
        int value = int.Parse(symbol);

        string binary = Convert.ToString(value, 2);

        int zeroPadCount = 16 - binary.Length;

        string padding = new('0', zeroPadCount);

        string converted = $"{padding}{binary}{Environment.NewLine}";

        return converted;
    }

    private static string HandleCInstruction(MnemonicsConverter converter, string? destination, string computation, string? jump)
    {
        string destinationBinary = converter.Destination(destination);
        string computationBinary = converter.Computation(computation);
        string jumpBinary = converter.Jump(jump);

        const int OnePadCount = 3;
        string padding = new('1', OnePadCount);

        string converted = $"{padding}{computationBinary}{destinationBinary}{jumpBinary}{Environment.NewLine}";

        return converted;
    }

    private static IDictionary<string, int> InitializeSymbolMap()
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

