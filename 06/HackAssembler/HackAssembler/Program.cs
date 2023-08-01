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
            Console.WriteLine($"Usage: HackAssembler <inputFile>{Constants.DefaultInputFileExtension}");
            return;
        }

        string fileName = args[0];

        if (!string.Equals(Path.GetExtension(fileName), Constants.DefaultInputFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Invalid input file. Please provide a {Constants.DefaultInputFileExtension} file.");
            return;
        }

        if (!File.Exists(fileName))
        {
            Console.WriteLine($"Input file '{fileName}' does not exist.");
            return;
        }

        string fileContents = File.ReadAllText(fileName);

        string compiled = CompileAssemblyCode(fileContents);

        SaveOutputFile(fileName, compiled);
    }

    private static string CompileAssemblyCode(string fileContents)
    {
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

        string compiled = sb.ToString();

        return compiled;
    }

    private static void SaveOutputFile(string fileName, string compiledCode)
    {
        // Save the compiled code to the output file
        string? directoryName = Path.GetDirectoryName(fileName);

        if (directoryName is null)
        {
            Console.WriteLine($"Could not get directory information for file - {fileName}");
            return;
        }

        string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileName);
        string outputFile = Path.Combine(directoryName, $"{fileNameWithoutExtensions}.hack");

        try
        {
            File.WriteAllText(outputFile, compiledCode);
            Console.WriteLine("Compiled successfully. Output file: " + outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving the output file: " + ex.Message);
        }
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

