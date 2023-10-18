using System.Text;
using JackAnalyzer.Contracts;
using JackAnalyzer.Enums;
using JackAnalyzer.Extensions;
using static JackAnalyzer.Constants;

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

            //this.CompileClass();
        }

        public void CompileClass()
        {
            string classKeyword = LexicalElements.ReverseKeywordMap[Keyword.Class];

            this.compiled.Append(classKeyword.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.Class);

            // className
            this.AppendNextIdentifierToCompiled();

            this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            // ?
            this.CompileClassVarDec();

            // ??
            this.CompileSubroutine();

            this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            this.compiled.Append(classKeyword.ConstructClosingTag());
        }

        public void CompileClassVarDec()
        {
            //('static' | 'field') type varName (', ' varName)* ';'

            string classVarDecTag = Tags.ClassVarDec;

            this.compiled.Append(classVarDecTag.ConstructOpeningTag());

            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword)
            {
                throw new Exception("");
            }

            Keyword currentKeyword = this.tokenizer.Keyword();

            if (currentKeyword != Keyword.Static && currentKeyword != Keyword.Static)
            {
                throw new Exception("");
            }

            this.AppendKeywordToCompiled(currentKeyword);

            Keyword typeKeyword = this.tokenizer.Keyword();

            // TODO: This can become a method
            // type is int | char | boolean | className
            //  <keyword> boolean </keyword>
            if (typeKeyword != Keyword.Integer && typeKeyword != Keyword.Char && typeKeyword != Keyword.Boolean && typeKeyword != Keyword.Class)
            {
                throw new Exception("");
            }

            this.AppendKeywordToCompiled(typeKeyword);


            // TODO: this is not entirely correct since it may be one or several varNames so it needs to be accounted for.
            this.AppendNextIdentifierToCompiled();
            //  <identifier> test </identifier>

            while(this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);


                this.AppendNextIdentifierToCompiled();
            }

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(classVarDecTag.ConstructClosingTag());
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
            return;
            //throw new NotImplementedException();
        }

        public void CompileDo()
        {
            throw new NotImplementedException();
        }

        public void CompileLet()
        {
            string letStatement = Statements.Let;

            this.compiled.Append(letStatement.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.Let);

            this.AppendNextIdentifierToCompiled();

            // TODO: optional Expression in-between?

            this.AppendTokenToCompiled(Symbols.EqualitySign, TokenType.Symbol);

            this.CompileExpression();

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(letStatement.ConstructClosingTag());
        }

        public void CompileWhile()
        {
            string whileStatement = Statements.While;

            this.compiled.Append(whileStatement.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.While);

            this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileExpression();

            this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);

            this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileStatements();

            this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            this.compiled.Append(whileStatement.ConstructClosingTag());
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
            return;
            //throw new NotImplementedException();
        }

        public void CompileTerm()
        {
            throw new NotImplementedException();
        }

        public void CompileExpressionList()
        {
            return;
            //throw new NotImplementedException();
        }

        public string Close()
        {
            return this.compiled.ToString();
        }

        // TODO: I dislike the name since it does not what the method does really
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

            //this.tokenizer.Advance();

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
            string keyword = LexicalElements.ReverseKeywordMap[key];

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

