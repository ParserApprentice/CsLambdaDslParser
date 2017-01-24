//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Parser.CodeDom
{
    public delegate bool CodeDomWalkerVisitExpressionDelegate(CodeExpression expr, CodeDomVisitor visitor);
    public delegate bool CodeDomWalkerVisitStatementDelegate(CodeStatement stm, CodeDomVisitor visitor);
    public delegate bool CodeDomWalkerVisitCodeTypeReference(CodeTypeReference codeTypeRef);

    public partial class CodeDomWalker
    {
        bool active = true;
        bool semanticMode = false;

        public CodeDomWalkerVisitExpressionDelegate PreVisitExpressionTest;
        public CodeDomWalkerVisitExpressionDelegate PostVisitExpressionTest;
        public CodeDomWalkerVisitStatementDelegate PreVisitStatementTest;
        public CodeDomWalkerVisitCodeTypeReference PreVisitCodeTypeReferenceTest;
        public CodeDomWalker()
        {

        }

        protected bool SemanticMode
        {
            get
            {
                return semanticMode;
            }
            set
            {
                semanticMode = value;
            }
        }
        public virtual void DomWalk(CompilationUnit cu, CodeDomVisitor visitor)
        {

            Active = true;
            CodeNamespaceImportCollection associatedNamespaces = cu.CodeNamespaceImportCollection;
            if (associatedNamespaces != null)
            {
                SemanticWalkCodeNamespaceImports(associatedNamespaces, visitor);
            }
            ////------------------------------------------------------------
            ////3. all namespace
            //int nameSpaceCount = cu.AsmNamespaceCount;
            //for (int n = 0; n < nameSpaceCount; ++n)
            //{
            //    if (!Active)
            //    {
            //        return;
            //    }
            //    WalkDomNamespace(cu.NamespaceCollection[n], visitor);
            //}

        }


        protected virtual void WalkCodeNamespaceImport(CodeNamespaceImport nsImport, CodeDomVisitor visitor)
        {


        }
        protected virtual void SemanticWalkCodeNamespaceImports(CodeNamespaceImportCollection associatedNamespaces, CodeDomVisitor visitor)
        {
            if (!Active)
            {
                return;
            }
            foreach (CodeNamespaceImport nsImport in associatedNamespaces)
            {
                WalkCodeNamespaceImport(nsImport, visitor);
                if (!Active)
                {
                    return;
                }
            }
        }


        protected virtual void WalkDomNamespace(CodeNamespace codeNamespace, CodeDomVisitor visitor)
        {

            int count = codeNamespace.Count;
            for (int i = 0; i < count; ++i)
            {
                var mb = codeNamespace.GetMember(i);
                if (mb is CodeTypeDeclaration)
                {
                    WriteCodeTypeDeclAndMemberTitles((CodeTypeDeclaration)mb, visitor);
                }
                else if (mb is CodeNamespace)
                {
                    WalkDomNamespace((CodeNamespace)mb, visitor);
                }
            }
        }

        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }
        protected virtual bool OnPreVisitCodeStatement(CodeStatement stm, CodeDomVisitor visitor)
        {
            if (this.PreVisitStatementTest != null)
            {
                this.PreVisitStatementTest(stm, visitor);
            }
            return true;
        }
        protected virtual bool OnPreVisitCodeStatementCollection(CodeStatementCollection stms, CodeDomVisitor visitor)
        {
            return true;
        }
        protected virtual bool OnPreVisitCodeExpressionCollection(CodeExpressionCollection exprs, CodeDomVisitor visitor)
        {
            return true;
        }
        protected virtual bool OnPreVisitCodeArgumentList(CodeArgumentList arglist, CodeDomVisitor visitor)
        {
            return true;
        }

        protected virtual bool OnPreVisitCodeExpression(CodeExpression expr, CodeDomVisitor visitor)
        {

            if (this.PreVisitExpressionTest != null)
            {
                return this.PreVisitExpressionTest(expr, visitor);
            }

            return true;
        }

        protected virtual bool OnPostVisitCodeExpression(CodeExpression expr, CodeDomVisitor visitor)
        {
            if (this.PostVisitExpressionTest != null)
            {
                return this.PostVisitExpressionTest(expr, visitor);
            }
            return true;
        }
        protected virtual void WalkCodeForEachStatement(CodeForEachStatement foreachStm, CodeDomVisitor visitor)
        {
            if (foreachStm.ForItemStatement != null)
            {
                WalkCodeStatement(foreachStm.ForItemStatement, visitor);
            }
            if (foreachStm.InWhatExpression != null)
            {
                WalkCodeExpression(foreachStm.InWhatExpression, visitor);
            }
            if (foreachStm.Body != null)
            {
                WalkCodeStatement(foreachStm.Body, visitor);
            }
        }
        protected virtual void WalkCodeForLoopStatement(CodeForLoopStatement forLoopStm, CodeDomVisitor visitor)
        {

            if (forLoopStm.ForInitializer != null)
            {
                WalkCodeExpressionCollection(forLoopStm.ForInitializer, visitor);

            }
            if (forLoopStm.ConditionExpression != null)
            {
                WalkCodeExpression(forLoopStm.ConditionExpression, visitor);
            }
            if (forLoopStm.ForIncrementors != null)
            {
                WalkCodeExpressionCollection(forLoopStm.ForIncrementors, visitor);
                //WalkCodeStatement(forLoopStm.IncrementStatement, visitor);
            }
            //if (forLoopStm.IncrementExpression != null)
            //{
            //    WalkCodeExpression(forLoopStm.IncrementExpression, visitor);
            //}
            if (forLoopStm.Body != null)
            {
                WalkCodeStatement(forLoopStm.Body, visitor);
                //WalkCodeStatementCollection(forLoopStm.Body, visitor);
            }
        }
        internal virtual void WalkCodeGotoStatement(CodeGotoStatement gotoStm, CodeDomVisitor visitor)
        {

        }
        internal virtual void WalkCodeLeaveStatement(CodeLeaveStatement leaveStm, CodeDomVisitor visitor)
        {

        }
        internal void WalkCodeLabelStatement(CodeLabelStatement labelStm, CodeDomVisitor visitor)
        {

        }
        protected virtual void WalkCodeIfElseStatement(CodeIfElseStatement ifelseStm, CodeDomVisitor visitor)
        {

            foreach (CodeIfBlock ifblock in ifelseStm.GetConditionBlocksIter())
            {
                WalkCodeExpression(ifblock.ConditionExpression, visitor);
                if (ifblock.Body != null)
                {
                    WalkCodeStatement(ifblock.Body, visitor);
                }
            }
            if (ifelseStm.ElseBlock != null)
            {
                WalkCodeStatement(ifelseStm.ElseBlock.Body, visitor);
            }
        }

        protected virtual void WalkCodeReturnStatement(CodeReturnStatement retStm, CodeDomVisitor visitor)
        {
            if (retStm.Expression != null)
            {
                WalkCodeExpression(retStm.Expression, visitor);
            }
        }
        protected virtual void WalkCodeThrowExceptionStatement(CodeThrowExceptionStatement thrStm, CodeDomVisitor visitor)
        {
            if (thrStm.ToThrow != null)
            {
                WalkCodeExpression(thrStm.ToThrow, visitor);
            }
        }
        protected virtual void WalkCodeSwitchStatement(CodeSwitchStatement switchStm, CodeDomVisitor visitor)
        {
            if (switchStm.SwitchExpression != null)
            {
                WalkCodeExpression(switchStm.SwitchExpression, visitor);
            }
            foreach (CodeSwitchBlock swtBlock in switchStm.GetAllSwitchBlockIter())
            {
                if (swtBlock.Label != null)
                {
                    WalkCodeExpression(swtBlock.Label, visitor);
                }
                if (swtBlock.Body != null)
                {
                    WalkCodeStatement(swtBlock.Body, visitor);
                }
            }
        }

        protected virtual void WalkCodeTryCatchFinallyStatement(CodeTryCatchFinallyStatement tryCatchFinallyStm, CodeDomVisitor visitor)
        {
            if (tryCatchFinallyStm.TryClause != null)
            {
                WalkCodeStatement(tryCatchFinallyStm.TryClause.Body, visitor);
            }
            foreach (CodeCatchClause catchClause in tryCatchFinallyStm.GetCatchClauseIter())
            {
                if (catchClause.CatchExceptionType != null)
                {
                    OnVisitCodeTypeReference(catchClause.CatchExceptionType);
                }
                //WriteTypeReferenceIndex(catchClause.CatchExceptionType, writer);
                //WriteSimpleName(catchClause.LocalName, writer);
                if (catchClause.compilerGenExceptionVarDeclStatement != null)
                {

                    WalkCodeStatement(catchClause.compilerGenExceptionVarDeclStatement, visitor);
                    //WriteCodeStatement(catchClause.compilerGenExceptionVarDeclStatement, writer);
                }
                //WalkCodeStatementCollection(catchClause.Body, visitor);
                WalkCodeStatement(catchClause.Body, visitor);
            }
            if (tryCatchFinallyStm.FinallyClause != null)
            {
                WalkCodeStatement(tryCatchFinallyStm.FinallyClause.Body, visitor);
                //WalkCodeStatementCollection(tryCatchFinallyStm.FinallyClause.Body, visitor);
            }

        }
        protected virtual void WalkCodeVariableDeclarationStatement(CodeVariableDeclarationStatement vardeclStm, CodeDomVisitor visitor)
        {
            //if (vardeclStm.VariableType != null)
            //{
            //    OnVisitCodeTypeReference(vardeclStm.VariableType);
            //}
            //if (vardeclStm.InitExpression != null)
            //{
            //    WalkCodeExpression(vardeclStm.InitExpression, visitor);
            //}
            int j = vardeclStm.DeclaratorCount;
            for (int i = 0; i < j; ++i)
            {
                CodeVariableDeclaratorExpression declarator = vardeclStm.GetDeclarator(i);
                if (declarator.VariableType != null)
                {
                    OnVisitCodeTypeReference(declarator.VariableType);
                }
                if (declarator.InitExpression != null)
                {
                    WalkCodeExpression(declarator.InitExpression, visitor);
                }
            }

        }
        protected virtual void WalkCodeWhileStatement(CodeWhileStatement whileStm, CodeDomVisitor visitor)
        {
            if (whileStm.Condition != null)
            {
                WalkCodeExpression(whileStm.Condition, visitor);
            }
            if (whileStm.Body != null)
            {
                WalkCodeStatement(whileStm.Body, visitor);
                //WalkCodeStatementCollection(whileStm.Body, visitor);
            }
        }
        protected virtual void WalkCodeYieldBreakStatement(CodeYieldBreakStatement yieldBreak, CodeDomVisitor visitor)
        {


        }
        protected virtual void WalkCodeYieldReturnStatement(CodeYieldReturnStatement yieldReturnStm, CodeDomVisitor visitor)
        {
            if (yieldReturnStm.Expression != null)
            {
                WalkCodeExpression(yieldReturnStm.Expression, visitor);
            }
        }

        protected virtual void WalkCodeArrayCreateExpression(CodeArrayCreateExpression arrCreateExpr, CodeDomVisitor visitor)
        {

            OnVisitCodeTypeReference(arrCreateExpr.CreateType);

            if (arrCreateExpr.InitSizeExpression != null)
            {
                WalkCodeExpression(arrCreateExpr.InitSizeExpression, visitor);
            }
            if (arrCreateExpr.Initializer != null)
            {
                WalkCodeExpressionCollection(arrCreateExpr.Initializer, visitor);
            }
        }
        protected virtual void WalkCodeArrayIndexerExpression(CodeIndexerAccessExpression arrIndexerExpr, CodeDomVisitor visitor)
        {
            WalkCodeExpression(arrIndexerExpr.TargetExpression, visitor);
            WalkCodeExpressionCollection(arrIndexerExpr.ArgList, visitor);
            //if (arrIndexerExpr.IndexerExpression != null)
            //{
            //    WalkCodeExpression(arrIndexerExpr.IndexerExpression, visitor);
            //}
        }
        protected virtual void WalkCodeBaseReferenceExpression(CodeBaseReferenceExpression baseRefExpr, CodeDomVisitor visitor)
        {


        }
        protected virtual void WalkCodeBinaryOperatorExpression(CodeBinaryOperatorExpression binOpExpr, CodeDomVisitor visitor)
        {
            if (binOpExpr.LeftExpression != null)
            {
                WalkCodeExpression(binOpExpr.LeftExpression, visitor);
            }
            if (binOpExpr.RightExpression != null)
            {
                WalkCodeExpression(binOpExpr.RightExpression, visitor);
            }
        }
        protected virtual void WalkCodeDecorationExpression(CodeDecorationExpression decorExpr, CodeDomVisitor visitor)
        {
            if (semanticMode)
            {

                if (decorExpr.CompilerGeneratedContextVariableDeclStm.CompilerReplaceStatement == null)
                {

                    if (decorExpr.TargetExpression != null)
                    {
                        WalkCodeExpression(decorExpr.TargetExpression, visitor);
                    }
                    if (decorExpr.DecorationStatements != null)
                    {
                        WalkCodeStatementCollection(decorExpr.DecorationStatements, visitor);
                    }
                }
                else
                {

                    WalkCodeStatementCollection(decorExpr.DecorationStatements, visitor);

                    CodeStatement compilerReplaceStm =
                        decorExpr.CompilerGeneratedContextVariableDeclStm.CompilerReplaceStatement;

                    if (compilerReplaceStm is CodeExpressionStatement
                        && ((CodeExpressionStatement)compilerReplaceStm).Expression is CodeBinaryOperatorExpression
                        )
                    {

                        CodeBinaryOperatorExpression binOp =
                            (CodeBinaryOperatorExpression)((CodeExpressionStatement)compilerReplaceStm).Expression;

                        if (binOp.BinaryOp == CodeBinaryOperatorName.Assign)
                        {

                            WalkCodeExpression(binOp.LeftExpression, visitor);
                            return;
                        }
                    }
                    throw new NotSupportedException();
                }

            }
            else
            {

                if (decorExpr.TargetExpression != null)
                {
                    WalkCodeExpression(decorExpr.TargetExpression, visitor);
                }
                if (decorExpr.DecorationStatements != null)
                {
                    WalkCodeStatementCollection(decorExpr.DecorationStatements, visitor);
                }
            }


        }

        void WalkCodeCustomAttr(CodeAttributeDeclaration attrDecl, CodeDomVisitor visitor)
        {
            CodeAttributeArgumentCollection attrArgs = attrDecl.Arguments;
            if (attrArgs != null)
            {
                int argCount = attrArgs.Count;
                for (int a = 0; a < argCount; ++a)
                {
                    WalkCodeExpression(attrArgs[a].Value, visitor);
                }
            }
        }
        void WalkCustomAttributeCollection(CodeAttributeDeclarationCollection customAttrCollection, CodeDomVisitor visitor)
        {
            if (customAttrCollection != null)
            {
                int j = customAttrCollection.Count;

                for (int i = 0; i < j; ++i)
                {
                    WalkCodeCustomAttr(customAttrCollection[i], visitor);
                }
            }
        }

        protected virtual void WalkCodeDefineFieldStatement(CodeDefineStatement defineStm, CodeDomVisitor visitor)
        {

            CodeDefineFieldExpressionCollection defineFieldExprCollection = defineStm.DefineFieldExpressionCollection;

            if (defineFieldExprCollection != null)
            {
                int j = defineFieldExprCollection.Count;

                for (int i = 0; i < j; ++i)
                {

                    OnVisitCodeTypeReference(defineFieldExprCollection[i].FieldType);
                }
            }
        }
        protected virtual void WalkCodeDefineFieldExpression(CodeDefineFieldDeclarationExpression defineFieldExpr, CodeDomVisitor visitor)
        {
            if (defineFieldExpr.DefaultValueExpression != null)
            {
                WalkCodeExpression(defineFieldExpr.DefaultValueExpression, visitor);
            }
            OnVisitCodeTypeReference(defineFieldExpr.FieldType);

        }


        protected virtual void OnVisitCodeTypeReference(CodeTypeReference typeref)
        {
            if (this.PreVisitCodeTypeReferenceTest != null)
            {
                PreVisitCodeTypeReferenceTest(typeref);
            }

        }
        protected virtual void WalkCodeDelegateCreateExpression(CodeDelegateCreateExpression delCreateExpr, CodeDomVisitor visitor)
        {
            if (delCreateExpr.TargetObject != null)
            {
                WalkCodeExpression(delCreateExpr.TargetObject, visitor);
            }
            OnVisitCodeTypeReference(delCreateExpr.DelegateType);
            //---------------------------------------------------------------
            //OnVisitSymbolReference(delCreateExpr.MapToMethodInfo);
            //OnVisitSymbolReference(delCreateExpr.MapToDelegateCtorMethodInfo);
            //---------------------------------------------------------------
        }
        protected virtual void WalkCodeDirectionExpression(CodeDirectionExpression directionExpr, CodeDomVisitor visitor)
        {

            if (directionExpr.Expression != null)
            {
                WalkCodeExpression(directionExpr.Expression, visitor);
            }

        }
        protected virtual void WalkCodeDynamicListExpression(CodeDynamicListExpression dynamicListExpr, CodeDomVisitor visitor)
        {
            if (dynamicListExpr.MemberExpressionCollection != null)
            {
                WalkCodeExpressionCollection(dynamicListExpr.MemberExpressionCollection, visitor);
            }
        }
        protected virtual void WalkCodeIdExpression(CodeIdExpression idExpr, CodeDomVisitor visitor)
        {

            //OnVisitSymbolReference(idExpr.MapToSymbol);

        }
        protected virtual void WalkCodeIncDecExpression(CodeIncrementDecrementOperatorExpression incDecExpr, CodeDomVisitor visitor)
        {
            WalkCodeExpression(incDecExpr.TargetExpression, visitor);
        }
        protected virtual void WalkCodeJsonObjectExpression(CodeJsonObjectExpression jsonObjectExpr, CodeDomVisitor visitor)
        {
            //1. expr-collection
            WalkCodeExpressionCollection(jsonObjectExpr.ContentCollection, visitor);
            if (semanticMode)
            {

                if (jsonObjectExpr.ContextVarDeclStatement != null)
                {
                    WalkCodeStatement(jsonObjectExpr.ContextVarDeclStatement, visitor);
                    WalkCodeStatementCollection(jsonObjectExpr.propAssignments, visitor);
                }
            }

        }
        protected virtual void WalkCodeLambdaExpression(CodeLambdaExpression lambdaExpr, CodeDomVisitor visitor)
        {
            if (lambdaExpr.ParameterList != null)
            {
                foreach (CodeParameterDeclarationExpression parDeclExpr in lambdaExpr.ParameterList)
                {
                    WalkCodeExpression(parDeclExpr, visitor);
                }
            }
            if (lambdaExpr.MethodBody != null)
            {
                WalkCodeStatementCollection(lambdaExpr.MethodBody, visitor);
            }
            else if (lambdaExpr.SingleExpression != null)
            {
                WalkCodeExpression(lambdaExpr.SingleExpression, visitor);
            }

            OnVisitCodeTypeReference(lambdaExpr.TypeReference);
        }
        protected virtual void WalkCodeLinqQueryExpression(CodeQueryExpression lambdaExpr, CodeDomVisitor visitor)
        {
        }
        protected virtual void WalkCodeMemberAccessExprssion(CodeMemberAccessExpression mbAccessExpr, CodeDomVisitor visitor)
        {
            if (mbAccessExpr.TargetExpression != null)
            {
                WalkCodeExpression(mbAccessExpr.TargetExpression, visitor);
            }
            if (mbAccessExpr.MemberExpression != null)
            {
                WalkCodeExpression(mbAccessExpr.MemberExpression, visitor);

                //if (mbAccessExpr.MemberExpression.MapToSymbol != null)
                //{

                //    WalkCodeIdExpression(mbAccessExpr.MemberExpression, visitor);
                //}
            }
        }
        protected virtual void WalkCodeMeReferenceExprssion(CodeMeReferenceExpression meRefExpr, CodeDomVisitor visitor)
        {

        }
        protected virtual void WalkCodeMethodInvokeExpression(CodeMethodInvokeExpression metInvExpr, CodeDomVisitor visitor)
        {

            if (metInvExpr.Method != null)
            {
                WalkCodeExpression(metInvExpr.Method, visitor);
            }
            if (metInvExpr.Arguments != null)
            {
                WalkCodeExpressionCollection(metInvExpr.Arguments, visitor);
            }
        }
        protected virtual void WalkCodeBaseCtorInvokeExpression(CodeBaseCtorInvoke baseCtorInvokeExpr, CodeDomVisitor visitor)
        {

            WalkCodeMethodReferenceExpression(baseCtorInvokeExpr, visitor);
            if (baseCtorInvokeExpr.Arguments != null)
            {
                WalkCodeExpressionCollection(baseCtorInvokeExpr.Arguments, visitor);
            }
        }
        protected virtual void WalkCodeMethodReferenceExpression(CodeMethodReferenceExpression metRefExpr, CodeDomVisitor visitor)
        {

            WalkCodeExpression(metRefExpr.Target, visitor);
        }
        protected virtual void WalkCodeNamedExpression(CodeNamedExpression namedExpr, CodeDomVisitor visitor)
        {
            if (namedExpr.Expression != null)
            {
                WalkCodeExpression(namedExpr.Expression, visitor);
            }
        }
        protected virtual void WalkCodeNullExpression(CodeNullExpression nullExpr, CodeDomVisitor visitor)
        {

        }
        protected virtual void WalkCodeObjectCreateExpression(CodeObjectCreateExpression objectCreateExpr, CodeDomVisitor visitor)
        {
            if (objectCreateExpr.Arguments != null)
            {
                WalkCodeExpressionCollection(objectCreateExpr.Arguments, visitor);
            }
            OnVisitCodeTypeReference(objectCreateExpr.ObjectType);
        }
        protected virtual void WalkCodeParameterDeclExpression(CodeParameterDeclarationExpression parameterDeclExpr, CodeDomVisitor visitor)
        {
            if (parameterDeclExpr.DefaultValueExpression != null)
            {
                WalkCodeExpression(parameterDeclExpr.DefaultValueExpression, visitor);
            }
            OnVisitCodeTypeReference(parameterDeclExpr.ParameterType);
        }
        protected virtual void WalkCodeParenthizeExpression(CodeParenthizedExpression parenthizedExpr, CodeDomVisitor visitor)
        {
            if (parenthizedExpr.ContentExpression != null)
            {
                WalkCodeExpression(parenthizedExpr.ContentExpression, visitor);
            }
        }
        protected virtual void WalkCodePrimitiveExpression(CodePrimitiveExpression primitiveExpr, CodeDomVisitor visitor)
        {


        }
        protected virtual void WalkCodeCustomExpression(CodeQueryExpression queryExpr, CodeDomVisitor visitor)
        {

        }
        protected virtual void WalkCodeThisCtorInvokeExpression(CodeThisCtorInvokeExpression thisCtorInvokeExpr, CodeDomVisitor visitor)
        {
            if (thisCtorInvokeExpr.Arguments != null)
            {
                WalkCodeExpressionCollection(thisCtorInvokeExpr.Arguments, visitor);
            }
        }
        protected virtual void WalkCodeThisReferenceExpression(CodeThisReferenceExpression thisRefExpr, CodeDomVisitor visitor)
        {


        }
        protected virtual void WalkCodeTypeConversionExpression(CodeTypeConversionExpression typeConvExpr, CodeDomVisitor visitor)
        {
            if (typeConvExpr.CastedExpression != null)
            {
                WalkCodeExpression(typeConvExpr.CastedExpression, visitor);
            }
            OnVisitCodeTypeReference(typeConvExpr.TargetType);

        }
        protected virtual void WalkCodeUnaryOperatorExpression(CodeUnaryOperatorExpression unaryOpExpr, CodeDomVisitor visitor)
        {
            if (unaryOpExpr.Expression != null)
            {
                WalkCodeExpression(unaryOpExpr.Expression, visitor);
            }
        }
        protected virtual void WalkCodeExpressionCollection(CodeArgumentList arglist, CodeDomVisitor visitor)
        {
            if (!active)
            {
                return;
            }
            if (!OnPreVisitCodeArgumentList(arglist, visitor))
            {

                return;
            }
            foreach (CodeArgument arg in arglist)
            {
                if (!active)
                {
                    break;
                }
                WalkCodeArgument(arg, visitor);
            }
        }
        protected virtual void WalkCodeExpressionCollection(CodeExpressionCollection exprCollection, CodeDomVisitor visitor)
        {
            if (!active)
            {
                return;
            }
            if (!OnPreVisitCodeExpressionCollection(exprCollection, visitor))
            {

                return;
            }
            foreach (CodeExpression expr in exprCollection)
            {
                if (!active)
                {
                    break;
                }
                WalkCodeExpression(expr, visitor);
            }

        }
        public virtual void WalkCodeArgument(CodeArgument arg, CodeDomVisitor visitor)
        {
            if (!active)
            {
                return;
            }

            WalkCodeExpression(arg.Value, visitor);

        }

        public virtual void WalkCodeExpression(CodeExpression expr, CodeDomVisitor visitor)
        {
            if (!active)
            {
                return;
            }
            //--------------------------------------------------------------------------------------
            if (semanticMode && expr.HasCompilerGeneratedReplacedExpression)
            {
                WalkCodeExpression(CodeExpression.GetCompilerGeneratedCodeExpression(expr), visitor);
                return;
            }
            //--------------------------------------------------------------------------------------
            if (!OnPreVisitCodeExpression(expr, visitor))
            {

                return;
            }
            if (expr.TypeReference != null)
            {

                OnVisitCodeTypeReference(expr.TypeReference);
            }
            switch (expr.ExpressionKind)
            {
                case CodeExpressionKind.ArrayCreateExpression:
                    {
                        WalkCodeArrayCreateExpression((CodeArrayCreateExpression)expr, visitor);
                    } break;

                case CodeExpressionKind.IndexerAccessExpression:
                    {
                        WalkCodeArrayIndexerExpression((CodeIndexerAccessExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.BaseCtorInvoke:
                    {
                        WalkCodeBaseCtorInvokeExpression((CodeBaseCtorInvoke)expr, visitor);

                    } break;
                case CodeExpressionKind.BaseRefererneceExpression:
                    {
                        WalkCodeBaseReferenceExpression((CodeBaseReferenceExpression)expr, visitor);

                    } break;
                case CodeExpressionKind.BinaryOperatorExpression:
                    {
                        WalkCodeBinaryOperatorExpression((CodeBinaryOperatorExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.DecorationExpression:
                    {
                        WalkCodeDecorationExpression((CodeDecorationExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.DefineFieldDeclarationExpression:
                    {
                        WalkCodeDefineFieldExpression((CodeDefineFieldDeclarationExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.DelegateCreateExpression:
                    {
                        WalkCodeDelegateCreateExpression((CodeDelegateCreateExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.DirectionExpression:
                    {
                        WalkCodeDirectionExpression((CodeDirectionExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.DynamicListExpression:
                    {
                        WalkCodeDynamicListExpression((CodeDynamicListExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.IdExpression:
                    {
                        WalkCodeIdExpression((CodeIdExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.IncDecOperatorExpression:
                    {
                        WalkCodeIncDecExpression((CodeIncrementDecrementOperatorExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.JsonObjectExpression:
                    {
                        WalkCodeJsonObjectExpression((CodeJsonObjectExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.LambdaExpression:
                    {
                        WalkCodeLambdaExpression((CodeLambdaExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.LinqQueryExpression:
                    {
                        WalkCodeLinqQueryExpression((CodeQueryExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.MemberAccessExpression:
                    {
                        WalkCodeMemberAccessExprssion((CodeMemberAccessExpression)expr, visitor);

                    } break;
                case CodeExpressionKind.MeReferenceExpression:
                    {
                        WalkCodeMeReferenceExprssion((CodeMeReferenceExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.MethodInvokeExpression:
                    {
                        WalkCodeMethodInvokeExpression((CodeMethodInvokeExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.MethodReferenceExpression:
                    {
                        WalkCodeMethodReferenceExpression((CodeMethodReferenceExpression)expr, visitor);

                    } break;
                case CodeExpressionKind.NamedExpression:
                    {
                        WalkCodeNamedExpression((CodeNamedExpression)expr, visitor);

                    } break;
                case CodeExpressionKind.NullExpression:
                    {
                        WalkCodeNullExpression((CodeNullExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.ObjectCreateExpression:
                    {
                        WalkCodeObjectCreateExpression((CodeObjectCreateExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.ParameterDeclarationExpression:
                    {
                        WalkCodeParameterDeclExpression((CodeParameterDeclarationExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.ParenthizedExpression:
                    {
                        WalkCodeParenthizeExpression((CodeParenthizedExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.PrimitiveExpression:
                    {
                        WalkCodePrimitiveExpression((CodePrimitiveExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.CustomExpression:
                    {
                        WalkCodeCustomExpression((CodeQueryExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.ThisCtorInvoke:
                    {
                        WalkCodeThisCtorInvokeExpression((CodeThisCtorInvokeExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.ThisReferenceExpression:
                    {
                        WalkCodeThisReferenceExpression((CodeThisReferenceExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.AsConversionExpression:
                case CodeExpressionKind.ExplicitConversionExpression:
                    {
                        WalkCodeTypeConversionExpression((CodeTypeConversionExpression)expr, visitor);
                    } break;
                case CodeExpressionKind.UnaryOperatorExpression:
                    {
                        WalkCodeUnaryOperatorExpression((CodeUnaryOperatorExpression)expr, visitor);

                    } break;
                case CodeExpressionKind.ProxyExpresion:
                    {
                        WalkCodeExpression(expr.GetInnerExpression(), visitor);
                    } break;
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
            //--------------------------------------------------
            if (!OnPostVisitCodeExpression(expr, visitor))
            {
                return;
            }
            //--------------------------------------------------
        }
        protected virtual void WalkCodeDoWhileStatement(CodeDoWhileStatement doWhileStm, CodeDomVisitor visitor)
        {
            WalkCodeExpression(doWhileStm.Condition, visitor);
            WalkCodeStatement(doWhileStm.Body, visitor);
        }
        public virtual void WalkCodeStatement(CodeStatement stm, CodeDomVisitor visitor)
        {

            if (!active)
            {
                return;
            }
            if (semanticMode)
            {
                if (stm.CompilerReplaceStatement != null)
                {

                    WalkCodeStatement(stm.CompilerReplaceStatement, visitor);

                    return;
                }
            }

            //-----------------------------
            if (!OnPreVisitCodeStatement(stm, visitor))
            {

                return;
            }
            //----------------------------- 

            switch (stm.StatementType)
            {
                //---------------------------------------------
                case CodeStatementType.CodeBlockStatement:
                    {

                        WalkCodeStatementCollection(((CodeGroupStatement)stm).Body, visitor);

                    } break;
                case CodeStatementType.CodeBreakStatement:
                    {

                    } break;
                case CodeStatementType.CodeBreakPointStatement:
                    {
                    } break;
                //---------------------------------------------
                case CodeStatementType.CodeContinueStatement:
                    {
                    } break;
                case CodeStatementType.CodeCommentStatement:
                    {

                    } break;
                //---------------------------------------------
                case CodeStatementType.CodeDoWhileStatement:
                    {
                        WalkCodeDoWhileStatement((CodeDoWhileStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeDefineStatement:
                    {
                        WalkCodeDefineFieldStatement((CodeDefineStatement)stm, visitor);
                    } break;
                //---------------------------------------------
                case CodeStatementType.CodeEndSpecialSegment:
                    {
                    } break;
                case CodeStatementType.CodeEmptyStatement:
                    {
                    } break;
                case CodeStatementType.CodeExpressionStatement:
                    {
                        WalkCodeExpressionStatement((CodeExpressionStatement)stm, visitor);
                    } break;

                //---------------------------------------------
                case CodeStatementType.CodeForEachStatement:
                    {
                        WalkCodeForEachStatement((CodeForEachStatement)stm, visitor);

                    } break;
                case CodeStatementType.CodeForLoopStatement:
                    {
                        WalkCodeForLoopStatement((CodeForLoopStatement)stm, visitor);
                    } break;

                //---------------------------------------------
                case CodeStatementType.CodeGoToStatement:
                    {
                        WalkCodeGotoStatement((CodeGotoStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeLeaveStatement:
                    {

                        WalkCodeLeaveStatement((CodeLeaveStatement)stm, visitor);
                    } break;
                //--------------------------------------------- 
                case CodeStatementType.CodeIfElseStatement:
                    {
                        WalkCodeIfElseStatement((CodeIfElseStatement)stm, visitor);
                    } break;

                case CodeStatementType.CodeLabelStatement:
                    {
                        WalkCodeLabelStatement((CodeLabelStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeReturnStatement:
                    {
                        WalkCodeReturnStatement((CodeReturnStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeSQLStatementExpression:
                    {

                    } break;
                case CodeStatementType.CodeStartSpecialSegment:
                    {

                    } break;
                case CodeStatementType.CodeSwitchStatement:
                    {
                        WalkCodeSwitchStatement((CodeSwitchStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeThrowExceptionStatement:
                    {
                        WalkCodeThrowExceptionStatement((CodeThrowExceptionStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeTryCatchFinallyStatement:
                    {
                        WalkCodeTryCatchFinallyStatement((CodeTryCatchFinallyStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeVariableDeclarationStatement:
                    {
                        WalkCodeVariableDeclarationStatement((CodeVariableDeclarationStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeWhileStatement:
                    {
                        WalkCodeWhileStatement((CodeWhileStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeYieldBreakStatement:
                    {
                        WalkCodeYieldBreakStatement((CodeYieldBreakStatement)stm, visitor);
                    } break;
                case CodeStatementType.CodeYieldReturnStatement:
                    {
                        WalkCodeYieldReturnStatement((CodeYieldReturnStatement)stm, visitor);
                    } break;

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
        protected virtual void WalkCodeExpressionStatement(CodeExpressionStatement exprStm, CodeDomVisitor visitor)
        {
            if (exprStm.Expression != null)
            {
                WalkCodeExpression(exprStm.Expression, visitor);
            }
        }

        protected virtual void WalkCodeStatementCollection(CodeStatementCollection stmCollection, CodeDomVisitor visitor)
        {

            if (stmCollection != null)
            {
                if (!active)
                {
                    return;
                }
                if (!OnPreVisitCodeStatementCollection(stmCollection, visitor))
                {

                    return;
                }
                foreach (CodeStatement stm in stmCollection)
                {
                    if (!active)
                    {
                        break;
                    }
                    WalkCodeStatement(stm, visitor);
                }
            }

        }

    }




}