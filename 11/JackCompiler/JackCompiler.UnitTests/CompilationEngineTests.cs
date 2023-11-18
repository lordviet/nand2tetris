using System.Linq.Expressions;
using System.Xml.Linq;
using JackCompiler.Contracts;
using JackCompiler.Implementations;

namespace JackCompiler.UnitTests;

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
        ICompilationEngine engine = new CompilationEngine(tokenizer, compileClass: false);

        engine.CompileWhile();

        string compiled = engine.Close();

        Assert.NotNull(compiled);
    }

    [Test]
    public void TestCompileClassVarDec()
    {
        string classVarDecExpression = "static boolean test, test2, test3;";

        IJackTokenizer tokenizer = new JackTokenizer(classVarDecExpression);
        ICompilationEngine engine = new CompilationEngine(tokenizer, compileClass: false);

        engine.CompileClassVarDec();

        string compiled = engine.Close();

        XDocument formatted = XDocument.Parse(compiled);

        Assert.NotNull(formatted.ToString());
    }

    [TestCase("int x, char y, boolean z)")]
    [TestCase("int x)")]
    public void TestCompileParameterList(string parameterListExpression)
    {
        IJackTokenizer tokenizer = new JackTokenizer(parameterListExpression);
        ICompilationEngine engine = new CompilationEngine(tokenizer, compileClass: false);

        engine.CompileParameterList();

        string compiled = engine.Close();

        XDocument formatted = XDocument.Parse(compiled);

        // Example assertion (modify as needed):
        Assert.NotNull(formatted.ToString());
    }

    [TestCase("let s = null;")]
    [TestCase("let s = \"string constant\";")]
    [TestCase("let i = i | j;")]
    [TestCase("let i = i * (-j);")]
    [TestCase("let j = j / (-2);")]
    [TestCase("let a[1] = a[2];")]
    public void TestCompileLetExpression(string letExpression)
    {
        IJackTokenizer tokenizer = new JackTokenizer(letExpression);
        ICompilationEngine engine = new CompilationEngine(tokenizer, compileClass: false);

        engine.CompileLet();

        string compiled = engine.Close();

        XDocument formatted = XDocument.Parse(compiled);

        string let = formatted.ToString();

        // Example assertion (modify as needed):
        Assert.NotNull(let);
    }


    //[TestCase("do Screen.drawRectangle(x, (y + size) - 1, x + size, y + size);")]
    [TestCase("do Screen.drawRectangle((x + size) - 1, y, x + size, y + size);")]
    public void TestCompileDoExpression(string doExpression)
    {
        IJackTokenizer tokenizer = new JackTokenizer(doExpression);
        ICompilationEngine engine = new CompilationEngine(tokenizer, compileClass: false);

        engine.CompileDo();

        string compiled = engine.Close();

        XDocument formatted = XDocument.Parse(compiled);

        string formattedDo = formatted.ToString();

        // Example assertion (modify as needed):
        Assert.NotNull(formattedDo);
    }
}
