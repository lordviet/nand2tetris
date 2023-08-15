using VMTranslator.Enums;

namespace VMTranslator.Contracts
{
    public interface IParser
    {
        string GetCurrentInstruction();

        int GetCurrentCounter();

        bool HasMoreCommands();

        void Advance();

        CommandType CommandType();

        string FirstArg();

        int SecondArg();
    }
}

