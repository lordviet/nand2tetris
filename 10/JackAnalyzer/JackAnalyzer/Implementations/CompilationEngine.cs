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

            this.CompileClass();
        }

        public void CompileClass()
        {
            // 'class' className '{' classVarDec* subroutineDec* '}'

            string classKeyword = LexicalElements.ReverseKeywordMap[Keyword.Class];

            this.compiled.Append(classKeyword.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.Class);

            this.AppendNextIdentifierToCompiled();

            this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileClassVarDecInClass();

            this.CompileSubroutineDecInClass();

            this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            this.compiled.Append(classKeyword.ConstructClosingTag());
        }

        private void CompileClassMemberInClass(Keyword[] acceptableKeywords, Action compilationAction)
        {
            if (this.tokenizer.TokenType() != TokenType.Keyword)
            {
                return;
            }

            Keyword keyword = this.tokenizer.Keyword();

            if (acceptableKeywords.Contains(keyword))
            {
                compilationAction(); // Execute the specified compilation action
                this.CompileClassMemberInClass(acceptableKeywords, compilationAction); // Recursive call
            }
        }

        private void CompileClassVarDecInClass()
        {
            Keyword[] acceptableKeywords = { Keyword.Static, Keyword.Field };
            this.CompileClassMemberInClass(acceptableKeywords, this.CompileClassVarDec);
        }

        private void CompileSubroutineDecInClass()
        {
            Keyword[] acceptableKeywords = { Keyword.Constructor, Keyword.Function, Keyword.Method };
            this.CompileClassMemberInClass(acceptableKeywords, this.CompileSubroutine);
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

            if (typeKeyword != Keyword.Void && !typeKeyword.IsType())
            {
                throw new Exception("Must be only void or a type");
            }

            this.AppendTokenToCompiled(this.tokenizer.GetCurrentToken(), TokenType.Keyword);

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
            string subroutineBodyTag = Tags.SubroutineBody;

            this.compiled.Append(subroutineBodyTag.ConstructOpeningTag());

            // '{' varDec* statements '}'
            this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileVarDecInSubroutineBody();

            this.CompileStatements();

            this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            this.compiled.Append(subroutineBodyTag.ConstructClosingTag());
        }

        private void CompileVarDecInSubroutineBody()
        {
            TokenType tokenType = this.tokenizer.TokenType();

            if (tokenType != TokenType.Keyword)
            {
                return;
            }

            Keyword keyword = this.tokenizer.Keyword();

            if(keyword != Keyword.Var)
            {
                return;
            }

            this.CompileVarDec();
            this.CompileVarDecInSubroutineBody();
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

            // TODO: Missing proper type handling
            if (this.tokenizer.TokenType() == TokenType.Keyword)
            {
                Keyword typeKeyword = this.tokenizer.Keyword();

                this.EnsureKeywordIsType(typeKeyword);

                this.AppendKeywordToCompiled(typeKeyword);
            }
            else
            {
                this.AppendNextIdentifierToCompiled();
            }


            this.AppendNextIdentifierToCompiled();

            // Replace with Recursion
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
            string statementsTag = Tags.Statements;

            this.compiled.Append(statementsTag.ConstructOpeningTag());

            TokenType currentTokenType = this.tokenizer.TokenType();

            if (currentTokenType != TokenType.Keyword)
            {
                throw new UnexpectedTokenTypeException(TokenType.Keyword, currentTokenType);
            }

            if (!this.tokenizer.Keyword().IsBeginningOfStatement())
            {
                throw new Exception("Current Token is not a valid beginning of a statement");
            }

            this.CompileStatement();

            this.compiled.Append(statementsTag.ConstructClosingTag());
        }

        // TODO: We've made sure that we're working with a Keyword
        // NOTE: Be careful with recursion
        private void CompileStatement()
        {
            switch (this.tokenizer.Keyword())
            {
                case Keyword.Let:
                    this.CompileLet();
                    break;
                case Keyword.If:
                    this.CompileIf();
                    break;
                case Keyword.While:
                    this.CompileWhile();
                    break;
                case Keyword.Do:
                    this.CompileDo();
                    break;
                case Keyword.Return:
                    this.CompileReturn();
                    break;
                default:
                    // Introduce Unexpected Keyword exception
                    throw new Exception();
            }

            if (this.tokenizer.TokenType() == TokenType.Keyword && this.tokenizer.Keyword().IsBeginningOfStatement())
            {
                this.CompileStatement();
            }
        }

        public void CompileDo()
        {
            // do subroutineCall;

            string doStatement = Statements.Do;

            this.compiled.Append(doStatement.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.Do);

            this.CompileSubroutineCall();

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(doStatement.ConstructClosingTag());
        }

        private void CompileSubroutineCall()
        {
            // subroutineName '(' expressionList ')' | (className | varName) '.' subroutineName '(' expressionList ')'
            // NOTE: all of subroutineName, className and varName are identifiers
            this.AppendNextIdentifierToCompiled();

            if (this.tokenizer.TokenType() != TokenType.Symbol)
            {
                throw new UnexpectedTokenTypeException(TokenType.Symbol, tokenizer.TokenType());
            }

            string currentToken = this.tokenizer.GetCurrentToken();

            switch (currentToken)
            {
                case Symbols.LeftParenthesis:
                    this.CompileExpressionListInSubroutineCall();
                    break;
                case Symbols.Dot:
                    this.AppendTokenToCompiled(Symbols.Dot, TokenType.Symbol);
                    // NOTE: Recursive call, be careful with this invocation, maybe it is required only once since this can be easily broken?
                    this.CompileSubroutineCall();
                    break;
                default:
                    throw new Exception("Expected either '(' or a '.'");
            }
        }

        private void CompileExpressionListInSubroutineCall()
        {
            this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileExpressionList();

            this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);
        }

        public void CompileLet()
        {
            // 'let' varName ('[' expression ']')? '=' expression ';'

            string letStatement = Statements.Let;

            this.compiled.Append(letStatement.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.Let);

            this.AppendNextIdentifierToCompiled();

            if (this.tokenizer.TokenType() == TokenType.Symbol)
            {
                this.AppendTokenToCompiled(Symbols.LeftSquareBracket, TokenType.Symbol);

                this.CompileExpression();

                this.AppendTokenToCompiled(Symbols.RightSquareBracket, TokenType.Symbol);
            }

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

            // TODO: alternative idea
            //if (this.tokenizer this.tokenizer.GetCurrentToken() != Symbols.Semicolon)
            //{
            //    // We assume that anything other than a semicolon is an expression
            //    this.CompileExpression();
            //}

            if (this.IsNextTokenTheBeginningOfExpression())
            {
                this.CompileExpression();
            }

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(returnStatement.ConstructClosingTag());
        }

        public void CompileIf()
        {
            string ifStatement = Statements.If;

            this.compiled.Append(ifStatement.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.If);

            this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileExpression();

            this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);

            this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileStatements();

            this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            if (this.tokenizer.TokenType() == TokenType.Keyword && this.tokenizer.Keyword() == Keyword.Else)
            {
                this.CompileElse();
            }

            this.compiled.Append(ifStatement.ConstructClosingTag());
        }

        private void CompileElse()
        {
            this.AppendKeywordToCompiled(Keyword.Else);

            this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileStatements();

            this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);
        }

        public void CompileExpression()
        {
            string expressionTag = Tags.Expression;

            this.compiled.Append(expressionTag.ConstructOpeningTag());

            this.CompileExpressionCore();

            this.compiled.Append(expressionTag.ConstructClosingTag());

            return;
        }

        private void CompileExpressionCore()
        {
            this.CompileTerm();

            TokenType currentTokenType = this.tokenizer.TokenType();
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentTokenType == TokenType.Symbol && currentToken.IsOp())
            {
                this.AppendTokenToCompiled(currentToken, TokenType.Symbol);

                // NOTE: Recursive Call
                this.CompileExpressionCore();
            }
        }

        public void CompileTerm()
        {
            string termTag = Tags.Term;

            this.compiled.Append(termTag.ConstructOpeningTag());

            TokenType currentTokenType = this.tokenizer.TokenType();
            string token = this.tokenizer.GetCurrentToken();

            switch (currentTokenType)
            {
                case TokenType.IntegerConstant:
                case TokenType.StringConstant:
                    this.AppendTokenToCompiled(token, currentTokenType);
                    break;
                case TokenType.Keyword:
                    this.HandleKeywordInTerm(this.tokenizer.Keyword());
                    break;
                case TokenType.Symbol:
                    this.HandleSymbolInTerm();
                    break;
                case TokenType.Identifier:
                    this.HandleIdentifierInTerm();
                    break;
                default:
                    throw new Exception("");
            }

            this.compiled.Append(termTag.ConstructClosingTag());
        }

        private void HandleKeywordInTerm(Keyword keyword)
        {
            if (!keyword.IsKeywordConstant())
            {
                // TODO: UnexpectedKeywordException
                throw new Exception("");
            }

            // TODO: Should we append keywordConstant node or should we keep it as keyword only? 
            this.AppendTokenToCompiled(LexicalElements.ReverseKeywordMap[keyword], TokenType.Keyword);
        }


        private void HandleSymbolInTerm()
        {
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentToken.IsUnaryOp())
            {
                this.AppendTokenToCompiled(currentToken, TokenType.Symbol);

                // Recursive call
                this.CompileTerm();

                return;
            }

            this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileExpression();

            this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);
        }

        private void HandleIdentifierInTerm()
        {
            // varName | varName ‘[‘ expression ‘]’ | subroutineCall
            this.AppendNextIdentifierToCompiled();

            TokenType currentTokenType = this.tokenizer.TokenType();

            if (currentTokenType != TokenType.Symbol)
            {
                // TODO: We probably don't need to throw an exception here since cutting the program would be enough
                return;
            }

            string currentToken = this.tokenizer.GetCurrentToken();
            char symbol = this.tokenizer.Symbol();

            if (symbol == LexicalElements.SymbolMap[Symbols.LeftSquareBracket])
            {
                this.AppendTokenToCompiled(Symbols.LeftSquareBracket, TokenType.Symbol);

                this.CompileExpression();

                this.AppendTokenToCompiled(Symbols.RightSquareBracket, TokenType.Symbol);
            }

            if (symbol == LexicalElements.SymbolMap[Symbols.LeftParenthesis] || symbol == LexicalElements.SymbolMap[Symbols.Dot])
            {
                // TODO: This switch can become its own expression
                switch (currentToken)
                {
                    case Symbols.LeftParenthesis:
                        this.CompileExpressionListInSubroutineCall();
                        break;
                    case Symbols.Dot:
                        this.AppendTokenToCompiled(Symbols.Dot, TokenType.Symbol);
                        // NOTE: Recursive call, be careful with this invocation, maybe it is required only once since this can be easily broken?
                        this.CompileSubroutineCall();
                        break;
                    default:
                        throw new Exception("Expected either '(' or a '.'");
                }
            }

            return;
        }

        public void CompileExpressionList()
        {
            // (expression (',' expression)*)?

            string expressionListTag = Tags.ExpressionList;

            this.compiled.Append(expressionListTag.ConstructOpeningTag());

            if (this.IsNextTokenTheBeginningOfExpression())
            {
                this.CompileExpression();

                this.CompileExpressionInExpressionList();
            }

            this.compiled.Append(expressionListTag.ConstructClosingTag());

            return;
        }

        private void CompileExpressionInExpressionList()
        {
            TokenType currentTokenType = this.tokenizer.TokenType();
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentTokenType == TokenType.Symbol && currentToken == Symbols.Comma)
            {
                this.AppendTokenToCompiled(currentToken, currentTokenType);
                this.CompileExpression();
                this.CompileExpressionList();
            }

            return;
        }

        public string Close()
        {
            return this.compiled.ToString();
        }

        private string AssertNextTokenIsOfType(TokenType expectedTokenType)
        {
            if (!this.tokenizer.HasMoreTokens())
            {
                throw new Exception("Unexpected end of tokenizer, no tokens left.");
            }

            TokenType currentTokenType = this.tokenizer.TokenType();

            if (currentTokenType != expectedTokenType)
            {
                throw new UnexpectedTokenTypeException(expectedTokenType, currentTokenType);
            }

            string token = this.tokenizer.GetCurrentToken();

            //this.tokenizer.Advance();

            return token;
        }

        private void Eat(string expectedToken)
        {
            if (!this.tokenizer.HasMoreTokens())
            {
                // TODO: Maybe throw?
                return;
            }

            string currentToken = this.tokenizer.GetCurrentToken();

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
            string token = this.AssertNextTokenIsOfType(TokenType.Identifier);

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
                TokenType.IntegerConstant => token.ConstructIntegerConstantNode(),
                TokenType.StringConstant => token.ConstructStringConstantNode(),
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

        private bool IsNextTokenTheBeginningOfExpression()
        {
            return this.IsNextTokenTerm();
        }

        private bool IsNextTokenTerm()
        {
            TokenType currentTokenType = this.tokenizer.TokenType();

            string currentToken = this.tokenizer.GetCurrentToken();

            bool startsWithLeftParenthesis = currentTokenType == TokenType.Keyword && currentToken == Symbols.LeftParenthesis;
            bool isKeywordConstant = currentTokenType == TokenType.Keyword && this.tokenizer.Keyword().IsKeywordConstant();
            bool isUnaryOp = currentTokenType == TokenType.Symbol && currentToken.IsUnaryOp();

            return currentTokenType == TokenType.IntegerConstant
                || currentTokenType == TokenType.StringConstant
                || currentTokenType == TokenType.Identifier
                || startsWithLeftParenthesis
                || isKeywordConstant
                || isUnaryOp;
        }
    }
}

