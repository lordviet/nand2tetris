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
            "class".ConstructOpeningTag();

            AppendKeywordToCompiled(Keyword.Class);

            // Name

            AppendTokenToCompiled(Constants.Symbols.LeftCurlyBrace);

            // ?
            this.CompileClassVarDec();

            // ??
            this.CompileSubroutine();

            this.AppendTokenToCompiled(Constants.Symbols.RightCurlyBrace);
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
            this.AppendKeywordToCompiled(Keyword.While);

            this.AppendTokenToCompiled(Constants.Symbols.LeftParenthesis);

            this.CompileExpression();

            this.AppendTokenToCompiled(Constants.Symbols.RightParenthesis);

            this.AppendTokenToCompiled(Constants.Symbols.LeftCurlyBrace);

            this.CompileStatements();

            this.AppendTokenToCompiled(Constants.Symbols.RightCurlyBrace);
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
    }
}

