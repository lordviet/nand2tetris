using JackAnalyzer.Contracts;
using JackAnalyzer.Extensions;

namespace JackAnalyzer.Implementations
{
    public class CompilationEngine : ICompilationEngine
    {
        IJackTokenizer tokenizer;

        public CompilationEngine(IJackTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public void CompileClass()
        {
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

            this.Eat("while");

            this.Eat("(");

            "(".ConstructSymbolNode();

            this.CompileExpression();

            this.Eat(")");

            this.Eat("{");

            this.CompileStatements();

            this.Eat("}");

            throw new NotImplementedException();
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
            //if (tokenizer.T currentToken != expectedToken)
            //{
            //    throw new Exception("Unexpected token ...");
            //}

            // TODO: Advance

            tokenizer.Advance();

            return;
        }
    }
}

