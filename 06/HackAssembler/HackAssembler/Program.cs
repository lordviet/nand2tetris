using System;
using System.Text;
using HackAssembler.Enums;
using HackAssembler.Implementations;

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

        string fileContents = File.ReadAllText("/Users/dmtodev/Desktop/Projects/nand2tetris/06/add/Add.asm");

        Parser parser = new Parser(fileContents);
        MnemonicsConverter converter = new MnemonicsConverter();

        StringBuilder sb = new StringBuilder();

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

        const int onePadCount = 3;
        string padding = new('1', onePadCount);

        string converted = $"{padding}{computationBinary}{destinationBinary}{jumpBinary}{Environment.NewLine}";

        return converted;
    }
}

