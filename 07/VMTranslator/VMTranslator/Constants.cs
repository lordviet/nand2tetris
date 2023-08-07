namespace VMTranslator
{
    public static class Constants
    {
        public const string DefaultInputFileExtension = ".vm";
        public const string DefaultOutputFileExtension = ".asm";

        public const string PushKeyword = "push";
        public const string PopKeyword = "pop";

        public static readonly string[] ArithmeticCommandKeywords = new string[] { "add", "sub", "neg", "eq", "gt", "lt", "and", "or", "not" };

        public static class SegmentMnemonics
        {
            public const string StackPointer = "SP";
            public const string Local = "LCL";
            public const string Arg = "ARG";
            public const string This = "THIS";
            public const string That = "THAT";
        }
    }
}

