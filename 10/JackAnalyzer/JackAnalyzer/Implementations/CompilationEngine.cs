using System.Text;
using JackAnalyzer.Contracts;
using JackAnalyzer.Enums;
using JackAnalyzer.Exceptions;
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

            this.CheckIfCurrentTokenIsAmongExpectedKeywords(new Keyword[] { Keyword.Static, Keyword.Field });

            this.AppendKeywordToCompiled(this.tokenizer.Keyword());

            Keyword typeKeyword = this.tokenizer.Keyword();

            this.EnsureKeywordIsType(typeKeyword);

            this.AppendKeywordToCompiled(typeKeyword);

            this.AppendNextIdentifierToCompiled();

            while (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);
                this.AppendNextIdentifierToCompiled();
            }

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(classVarDecTag.ConstructClosingTag());
        }

        private void CheckIfCurrentTokenIsAmongExpectedKeywords(Keyword[] expectedKeywords)
        {
            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword)
            {
                throw new Exception("");
            }

            Keyword currentKeyword = this.tokenizer.Keyword();

            if (!expectedKeywords.Contains(currentKeyword))
            {
                throw new Exception("");
            }
        }

        public void CompileSubroutine()
        {
            // ('constructor' | 'function' | 'method') ('void' | type) subroutineName '(' paramList ')' subroutineBody

            string subroutineDecTag = Tags.SubroutineDec;

            this.compiled.Append(subroutineDecTag.ConstructOpeningTag());

            this.CheckIfCurrentTokenIsAmongExpectedKeywords(new Keyword[] { Keyword.Constructor, Keyword.Function, Keyword.Method });
            this.AppendKeywordToCompiled(this.tokenizer.Keyword());

            // TODO: ('void' | type) fn?
            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword)
            {
                throw new UnexpectedTokenTypeException(TokenType.Keyword, currentToken);
            }

            Keyword typeKeyword = this.tokenizer.Keyword();

            if (typeKeyword != Keyword.Void || !typeKeyword.IsType())
            {
                throw new Exception("Must be only void or a type");
            }

            // subroutineName
            this.AppendNextIdentifierToCompiled();

            this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileParameterList();

            this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);

            this.CompileSubroutineBody();

            this.compiled.Append(subroutineDecTag.ConstructClosingTag());
        }

        // TODO: needs to be thouroughly tested
        private void CompileSubroutineBody()
        {
            // '{' varDec* statements '}'
            this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            // varDec
            while (this.tokenizer.TokenType() == TokenType.Keyword && this.tokenizer.Keyword().IsType())
            {
                this.CompileVarDec();
            }

            this.CompileStatements();

            this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            throw new NotImplementedException();
        }

        public void CompileParameterList()
        {
            //((type varName) (',' type varName)*)?
            string parameterListTag = Tags.ParameterList;

            this.compiled.Append(parameterListTag.ConstructOpeningTag());

            // NOTE: Gateway to a recursive method
            this.CompileParameterListInner();

            this.compiled.Append(parameterListTag.ConstructClosingTag());
        }

        // TODO: needs to be thouroughly tested
        private void CompileParameterListInner()
        {
            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword)
            {
                return;
            }

            Keyword typeKeyword = this.tokenizer.Keyword();

            this.EnsureKeywordIsType(typeKeyword);

            this.AppendKeywordToCompiled(typeKeyword);

            this.AppendNextIdentifierToCompiled();

            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);

                if (this.tokenizer.TokenType() != TokenType.Keyword)
                {
                    // TODO: Refactor this by introducing an exception and making the code more readable.
                    throw new Exception(", can be followed only by a keyword in the case of param lists");
                }
            }

            this.CompileParameterListInner();
        }

        public void CompileVarDec()
        {
            // var type varName (', ' varName)* ';'

            string varDecTag = Tags.VarDec;

            this.compiled.Append(varDecTag.ConstructOpeningTag());

            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword)
            {
                throw new Exception("");
            }

            Keyword currentKeyword = this.tokenizer.Keyword();

            if (currentKeyword != Keyword.Var)
            {
                throw new Exception("");
            }

            this.AppendKeywordToCompiled(currentKeyword);

            Keyword typeKeyword = this.tokenizer.Keyword();

            this.EnsureKeywordIsType(typeKeyword);

            this.AppendKeywordToCompiled(typeKeyword);

            this.AppendNextIdentifierToCompiled();

            while (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);
                this.AppendNextIdentifierToCompiled();
            }

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(varDecTag.ConstructClosingTag());
        }

        public void CompileStatements()
        {
            return;
            //throw new NotImplementedException();
        }

        public void CompileDo()
        {
            // do subroutineCall;

            string doStatement = Statements.Do;

            this.compiled.Append(doStatement.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.Do);

            // TODO this should be subroutineCall
            this.CompileSubroutine();

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(doStatement.ConstructClosingTag());
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
            // 'return' expression? ';'

            string returnStatement = Statements.Return;

            this.compiled.Append(returnStatement.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.Return);

            // TODO: compile expression

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(returnStatement.ConstructClosingTag());
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

        private void EnsureKeywordIsType(Keyword keyword)
        {
            if (!keyword.IsType())
            {
                throw new Exception($"Keyword '{keyword}' is not a valid type.");
            }
        }
    }
}

