using System.Text;
using JackAnalyzer.Contracts;
using JackAnalyzer.Enums;
using JackAnalyzer.Extensions;

namespace JackAnalyzer.Implementations
{
    public class CompilationEngine : ICompilationEngine
    {
        private readonly IJackTokenizer tokenizer;
        private readonly StringBuilder compiled;

        public CompilationEngine(IJackTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            this.compiled = new StringBuilder();

            this.CompileClass();
        }

        public void CompileClass()
        {
            string classTag = "<class>";

            "class".ConstructKeywordNode();

            this.tokenizer.Advance();

            // TODO: handle classname
            // + tokenizer>Advance();

            this.Eat("{");

            "{".ConstructSymbolNode();

            // ?
            this.CompileClassVarDec();

            // ??
            this.CompileSubroutine();

            this.Eat("}");

            "}".ConstructSymbolNode();

            throw new NotImplementedException();
        }

        public void CompileClassVarDec()
        {
            throw new NotImplementedException();
        }

        public void CompileSubroutine()
        {
            throw new NotImplementedException();
        }

        public void CompileParameterList()
        {
            throw new NotImplementedException();
        }

        public void CompileVarDec()
        {
            throw new NotImplementedException();
        }

        public void CompileStatements()
        {
            throw new NotImplementedException();
        }

        public void CompileDo()
        {
            throw new NotImplementedException();
        }

        public void CompileLet()
        {
            throw new NotImplementedException();
        }

        public void CompileWhile()
        {
            // TODO: where do these hard-coded strings come from, do they live in both states?

            AppendKeywordToCompiled(Keyword.While);

            AppendTokenToCompiled("(");

            this.CompileExpression();

            AppendTokenToCompiled(")");

            AppendTokenToCompiled("{");

            this.CompileStatements();

            AppendTokenToCompiled("}");

        }

        private void AppendKeywordToCompiled(Keyword key)
        {
            string keyword = Constants.LexicalElements.ReverseKeywordMap[key];

            this.AppendTokenToCompiled(keyword);

            return;
        }

        private void AppendTokenToCompiled(string token)
        {
            this.Eat(token);

            string node = token.ConstructKeywordNode();

            this.compiled.Append(node);

            return;
        }

        public void CompileReturn()
        {
            throw new NotImplementedException();
        }

        public void CompileIf()
        {
            throw new NotImplementedException();
        }

        public void CompileExpression()
        {
            throw new NotImplementedException();
        }

        public void CompileTerm()
        {
            throw new NotImplementedException();
        }

        public void CompileExpressionList()
        {
            throw new NotImplementedException();
        }

        private void Eat(string expectedToken)
        {
            if (!tokenizer.HasMoreTokens())
            {
                // TODO: Maybe throw?
                return;
            }

            string currentToken = tokenizer.GetCurrentToken();

            if (tokenizer.GetCurrentToken() != expectedToken)
            {
                // TODO: Introduce unexpected token exception
                throw new Exception($"Expected token {expectedToken} but got {currentToken} instead.");
            }

            this.tokenizer.Advance();

            return;
        }
    }
}

