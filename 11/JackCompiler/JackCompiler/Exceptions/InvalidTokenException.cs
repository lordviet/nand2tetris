namespace JackCompiler.Exceptions
{
    public class InvalidTokenException : FormatException
    {
        public InvalidTokenException(string token) : base($"The current token - {token} does not match any known format.")
        {
        }
    }

}

