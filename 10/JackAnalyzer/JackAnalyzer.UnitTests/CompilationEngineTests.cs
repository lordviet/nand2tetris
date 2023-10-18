using System.Xml.Linq;
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

    [Test]
    public void TestCompileClassVarDec()
    {
        string classVarDecExpression = "static boolean test, test2, test3;";

        IJackTokenizer tokenizer = new JackTokenizer(classVarDecExpression);
        ICompilationEngine engine = new CompilationEngine(tokenizer);

        engine.CompileClassVarDec();

        string compiled = engine.Close();

        XDocument formatted = XDocument.Parse(compiled);

        Assert.NotNull(formatted.ToString());
    }
}
