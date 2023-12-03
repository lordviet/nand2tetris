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
        private readonly IVMWriter writer;

        private readonly StringBuilder compiled;

        private int labelIndex;
        private string? className;
        private string? subroutineName;

        public CompilationEngine(IJackTokenizer tokenizer, ISymbolTable symbolTable, IVMWriter writer, bool compileClass = true)
        {
            this.tokenizer = tokenizer;
            this.symbolTable = symbolTable;
            this.writer = writer;

            this.compiled = new StringBuilder();

            this.labelIndex = 0;

            if (compileClass)
            {
                this.CompileClass();
            }
        }

        public void CompileClass()
        {
            // 'class' className '{' classVarDec* subroutineDec* '}'

            //string classKeyword = LexicalElements.ReverseKeywordMap[Keyword.Class];

            //this.compiled.Append(classKeyword.ConstructOpeningTag());

            //this.AppendKeywordToCompiled(Keyword.Class);
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Class]);

            // Save the classname for later usage in the symbol table
            string className = this.tokenizer.GetCurrentToken();
            this.className = className;

            //this.AppendNextIdentifierToCompiled();
            this.tokenizer.Advance();

            //this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);
            this.Eat(Symbols.LeftCurlyBrace);

            this.CompileClassVarDecInClass();

            this.CompileSubroutineDecInClass();

            //this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);
            this.Eat(Symbols.RightCurlyBrace);

            //this.compiled.Append(classKeyword.ConstructClosingTag());
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

        public void CompileClassVarDec()
        {
            //('static' | 'field') type varName (', ' varName)* ';'

            //string classVarDecTag = Tags.ClassVarDec;

            //this.compiled.Append(classVarDecTag.ConstructOpeningTag());

            this.CheckIfCurrentTokenIsAmongExpectedKeywords(new Keyword[] { Keyword.Static, Keyword.Field });

            Keyword currentKeyword = this.tokenizer.Keyword();

            //this.AppendKeywordToCompiled(this.tokenizer.Keyword());
            this.tokenizer.Advance();

            string type = this.CompileType();

            string varName = this.tokenizer.GetCurrentToken();

            IdentifierKind kind = currentKeyword.ToIdentifierKind();

            this.symbolTable.Define(varName, type, kind);

            //this.AppendNextIdentifierToCompiled();
            this.tokenizer.Advance();

            this.CompileCommaSeparatedVarNames(type, kind);

            //this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);
            this.Eat(Symbols.Semicolon);

            //this.compiled.Append(classVarDecTag.ConstructClosingTag());
        }

        private void CompileCommaSeparatedVarNames(string parentType, IdentifierKind parentIdentifierKind)
        {
            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                //this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);
                this.Eat(Symbols.Comma);

                string varName = this.tokenizer.GetCurrentToken();
                this.symbolTable.Define(varName, parentType, parentIdentifierKind);

                this.tokenizer.Advance();
                //this.AppendNextIdentifierToCompiled();
                this.CompileCommaSeparatedVarNames(parentType, parentIdentifierKind);
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

            //string subroutineDecTag = Tags.SubroutineDec;

            //this.compiled.Append(subroutineDecTag.ConstructOpeningTag());

            this.CheckIfCurrentTokenIsAmongExpectedKeywords(new Keyword[] { Keyword.Constructor, Keyword.Function, Keyword.Method });
            //this.AppendKeywordToCompiled(this.tokenizer.Keyword());
            Keyword subroutineKeyword = this.tokenizer.Keyword();
            this.tokenizer.Advance();

            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken == TokenType.Keyword && this.tokenizer.Keyword() == Keyword.Void)
            {
                // TODO: Compile void
                //this.AppendTokenToCompiled(this.tokenizer.GetCurrentToken(), TokenType.Keyword);
                this.tokenizer.Advance();
            }
            else
            {
                this.CompileType();
            }

            // subroutineName
            string subroutineName = this.tokenizer.GetCurrentToken();
            this.subroutineName = subroutineName;
            this.tokenizer.Advance();

            //this.AppendNextIdentifierToCompiled();

            this.Eat(Symbols.LeftParenthesis);
            //this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileParameterList();

            this.Eat(Symbols.RightParenthesis);
            //this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);

            this.CompileSubroutineBody(subroutineKeyword);

            //this.compiled.Append(subroutineDecTag.ConstructClosingTag());
        }

        private void CompileSubroutineBody(Keyword subroutineKeyword)
        {
            //string subroutineBodyTag = Tags.SubroutineBody;

            //this.compiled.Append(subroutineBodyTag.ConstructOpeningTag());

            // '{' varDec* statements '}'
            this.Eat(Symbols.LeftCurlyBrace);
            //this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileVarDecInSubroutineBody();

            this.WriteSubroutineDec(subroutineKeyword);

            this.CompileStatements();

            this.Eat(Symbols.RightCurlyBrace);
            //this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            //this.compiled.Append(subroutineBodyTag.ConstructClosingTag());
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

        private void WriteSubroutineDec(Keyword keyword)
        {
            if (this.className == null || this.subroutineName == null)
            {
                throw new Exception("Both class name and subroutine name must be present when constructing a function");
            }

            string functionName = this.className.ConstructFunctionName(this.subroutineName);
            int varCountOfCurrentFunction = this.symbolTable.VarCount(IdentifierKind.Var);

            string functionCommand = this.writer.WriteFunction(functionName, varCountOfCurrentFunction);
            this.compiled.Append(functionCommand);

            switch (keyword)
            {
                // Method with k arguments is compiled to a VM function with k + 1 arguments
                case Keyword.Method:
                    this.compiled.Append(this.writer.WritePush(Segment.Argument, 0));
                    this.compiled.Append(this.writer.WritePop(Segment.Pointer, 0));
                    return;
                // Constructor with k arguments is compiled to a VM function with k arguments
                case Keyword.Constructor:
                    this.compiled.Append(this.writer.WritePush(Segment.Constant, this.symbolTable.VarCount(IdentifierKind.Field)));
                    this.compiled.Append(this.writer.WriteCall(OS.Memory.Alloc, 1));
                    this.compiled.Append(this.writer.WritePop(Segment.Pointer, 0));
                    return;
                default:
                    return;
            }
        }

        // TODO: Double check this one?
        public void CompileParameterList()
        {
            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken != TokenType.Keyword && currentToken != TokenType.Identifier)
            {
                return;
            }

            // Saving the value of the type as a string to be passed in the symbol table
            string type = this.tokenizer.GetCurrentToken();

            if (currentToken == TokenType.Keyword)
            {
                Keyword typeKeyword = this.tokenizer.Keyword();

                this.EnsureKeywordIsType(typeKeyword);
                this.tokenizer.Advance();
                //this.AppendKeywordToCompiled(typeKeyword);
            }
            else
            {
                this.AssertNextTokenIsOfType(TokenType.Identifier);
                this.tokenizer.Advance();
                //this.AppendNextIdentifierToCompiled();
            }

            string paramName = this.tokenizer.GetCurrentToken();

            this.symbolTable.Define(paramName, type, IdentifierKind.Argument);

            this.AssertNextTokenIsOfType(TokenType.Identifier);
            this.tokenizer.Advance();
            //this.AppendNextIdentifierToCompiled();

            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.Eat(Symbols.Comma);
                //this.AppendTokenToCompiled(Symbols.Comma, TokenType.Symbol);

                if (this.tokenizer.TokenType() != TokenType.Keyword)
                {
                    throw new Exception("A comma in the parameter list should be followed by a keyword.");
                }
            }

            this.CompileParameterList();
        }

        private string CompileType()
        {
            string type = this.tokenizer.GetCurrentToken();

            if (this.tokenizer.TokenType() == TokenType.Keyword)
            {
                Keyword typeKeyword = this.tokenizer.Keyword();

                this.EnsureKeywordIsType(typeKeyword);

                //this.AppendKeywordToCompiled(typeKeyword);
            }
            else
            {
                this.AssertNextTokenIsOfType(TokenType.Identifier);
                //this.AppendNextIdentifierToCompiled();
            }

            this.tokenizer.Advance();

            return type;
        }

        public void CompileVarDec()
        {
            // var type varName (', ' varName)* ';'

            //string varDecTag = Tags.VarDec;

            //this.compiled.Append(varDecTag.ConstructOpeningTag());

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

            //this.AppendKeywordToCompiled(currentKeyword);
            this.tokenizer.Advance();

            string type = this.CompileType();

            string varName = this.tokenizer.GetCurrentToken();

            this.symbolTable.Define(varName, type, IdentifierKind.Var);

            //this.AppendNextIdentifierToCompiled();
            this.tokenizer.Advance();

            this.CompileCommaSeparatedVarNames(type, IdentifierKind.Var);

            //this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);
            this.Eat(Symbols.Semicolon);

            //this.compiled.Append(varDecTag.ConstructClosingTag());
        }

        public void CompileStatements()
        {
            //string statementsTag = Tags.Statements;

            //this.compiled.Append(statementsTag.ConstructOpeningTag());

            TokenType currentTokenType = this.tokenizer.TokenType();

            // To compile a statement we need it to be a keyword that marks the beginning of a statement
            if (currentTokenType != TokenType.Keyword || !this.tokenizer.Keyword().IsBeginningOfStatement())
            {
                //this.compiled.Append(statementsTag.ConstructClosingTag());
                return;
            }

            this.CompileStatement();

            //this.compiled.Append(statementsTag.ConstructClosingTag());
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

            //string doStatement = Statements.Do;

            //this.compiled.Append(doStatement.ConstructOpeningTag());

            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Do]);

            //this.AppendKeywordToCompiled(Keyword.Do);

            this.CompileSubroutineCall();

            this.Eat(Symbols.Semicolon);
            //this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            this.writer.WritePop(Segment.Temp, 0);
            //this.compiled.Append(doStatement.ConstructClosingTag());
        }

        private void CompileSubroutineCall()
        {
            // subroutineName '(' expressionList ')' | (className | varName) '.' subroutineName '(' expressionList ')'
            // NOTE: all of subroutineName, className and varName are identifiers

            //this.AppendNextIdentifierToCompiled();
            string currentIdentifier = this.AssertNextTokenIsOfType(TokenType.Identifier);
            this.tokenizer.Advance();

            if (this.tokenizer.TokenType() != TokenType.Symbol)
            {
                throw new UnexpectedTokenTypeException(TokenType.Symbol, tokenizer.TokenType());
            }

            string currentToken = this.tokenizer.GetCurrentToken();

            this.HandleSubroutineCallSymbol(currentIdentifier, currentToken);
        }

        private void HandleSubroutineCallSymbol(string previousIdentifierToken, string currentSymbolToken)
        {
            switch (currentSymbolToken)
            {
                case Symbols.LeftParenthesis:
                    this.CompileExpressionListInSubroutineCall(previousIdentifierToken);
                    break;
                case Symbols.Dot:
                    //this.Eat(Symbols.Dot);
                    //this.AppendTokenToCompiled(Symbols.Dot, TokenType.Symbol);
                    // NOTE: Recursive call, be careful with this invocation, maybe it is required only once since this can be easily broken?
                    //this.CompileSubroutineCall();
                    this.CompileDotSymbolInSubroutineCall(previousIdentifierToken);
                    break;
                default:
                    throw new Exception($"Expected either a '{Symbols.LeftParenthesis}' or a '{Symbols.Dot}' when handling subroutine call.");
            }
        }

        private void CompileExpressionListInSubroutineCall(string subroutineNameToken)
        {
            this.compiled.Append(this.writer.WritePush(Segment.Pointer, 0));

            //this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);
            this.Eat(Symbols.LeftParenthesis);

            int expressionCount = this.CompileExpressionList();

            //this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);
            this.Eat(Symbols.RightParenthesis);

            string subroutineName = $"{this.className}.{subroutineNameToken}";
            this.compiled.Append(this.writer.WriteCall(subroutineName, expressionCount));
        }

        //private void CompileExpressionListInSubroutineCallCore(string subroutineNameToken)
        //{
        //    this.Eat(Symbols.LeftParenthesis);

        //    int expressionCount = this.CompileExpressionList();

        //    this.Eat(Symbols.RightParenthesis);

        //    string subroutineName = $"{this.className}.{subroutineNameToken}";
        //    this.compiled.Append(this.writer.WriteCall(subroutineName, expressionCount));
        //}

        private void CompileDotSymbolInSubroutineCall(string objectName)
        {
            this.Eat(Symbols.Dot);

            int arguments = 0;

            string subroutineName = this.AssertNextTokenIsOfType(TokenType.Identifier);
            this.tokenizer.Advance();

            string? objectType = this.symbolTable.TypeOf(objectName);

            if (objectType is not null && LexicalElements.KeywordMap.ContainsKey(objectType))
            {
                throw new Exception("Object type in subroutine call must not be an in-built type!");
            }

            string symbolTableEntryName;

            if (objectType is null)
            {
                symbolTableEntryName = $"{objectName}.{subroutineName}";
            }
            else
            {
                arguments++;

                IdentifierKind objectKind = this.symbolTable.KindOf(objectName);
                int objectIndex = this.symbolTable.IndexOf(objectName);

                this.compiled.Append(writer.WritePush(objectKind.ToSegment(), objectIndex));
                symbolTableEntryName = $"{objectType}.{subroutineName}";
            }

            this.Eat(Symbols.LeftParenthesis);

            arguments += this.CompileExpressionList();

            this.Eat(Symbols.RightParenthesis);

            this.compiled.Append(this.writer.WriteCall(symbolTableEntryName, arguments));
        }

        public void CompileLet()
        {
            // 'let' varName ('[' expression ']')? '=' expression ';'

            //string letStatement = Statements.Let;

            //this.compiled.Append(letStatement.ConstructOpeningTag());

            // TODO: Newline for testing purposes
            //this.compiled.AppendLine();

            //this.AppendKeywordToCompiled(Keyword.Let);
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Let]);

            // TODO: Potentially make AppendNextIdentifierToCompiled return a string;
            string varName = this.AssertNextTokenIsOfType(TokenType.Identifier);
            this.tokenizer.Advance();

            IdentifierKind kind = this.symbolTable.KindOf(varName);
            int index = this.symbolTable.IndexOf(varName);

            bool complexVariableAccessor = false;

            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.LeftSquareBracket])
            {
                complexVariableAccessor = true;

                // NOTE: Push array variable and base address to the Stack
                this.compiled.Append(this.writer.WritePush(kind.ToSegment(), index));

                //this.AppendTokenToCompiled(Symbols.LeftSquareBracket, TokenType.Symbol);
                this.Eat(Symbols.LeftSquareBracket);

                this.CompileExpression();

                //this.AppendTokenToCompiled(Symbols.RightSquareBracket, TokenType.Symbol);
                this.Eat(Symbols.RightSquareBracket);

                this.compiled.Append(this.writer.WriteArithmetic(Command.Add));
            }

            //this.AppendTokenToCompiled(Symbols.EqualitySign, TokenType.Symbol);
            this.Eat(Symbols.EqualitySign);

            this.CompileExpression();

            //this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);
            this.Eat(Symbols.Semicolon);

            if (complexVariableAccessor)
            {
                this.compiled.Append(this.writer.WritePop(Segment.Temp, 0));
                this.compiled.Append(this.writer.WritePop(Segment.Pointer, 1));
                this.compiled.Append(this.writer.WritePush(Segment.Temp, 0));
                this.compiled.Append(this.writer.WritePop(Segment.That, 0));
            }
            else
            {
                this.compiled.Append(this.writer.WritePop(kind.ToSegment(), index));
            }
        }

        public void CompileWhile()
        {
            string whileLabel = this.ConstructLabel("WHILE");
            string continueLabel = this.ConstructLabel("CONTINUE_WHILE");

            //string whileStatement = Statements.While;

            //this.compiled.Append(whileStatement.ConstructOpeningTag());

            //this.AppendKeywordToCompiled(Keyword.While);
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.While]);

            this.compiled.Append(this.writer.WriteLabel(whileLabel));

            this.Eat(Symbols.LeftParenthesis);
            //this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);

            this.CompileExpression();

            this.Eat(Symbols.RightParenthesis);
            //this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);

            this.compiled.Append(this.writer.WriteArithmetic(Command.Not));
            this.compiled.Append(this.writer.WriteIf(continueLabel));

            this.Eat(Symbols.LeftCurlyBrace);
            //this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileStatements();

            this.Eat(Symbols.RightCurlyBrace);
            //this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            this.compiled.Append(this.writer.WriteGoto(whileLabel));
            this.compiled.Append(this.writer.WriteLabel(continueLabel));

            //this.compiled.Append(whileStatement.ConstructClosingTag());
        }

        public void CompileReturn()
        {
            // 'return' expression? ';'

            //string returnStatement = Statements.Return;

            //this.compiled.Append(returnStatement.ConstructOpeningTag());


            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Return]);
            //this.AppendKeywordToCompiled(Keyword.Return);

            if (this.IsNextTokenTheBeginningOfExpression())
            {
                this.CompileExpression();
            }
            else
            {
                // if there's no expression, we push 0 to the stack
                this.compiled.Append(this.writer.WritePush(Segment.Constant, 0));
            }

            this.Eat(Symbols.Semicolon);
            //this.AppendTokenToCompiled(Symbols.Semicolon, TokenType.Symbol);

            //this.compiled.Append(returnStatement.ConstructClosingTag());
            this.compiled.Append(this.writer.WriteReturn());
        }

        public void CompileIf()
        {
            string elseLabel = this.ConstructLabel("ELSE");
            string endLabel = this.ConstructLabel("END_IF");
            //string ifStatement = Statements.If;

            //this.compiled.Append(ifStatement.ConstructOpeningTag());

            //this.AppendKeywordToCompiled(Keyword.If);
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.If]);

            //this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);
            this.Eat(Symbols.LeftParenthesis);

            this.CompileExpression();

            //this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);
            this.Eat(Symbols.RightParenthesis);

            this.writer.WriteArithmetic(Command.Not);
            this.writer.WriteIf(elseLabel);

            this.Eat(Symbols.LeftCurlyBrace);
            //this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileStatements();

            this.Eat(Symbols.RightCurlyBrace);
            //this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);

            this.writer.WriteGoto(endLabel);
            this.writer.WriteLabel(elseLabel);

            if (this.tokenizer.TokenType() == TokenType.Keyword && this.tokenizer.Keyword() == Keyword.Else)
            {
                this.CompileElse();
            }

            this.writer.WriteLabel(endLabel);
            //this.compiled.Append(ifStatement.ConstructClosingTag());
        }

        private void CompileElse()
        {
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Else]);
            //this.AppendKeywordToCompiled(Keyword.Else);

            this.Eat(Symbols.LeftCurlyBrace);
            //this.AppendTokenToCompiled(Symbols.LeftCurlyBrace, TokenType.Symbol);

            this.CompileStatements();

            this.Eat(Symbols.RightCurlyBrace);
            //this.AppendTokenToCompiled(Symbols.RightCurlyBrace, TokenType.Symbol);
        }

        public void CompileExpression()
        {
            this.CompileTerm();

            TokenType currentTokenType = this.tokenizer.TokenType();
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentTokenType == TokenType.Symbol && currentToken.IsOp())
            {
                string compiledOp = HandleOpInTerm(this.tokenizer.Symbol());

                //this.AppendTokenToCompiled(currentToken, TokenType.Symbol);

                // NOTE: Recursive Call
                this.CompileExpression();

                this.compiled.Append(compiledOp);
            }
        }

        private string HandleOpInTerm(char symbol)
        {
            this.tokenizer.Advance();

            return symbol switch
            {
                '*' => this.writer.WriteCall(OS.Math.Multiply, OS.Math.ArithmeticOperationParameters),
                '/' => this.writer.WriteCall(OS.Math.Divide, OS.Math.ArithmeticOperationParameters),
                _ => this.writer.WriteArithmetic(LexicalElements.NonUnaryOpSymbolCommandMap[symbol])
            };
        }

        public void CompileTerm()
        {
            //string termTag = Tags.Term;

            //this.compiled.Append(termTag.ConstructOpeningTag());

            TokenType currentTokenType = this.tokenizer.TokenType();
            //string token = this.tokenizer.GetCurrentToken();

            switch (currentTokenType)
            {
                case TokenType.IntegerConstant:
                    this.HandleIntegerConstantInTerm();
                    break;
                case TokenType.StringConstant:
                    this.HandleStringConstantInTerm();
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

            //this.compiled.Append(termTag.ConstructClosingTag());
        }

        private void HandleIntegerConstantInTerm()
        {
            int integerConstant = this.tokenizer.IntegerValue();

            this.compiled.Append(this.writer.WritePush(Segment.Constant, integerConstant));
            this.tokenizer.Advance();
        }

        private void HandleStringConstantInTerm()
        {
            // TODO: avoid magic numbers
            string stringConstant = this.tokenizer.GetCurrentToken();

            this.compiled.Append(this.writer.WritePush(Segment.Constant, stringConstant.Length));
            this.compiled.Append(this.writer.WriteCall(OS.String.New, 1));

            for (int i = 0; i < stringConstant.Length; i++)
            {
                this.compiled.Append(this.writer.WritePush(Segment.Constant, stringConstant[i]));
                // TODO: double check the 2
                this.compiled.Append(this.writer.WriteCall(OS.String.AppendChar, 2));
            }

            this.tokenizer.Advance();
        }

        private void HandleKeywordInTerm(Keyword keyword)
        {
            if (!keyword.IsKeywordConstant())
            {
                throw new UnexpectedKeywordException();
            }

            switch (keyword)
            {
                case Keyword.True:
                    this.compiled.Append(this.writer.WritePush(Segment.Constant, 0));
                    this.compiled.Append(this.writer.WriteArithmetic(Command.Not));
                    break;
                case Keyword.False:
                case Keyword.Null:
                    this.compiled.Append(this.writer.WritePush(Segment.Constant, 0));
                    break;
                case Keyword.This:
                    this.compiled.Append(this.writer.WritePush(Segment.Pointer, 0));
                    break;
            }

            this.tokenizer.Advance();
            //this.AppendTokenToCompiled(LexicalElements.ReverseKeywordMap[keyword], TokenType.Keyword);
        }


        private void HandleSymbolInTerm()
        {
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentToken.IsUnaryOp())
            {
                //this.AppendTokenToCompiled(currentToken, TokenType.Symbol);
                this.tokenizer.Advance();

                // NOTE: Recursive call
                this.CompileTerm();

                this.compiled.Append(currentToken switch
                {
                    "-" => this.writer.WriteArithmetic(Command.Neg),
                    "~" => this.writer.WriteArithmetic(Command.Not),
                    _ => throw new Exception($"Unexpected token: {currentToken}")
                });


                return;
            }

            //this.AppendTokenToCompiled(Symbols.LeftParenthesis, TokenType.Symbol);
            this.Eat(Symbols.LeftParenthesis);

            this.CompileExpression();

            //this.AppendTokenToCompiled(Symbols.RightParenthesis, TokenType.Symbol);
            this.Eat(Symbols.RightParenthesis);
        }

        private void HandleIdentifierInTerm()
        {
            //this.AppendNextIdentifierToCompiled();

            // varName | varName ‘[‘ expression ‘]’ | subroutineCall
            string currentIdentifier = this.AssertNextTokenIsOfType(TokenType.Identifier);
            this.tokenizer.Advance();

            TokenType currentTokenType = this.tokenizer.TokenType();

            // NOTE: If the current token type is not a symbol, it must be a varName
            if (currentTokenType != TokenType.Symbol)
            {

                IdentifierKind varKind = symbolTable.KindOf(currentIdentifier);
                int varNameIndex = symbolTable.IndexOf(currentIdentifier);

                this.compiled.Append(this.writer.WritePush(varKind.ToSegment(), varNameIndex));

                return;
            }

            string currentToken = this.tokenizer.GetCurrentToken();
            char symbol = this.tokenizer.Symbol();

            if (symbol == LexicalElements.SymbolMap[Symbols.LeftSquareBracket])
            {
                // NOTE: Push array variable and its base address to the stack
                IdentifierKind varKind = symbolTable.KindOf(currentIdentifier);
                int varNameIndex = symbolTable.IndexOf(currentIdentifier);

                this.compiled.Append(this.writer.WritePush(varKind.ToSegment(), varNameIndex));

                //this.AppendTokenToCompiled(Symbols.LeftSquareBracket, TokenType.Symbol);
                this.Eat(Symbols.LeftSquareBracket);

                this.CompileExpression();

                //this.AppendTokenToCompiled(Symbols.RightSquareBracket, TokenType.Symbol);
                this.Eat(Symbols.RightSquareBracket);

                // NOTE: Take into consideration the offset
                this.compiled.Append(this.writer.WriteArithmetic(Command.Add));

                this.compiled.Append(this.writer.WritePop(Segment.Pointer, 1));
                this.compiled.Append(this.writer.WritePush(Segment.That, 0));
            }

            if (symbol == LexicalElements.SymbolMap[Symbols.LeftParenthesis] || symbol == LexicalElements.SymbolMap[Symbols.Dot])
            {
                this.HandleSubroutineCallSymbol(currentIdentifier, currentToken);
            }

            return;
        }

        public int CompileExpressionList()
        {
            // (expression (',' expression)*)?

            //string expressionListTag = Tags.ExpressionList;

            //this.compiled.Append(expressionListTag.ConstructOpeningTag());
            int expressionCount = 0;


            if (this.IsNextTokenTheBeginningOfExpression())
            {
                this.CompileExpression();
                expressionCount++;

                expressionCount += this.CompileExpressionInExpressionList();
            }

            //this.compiled.Append(expressionListTag.ConstructClosingTag());

            return expressionCount;
        }

        private int CompileExpressionInExpressionList()
        {
            TokenType currentTokenType = this.tokenizer.TokenType();
            string currentToken = this.tokenizer.GetCurrentToken();

            int expressionCount = 0;

            if (currentTokenType == TokenType.Symbol && currentToken == Symbols.Comma)
            {
                //this.AppendTokenToCompiled(currentToken, currentTokenType);
                this.tokenizer.Advance();

                this.CompileExpression();
                expressionCount++;

                expressionCount += this.CompileExpressionInExpressionList();
            }

            return expressionCount;
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

        // TODO: Possibly remove
        private void AppendNextIdentifierToCompiled()
        {
            string token = this.AssertNextTokenIsOfType(TokenType.Identifier);

            //this.AppendTokenToCompiled(token, TokenType.Identifier);
        }

        // TODO: Possibly remove
        private void AppendKeywordToCompiled(Keyword key)
        {
            string keyword = LexicalElements.ReverseKeywordMap[key];

            //this.AppendTokenToCompiled(keyword, TokenType.Keyword);

            return;
        }

        // TODO: Possibly remove
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

        private string ConstructLabel(string labelName)
        {
            string label = $"LABEL_{labelName}_{this.labelIndex}";

            this.labelIndex++;

            return label;
        }

        private string ResolveIdentifierNodeConstruction(string token)
        {
            int symbolTableIndex = this.symbolTable.IndexOf(token);

            // TODO: I have some missing implementations for class list, subroutines and expressions
            if (symbolTableIndex == -1)
            {
                return token.ConstructIdentifierNode();
            }

            return token.ConstructIdentifierNodeEnhanced(this.symbolTable.TypeOf(token) ?? string.Empty, this.symbolTable.KindOf(token), symbolTableIndex);
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

