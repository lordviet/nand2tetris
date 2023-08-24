using VMTranslator.Enums;
using VMTranslator.Implementations;

namespace VMTranslator;

class Program
{
    static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine($"Usage: VMTranslator <inputFile>{Constants.DefaultInputFileExtension}");
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
        string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileName);

        string translated = TranslateIntermediateCode(fileContents, fileNameWithoutExtensions);

        SaveOutputFile(fileName, translated);
    }

    private static string TranslateIntermediateCode(string fileContents, string fileName)
    {
        Parser parser = new Parser(fileContents);

        CodeWriter writer = new CodeWriter(fileName);

        while (parser.HasMoreCommands())
        {
            CommandType currentInstructionType = parser.CommandType();

            string currentInstruction = parser.GetCurrentInstruction();

            writer.WriteCommentedOutInstruction(currentInstruction);

            switch (currentInstructionType)
            {
                case CommandType.Push:
                case CommandType.Pop:
                    writer.WritePushPop(currentInstructionType, parser.FirstArg(), parser.SecondArg());
                    break;

                case CommandType.Arithmetic:
                    writer.WriteArithmetic(parser.FirstArg(), parser.GetCurrentCounter());
                    break;

                case CommandType.Label:
                    writer.WriteLabel(parser.FirstArg());
                    break;

                case CommandType.Goto:
                    writer.WriteGoto(parser.FirstArg());
                    break;

                case CommandType.If:
                    writer.WriteIf(parser.FirstArg());
                    break;

                default:
                    // TODO: Handle the other cases
                    break;
            }

            parser.Advance();
        }

        return writer.Close();
    }

    private static void SaveOutputFile(string fileName, string translatedCode)
    {
        // Save the compiled code to the output file
        string? directoryName = Path.GetDirectoryName(fileName);

        if (directoryName is null)
        {
            Console.WriteLine($"Could not get directory information for file - {fileName}");
            return;
        }

        string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileName);
        string outputFile = Path.Combine(directoryName, $"{fileNameWithoutExtensions}{Constants.DefaultOutputFileExtension}");

        try
        {
            File.WriteAllText(outputFile, translatedCode);
            Console.WriteLine("Translated successfully. Output file: " + outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving the output file: " + ex.Message);
        }
    }
}