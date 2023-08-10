using VMTranslator.Enums;

namespace VMTranslator.Extensions
{
    public static class SegmentExtensions
    {

        public static string ToSegmentMnemonic(this Segment source)
        {
            return source switch
            {
                Segment.Local => Constants.Mnemonics.Segments.Local,
                Segment.Arg => Constants.Mnemonics.Segments.Arg,
                Segment.This => Constants.Mnemonics.Segments.This,
                Segment.That => Constants.Mnemonics.Segments.That,
                _ => throw new ArgumentException($"Unexpected segment '{source}' does not have a corresponding mnemonic!")
            };
        }
    }
}

