using System.Data;
using System.Text;
using JackAnalyzer;
using JackAnalyzer.Contracts;
using JackAnalyzer.Enums;
using JackAnalyzer.Exceptions;
using JackAnalyzer.Implementations;

class Program
{
    static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine($"Usage: JackAnalyzer <inputFile>{Constants.DefaultInputFileExtension} or JackAnalyzer <directory>");
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
        // Get all .jack files in the directory
        string[] jackFiles = Directory.GetFiles(directoryPath, $"*{Constants.DefaultInputFileExtension}");

        if (jackFiles.Length == 0)
        {
            Console.WriteLine($"No '.{Constants.DefaultInputFileExtension}' files found in the directory.");
            return;
        }

        StringBuilder translatedFiles = new StringBuilder();

        foreach (string filePath in jackFiles)
        {
            string fileContents = File.ReadAllText(filePath);
            string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(filePath);

            string analyzedFile = AnalyzeFile(fileContents, fileNameWithoutExtensions);

            jackFiles.Append(analyzedFile);
        }

        // Get the inside directory by using the first element of the.jack files
        string? saveDirectoryPath = Path.GetDirectoryName(jackFiles[0]);

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

        string analyzed = AnalyzeFile(fileContents, fileNameWithoutExtensions);

        SaveOutputFile(filePath, fileNameWithoutExtensions, analyzed);
    }

    private static string AnalyzeFile(string fileContents, string fileName)
    {
        IJackTokenizer tokenizer = new JackTokenizer();
        ICompilationEngine engine = new CompilationEngine();

        while (tokenizer.HasMoreTokens())
        {
            TokenType currentTokenType = tokenizer.TokenType();

            switch (currentTokenType)
            {
                default:
                    throw new UnexpectedTokenTypeException($"Unexpected token type '{currentTokenType}'!");
            }

            tokenizer.Advance();
        }

        return "";
    }

    private static void SaveOutputFileDir(string directoryName, string fileName, string analyzedCode)
    {
        try
        {
            // Combine the directory path with the default output file extension to create the output file path
            string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(fileName);
            string outputFile = Path.Combine(directoryName, $"{fileNameWithoutExtensions}{Constants.DefaultOutputFileExtension}");

            File.WriteAllText(outputFile, analyzedCode);
            Console.WriteLine("Syntax analysis successfully completed. Output file: " + outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving the output file: " + ex.Message);
        }
    }

    private static void SaveOutputFile(string basePath, string fileName, string analyzedCode)
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
            File.WriteAllText(outputFile, analyzedCode);
            Console.WriteLine("Syntax analysis successfully completed. Output file: " + outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving the output file: " + ex.Message);
        }
    }
}