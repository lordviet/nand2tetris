using System.Text;
using VMTranslator.Enums;
using VMTranslator.Exceptions;
using VMTranslator.Implementations;

namespace VMTranslator;

class Program
{
    static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine($"Usage: VMTranslator <inputFile>{Constants.DefaultInputFileExtension} or VMTranslator <directory>");
            return;
        }

        string input = args[0];

        // TODO: Separate into methods of HandleDirectory, HandleFile, all calling translating and saving
        if (Directory.Exists(input)) // Check if it's a directory
        {
            // Get all .vm files in the directory
            string[] vmFiles = Directory.GetFiles(input, $"*{Constants.DefaultInputFileExtension}");

            if (vmFiles.Length == 0)
            {
                Console.WriteLine($"No '.{Constants.DefaultInputFileExtension}' files found in the directory.");
                return;
            }

            StringBuilder translatedFiles = new StringBuilder();

            bool initialBootstrap = true;

            foreach (string filePath in vmFiles)
            {
                string fileContents = File.ReadAllText(filePath);
                string fileNameWithoutExtensionsInternal = Path.GetFileNameWithoutExtension(filePath);

                string translatedInternal = TranslateIntermediateCode(fileContents, fileNameWithoutExtensionsInternal, initialBootstrap);

                initialBootstrap = false;

                translatedFiles.Append(translatedInternal);
            }

            string? directoryName = Path.GetDirectoryName(vmFiles[0]);

            SaveOutputFileDir(directoryName ?? input, input, translatedFiles.ToString());
            //SaveOutputFile(input, translatedFiles.ToString());
        }

        else if (File.Exists(input)) // Check if it's a file
        {
            if (!string.Equals(Path.GetExtension(input), Constants.DefaultInputFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Invalid input file. Please provide a {Constants.DefaultInputFileExtension} file.");
                return;
            }

            string fileContents = File.ReadAllText(input);
            string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(input);

            string translated = TranslateIntermediateCode(fileContents, fileNameWithoutExtensions, bootstrap: false);

            SaveOutputFile(input, translated);
        }
        else
        {
            Console.WriteLine($"Input path '{input}' does not exist.");
        }
    }

    private static string TranslateIntermediateCode(string fileContents, string fileName, bool bootstrap)
    {
        Parser parser = new Parser(fileContents);

        CodeWriter writer = new CodeWriter(fileName);

        if (bootstrap)
        {
            // Bootstrap code
            writer.WriteInit();
        }

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

                case CommandType.Call:
                    writer.WriteCall(parser.FirstArg(), parser.SecondArg(), parser.GetCurrentCounter());
                    break;

                case CommandType.Return:
                    writer.WriteReturn();
                    break;

                case CommandType.Function:
                    writer.WriteFunction(parser.FirstArg(), parser.SecondArg());
                    break;

                default:
                    throw new UnexpectedCommandTypeException($"Unexpected command type '{currentInstructionType}'!");
            }

            parser.Advance();
        }

        return writer.Close();
    }

    private static void SaveOutputFileDir(string directoryName, string fileName, string translatedCode)
    {
        try
        {
            // Combine the directory path with the default output file extension to create the output file path
            string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileName);
            string outputFile = Path.Combine(directoryName, $"{fileNameWithoutExtensions}{Constants.DefaultOutputFileExtension}");

            File.WriteAllText(outputFile, translatedCode);
            Console.WriteLine("Translated successfully. Output file: " + outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving the output file: " + ex.Message);
        }
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