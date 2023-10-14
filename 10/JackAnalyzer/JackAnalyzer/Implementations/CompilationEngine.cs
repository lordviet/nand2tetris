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
            string classKeyword = Constants.LexicalElements.ReverseKeywordMap[Keyword.Class];

            classKeyword.ConstructOpeningTag();

            this.AppendKeywordToCompiled(Keyword.Class);

            // className
            this.AppendNextIdentifierToCompiled();

            this.AppendTokenToCompiled(Constants.Symbols.LeftCurlyBrace, TokenType.Symbol);

            // ?
            this.CompileClassVarDec();

            // ??
            this.CompileSubroutine();

            this.AppendTokenToCompiled(Constants.Symbols.RightCurlyBrace, TokenType.Symbol);

            classKeyword.ConstructClosingTag();
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
            // What about let statement tag, should I do it here?
            //string letKeyword = Constants.LexicalElements.ReverseKeywordMap[Keyword.Let];
            this.AppendKeywordToCompiled(Keyword.Let);

            this.AppendNextIdentifierToCompiled();

            // TODO: optional Expression in-between?

            this.AppendTokenToCompiled(Constants.Symbols.EqualitySign, TokenType.Symbol);

            this.CompileExpression();

            this.AppendTokenToCompiled(Constants.Symbols.Semicolon, TokenType.Symbol);

            throw new NotImplementedException();
        }

        public void CompileWhile()
        {
            this.AppendKeywordToCompiled(Keyword.While);

            this.AppendTokenToCompiled(Constants.Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileExpression();

            this.AppendTokenToCompiled(Constants.Symbols.RightParenthesis, TokenType.Symbol);

            this.AppendTokenToCompiled(Constants.Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileStatements();

            this.AppendTokenToCompiled(Constants.Symbols.RightCurlyBrace, TokenType.Symbol);
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

        private string RetrieveNextExpectedOfType(TokenType expectedTokenType)
        {
            if (!tokenizer.HasMoreTokens())
            {
                // TODO: better error message
                throw new Exception("No more tokens in tokenizer");
            }

            TokenType currentTokenType = tokenizer.TokenType();

            if (currentTokenType != expectedTokenType)
            {
                throw new Exception($"Expected token type {expectedTokenType} but got {currentTokenType} instead.");
            }

            string token = tokenizer.GetCurrentToken();

            this.tokenizer.Advance();

            return token;
        }

        private void Eat(string expectedToken)
        {
            if (!tokenizer.HasMoreTokens())
            {
                // TODO: Maybe throw?
                return;
            }

            string currentToken = tokenizer.GetCurrentToken();

            if (currentToken != expectedToken)
            {
                // TODO: Introduce unexpected token exception
                throw new Exception($"Expected token {expectedToken} but got {currentToken} instead.");
            }

            this.tokenizer.Advance();

            return;
        }

        private void AppendNextIdentifierToCompiled()
        {
            string token = this.RetrieveNextExpectedOfType(TokenType.Identifier);

            this.AppendTokenToCompiled(token, TokenType.Identifier);
        }

        private void AppendKeywordToCompiled(Keyword key)
        {
            string keyword = Constants.LexicalElements.ReverseKeywordMap[key];

            this.AppendTokenToCompiled(keyword, TokenType.Keyword);

            return;
        }

        private void AppendTokenToCompiled(string token, TokenType type)
        {
            this.Eat(token);

            string node = type switch
            {
                TokenType.Keyword => token.ConstructKeywordNode(),
                TokenType.Symbol => token.ConstructSymbolNode(),
                TokenType.Identifier => token.ConstructIdentifierNode(),
                _ => throw new NotSupportedException("") // TODO: Exception message
            };

            this.compiled.Append(node);

            return;
        }
    }
}

