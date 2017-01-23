//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Parser.CodeDom
{

    //----------------------------------
    //Exception handling mechanism
    //----------------------------------
    public class CodeThrowExceptionStatement : CodeStatement
    {
        CodeExpression toThrow;
        public CodeThrowExceptionStatement()
        { 
        }
        public CodeThrowExceptionStatement(CodeExpression toThrow)
        {
            this.toThrow = toThrow;
            AcceptChild(toThrow, CodeObjectRoles.CodeThrowExceptionStatement_Expression);
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeThrowExceptionStatement; }
        }
        public CodeExpression ToThrow
        {
            get
            {
                return toThrow;
            }

        }

    }
    /// <summary>
    /// Represents a try block with any number of catch clauses and, optionally, a finally block. 
    /// </summary>
    public class CodeTryCatchFinallyStatement : CodeStatement
    {
        CodeTryClause tryClause;
        List<CodeCatchClause> catchClauses;
        CodeFinallyClause finalyClause;

        public CodeTryCatchFinallyStatement(CodeBlockStatement tryStatements)
        {
            tryClause = new CodeTryClause(tryStatements);
            AcceptChild(tryClause, CodeObjectRoles.CodeTryCatchFinallyStatement_TryClause);
        }
        public CodeTryClause TryClause
        {
            get
            {
                return tryClause;
            }
        }

        public CodeFinallyClause FinallyClause
        {
            get
            {
                return finalyClause;
            }
            set
            {
                finalyClause = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeTryCatchFinallyStatement_FinallyClause);
                }
            }
        } 
        public void AddCatchClause(CodeCatchClause catchClause)
        {
            if (catchClauses == null)
            {
                catchClauses = new List<CodeCatchClause>();
            }

            catchClauses.Add(catchClause);
            AcceptChild(catchClause, CodeObjectRoles.CodeTryCatchFinallyStatement_CatchClause);

        }
        public IEnumerable<CodeCatchClause> GetCatchClauseIter()
        {
            if (catchClauses != null)
            {
                foreach (CodeCatchClause ct in catchClauses)
                {
                    yield return ct;
                }
            }
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeTryCatchFinallyStatement; }
        }
        public int CatchClauseCount
        {
            get
            {
                if (catchClauses != null)
                {
                    return catchClauses.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

    } 
    public class CodeCatchClause : CodeObject
    {
        CodeTypeReference catchExceptionType; 
        CodeNamedItem catchExceptionTypeName;
        CodeBlockStatement body;
        CodeSimpleName localName; 
        public CodeVariableDeclarationStatement compilerGenExceptionVarDeclStatement;
        public CodeCatchClause()
        {

        }
        public CodeCatchClause(CodeStatementCollection stms)
        {
            //this.Body = new CodeBlockStatement(stms);
        }
        public CodeCatchClause(CodeBlockStatement blockStmt)
        {
            this.Body = blockStmt;
        }
        public CodeBlockStatement Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;

            }
        } 
        public CodeTypeReference CatchExceptionType
        {
            get
            {
                return catchExceptionType;
            }
            set
            {
                catchExceptionType = value;
            }
        }

        public void SetCatchExceptionTypeName(CodeNamedItem nmItem)
        {
            this.catchExceptionTypeName = nmItem;
            this.CatchExceptionType = new CodeTypeReference(nmItem);
        }
        /// <summary>
        ///Gets or sets the variable name of the exception that the catch clause handles. 
        /// </summary> 
        public string LocalNameAsString
        {
            
            get
            {
                if (localName != null)
                {
                    return localName.NormalName;
                }
                else
                {
                    return null;
                }
            }
        }
        public CodeSimpleName LocalName
        {
            get
            {
                return localName;
            }
            set
            {
                localName = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeCatchClause_LocalName);
                }

            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CatchClause; }
        }

    }
    public class CodeTryClause : CodeObject
    {
        CodeBlockStatement stmBlock;
        public CodeTryClause(CodeBlockStatement stmBlock)
        {
            this.stmBlock = stmBlock;
        }
        public CodeBlockStatement Body
        {
            get
            {
                return this.stmBlock;
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.TryClause; }
        }
    }
    public class CodeFinallyClause : CodeObject
    {
        CodeBlockStatement body;
        public CodeFinallyClause(CodeBlockStatement stmBlock)
        {
            this.body = stmBlock;
        }
        public CodeBlockStatement Body
        {
            get
            {
                return this.body;
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.FinallyClause; }
        }
    }

}