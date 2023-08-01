﻿using System.Text;
using VMTranslator.Enums;
using VMTranslator.Extensions;
using VMTranslator.Implementations;

namespace VMTranslator;

class Program
{
    static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine($"Usage: VMTranslator <inputFile>.{Constants.DefaultInputFileExtension}");
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

        string translated = TranslateIntermediateCode(fileContents);

        SaveOutputFile(fileName, translated);
    }

    private static string TranslateIntermediateCode(string fileContents)
    {
        Parser parser = new Parser(fileContents);

        StringBuilder sb = new StringBuilder();

        while (parser.HasMoreCommands())
        {
            CommandType currentInstructionType = parser.CommandType();

            string currentInstruction = parser.GetCurrentInstruction();

            sb.Append(currentInstruction.CommentOut());

            if (currentInstructionType == CommandType.Push)
            {   
                //sb.Append(...);
            }

            if (currentInstructionType == CommandType.Pop)
            {

                //sb.Append(...);
            }

            parser.Advance();
        }

        return string.Empty;
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
        string outputFile = Path.Combine(directoryName, $"{fileNameWithoutExtensions}.hack");

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