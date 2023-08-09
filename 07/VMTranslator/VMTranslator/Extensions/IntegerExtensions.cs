namespace VMTranslator.Extensions
{
    public static class IntegerExtensions
    {
        public static bool IsValidPointerIndex(this int index)
        {
            return index == 0 || index == 1;
        }

        public static string PointerIndexToMnemonicMemorySegment(this int index)
        {
            return index switch
            {
                0 => Constants.Mnemonics.Segments.This,
                1 => Constants.Mnemonics.Segments.That,
                _ => throw new ArgumentException($"Unexpected index for pointer command '{index}'! Expected either '0' or '1'") 
            };
        }
    }
}

