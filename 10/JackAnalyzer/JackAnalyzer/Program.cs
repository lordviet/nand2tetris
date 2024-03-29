﻿using System.Xml.Linq;
using JackAnalyzer;
using JackAnalyzer.Contracts;
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

        foreach (string filePath in jackFiles)
        {
            string fileContents = File.ReadAllText(filePath);
            string fileNameWithoutExtensions = Path.GetFileNameWithoutExtension(filePath);

            string analyzed = AnalyzeFile(fileContents);

            SaveOutputFile(filePath, fileNameWithoutExtensions, analyzed);
        }
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

        string analyzed = AnalyzeFile(fileContents);

        SaveOutputFile(filePath, fileNameWithoutExtensions, analyzed);
    }

    private static string AnalyzeFile(string fileContents)
    {
        IJackTokenizer tokenizer = new JackTokenizer(fileContents);
        ICompilationEngine engine = new CompilationEngine(tokenizer);

        return engine.Close();
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
            XDocument formatted = XDocument.Parse(analyzedCode);
            File.WriteAllText(outputFile, formatted.ToString());
            Console.WriteLine("Syntax analysis successfully completed. Output file: " + outputFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving the output file: " + ex.Message);
        }
    }
}