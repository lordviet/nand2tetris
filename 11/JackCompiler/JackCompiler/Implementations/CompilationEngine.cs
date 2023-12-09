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

        private string? className;
        private string? subroutineName;

        private int ifLabelIndex;
        private int whileLabelIndex;

        public CompilationEngine(IJackTokenizer tokenizer, ISymbolTable symbolTable, IVMWriter writer)
        {
            this.tokenizer = tokenizer;
            this.symbolTable = symbolTable;
            this.writer = writer;

            this.compiled = new StringBuilder();

            this.ifLabelIndex = 0;
            this.whileLabelIndex = 0;

            this.CompileClass();
        }

        // 'class' className '{' classVarDec* subroutineDec* '}'
        public void CompileClass()
        {
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Class]);

            // Save the classname for later usage in the symbol table
            string className = this.tokenizer.GetCurrentToken();
            this.className = className;

            this.tokenizer.Advance();

            this.Eat(Symbols.LeftCurlyBrace);
            this.CompileClassVarDecInClass();
            this.CompileSubroutineDecInClass();
            this.Eat(Symbols.RightCurlyBrace);
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

        // ('static' | 'field') type varName (', ' varName)* ';'
        public void CompileClassVarDec()
        {
            this.CheckIfCurrentTokenIsAmongExpectedKeywords(new Keyword[] { Keyword.Static, Keyword.Field });

            Keyword currentKeyword = this.tokenizer.Keyword();

            this.tokenizer.Advance();

            string type = this.CompileType();
            string varName = this.tokenizer.GetCurrentToken();

            IdentifierKind kind = currentKeyword.ToIdentifierKind();

            this.symbolTable.Define(varName, type, kind);
            this.tokenizer.Advance();

            this.CompileCommaSeparatedVarNames(type, kind);
            this.Eat(Symbols.Semicolon);
        }

        private void CompileCommaSeparatedVarNames(string parentType, IdentifierKind parentIdentifierKind)
        {
            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.Eat(Symbols.Comma);

                string varName = this.tokenizer.GetCurrentToken();
                this.symbolTable.Define(varName, parentType, parentIdentifierKind);
                this.tokenizer.Advance();

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

        // ('constructor' | 'function' | 'method') ('void' | type) subroutineName '(' paramList ')' subroutineBody
        public void CompileSubroutine()
        {
            this.ifLabelIndex = 0;
            this.whileLabelIndex = 0;

            this.symbolTable.StartSubroutine();

            this.CheckIfCurrentTokenIsAmongExpectedKeywords(new Keyword[] { Keyword.Constructor, Keyword.Function, Keyword.Method });
            Keyword subroutineKeyword = this.tokenizer.Keyword();

            if (subroutineKeyword == Keyword.Method)
            {
                this.symbolTable.Define(name: "this", type: this.className ?? string.Empty, IdentifierKind.Argument);
            }

            this.tokenizer.Advance();

            TokenType currentToken = this.tokenizer.TokenType();

            if (currentToken == TokenType.Keyword && this.tokenizer.Keyword() == Keyword.Void)
            {
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


            this.Eat(Symbols.LeftParenthesis);
            this.CompileParameterList();
            this.Eat(Symbols.RightParenthesis);

            this.CompileSubroutineBody(subroutineKeyword);
        }

        // '{' varDec* statements '}'
        private void CompileSubroutineBody(Keyword subroutineKeyword)
        {
            this.Eat(Symbols.LeftCurlyBrace);
            this.CompileVarDecInSubroutineBody();
            this.WriteSubroutineDec(subroutineKeyword);
            this.CompileStatements();
            this.Eat(Symbols.RightCurlyBrace);
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
            }
            else
            {
                this.AssertNextTokenIsOfType(TokenType.Identifier);
                this.tokenizer.Advance();
            }

            string paramName = this.tokenizer.GetCurrentToken();

            this.symbolTable.Define(paramName, type, IdentifierKind.Argument);

            this.AssertNextTokenIsOfType(TokenType.Identifier);
            this.tokenizer.Advance();

            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Comma])
            {
                this.Eat(Symbols.Comma);

                if (this.tokenizer.TokenType() != TokenType.Keyword)
                {
                    throw new UnexpectedTokenTypeException(TokenType.Keyword, this.tokenizer.TokenType());
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
            }
            else
            {
                this.AssertNextTokenIsOfType(TokenType.Identifier);
            }

            this.tokenizer.Advance();

            return type;
        }

        // var type varName (', ' varName)* ';'
        public void CompileVarDec()
        {
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

            this.tokenizer.Advance();

            string type = this.CompileType();
            string varName = this.tokenizer.GetCurrentToken();
            this.symbolTable.Define(varName, type, IdentifierKind.Var);

            this.tokenizer.Advance();

            this.CompileCommaSeparatedVarNames(type, IdentifierKind.Var);

            this.Eat(Symbols.Semicolon);
        }

        public void CompileStatements()
        {
            TokenType currentTokenType = this.tokenizer.TokenType();

            // To compile a statement we need it to be a keyword that marks the beginning of a statement
            if (currentTokenType != TokenType.Keyword || !this.tokenizer.Keyword().IsBeginningOfStatement())
            {
                return;
            }

            this.CompileStatement();
        }

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

        // do subroutineCall;
        public void CompileDo()
        {
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Do]);
            this.CompileSubroutineCall();
            this.Eat(Symbols.Semicolon);

            this.compiled.Append(this.writer.WritePop(Segment.Temp, 0));
        }

        // subroutineName '(' expressionList ')' | (className | varName) '.' subroutineName '(' expressionList ')'
        // NOTE: all of subroutineName, className and varName are identifiers
        private void CompileSubroutineCall()
        {
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
                    this.CompileDotSymbolInSubroutineCall(previousIdentifierToken);
                    break;
                default:
                    throw new Exception($"Expected either a '{Symbols.LeftParenthesis}' or a '{Symbols.Dot}' when handling subroutine call.");
            }
        }

        private void CompileExpressionListInSubroutineCall(string subroutineNameToken)
        {
            this.compiled.Append(this.writer.WritePush(Segment.Pointer, 0));

            this.Eat(Symbols.LeftParenthesis);

            // NOTE: Add 1 for the this pointer
            int expressionCount = this.CompileExpressionList() + 1;

            this.Eat(Symbols.RightParenthesis);

            string subroutineName = $"{this.className}.{subroutineNameToken}";
            this.compiled.Append(this.writer.WriteCall(subroutineName, expressionCount));
        }

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

                this.compiled.Append(this.writer.WritePush(objectKind.ToSegment(), objectIndex));
                symbolTableEntryName = $"{objectType}.{subroutineName}";
            }

            this.Eat(Symbols.LeftParenthesis);

            arguments += this.CompileExpressionList();

            this.Eat(Symbols.RightParenthesis);

            this.compiled.Append(this.writer.WriteCall(symbolTableEntryName, arguments));
        }

        // 'let' varName ('[' expression ']')? '=' expression ';'
        public void CompileLet()
        {
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Let]);

            string varName = this.AssertNextTokenIsOfType(TokenType.Identifier);
            this.tokenizer.Advance();

            IdentifierKind kind = this.symbolTable.KindOf(varName);
            int index = this.symbolTable.IndexOf(varName);

            bool complexVariableAccessor = false;

            if (this.tokenizer.TokenType() == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.LeftSquareBracket])
            {
                complexVariableAccessor = true;

                this.Eat(Symbols.LeftSquareBracket);
                this.CompileExpression();
                this.Eat(Symbols.RightSquareBracket);

                // NOTE: Push array variable and base address to the Stack
                this.compiled.Append(this.writer.WritePush(kind.ToSegment(), index));
                this.compiled.Append(this.writer.WriteArithmetic(Command.Add));
            }

            this.Eat(Symbols.EqualitySign);
            this.CompileExpression();
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
            string whileLabel = this.ConstructLabel("WHILE_EXP", this.whileLabelIndex);
            string endLabel = this.ConstructLabel("WHILE_END", this.whileLabelIndex);

            this.whileLabelIndex++;

            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.While]);
            this.compiled.Append(this.writer.WriteLabel(whileLabel));

            this.Eat(Symbols.LeftParenthesis);
            this.CompileExpression();
            this.Eat(Symbols.RightParenthesis);

            this.compiled.Append(this.writer.WriteArithmetic(Command.Not));
            this.compiled.Append(this.writer.WriteIf(endLabel));

            this.Eat(Symbols.LeftCurlyBrace);
            this.CompileStatements();
            this.Eat(Symbols.RightCurlyBrace);

            this.compiled.Append(this.writer.WriteGoto(whileLabel));
            this.compiled.Append(this.writer.WriteLabel(endLabel));
        }

        public void CompileReturn()
        {
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Return]);

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
            this.compiled.Append(this.writer.WriteReturn());
        }

        public void CompileIf()
        {
            string ifTrueLabel = this.ConstructLabel("IF_TRUE", this.ifLabelIndex);
            string ifFalseLabel = this.ConstructLabel("IF_FALSE", this.ifLabelIndex);
            string endLabel = this.ConstructLabel("IF_END", this.ifLabelIndex);

            this.ifLabelIndex++;

            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.If]);

            this.Eat(Symbols.LeftParenthesis);
            this.CompileExpression();
            this.Eat(Symbols.RightParenthesis);

            this.compiled.Append(this.writer.WriteIf(ifTrueLabel));
            this.compiled.Append(this.writer.WriteGoto(ifFalseLabel));
            this.compiled.Append(this.writer.WriteLabel(ifTrueLabel));

            this.Eat(Symbols.LeftCurlyBrace);
            this.CompileStatements();
            this.Eat(Symbols.RightCurlyBrace);

            if (this.tokenizer.TokenType() == TokenType.Keyword && this.tokenizer.Keyword() == Keyword.Else)
            {
                this.compiled.Append(this.writer.WriteGoto(endLabel));
                this.compiled.Append(this.writer.WriteLabel(ifFalseLabel));

                this.CompileElse();

                this.compiled.Append(this.writer.WriteLabel(endLabel));
            }
            else
            {
                this.compiled.Append(this.writer.WriteLabel(ifFalseLabel));
            }
        }

        private void CompileElse()
        {
            this.Eat(LexicalElements.ReverseKeywordMap[Keyword.Else]);

            this.Eat(Symbols.LeftCurlyBrace);
            this.CompileStatements();
            this.Eat(Symbols.RightCurlyBrace);
        }

        public void CompileExpression()
        {
            this.CompileTerm();

            TokenType currentTokenType = this.tokenizer.TokenType();
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentTokenType == TokenType.Symbol && currentToken.IsOp())
            {
                string compiledOp = this.HandleOpInTerm(this.tokenizer.Symbol());

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
            TokenType currentTokenType = this.tokenizer.TokenType();

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
            string stringConstant = this.tokenizer.GetCurrentToken().Trim('"');

            this.compiled.Append(this.writer.WritePush(Segment.Constant, stringConstant.Length));
            this.compiled.Append(this.writer.WriteCall(OS.String.New, 1));

            for (int i = 0; i < stringConstant.Length; i++)
            {
                this.compiled.Append(this.writer.WritePush(Segment.Constant, stringConstant[i]));
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
        }


        private void HandleSymbolInTerm()
        {
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentToken.IsUnaryOp())
            {
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

            this.Eat(Symbols.LeftParenthesis);
            this.CompileExpression();
            this.Eat(Symbols.RightParenthesis);
        }

        // varName | varName ‘[‘ expression ‘]’ | subroutineCall
        private void HandleIdentifierInTerm()
        {
            string currentIdentifier = this.AssertNextTokenIsOfType(TokenType.Identifier);

            IdentifierKind varKind = symbolTable.KindOf(currentIdentifier);
            int varNameIndex = symbolTable.IndexOf(currentIdentifier);

            this.tokenizer.Advance();

            TokenType currentTokenType = this.tokenizer.TokenType();

            // NOTE: If the current token type is not a symbol, it must be a varName
            // NOTE: If the current token type is a symbol and it is a ")", it must be 
            string currentToken = this.tokenizer.GetCurrentToken();

            if (currentTokenType == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.LeftSquareBracket])
            {
                this.Eat(Symbols.LeftSquareBracket);
                this.CompileExpression();
                this.Eat(Symbols.RightSquareBracket);

                // NOTE: Push array variable and its base address to the stack
                this.compiled.Append(this.writer.WritePush(varKind.ToSegment(), varNameIndex));

                // NOTE: Take into consideration the offset
                this.compiled.Append(this.writer.WriteArithmetic(Command.Add));

                this.compiled.Append(this.writer.WritePop(Segment.Pointer, 1));
                this.compiled.Append(this.writer.WritePush(Segment.That, 0));

                return;
            }

            if (currentTokenType == TokenType.Symbol && this.tokenizer.Symbol() == LexicalElements.SymbolMap[Symbols.Dot])
            {
                this.HandleSubroutineCallSymbol(currentIdentifier, currentToken);
                return;
            }

            this.compiled.Append(this.writer.WritePush(varKind.ToSegment(), varNameIndex));

            return;
        }

        // (expression (',' expression)*)?
        public int CompileExpressionList()
        {
            int expressionCount = 0;

            if (this.IsNextTokenTheBeginningOfExpression())
            {
                this.CompileExpression();
                expressionCount++;

                expressionCount += this.CompileExpressionInExpressionList();
            }

            return expressionCount;
        }

        private int CompileExpressionInExpressionList()
        {
            TokenType currentTokenType = this.tokenizer.TokenType();
            string currentToken = this.tokenizer.GetCurrentToken();

            int expressionCount = 0;

            if (currentTokenType == TokenType.Symbol && currentToken == Symbols.Comma)
            {
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

        private string ConstructLabel(string labelName, int index)
        {
            return $"{labelName}{index}";
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

