using System.Text;
using JackCompiler.Contracts;
using JackCompiler.Enums;
using JackCompiler.Exceptions;
using JackCompiler.Extensions;
using static JackCompiler.Constants;

namespace JackCompiler.Implementations
{
    public class CompilationEngine : ICompilationEngine
    {
        private readonly IJackTokenizer tokenizer;
        private readonly ISymbolTable symbolTable;

        private readonly StringBuilder compiled;

        private string? className;

        public CompilationEngine(IJackTokenizer tokenizer, ISymbolTable symbolTable, bool compileClass = true)
        {
            this.tokenizer = tokenizer;
            this.symbolTable = symbolTable;

            this.compiled = new StringBuilder();

            if (compileClass)
            {
                this.CompileClass();
            }
        }

        public void CompileClass()
        {
            // 'class' className '{' classVarDec* subroutineDec* '}'

            string classKeyword = LexicalElements.ReverseKeywordMap[Keyword.Class];

            this.compiled.Append(classKeyword.ConstructOpeningTag());

            this.AppendKeywordToCompiled(Keyword.Class);

            // Save the classname for later usage in the symbol table
            string className = this.tokenizer.GetCurrentToken();
            this.className = className;

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

            Keyword currentKeyword = this.tokenizer.Keyword();

            this.AppendKeywordToCompiled(this.tokenizer.Keyword());

            string type = this.CompileType();

            string varName = this.tokenizer.GetCurrentToken();

            IdentifierKind kind = currentKeyword.ToIdentifierKind();

            this.symbolTable.Define(varName, type, kind);

            this.AppendNextIdentifierToCompiled();

            this.CompileCommaSeparatedVarNames(type, kind);

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(classVarDecTag.ConstructClosingTag());
        }

        private void CompileCommaSeparatedVarNames(string parentType, IdentifierKind parentIdentifierKind)
        {
            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);

                string varName = this.tokenizer.GetCurrentToken();
                this.symbolTable.Define(varName, parentType, parentIdentifierKind);

                this.AppendNextIdentifierToCompiled();
                this.CompileCommaSeparatedVarNames(parentType, parentIdentifierKind);
            }

