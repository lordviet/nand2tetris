using VMTranslator.Enums;

namespace VMTranslator.Contracts
{
    public interface ICodeWriter
    {
        void WriteCommentedOutInstruction(string instrction);

        void WriteArithmetic(string command, int counter);

        void WritePushPop(CommandType commandType, string segment, int index);

        void WriteInit();

        void WriteLabel(string label);

        void WriteGoto(string label);

        void WriteIf(string label);

        void WriteCall(string functionName, int numberOfArguments, int counter);

        void WriteReturn();

        void WriteFunction(string functionName, int numberOfLocals);

        string Close();
    }
}

