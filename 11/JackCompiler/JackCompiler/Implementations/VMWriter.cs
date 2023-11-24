using JackCompiler.Contracts;
using JackCompiler.Enums;

namespace JackCompiler.Implementations
{
    // TODO: Introduce mappers for the enums and change the contracts to return concrete values
    public class VMWriter : IVMWriter
    {
        public void WritePush(Segment segment, int index)
        {
            string pushCommand = $"push {segment} {index}";
        }

        public void WritePop(Segment segment, int index)
        {
            string popCommand = $"pop {segment} {index}";
        }

        public void WriteArithmetic(Command command)
        {
            string arithmeticCommand = $"{command}";
        }

        public void WriteLabel(string label)
        {
            string labelCommand = $"label {label}";
        }

        public void WriteGoto(string label)
        {
            string gotoCommand = $"goto {label}";
        }

        public void WriteIf(string label)
        {
            string ifCommand = $"if-goto {label}";
        }

        public void WriteCall(string name, int nArgs)
        {
            string callCommand = $"call {name} {nArgs}";
        }

        public void WriteFunction(string name, int nLocals)
        {
            string functionCommand = $"function ${name} ${nLocals}";
        }

        public void WriteReturn()
        {
            string returnCommand = "return";
        }
    }
}

