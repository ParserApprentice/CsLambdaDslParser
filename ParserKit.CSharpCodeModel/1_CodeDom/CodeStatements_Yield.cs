//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Parser.CodeDom
{
    //---------------------------------------
    /// <summary>
    /// yield return
    /// </summary>
    public class CodeYieldReturnStatement : CodeStatement
    {
        CodeExpression returnExpr;

       
        public CodeYieldReturnStatement(CodeExpression returnExpr)
        {
            this.Expression = returnExpr;
        }
        public CodeExpression Expression
        {
            get
            {
                return returnExpr;
            }
            set
            {
                returnExpr = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeYieldReturnStatement_ReturnExpr);
                }
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression childExpr)
        {
            if (returnExpr.CodeObjectRole == childExpr.CodeObjectRole)
            {
                Expression = childExpr;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(childExpr);
        }
        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeYieldReturnStatement;
            }
        }

    }

     
    //---------------------------------------
    /// <summary>
    /// yield break
    /// </summary>
    public class CodeYieldBreakStatement : CodeStatement
    {
        public CodeYieldBreakStatement()
        {

        }
        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeYieldBreakStatement;
            }
        } 
    } 
}