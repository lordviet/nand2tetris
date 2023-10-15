using JackAnalyzer.Contracts;
using JackAnalyzer.Implementations;

namespace JackAnalyzer.UnitTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestWhile()
    {
        string whileExpression = """
        while (i < length) {
            let sum = sum + a[i];
            let i = i + 1;
        }   
        """;

        IJackTokenizer tokenizer = new JackTokenizer(whileExpression);
        ICompilationEngine engine = new CompilationEngine(tokenizer);

        engine.CompileWhile();

        string compiled = engine.Close();

        Assert.NotNull(compiled);
    }
}
