//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
 

namespace Parser.CodeDom
{

    public struct JsonNamedMemberExpression
    {
        public readonly string name;
        public readonly CodeExpression expr;
        public JsonNamedMemberExpression(string name, CodeExpression expr)
        {
            this.name = name;
            this.expr = expr;
        }
    }
    /// <summary>
    /// json key pair expression
    /// </summary>
    public class CodeJsonObjectExpression : CodeExpression
    {

        List<JsonNamedMemberExpression> compilerGenNamedMembers;

        //temp var decl expression
        CodeVariableDeclarationStatement contextVarDeclStatement;

        public CodeStatementCollection propAssignments;
        // compiler generate  
        CodeExpressionCollection exprCollection;


        public CodeJsonObjectExpression(CodeExpressionCollection exprCollection)
        {
            this.exprCollection = exprCollection;
            AcceptChild(exprCollection, CodeObjectRoles.CodeExpressionSet_Content);
        }

        public CodeJsonObjectExpression()
        {
            exprCollection = new CodeExpressionCollection();
            AcceptChild(exprCollection, CodeObjectRoles.CodeExpressionSet_Content);

        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldExpr, CodeExpression newcodeExpression)
        {
            return CodeReplacingResult.Ok;
        }

        public CodeExpressionCollection ContentCollection
        {
            get
            {
                return this.exprCollection;
            }
        }
        public List<JsonNamedMemberExpression> GetNamedMemberCollections()
        {
            if (this.compilerGenNamedMembers == null)
            {
                List<JsonNamedMemberExpression> mbList = new List<JsonNamedMemberExpression>();
                this.compilerGenNamedMembers = mbList;

                foreach (CodeExpression mb in this.exprCollection)
                {
                    if (mb is CodeNamedExpression)
                    {
                        CodeNamedExpression codeNmExpr = (CodeNamedExpression)mb;
                        mbList.Add(new JsonNamedMemberExpression(
                            codeNmExpr.NameAsString,
                            codeNmExpr.Expression));
                        continue;
                    }
                    else if (mb is CodeMemberAccessExpression)
                    {

                        CodeMemberAccessExpression mbAccessExpr = (CodeMemberAccessExpression)mb;
                        mbList.Add(new JsonNamedMemberExpression(
                          mbAccessExpr.MemberExpression.NameAsString,
                          mbAccessExpr));
                        continue;
                    }
                    else if (mb is CodeIdExpression)
                    {
                        CodeIdExpression idExpr = (CodeIdExpression)mb;
                        mbList.Add(new JsonNamedMemberExpression(
                           idExpr.NameAsString,
                           idExpr));
                        continue;
                    }
                    else if (mb is CodeBinaryOperatorExpression)
                    {

                        CodeBinaryOperatorExpression binOp = (CodeBinaryOperatorExpression)mb;
                        if (binOp.BinaryOp == CodeBinaryOperatorName.Assign)
                        {
                            if (binOp.LeftExpression is CodeIdExpression)
                            {
                                mbList.Add(new JsonNamedMemberExpression(
                                 ((CodeIdExpression)binOp.LeftExpression).NameAsString,
                                 binOp.RightExpression));
                                continue;
                            }
                            else if (binOp.LeftExpression is CodePrimitiveExpression)
                            {


                                string strValue = ((CodePrimitiveExpression)binOp.LeftExpression).Value;
                                if (strValue.StartsWith("\""))
                                {
                                    strValue = strValue.Substring(1, strValue.Length - 2);
                                }
                                mbList.Add(new JsonNamedMemberExpression(
                                    strValue,
                                    binOp.RightExpression));
                                continue;
                            }
                        }
                    }

                    throw new NotSupportedException();

                }

            }
            return this.compilerGenNamedMembers;


        }
        public int ExpressionCount
        {
            get
            {
                if (exprCollection == null)
                {
                    return 0;
                }
                else
                {
                    return exprCollection.Count;
                }
            }
        }
        public void AddMember(CodeNamedExpression expr)
        {
            exprCollection.AddCodeObject(expr);
        }

        public CodeVariableDeclarationStatement ContextVarDeclStatement
        {
            get
            {
                return contextVarDeclStatement;
            }
            set
            {
                contextVarDeclStatement = value;
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.JsonObjectExpression; }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
    }
}