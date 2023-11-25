using JackCompiler.Enums;

namespace JackCompiler.Contracts
{
    public interface IVMWriter
    {
        public string WritePush(Segment segment, int index);

        public string WritePop(Segment segment, int index);

        public string WriteArithmetic(Command command);

        public string WriteLabel(string label);

        public string WriteGoto(string label);

        public string WriteIf(string label);

        public string WriteCall(string name, int nArgs);

        public string WriteFunction(string name, int nLocals);

        public string WriteReturn();
    }
}