            return;
        }


        // TODO: Overload to compile code until the subroutine symbol table is done for
        private void CompileCommaSeparatedVarNames()
        {
            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);

                this.AppendNextIdentifierToCompiled();
                this.CompileCommaSeparatedVarNames();
            }

            return;
        }

        private void CheckIfCurrentTokenIsAmongExpectedKeywords(Keyword[] expectedKeywords)
        {
            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword)
            {
                throw new UnexpectedTokenTypeException(TokenType.Keyword, currentToken);
            }

            Keyword currentKeyword = this.tokenizer.Keyword();

            if (!expectedKeywords.Contains(currentKeyword))
            {
                throw new Exception($"Expected one of the following keywords: {string.Join(", ", expectedKeywords)}, but found: {currentKeyword}");
            }
        }

        public void CompileSubroutine()
        {
            this.symbolTable.StartSubroutine();

            this.symbolTable.Define(name: "this", type: this.className ?? string.Empty, IdentifierKind.Argument);

            // ('constructor' | 'function' | 'method') ('void' | type) subroutineName '(' paramList ')' subroutineBody

            string subroutineDecTag = Tags.SubroutineDec;

            this.compiled.Append(subroutineDecTag.ConstructOpeningTag());

            this.CheckIfCurrentTokenIsAmongExpectedKeywords(new Keyword[] { Keyword.Constructor, Keyword.Function, Keyword.Method });
            this.AppendKeywordToCompiled(this.tokenizer.Keyword());

            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken == TokenType.Keyword && this.tokenizer.Keyword() == Keyword.Void)
            {
                this.AppendTokenToCompiled(this.tokenizer.GetCurrentToken(), TokenType.Keyword);
            }
            else
            {
                this.CompileType();
            }

            // subroutineName
            this.AppendNextIdentifierToCompiled();

            this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileParameterList();

            this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);

            this.CompileSubroutineBody();

            this.compiled.Append(subroutineDecTag.ConstructClosingTag());
        }

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

            if (keyword != Keyword.Var)
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

        private void CompileParameterListInner()
        {
            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword)
            {
                return;
            }

            Keyword typeKeyword = this.tokenizer.Keyword();

            this.EnsureKeywordIsType(typeKeyword);

            // Saving the value of the type as a string to be passed in the symbol table
            string type = this.tokenizer.GetCurrentToken();

            this.AppendKeywordToCompiled(typeKeyword);

            string paramName = this.tokenizer.GetCurrentToken();

            this.symbolTable.Define(paramName, type, IdentifierKind.Argument);

            this.AppendNextIdentifierToCompiled();

            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);

                if (this.tokenizer.TokenType() != TokenType.Keyword)
                {
                    throw new Exception("A comma in the parameter list should be followed by a keyword.");
                }
            }

            this.CompileParameterListInner();
        }

        private string CompileType()
        {
            string type = this.tokenizer.GetCurrentToken();

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

            return type;
        }

        public void CompileVarDec()
        {
            // var type varName (', ' varName)* ';'

            string varDecTag = Tags.VarDec;

            this.compiled.Append(varDecTag.ConstructOpeningTag());

            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword)
            {
                throw new UnexpectedTokenTypeException(TokenType.Keyword, currentToken);
            }

            Keyword currentKeyword = this.tokenizer.Keyword();

            if (currentKeyword != Keyword.Var)
            {
                throw new UnexpectedKeywordException(Keyword.Var, currentKeyword);
            }

            this.AppendKeywordToCompiled(currentKeyword);

            string type = this.CompileType();

            string varName = this.tokenizer.GetCurrentToken();

            // TODO: Hard-coded identifier kind, might also be local?
            this.symbolTable.Define(varName, type, IdentifierKind.Var);

            this.AppendNextIdentifierToCompiled();

            this.CompileCommaSeparatedVarNames(type, IdentifierKind.Var);

            this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.compiled.Append(varDecTag.ConstructClosingTag());
        }

        public void CompileStatements()
        {
            string statementsTag = Tags.Statements;

            this.compiled.Append(statementsTag.ConstructOpeningTag());

            TokenType currentTokenType = this.tokenizer.TokenType();

            // To compile a statement we need it to be a keyword that marks the beginning of a statement
            if (currentTokenType != TokenType.Keyword || !this.tokenizer.Keyword().IsBeginningOfStatement())
            {
                this.compiled.Append(statementsTag.ConstructClosingTag());
                return;
            }

            this.CompileStatement();

            this.compiled.Append(statementsTag.ConstructClosingTag());
        }

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
                    throw new UnexpectedKeywordException();
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

            this.HandleSubroutineCallSymbol(currentToken);
        }

        private void HandleSubroutineCallSymbol(string currentToken)
        {
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
                    throw new Exception($"Expected either a '{Symbols.LeftParenthesis}' or a '{Symbols.Dot}' when handling subroutine call.");
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

            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.LeftSquareBracket])
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
                    throw new UnexpectedTokenTypeException($"Unexpected token type: {currentTokenType}");
            }

            this.compiled.Append(termTag.ConstructClosingTag());
        }

        private void HandleKeywordInTerm(Keyword keyword)
        {
            if (!keyword.IsKeywordConstant())
            {
                throw new UnexpectedKeywordException();
            }

            this.AppendTokenToCompiled(LexicalElements.ReverseKeywordMap[keyword], TokenType.Keyword);
        }


        private void HandleSymbolInTerm()
        {
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentToken.IsUnaryOp())
            {
                this.AppendTokenToCompiled(currentToken, TokenType.Symbol);

                // NOTE: Recursive call
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
                this.HandleSubroutineCallSymbol(currentToken);
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
                this.CompileExpressionInExpressionList();
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

            return token;
        }

        private void Eat(string expectedToken)
        {
            if (!this.tokenizer.HasMoreTokens())
            {
                return;
            }

            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentToken != expectedToken)
            {
                throw new UnexpectedTokenTypeException($"Expected token {expectedToken} but got {currentToken} instead.");
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
                TokenType.Identifier => ResolveIdentifierNodeConstruction(token),
                TokenType.IntegerConstant => token.ConstructIntegerConstantNode(),
                TokenType.StringConstant => token.ConstructStringConstantNode(),
                _ => throw new UnexpectedTokenTypeException()
            };

            this.compiled.Append(node);

            return;
        }

        private string ResolveIdentifierNodeConstruction(string token)
        {
            int symbolTableIndex = this.symbolTable.IndexOf(token);

            // TODO: I have some missing implementations for class list, subroutines and expressions
            if (symbolTableIndex == -1)
            {
                return token.ConstructIdentifierNode();
            }

            return token.ConstructIdentifierNodeEnhanced(this.symbolTable.TypeOf(token), this.symbolTable.KindOf(token), symbolTableIndex);
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

            bool startsWithLeftParenthesis = currentTokenType == TokenType.Symbol && currentToken == Symbols.LeftParenthesis;
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

