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

        try
        {
            if (Directory.Exists(input))
            {
                HandleDirectory(input);
            }
            else if (File.Exists(input))
            {
                HandleFile(input);
            }
            else
            {
                Console.WriteLine($"Input path '{input}' does not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void HandleDirectory(string directoryPath)
    {
        // Get all .vm files in the directory
        string[] vmFiles = Directory.GetFiles(directoryPath, $"*{Constants.DefaultInputFileExtension}");

        if (vmFiles.Length == 0)
        {
            Console.WriteLine($"No '.{Constants.DefaultInputFileExtension}' files found in the directory.");
            return;
        }

        StringBuilder translatedFiles = new StringBuilder();

        bool bootstrap = true;

        foreach (string filePath in vmFiles)
        {
            string fileContents = File.ReadAllText(filePath);
            string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(filePath);

            string translatedInternal = TranslateIntermediateCode(fileContents, fileNameWithoutExtensions, bootstrap);
            bootstrap = false;

            translatedFiles.Append(translatedInternal);
        }

        // Get the inside directory by using the first element of the.vm files
        string? saveDirectoryPath = Path.GetDirectoryName(vmFiles[0]);

        SaveOutputFileDir(saveDirectoryPath ?? directoryPath, directoryPath, translatedFiles.ToString());
    }

    private static void HandleFile(string filePath)
    {
        if (!string.Equals(Path.GetExtension(filePath), Constants.DefaultInputFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Invalid input file. Please provide a '{Constants.DefaultInputFileExtension}' file or a directory containing '{Constants.DefaultInputFileExtension}' files.");
            return;
        }

        string fileContents = File.ReadAllText(filePath);
        string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(filePath);

        string translated = TranslateIntermediateCode(fileContents, fileNameWithoutExtensions, bootstrap: false);

        SaveOutputFile(filePath, fileNameWithoutExtensions, translated);
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

    private static void SaveOutputFile(string basePath, string fileName, string translatedCode)
    {
        // Save the compiled code to the output file
        string? directoryName = Path.GetDirectoryName(basePath);

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