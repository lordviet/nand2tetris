using VMTranslator.Enums;

namespace VMTranslator.Extensions
{
    public static class StringExtensions
    {
        public static string StripComment(this string source)
        {
            int commentStart = source.IndexOf("//");

            return commentStart == -1
                ? source
                : source[0..commentStart];
        }

        public static string ExtractFirstArgumentFromInstruction(this string source)
        {
            return source.Split(" ")[1];
        }

        public static int ExtractSecondArgumentFromInstruction(this string source)
        {
            string extracted = source.Split(" ").Last();

            return int.TryParse(extracted, out int parsedArg)
                ? parsedArg
                : int.MinValue;
        }

        public static string CommentOut(this string source)
        {
            return $"// {source}\n";
        }

        public static string ToAInstruction(this string source)
        {
            return $"@{source}\n";
        }

        public static string ToLabelSymbolDeclaration(this string source)
        {
            return $"({source})\n";
        }

        public static string ToJumpCommand(this string operand, string jumpCommand)
        {
            return $"{operand};{jumpCommand}\n";
        }

        public static string? ToSegmentMnemonic(this string source)
        {
            return source switch
            {
                "local" => Constants.Mnemonics.Segments.Local,
                "argument" => Constants.Mnemonics.Segments.Arg,
                "this" => Constants.Mnemonics.Segments.This,
                "that" => Constants.Mnemonics.Segments.That,
                _ => null
            };
        }

        public static Segment ToSegmentEnum(this string source)
        {
            return source switch
            {
                "constant" => Segment.Constant,
                "static" => Segment.Static,
                "temp" => Segment.Temp,
                "pointer" => Segment.Pointer,
                "local" => Segment.Local,
                "arg" => Segment.Arg,
                "this" => Segment.This,
                "that" => Segment.That,
                _ => throw new ArgumentException($"Unexpected segment '{source}'!")
            };
        }

        public static ArithmeticCommand ToArithmeticCommand(this string source)
        {
            return source switch
            {
                "add" => ArithmeticCommand.Addition,
                "sub" => ArithmeticCommand.Subtraction,
                "neg" => ArithmeticCommand.Negation,
                "eq" => ArithmeticCommand.Equality,
                "gt" => ArithmeticCommand.GreaterThan,
                "lt" => ArithmeticCommand.LessThan,
                "and" => ArithmeticCommand.And,
                "or" => ArithmeticCommand.Or,
                "not" => ArithmeticCommand.Not,
                _ => throw new ArgumentException($"Unexpected arithmetic command '{source}'!")
            };
        }
    }
}

