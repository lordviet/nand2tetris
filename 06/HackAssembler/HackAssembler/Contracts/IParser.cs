using HackAssembler.Enums;
using HackAssembler.Models;

namespace HackAssembler.Contracts
{
    public interface IParser
    {
        IEnumerable<FileContentMeta> ScrapeLabelsWithLineNumbers();

        string GetCurrentInstruction();

        bool HasMoreCommands();

        void Advance();

        CommandType CommandType();

        string Symbol();

        string? Destination();

        string Computation();

        string? Jump();
    }
}

