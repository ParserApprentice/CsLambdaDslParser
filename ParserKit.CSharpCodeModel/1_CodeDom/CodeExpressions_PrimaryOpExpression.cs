//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;
using Parser.AsmInfrastructures; 
namespace Parser.CodeDom
{

    public abstract class CodePrimaryOperatorExpression : CodeExpression
    {
        public CodePrimaryOperatorExpression()
        {
        }
    }
    //==============================================================
    public class CodeArrayCreateExpression : CodePrimaryOperatorExpression
    {

        CodeTypeReference createType;
        CodeExpressionCollection initializer;
        CodeExpression initSizeExpr;

        CodeNamedItem createTypeName;

        public CodeArrayCreateExpression()
        {

        }
        public CodeArrayCreateExpression(CodeTypeReference createType, CodeExpressionCollection initializer)
        {
            this.CreateType = createType;
            if (initializer != null)
            {
                this.Initializer = initializer;
            }
        }
        public CodeArrayCreateExpression(CodeTypeReference createType, CodeExpression initSizeExpr)
        {
            this.CreateType = createType;
            if (initSizeExpr != null)
            {
                this.InitSizeExpression = initSizeExpr;
            }
        }
        public CodeArrayCreateExpression(CodeNamedItem createTypeName)
        {
            this.createTypeName = createTypeName;
            this.CreateType = new CodeTypeReference(createTypeName);

        }
        public CodeTypeReference CreateType
        {
            get
            {
                return createType;
            }
            set
            {
                createType = value;
            }
        }
        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.ArrayCreateExpression;
            }
        }
        public CodeExpression InitSizeExpression
        {
            get
            {
                return initSizeExpr;
            }
            set
            {

                initSizeExpr = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeArrayCreateExpression_InitSizeExpr);
                }
            }
        }
        public CodeExpressionCollection Initializer
        {
            get
            {
                return initializer;
            }
            set
            {

                initializer = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeArrayCreateExpression_Initializer);
                }
            }
        }
        public CodeArrayInitailizationExprssion Initializer2
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }

        }
        public bool UserImplicitArrayType
        {
            get;
            set;
        }
        public CodeArrayRankSpecifier RankSpec
        {
            get;
            set;
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (initSizeExpr.CodeObjectRole == newcodeExpression.CodeObjectRole)
            {
                InitSizeExpression = newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }
#if DEBUG

        public override string ToString()
        {
            if (initSizeExpr != null)
            {
                return "new " + createType.FullName + "[" + initSizeExpr.ToString() + "]";
            }
            else
            {
                return "new " + createType + "[]";
            }
        }
#endif

        public override int OperatorPrecedence
        {
            get { return CodeOperatorPrecendence.NEW; }
        }

    }

    /// <summary>
    /// array initialization object
    /// </summary>
    public class CodeArrayInitailizationExprssion : CodeExpression
    {
        CodeExpressionCollection memberExpressionList = new CodeExpressionCollection();
        public CodeExpressionCollection MemberExpressionList
        {
            get
            {
                return memberExpressionList;
            }
            set
            {
                this.memberExpressionList = value;
            }
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.ArrayInitialization; }
        }
        public void AddExpression(CodeExpression expr)
        {
            this.memberExpressionList.AddCodeObject(expr);
        }
    }

    //==============================================================
    public class CodeIndexerAccessExpression : CodePrimaryOperatorExpression
    {
        CodeExpression targetExpr;

        CodeArgumentList argList;
        bool isArrIndexAccess;

        public CodeIndexerAccessExpression(CodeExpression targetExpression, CodeExpressionCollection argList)
        {
            this.ArgList = new CodeArgumentList(argList);
            this.TargetExpression = targetExpression;
        }
        public CodeIndexerAccessExpression(CodeExpression targetExpression, CodeArgumentList argList)
        {
            this.ArgList = argList;
            this.TargetExpression = targetExpression;
        }
        public CodeExpression TargetExpression
        {
            get
            {
                return targetExpr;
            }
            set
            {

                targetExpr = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeArrayIndexerExpression_Target);
                }
            }
        }
        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.IndexerAccessExpression;
            }
        }
        public CodeArgumentList ArgList
        {
            get
            {
                return this.argList;
            }
            set
            {
                this.argList = value;

                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeArrayIndexerExpression_Indexer);
                }
            }
        }

        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {

            if (targetExpr.CodeObjectRole == newcodeExpression.CodeObjectRole)
            {
                TargetExpression = newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }

        public override int OperatorPrecedence
        {
            get { return CodeOperatorPrecendence.ARRAY_INDEXER; }
        }
        public bool IsArrayIndexAccess
        {
            get
            {
                return this.isArrIndexAccess;
            }
            set
            {
                this.isArrIndexAccess = value;
            }
        }
    }


    //==============================================================
    public class CodeDelegateCreateExpression : CodePrimaryOperatorExpression
    {

        CodeExpression targetObject;
        CodeTypeReference delegateType;
        readonly CodeIdExpression myMethodName;


        public CodeDelegateCreateExpression(CodeTypeReference delegateType, CodeExpression targetObject, string methodName)
        {
            this.DelegateType = delegateType;
            this.TargetObject = targetObject;
            this.myMethodName = new CodeIdExpression(methodName);
            AcceptChild(myMethodName, CodeObjectRoles.CodeDelegateCreate_Method);
        }
        public CodeExpression TargetObject
        {
            get
            {
                return targetObject;
            }
            set
            {
                targetObject = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeDelegateCreate_Target);
                }
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (newcodeExpression.CodeObjectRole == CodeObjectRoles.CodeDelegateCreate_Target)
            {
                TargetObject = newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }

        public CodeTypeReference DelegateType
        {
            get
            {
                return delegateType;
            }
            set
            {
                delegateType = value;
            }
        }
        public CodeSimpleName MethodName
        {
            get
            {
                return myMethodName.Name;
            }
        }

        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.DelegateCreateExpression; }
        }
        public override int OperatorPrecedence
        {
            get { return CodeOperatorPrecendence.NEW; }
        }
    }

    //==============================================================
    public class CodeObjectOrArrayInitializer
    {
        List<CodeExpression> exprssions = new List<CodeExpression>();
        public void AddExpression(CodeExpression expr)
        {
            this.exprssions.Add(expr);
        }
    }

    public class CodeObjectCreateExpression : CodePrimaryOperatorExpression
    {
        CodeExpressionCollection args;
        CodeTypeReference objectType;
        CodeNamedItem objectTypeName;
        bool maybeSpecialMap;


        public CodeObjectCreateExpression(CodeTypeReference objType)
        {
            this.objectType = objType;
            Arguments = new CodeExpressionCollection();
        }
        public CodeObjectCreateExpression(CodeTypeReference objType, bool maybeSpecialMap)
        {
            this.objectType = objType;
            Arguments = new CodeExpressionCollection();
            this.maybeSpecialMap = maybeSpecialMap;
        }
        public CodeObjectCreateExpression(CodeNamedItem objectTypeName)
            : this(new CodeTypeReference(objectTypeName))
        {
            this.objectTypeName = objectTypeName;
        }
        public CodeObjectCreateExpression(CodeTypeReference objType, CodeExpressionCollection ctorParams)
        {
            this.objectType = objType;
            Arguments = ctorParams;
        }
        public bool MaybeSpecialMap
        {
            get
            {
                return maybeSpecialMap;
            }

        }
        public CodeTypeReference ObjectType
        {
            get
            {
                return objectType;
            }
        }
        public CodeExpressionCollection Arguments
        {
            get
            {

                return args;
            }
            set
            {

                args = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeObjectCreateExpression_Parameters);
                }
            }
        }
        public override int OperatorPrecedence
        {
            get { return CodeOperatorPrecendence.NEW; }
        }
        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.ObjectCreateExpression;
            }
        }
        public CodeObjectOrArrayInitializer ObjectOrArrayInitializer
        {
            get;
            set;
        }
    }

    public class CodeAnonymousObjectCreateExprssion : CodePrimaryOperatorExpression
    {
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.AnonymousObjectCreateExpression; }
        }
        public CodeObjectOrArrayInitializer Initializer
        {
            get;
            set;
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }

    }

    //==============================================================
    public class CodeMethodInvokeExpression : CodePrimaryOperatorExpression
    {

        CodeMethodReferenceExpression methodRefExpr;
        CodeArgumentList arguments;
        public CodeMethodInvokeExpression()
        {

        }
        public CodeMethodInvokeExpression(CodeMethodReferenceExpression methodRef, CodeArgumentList arglist)
        {
            this.Method = methodRef;
            this.Arguments = arglist;
        }
        public CodeMethodInvokeExpression(CodeMethodReferenceExpression methodRef, CodeExpressionCollection exprs)
        {
            this.Method = methodRef;
            this.Arguments = new CodeArgumentList(exprs);
        }
        public CodeMethodReferenceExpression Method
        {
            get
            {
                return methodRefExpr;
            }
            set
            {

                methodRefExpr = value;
                if (value != null)
                {

                    AcceptChild(value, CodeObjectRoles.CodeMethodInvokeExpression_Method);
                }
            }
        }
        public CodeArgumentList Arguments
        {
            get
            {
                return arguments;
            }
            set
            {
                arguments = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeMethodInvokeExpression_Arguments);
                }
            }
        }
        public CodeExpression MethodInvokeTarget
        {
            get
            {
                return this.methodRefExpr.Target;
            }
        }
        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.MethodInvokeExpression;
            }
        }
        public override int OperatorPrecedence
        {
            get { return CodeOperatorPrecendence.METHOD_INVOKE; }
        }

        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (methodRefExpr.CodeObjectRole == newcodeExpression.CodeObjectRole)
            {
                Method = (CodeMethodReferenceExpression)newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }
    }

    //==============================================================
    public class CodeIncrementDecrementOperatorExpression : CodePrimaryOperatorExpression
    {
        CodeBinaryOperatorName op;
        CodeExpression targetExpr;
        //eg x++,i--
        public CodeIncrementDecrementOperatorExpression(CodeExpression targetExpr, bool isDecrement)
        {
            if (isDecrement)
            {
                op = CodeBinaryOperatorName.MinusMinus;
            }
            else
            {
                op = CodeBinaryOperatorName.PlusPlus;
            }
            TargetExpression = targetExpr;
        }
        public override int OperatorPrecedence
        {
            get
            {
                return CodeOperatorPrecendence.INC_DEC;

            }
        }
        public CodeBinaryOperatorName Operator
        {
            get
            {
                return op;
            }
        }
        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.IncDecOperatorExpression;
            }
        }

        public CodeExpression TargetExpression
        {
            get
            {
                return targetExpr;
            }
            set
            {

                targetExpr = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeIncrementDecrementOperatorExpression_TargetExpr);
                }

            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (newcodeExpression.CodeObjectRole == targetExpr.CodeObjectRole)
            {

                TargetExpression = newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }

    }


    public class CodeMemberAccessExpression : CodePrimaryOperatorExpression
    {
        CodeExpression targetExpr;
        CodeIdExpression memberExpr;
        /// <summary>
        /// experiment: special steps
        /// </summary>
        bool maybeSpecialMap;
        /// <summary>
        /// experiment
        /// </summary>
        bool isVirtualAccesss;
        //---------------
        public CodeMemberAccessExpression(CodeExpression targetExpr, CodeIdExpression memberExpr)
        {

            TargetExpression = targetExpr;
            MemberExpression = memberExpr;

        }
        public CodeMemberAccessExpression(string target, string mb)
        {

            TargetExpression = new CodeIdExpression(target);
            MemberExpression = new CodeIdExpression(mb);
        }
        public CodeMemberAccessExpression(string target, CodeSimpleName mb)
        {

            TargetExpression = new CodeIdExpression(target);
            MemberExpression = new CodeIdExpression(mb);
        }
        public CodeMemberAccessExpression(CodeSimpleName target, string mb)
        {

            TargetExpression = new CodeIdExpression(target);
            MemberExpression = new CodeIdExpression(mb);
        }
        public CodeMemberAccessExpression(CodeThisReferenceExpression thisref, string mb)
        {
            TargetExpression = thisref;
            MemberExpression = new CodeIdExpression(mb);
        }
        public CodeMemberAccessExpression(CodeThisReferenceExpression thisref, CodeSimpleName mb)
        {
            TargetExpression = thisref;
            MemberExpression = new CodeIdExpression(mb);
        }
        public bool IsVitualAccess
        {
            get
            {
                return this.isVirtualAccesss;
            }
            set
            {
                this.isVirtualAccesss = value;
            }
        }
        public bool MaybeSpecialMap
        {
            get
            {
                return maybeSpecialMap;
            }
            set
            {
                this.maybeSpecialMap = value;
            }
        }
        public CodeExpression TargetExpression
        {
            get
            {
                return targetExpr;
            }
            set
            {

                targetExpr = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeMemberAccessExpression_Target);
                }

            }
        }
        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.MemberAccessExpression;
            }
        }
        public CodeIdExpression MemberExpression
        {
            get
            {
                return memberExpr;
            }
            set
            {

                memberExpr = value;
                if (value != null)
                {

                    AcceptChild(value, CodeObjectRoles.CodeMemberAccessExpression_Member);
                }
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (newcodeExpression.CodeObjectRole == targetExpr.CodeObjectRole)
            {

                TargetExpression = newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            else if (newcodeExpression.CodeObjectRole == memberExpr.CodeObjectRole)
            {

                MemberExpression = (CodeIdExpression)newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }
        public override string ToString()
        {
            //need in debug and release ***
            return targetExpr.ToString() + "." + memberExpr.ToString();
        }
        public override int OperatorPrecedence
        {
            get
            {
                return CodeOperatorPrecendence.MEMBER_ACCESS;
            }
        }

    }

    /// <summary>
    /// method refernece
    /// </summary>
    public class CodeMethodReferenceExpression : CodeExpression
    {
        CodeExpression targetObject;
        CodeSimpleName methodName;

        public CodeMethodReferenceExpression(CodeExpression targetObject, CodeSimpleName methodName)
        {

            MethodName = methodName;
            Target = targetObject;
        }
        public CodeMethodReferenceExpression(CodeExpression targetObject, string methodName)
        {
            MethodName = new CodeSimpleName(methodName);
            Target = targetObject;
        }
        public CodeMethodReferenceExpression(CodeSimpleName methodName)
        {

            MethodName = methodName;
        }

        public CodeMethodReferenceExpression(string targetId, string methodName)
        {
            MethodName = new CodeSimpleName(methodName);
            Target = new CodeIdExpression(targetId);
        }



        public CodeSimpleName MethodName
        {
            get
            {
                return this.methodName;
            }
            set
            {
                this.methodName = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeMethodReferenceExpression_MethodName);
                }
            }

        }
        public string MethodNameAsString
        {
            get
            {
                return methodName.NameWithoutArity;
            }
        }
        public override int OperatorPrecedence
        {
            get
            {
                return 0;
            }
        }

        public CodeExpression Target
        {
            get
            {
                return targetObject;
            }
            set
            {

                targetObject = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeMethodReferenceExpression_Target);
                }
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (newcodeExpression.CodeObjectRole == CodeObjectRoles.CodeMethodReferenceExpression_Target)
            {
                Target = newcodeExpression;
                return CodeReplacingResult.Ok;
            }

            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }

        public override CodeExpressionKind ExpressionKind
        {
            get
            {
                return CodeExpressionKind.MethodReferenceExpression;
            }
        }
        public CodeSimpleName MethodSimpleName
        {
            get
            {
                return methodName;
            }
        }

    }



    //------------------------------------------------------------------------------
    //ParserKit  extension
    /// <summary>
    /// decoration  expression  consist of ... target (left) expession and right expression
    /// </summary>
    public class CodeDecorationExpression : CodePrimaryOperatorExpression
    {

        CodeExpression targetExpression;
        CodeStatementCollection decorationStatements;
        CodeVariableDeclarationStatement contextVarStm;

        public CodeDecorationExpression()
        {
        }

        public CodeDecorationExpression(CodeExpression targetExpr, CodeStatementCollection decorationStatements)
        {

            this.TargetExpression = targetExpr;
            this.DecorationStatements = decorationStatements;
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (targetExpression == oldcodeExpression)
            {
                this.TargetExpression = newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            else
            {
                return CodeReplacingResult.NotSupport;
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.DecorationExpression; }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }

        public object Creator
        {
            get;
            set;
        }
        public string CreatorNote
        {
            get;
            set;
        }
        public CodeExpression TargetExpression
        {
            get { return targetExpression; }

            set
            {
                targetExpression = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeDecoreExpr_TargetExpression);
                }
            }
        }

        public CodeStatementCollection DecorationStatements
        {
            get
            {
                return decorationStatements;
            }
            set
            {
                decorationStatements = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeDecoreExpr_DecorStatements);
                }
            }
        }

        public CodeVariableDeclarationStatement CompilerGeneratedContextVariableDeclStm
        {
            get
            {   //created from semantic check  steps
                return contextVarStm;
            }
            set
            {
                contextVarStm = value;
            }
        }
        /// <summary>
        /// compiler generated
        /// </summary>
        public string ContextVariableName
        {
            get
            {
                if (contextVarStm != null)
                {
                    return contextVarStm.GetDeclarator(0).VariableNameAsString;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// compiler generated
        /// </summary>
        public CodeTypeReference ContextVariableTypeInfo
        {
            get
            {
                if (contextVarStm != null)
                {
                    return contextVarStm.GetDeclarator(0).VariableType;
                }
                else
                {
                    return null;
                }
            }
        }

    }

}