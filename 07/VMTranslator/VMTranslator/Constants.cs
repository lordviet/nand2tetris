namespace VMTranslator
{
    public static class Constants
    {
        public const string DefaultInputFileExtension = ".vm";
        public const string DefaultOutputFileExtension = ".asm";

        public const string DefaultBootstrapFunctionName = "Sys.init";

        public const int DefaultTempRegisterIndex = 5;
        public const int DefaultStackPushesBeforeMethodInvocation = 5;
        public const int DefaultStandardMappingStackBeginningAddress = 256;

        public static class Keywords
        {
            public const string Push = "push";
            public const string Pop = "pop";
            public const string Label = "label";
            public const string Goto = "goto";
            public const string If = "if-goto";
            public const string Function = "function";
            public const string Call = "call";
            public const string Return = "return";
            public static readonly string[] ArithmeticCommands = new string[] { "add", "sub", "neg", "eq", "gt", "lt", "and", "or", "not" };
        }

        public static class Mnemonics
        {
            public const string StackPointer = "SP";

            public static class Segments
            {
                public const string Local = "LCL";
                public const string Arg = "ARG";
                public const string This = "THIS";
                public const string That = "THAT";
            }

            public static class Jumps
            {
                public const string GreaterThan = "JGT";
                public const string LessThan = "JLT";
                public const string Uncoditional = "JMP";
                public const string EqToZero = "JEQ";
                public const string NotEqToZero = "JNE";
            }
        }

        public static class RegisterCommands
        {
            public static class A
            {
                public const string EqM = "A=M\n";
                public const string EqDPlusM = "A=D+M\n";
                public const string EqMMinusD = "A=M-D\n";
            }

            public static class D
            {
                public const string EqA = "D=A\n";
                public const string EqM = "D=M\n";
                public const string EqDPlusM = "D=D+M\n";
                public const string EqMMinusD = "D=M-D\n";
                public const string EqDMinusM = "D=D-M\n";
                public const string EqAPlusD = "D=A+D\n";
                public const string EqDAndM = "D=D&M\n";
                public const string EqDOrM = "D=D|M\n";
                public const string EqExclD = "D=!D\n";
                public const string EqExclM = "D=!M\n";
            }

            public static class M
            {
                public const string EqD = "M=D\n";
                public const string EqDPlusOne = "M=D+1\n";
                public const string EqExclD = "M=!D\n";
                public const string EqMPlusOne = "M=M+1\n";
                public const string EqMMinusOne = "M=M-1\n";
                public const string EqMinusM = "M=-M\n";
                public const string EqOne = "M=1\n";
                public const string EqMinusOne = "M=-1\n";
                public const string EqZero = "M=0\n";
            }
        }
    }
}

