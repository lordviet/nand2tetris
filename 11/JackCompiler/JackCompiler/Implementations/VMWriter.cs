using System.Text;
using JackCompiler.Contracts;
using JackCompiler.Enums;

namespace JackCompiler.Implementations
{
    public class VMWriter : IVMWriter
    {
        private readonly StringBuilder compiled;

        public VMWriter()
        {
            this.compiled = new StringBuilder();
        }

        public void WritePush(Segment segment, int index)
        {
            string pushCommand = $"push {segment.ToString().ToLower()} {index}\n";
            this.compiled.Append(pushCommand);
        }

        public void WritePop(Segment segment, int index)
        {
            string popCommand = $"pop {segment.ToString().ToLower()} {index}\n";
            this.compiled.Append(popCommand);
        }

        public void WriteArithmetic(Command command)
        {
            string arithmeticCommand = $"{command.ToString().ToLower()}\n";
            this.compiled.Append(arithmeticCommand);
        }

        public void WriteLabel(string label)
        {
            string labelCommand = $"label {label}\n";
            this.compiled.Append(labelCommand);
        }

        public void WriteGoto(string label)
        {
            string gotoCommand = $"goto {label}\n";
            this.compiled.Append(gotoCommand);
        }

        public void WriteIf(string label)
        {
            string ifCommand = $"if-goto {label}\n";
            this.compiled.Append(ifCommand);
        }

        public void WriteCall(string name, int nArgs)
        {
            string callCommand = $"call {name} {nArgs}\n";
            this.compiled.Append(callCommand);
        }

        public void WriteFunction(string name, int nLocals)
        {
            string functionCommand = $"function {name} {nLocals}\n";
            this.compiled.Append(functionCommand);
        }

        public void WriteReturn()
        {
            string returnCommand = "return\n";
            this.compiled.Append(returnCommand);
        }

        public string ExportVMCode()
        {
            return this.compiled.ToString();
        }
    }
}

