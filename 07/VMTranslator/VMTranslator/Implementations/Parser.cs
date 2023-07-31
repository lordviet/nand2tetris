using VMTranslator.Contracts;
using VMTranslator.Enums;
using VMTranslator.Extensions;

namespace VMTranslator.Implementations
{
    public class Parser : IParser
	{
        private string[] fileContents;
        private int counter;

		public Parser(string fileContents)
		{
            this.fileContents = PreprocessFileContents(fileContents);
            this.counter = 0;
        }

        // TODO: Possibly private, we'll see.
        public string GetCurrentInstruction()
        {
            return this.fileContents[this.counter];
        }

        public bool HasMoreCommands()
        {
            return this.counter < this.fileContents.Length;
        }

        public void Advance()
        {
            this.counter++;
        }

        public CommandType CommandType()
        {
            throw new NotImplementedException();
        }

        public string FirstArg()
        {
            string currentInstruction = this.GetCurrentInstruction();

            CommandType currentCommand = this.CommandType();

            return currentCommand switch
            {
                Enums.CommandType.Arithmetic => currentInstruction, // add, sub, etc...
                Enums.CommandType.Return => string.Empty, // should not be called with return command type
                _ => string.Empty, // TODO: Implement, e.g push constant 18 (should return push)
            };
        }

        public int SecondArg()
        {
            throw new NotImplementedException();
        }

        private string[] PreprocessFileContents(string fileContents)
        {
            return fileContents
                .Split(Environment.NewLine)
                .Select(line => line.StripComment())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimEnd('\r', '\n'))
                .Select(line => line.Trim())
                .ToArray();
        }
    }
}

