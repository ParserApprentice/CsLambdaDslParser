//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Parser.CodeDom
{

    public class CodeForEachStatement : CodeStatement
    {
        CodeVariableDeclarationStatement forType;
        CodeExpression inWhat;
        CodeExpression primaryInWhatExpression;
        List<CodeStatement> userForeachBody;
        CodeStatement body;
        public CodeForEachStatement()
        {

        }
        public CodeStatement Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeForEachStatement_Body);
                }
            }
        }
        public CodeVariableDeclarationStatement ForItemStatement
        {
            get
            {
                return forType;
            }
            set
            {
                forType = value;
                AcceptChild(forType, CodeObjectRoles.CodeForEachStatement_ForType);
            }
        }
        public CodeExpression PrimaryInWhatExpression
        {
            get
            {
                return this.primaryInWhatExpression;
            }
        }
        public CodeExpression InWhatExpression
        {
            get
            {
                return inWhat;
            }
            set
            {
                if (inWhat == null && value != null)
                {
                    this.primaryInWhatExpression = value;
                }

                inWhat = value;
                AcceptChild(inWhat, CodeObjectRoles.CodeForEachStatement_InWhat);
            }
        }

        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeForEachStatement;
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression childExpr)
        {
            switch (childExpr.CodeObjectRole)
            {
                case CodeObjectRoles.CodeForEachStatement_InWhat:
                    {
                        this.InWhatExpression = childExpr;
                        return CodeReplacingResult.Ok;
                    }
            }
            return CodeReplacingResult.NotSupport;
        }
        public List<CodeStatement> UserForEachBody
        {
            get
            {
                return this.userForeachBody;
            }
            set
            {
                this.userForeachBody = value;
            }
        }

    }



    //-------------------------------------------------------------
    public class CodeForLoopStatement : CodeStatement
    {

        CodeExpressionCollection initExpr;
        CodeExpression conditionExpr;
        CodeExpressionCollection incExprStm;
        CodeStatement body;
        public CodeForLoopStatement()
        {
            this.Body = new CodeBlockStatement();
        }
        public CodeExpressionCollection ForInitializer
        {
            get
            {
                return initExpr;
            }
            set
            {

                initExpr = value;
                AcceptChild(initExpr, CodeObjectRoles.CodeForLoopStatement_InitExpr);
            }
        }
        public CodeExpression ConditionExpression
        {
            get
            {
                return conditionExpr;
            }
            set
            {

                conditionExpr = value;
                AcceptChild(conditionExpr, CodeObjectRoles.CodeForLoopStatement_Condition);
            }
        }

        public CodeExpressionCollection ForIncrementors
        {
            get
            {
                return incExprStm;
            }
            set
            {

                incExprStm = value;

                AcceptChild(incExprStm, CodeObjectRoles.CodeForLoopStatement_IncStm);
            }
        }

        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeForLoopStatement;
            }
        }
        public CodeStatement Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeForLoopStatement_Body);
                }
            }
        }


    }

    public class CodeDoWhileStatement : CodeStatement
    {
        CodeExpression conditionExpr;
        CodeStatement body;
        public CodeDoWhileStatement()
        {
            this.Body = new CodeBlockStatement();
        }
        public CodeDoWhileStatement(CodeStatement body, CodeExpression condition)
        {
            this.Body = body;
            this.Condition = condition;
        }
        public CodeExpression Condition
        {
            get
            {
                return conditionExpr;
            }
            set
            {
                conditionExpr = value;

                AcceptChild(conditionExpr, CodeObjectRoles.CodeDoWhileStatement_Condition);

            }
        }
        public CodeStatement Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeDoWhileStatement_Body);
                }
            }
        }


        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeDoWhileStatement;
            }
        }

    }

    public class CodeWhileStatement : CodeStatement
    {
        CodeExpression conditionExpr;
        CodeStatement body;
        public const int codeobj_conditionExpr = 1;
        public CodeWhileStatement()
        {
            this.Body = new CodeBlockStatement();
        }
        public CodeWhileStatement(CodeExpression condExpr, CodeStatement body)
        {
            this.Condition = condExpr;
            this.Body = body;
        }

        public CodeStatement Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeWhileStatement_Body);
                }
            }
        }
        public CodeExpression Condition
        {
            get
            {
                return conditionExpr;
            }
            set
            {
                conditionExpr = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeWhileStatement_Condition);
                }
            }
        }
        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeWhileStatement;
            }
        }
    }

}