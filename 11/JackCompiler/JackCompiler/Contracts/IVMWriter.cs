using JackCompiler.Enums;

namespace JackCompiler.Contracts
{
    public interface IVMWriter
    {
        public void WritePush(Segment segment, int index);

        public void WritePop(Segment segment, int index);

        public void WriteArithmetic(Command command, int index);

        public void WriteLabel(string label);

        public void WriteGoto(string label);

        public void WriteIf(string label);

        public void WriteCall(string name, int nArgs);

        public void WriteFunction(string name, int nLocals);

        public void WriteReturn();
    }
}

