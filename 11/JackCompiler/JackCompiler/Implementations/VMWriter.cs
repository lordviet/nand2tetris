using JackCompiler.Contracts;
using JackCompiler.Enums;

namespace JackCompiler.Implementations
{
    public class VMWriter : IVMWriter
    {
        public string WritePush(Segment segment, int index)
        {
            string pushCommand = $"push {segment.ToString().ToLower()} {index}\n";

            return pushCommand;
        }

        public string WritePop(Segment segment, int index)
        {
            string popCommand = $"pop {segment.ToString().ToLower()} {index}\n";

            return popCommand;
        }

        public string WriteArithmetic(Command command)
        {
            string arithmeticCommand = $"{command.ToString().ToLower()}\n";

            return arithmeticCommand;
        }

        public string WriteLabel(string label)
        {
            string labelCommand = $"label {label}\n";

            return labelCommand;
        }

        public string WriteGoto(string label)
        {
            string gotoCommand = $"goto {label}\n";

            return gotoCommand;
        }

        public string WriteIf(string label)
        {
            string ifCommand = $"if-goto {label}\n";

            return ifCommand;
        }

        public string WriteCall(string name, int nArgs)
        {
            string callCommand = $"call {name} {nArgs}\n";

            return callCommand;
        }

        public string WriteFunction(string name, int nLocals)
        {
            string functionCommand = $"function ${name} ${nLocals}\n";

            return functionCommand;
        }

        public string WriteReturn()
        {
            string returnCommand = "return\n";

            return returnCommand;
        }
    }
}

