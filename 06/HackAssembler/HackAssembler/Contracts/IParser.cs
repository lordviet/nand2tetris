using HackAssembler.Enums;

namespace HackAssembler.Contracts
{
    public interface IParser
    {
        string GetCurrentInstruction();

        bool HasMoreCommands();

        void Advance();

        CommandType CommandType();

        string Symbol();

        string Destination();

        string Computation();

        string Jump();
    }
}

