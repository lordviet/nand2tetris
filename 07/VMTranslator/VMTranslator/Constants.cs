namespace VMTranslator
{
    public static class Constants
    {
        public const string DefaultInputFileExtension = ".vm";
        public const string DefaultOutputFileExtension = ".asm";

        public const string PushKeyword = "push";
        public const string PopKeyword = "pop";

        public const int DefaultTempRegisterIndex = 5;

        public static readonly string[] ArithmeticCommandKeywords = new string[] { "add", "sub", "neg", "eq", "gt", "lt", "and", "or", "not" };

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

        }
    }
}

