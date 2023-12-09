using JackCompiler.Enums;

namespace JackCompiler.Extensions
{
    public static class IdentifierKindExtensions
    {
        public static Segment ToSegment(this IdentifierKind source)
        {
            return source switch
            {
                IdentifierKind.Field => Segment.This,
                IdentifierKind.Static => Segment.Static,
                IdentifierKind.Var => Segment.Local,
                IdentifierKind.Argument => Segment.Argument,
                _ => throw new Exception($"Identifier kind {source} is not a valid segment!")
            };
        }

    }
}

