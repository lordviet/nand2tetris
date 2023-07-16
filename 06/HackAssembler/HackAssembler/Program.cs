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

        while (parser.HasMoreCommands())
        {
            parser.Advance();
        }
    }
}

