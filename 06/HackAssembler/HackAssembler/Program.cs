using System.Text;
using HackAssembler.Enums;

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

        StringBuilder sb = new StringBuilder();

        while (parser.HasMoreCommands())
        {
            CommandType currentInstructionType = parser.CommandType();

            if (currentInstructionType == CommandType.A)
            {
                // TODO extract as method
                string symbol = parser.Symbol();

                int value = int.Parse(symbol);

                string binary = Convert.ToString(value, 2);

                int zeroPadCount = 16 - binary.Length;

                string padding = new('0', zeroPadCount);

                string converted = $"{padding}{binary}{Environment.NewLine}";

                sb.Append(converted);
            }

            parser.Advance();
        }

        Console.WriteLine(sb.ToString());
    }
}

