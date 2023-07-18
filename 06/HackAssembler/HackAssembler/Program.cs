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

        string fileContents = File.ReadAllText(args[0]);

        //string fileContents = File.ReadAllText("/Users/dmtodev/Desktop/Projects/nand2tetris/06/rect/Rect.asm");

        SymbolTable symbolTable = new SymbolTable();

        Parser parser = new Parser(fileContents);

        // First pass
        IEnumerable<FileContentMeta> scrapedSymbols = parser.ScrapeLabelsWithLineNumbers();

        foreach (FileContentMeta symbol in scrapedSymbols)
        {
            symbolTable.AddEntry(symbol.Content, symbol.LineNumber);
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

                string aInstructionBits = HandleAInstruction(symbol, symbolTable);

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

        string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(directoryPath, "output.hack");

        File.WriteAllText(filePath, sb.ToString());
        Console.WriteLine("Compiled successfully.");
    }

    private static string HandleAInstruction(string symbol, SymbolTable table)
    {
        if (int.TryParse(symbol, out int parsed))
        {
            return ConvertAInstructionToBinary(parsed);
        }

        if (!table.Contains(symbol))
        {
            table.AddEntry(symbol);
        }

        int stored = table.GetAddress(symbol);

        return ConvertAInstructionToBinary(stored);
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

    private static string ConvertAInstructionToBinary(int value)
    {
        string coreBinary = Convert.ToString(value, 2);

        int zeroPadCount = 16 - coreBinary.Length;

        string padding = new('0', zeroPadCount);

        string converted = $"{padding}{coreBinary}{Environment.NewLine}";

        return converted;
    }
}

