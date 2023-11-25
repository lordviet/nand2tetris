using JackCompiler.Contracts;
using JackCompiler.Enums;

namespace JackCompiler.Implementations
{
    public class VMWriter : IVMWriter
    {
        public string WritePush(Segment segment, int index)
        {
            string pushCommand = $"push {segment.ToString().ToLower()} {index}";

            return pushCommand;
        }

        public string WritePop(Segment segment, int index)
        {
            string popCommand = $"pop {segment.ToString().ToLower()} {index}";

            return popCommand;
        }

        public string WriteArithmetic(Command command)
        {
            string arithmeticCommand = command.ToString().ToLower();

            return arithmeticCommand;
        }

        public string WriteLabel(string label)
        {
            string labelCommand = $"label {label}";

            return labelCommand;
        }

        public string WriteGoto(string label)
        {
            string gotoCommand = $"goto {label}";

            return gotoCommand;
        }

        public string WriteIf(string label)
        {
            string ifCommand = $"if-goto {label}";

            return ifCommand;
        }

        public string WriteCall(string name, int nArgs)
        {
            string callCommand = $"call {name} {nArgs}";

            return callCommand;
        }

        public string WriteFunction(string name, int nLocals)
        {
            string functionCommand = $"function ${name} ${nLocals}";

            return functionCommand;
        }

        public string WriteReturn()
        {
            string returnCommand = "return";

            return returnCommand;
        }
    }
}

