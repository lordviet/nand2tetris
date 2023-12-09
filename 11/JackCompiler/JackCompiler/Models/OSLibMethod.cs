namespace JackCompiler.Models
{
    public class OSLibMethod
    {
        public OSLibMethod(string methodName, int defaultParameter)
        {
            this.MethodName = methodName;
            this.DefaultParameter = defaultParameter;
        }

        public string MethodName { get; }

        public int DefaultParameter { get; }
    }
}
