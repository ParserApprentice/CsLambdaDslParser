//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Parser.AsmInfrastructures;

namespace Parser.CodeDom
{


    public enum CodeStatementType
    {
        Unknown,
        CodeBreakStatement,
        CodeContinueStatement,
        CodeIfElseStatement,
        CodeDoWhileStatement,
        CodeExpressionStatement,
        CodeForEachStatement,
        CodeForLoopStatement,
        CodeReturnStatement,
        CodeBlockStatement,
        CodeSwitchStatement,
        CodeWhileStatement,
        CodeEmptyStatement,

        CodeTryCatchFinallyStatement,
        CodeThrowExceptionStatement,

        CodeYieldReturnStatement,
        CodeYieldBreakStatement,
        CodeVariableDeclarationStatement,


        CodeSQLStatementExpression,

        //generate by compiler?
        CodeGoToStatement,
        CodeLeaveStatement,
        CodeLabelStatement,

        //experiment: for compiler optimization
        CodeBreakPointStatement,
        CodeStartSpecialSegment,
        CodeEndSpecialSegment,
        CodeCommentStatement,


        CodeDefineStatement,
        CodeCheckedStatement,
        CodeUnCheckedStatement,
        CodeLockStatement,


    }

    public abstract class CodeStatement : CodeObject
    {


        CodeStatement compilerReplaceStatement;


        int statementFlags;
        internal const int IS_COMPILER_GEN = 1 << (9 - 1);

        internal const int HAS_COMPILER_GEN_STM = 1 << (11 - 1);
        internal const int HAS_COMMENTS = 1 << (12 - 1);
        internal const int HAS_LOCATION = 1 << (13 - 1);

        //-----------------------------------------------------
        internal const int STM_RETURN_EXPR = 1 << (14 - 1);
        //-----------------------------------------------------
        internal const int STM_VARDECL_EXPLICIT = 1 << (14 - 1);
        internal const int STM_VARDECL_HAS_INIT = 1 << (15 - 1);
        //-----------------------------------------------------
        internal const int STM_FOR_LOOP_HAS_INIT = 1 << (14 - 1);
        internal const int STM_FOR_LOOP_HAS_COND = 1 << (15 - 1);
        internal const int STM_FOR_LOOP_HAS_INC = 1 << (16 - 1);
        //-----------------------------------------------------
        internal const int STM_DEFINE_HAS_CUSTOM_ATTRS = 1 << (14 - 1);
        //-----------------------------------------------------
#if DEBUG

        CodeStatement dbug_originalUserCodeStatement;
#endif

        public CodeStatement()
        {
        }

        public abstract CodeStatementType StatementType { get; }

        public virtual CodeReplacingResult ReplaceChildExpression(CodeExpression childExpr)
        {
            return CodeReplacingResult.NotSupport;
        }
        public bool HasExtraTransformCodeNote
        {
            get
            {
                return this.ExtraTransformCodeNote != null;
            }
        }
        public ICodeTransform ExtraTransformCodeNote
        {
            get;
            set;
        }

        public CodeStatement CompilerReplaceStatement
        {
            get
            {
                return compilerReplaceStatement;
            }
            set
            {
                this.compilerReplaceStatement = value;
                value.statementFlags |= IS_COMPILER_GEN;
#if DEBUG
                value.dbug_originalUserCodeStatement = this;
#endif
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.Statement; }
        }

        public bool IsCompilerGen
        {
            get
            {
                return (statementFlags & IS_COMPILER_GEN) != 0;
            }
            set
            {
                if (value)
                {
                    statementFlags |= IS_COMPILER_GEN;
                }
                else
                {
                    statementFlags &= ~IS_COMPILER_GEN;
                }
            }
        }

        internal static int GetStatmentFlags(CodeStatement stm)
        {
            return stm.statementFlags;
        }

#if DEBUG

        bool dbugPassSemanticCheck;
        internal bool dbug_IsPassSemanticCheck
        {
            get
            {
                return dbugPassSemanticCheck;
            }
            set
            {
                dbugPassSemanticCheck = value;
            }
        }
        protected string debug_ToCodeString()
        {

            StringBuilder stBuilder = new StringBuilder();
            Parser.AsmInfrastructures.AsmIndentTextWriter writer = new
                Parser.AsmInfrastructures.AsmIndentTextWriter(stBuilder);
            if (this.CompilerReplaceStatement != null)
            {
                Parser.AsmInfrastructures.CodeDomToSourceCodeConverter.GenerateStatement(this.CompilerReplaceStatement, writer);
            }
            else
            {
                Parser.AsmInfrastructures.CodeDomToSourceCodeConverter.GenerateStatement(this, writer);
            }
            return stBuilder.ToString();
        }
        public override string ToString()
        {
            return debug_ToCodeString();

        }
#endif
        public CodeStatementCollection ParentCodeBlock
        {
            get
            {
                CodeObject parentCodeObject = ParentCodeObject;
                if (parentCodeObject != null)
                {
                    if (parentCodeObject is CodeStatementCollection)
                    {
                        return (CodeStatementCollection)parentCodeObject;
                    }
                    else if (parentCodeObject is CodeStatement)
                    {
                        return ((CodeStatement)parentCodeObject).ParentCodeBlock;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }


    }

    public class CodeLocalVarDecl
    {
        public string LocalVarName;
        public CodeExpression InitExpression;
    }
    public class CodeLocalVarDeclStatement : CodeStatement
    {
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeVariableDeclarationStatement; }
        }

    }
    public class CodeEmptyStatement : CodeStatement
    {
        public CodeEmptyStatement()
        {

        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeEmptyStatement; }
        }

    }

    public class CodeGroupStatement : CodeStatement
    {

        bool needAutoGenTripleLables;
        CodeStatementCollection stms;
        public CodeGroupStatement()
        {
        }
        public CodeGroupStatement(CodeStatementCollection stms)
        {
            this.Body = stms;
        }
        public override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeBlockStatement;
            }
        }
        public CodeStatementCollection Body
        {
            get
            {
                return stms;
            }
            set
            {
                stms = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeStatementCollection);
                }
            }
        }
        public bool NeedAutoGenTripleLables
        {
            get
            {
                return this.needAutoGenTripleLables;
            }
            set
            {
                this.needAutoGenTripleLables = value;
            }
        }
    }

    public class CodeBlockStatement : CodeStatement
    {
        public CodeBlockStatement()
        {
            
        }
        public List<CodeStatement> Body { get; set; }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeBlockStatement; }
        }
    }
    public class CodeBreakStatement : CodeStatement
    {

        public CodeBreakStatement()
        {

        }
        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeBreakStatement;
            }
        }

    }
    public class CodeContinueStatement : CodeStatement
    {
        public CodeContinueStatement()
        {

        }
        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeContinueStatement;
            }
        }
    }

    public class CodeVariableDeclaratorExpression : CodeExpression
    {
        CodeParameterDeclarationExpression internalParameter;

        public CodeVariableDeclaratorExpression(CodeParameterDeclarationExpression par)
        {
            this.internalParameter = par;
        }
        public CodeVariableDeclaratorExpression(string variableName,
          CodeTypeReference variableType, CodeExpression initExpression)
        {
            this.internalParameter = new CodeParameterDeclarationExpression(variableName, variableType);
            this.internalParameter.DefaultValueExpression = initExpression;
        }

        public CodeVariableDeclaratorExpression(string variableName, bool startImplcitTypeDecl, CodeExpression initExpression)
        {
            this.internalParameter = new CodeParameterDeclarationExpression(variableName, null);
            this.internalParameter.DefaultValueExpression = initExpression;
            this.internalParameter.UseImplicitType = startImplcitTypeDecl;
        }
        public CodeVariableDeclaratorExpression(string variableName, CodeTypeReference variableType)
        {
            this.internalParameter = new CodeParameterDeclarationExpression(variableName, variableType);
        }
        //--------------------------------------------------------------------------------------

        public CodeVariableDeclaratorExpression(CodeSimpleName varSimpleName, bool startImplcitTypeDecl)
        {
            this.internalParameter = new CodeParameterDeclarationExpression(varSimpleName, null);
            this.internalParameter.UseImplicitType = startImplcitTypeDecl;
        }
        public CodeVariableDeclaratorExpression(CodeSimpleName varSimpleName, CodeTypeReference variableType)
        {
            this.internalParameter = new CodeParameterDeclarationExpression(varSimpleName, variableType);
        }
        //--------------------------------------------------------------------------------------
        public CodeVariableDeclaratorExpression(CodeSimpleName varSimpleName)
        {
            //new 2014***
            //this.internalParameter = new CodeParameterDeclarationExpression(varSimpleName, variableType);
            this.internalParameter = new CodeParameterDeclarationExpression();
            this.internalParameter.ParameterName = varSimpleName;
        }
        //--------------------------------------------------------------------------------------
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.VariableDeclarator; }
        }

        public CodeParameterDeclarationExpression InternalParameterDecl
        {
            get
            {
                return this.internalParameter;
            }
        }

        public CodeExpression InitExpression
        {
            get
            {
                return this.internalParameter.DefaultValueExpression;
            }
            set
            {
                this.internalParameter.DefaultValueExpression = value;
            }
        }

        public string VariableNameAsString
        {
            get
            {
                return this.internalParameter.ParameterNameAsString;
            }
        }


        public CodeTypeReference VariableType
        {
            get
            {
                return this.internalParameter.ParameterType;
            }
            private set
            {
                this.internalParameter.SetReferingCodeTypeReference(value);
            }
        }
        public void SetImplicitVariableType(CodeTypeReference typeRef)
        {
            this.internalParameter.SetReferingCodeTypeReference(typeRef);
            this.internalParameter.SetParameterTypeReference(typeRef);

        }

        public bool StartWithImplicitTypeDeclaration
        {
            get
            {
                return this.internalParameter.UseImplicitType;

            }
            set
            {
                this.internalParameter.UseImplicitType = value;
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.VariableDeclarator; }
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
    }
    public class CodeVariableDeclarationStatement : CodeStatement
    {

        CodeVariableDeclaratorExpression defaultVariableDeclarator;
        List<CodeVariableDeclaratorExpression> moreVarDeclarators;

        public CodeVariableDeclarationStatement(CodeVariableDeclaratorExpression defaultVariableDeclarator)
        {
            this.defaultVariableDeclarator = defaultVariableDeclarator;
        }

        public CodeVariableDeclarationStatement(string variableName,
          CodeTypeReference variableType, CodeExpression initExpression)
        {
            CodeParameterDeclarationExpression internalParameter = new CodeParameterDeclarationExpression(variableName, variableType);
            internalParameter.DefaultValueExpression = initExpression;
            this.defaultVariableDeclarator = new CodeVariableDeclaratorExpression(internalParameter);
        }

        public CodeVariableDeclarationStatement(string variableName, bool startImplcitTypeDecl, CodeExpression initExpression)
        {
            CodeParameterDeclarationExpression internalParameter = new CodeParameterDeclarationExpression(variableName, null);
            internalParameter.DefaultValueExpression = initExpression;
            internalParameter.UseImplicitType = startImplcitTypeDecl;
            this.defaultVariableDeclarator = new CodeVariableDeclaratorExpression(internalParameter);
        }
        public CodeVariableDeclarationStatement(string variableName, CodeTypeReference variableType)
        {

            this.defaultVariableDeclarator = new CodeVariableDeclaratorExpression(
                new CodeParameterDeclarationExpression(variableName, variableType));
        }
        //--------------------------------------------------------------------------------------
        public CodeVariableDeclarationStatement(CodeSimpleName varSimpleName, bool startImplcitTypeDecl)
        {
            CodeParameterDeclarationExpression internalParameter = new CodeParameterDeclarationExpression(varSimpleName, null);
            internalParameter.UseImplicitType = startImplcitTypeDecl;
            this.defaultVariableDeclarator = new CodeVariableDeclaratorExpression(internalParameter);
        }
        public CodeVariableDeclarationStatement(CodeSimpleName varSimpleName, CodeTypeReference variableType)
        {
            this.defaultVariableDeclarator = new CodeVariableDeclaratorExpression(new CodeParameterDeclarationExpression(varSimpleName, variableType));
        }

        //--------------------------------------------------------------------------------------

        public void AddVariableDeclarator(CodeVariableDeclaratorExpression varDel)
        {

            if (this.moreVarDeclarators == null)
            {
                this.moreVarDeclarators = new List<CodeVariableDeclaratorExpression>();
                this.moreVarDeclarators.Add(this.defaultVariableDeclarator);
            }
            this.moreVarDeclarators.Add(varDel);

        }
        public int DeclaratorCount
        {
            get
            {
                if (this.moreVarDeclarators == null)
                {
                    return 1;//default
                }
                else
                {
                    return this.moreVarDeclarators.Count;
                }
            }
        }
        public CodeVariableDeclaratorExpression GetDeclarator(int index)
        {
            if (this.moreVarDeclarators == null)
            {
                return this.defaultVariableDeclarator;
            }
            else
            {
                return this.moreVarDeclarators[index];
            }
        }


        public override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeVariableDeclarationStatement;
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression childExpr)
        {
            throw new NotSupportedException();
            //if (childExpr.CodeObjectRole == CodeObjectRoles.CodeVariableDeclStatement_InitExpression)
            //{
            //    InitExpression = childExpr;
            //    return CodeReplacingResult.Ok;
            //}
            //else
            //{
            //    return CodeReplacingResult.NotSupport;
            //}
        }

    }


    public class CodeReturnStatement : CodeStatement
    {

        CodeExpression returnExpr;
        public CodeReturnStatement()
        {

        }
        public CodeReturnStatement(CodeExpression returnExpression)
        {
            this.Expression = returnExpression;
        }
        /// <summary>
        /// expression to be returned
        /// </summary>
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
                    AcceptChild(value, CodeObjectRoles.CodeReturnStatement_ReturnExpr);
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
                return CodeStatementType.CodeReturnStatement;
            }
        }
    }



    public class CodeExpressionStatement : CodeStatement
    {
        CodeExpression exp;

        public CodeExpressionStatement(CodeExpression exp)
        {
            this.Expression = exp;

        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression childExpr)
        {

            this.Expression = childExpr;
            return CodeReplacingResult.Ok;
        }
        public CodeExpression Expression
        {
            get
            {
                return exp;
            }
            set
            {
                exp = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeExpressionStatement_Expr);
                }
            }
        }
        public sealed override CodeStatementType StatementType
        {
            get
            {
                return CodeStatementType.CodeExpressionStatement;
            }
        }
    }

    //-------------------------------------------------------------------------------------
    public class CodeBreakPointStatement : CodeStatement
    {

        CodeExpression literalSourceFile;
        CodeExpression literalLineNumber;

        public CodeBreakPointStatement()
        {

        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeBreakPointStatement; }
        }
    }
    public class CodeStartSpecialSegmentStatement : CodeStatement
    {

        string name;
        public CodeStartSpecialSegmentStatement(string name)
        {

            this.name = name;

        }
        public CodeStartSpecialSegmentStatement()
        {
        }
        public string SegmentName
        {
            get
            {
                return name;
            }
        }

        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeStartSpecialSegment; }
        }
    }

    //---------------------------------------------------------------------

    public class CodeEndSpecialSegmentStatement : CodeStatement
    {
        string segmentName;
        public CodeEndSpecialSegmentStatement()
        {

        }
        public CodeEndSpecialSegmentStatement(string segmentName)
        {

            this.segmentName = segmentName;

        }
        public string SegmentName
        {
            get
            {
                return segmentName;
            }

        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeEndSpecialSegment; }
        }

    }
    //---------------------------------------------------------------------
    public enum GotoTargetType
    {
        Label,
        Case,
        Default
    }
    public class CodeGotoStatement : CodeStatement
    {

        string labelName;
        GotoTargetType labelTargetType;
        CodeExpression constantExpression;
        public CodeGotoStatement(GotoTargetType labelTargetType)
        {
            this.labelTargetType = labelTargetType;
        }
        public GotoTargetType TargetType
        {
            get
            {
                return this.labelTargetType;
            }
            set
            {
                this.labelTargetType = value;
            }
        }
        public string LabelName
        {
            get
            {
                return labelName;
            }
            set
            {
                this.labelName = value;
            }
        }
        public CodeExpression CaseExpression
        {
            get
            {
                return constantExpression;
            }
            set
            {
                this.constantExpression = value;
            }
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeGoToStatement; }
        }
    }
    public class CodeLeaveStatement : CodeStatement
    {
        string labelName;
        public CodeLeaveStatement(string labelName)
        {

            this.labelName = labelName;
        }
        public string LabelName
        {
            get
            {
                return labelName;
            }
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeLeaveStatement; }
        }
    }
    public class CodeLabelStatement : CodeStatement
    {
        string labelName;
        CodeStatement codeStatement;
        public CodeLabelStatement(string labelName)
        {

            this.labelName = labelName;
        }
        public string LabelName
        {
            get
            {
                return labelName;
            }
        }
        public CodeStatement Statement
        {
            get
            {
                return this.codeStatement;
            }
            set
            {
                this.codeStatement = value;
            }
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeLabelStatement; }
        }
    }
    public class CodeCheckedStatement : CodeStatement
    {
        CodeBlockStatement blockStatement;
        public CodeCheckedStatement(CodeBlockStatement blockStatement)
        {
            this.Block = blockStatement;
        }
        public CodeBlockStatement Block
        {
            get
            {
                return this.blockStatement;
            }
            set
            {
                this.blockStatement = value;
            }
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeCheckedStatement; }
        }
    }
    public class CodeUnCheckedStatement : CodeStatement
    {
        CodeBlockStatement blockStatement;
        public CodeUnCheckedStatement(CodeBlockStatement blockStatment)
        {
            this.Block = blockStatment;
        }
        public CodeBlockStatement Block
        {
            get
            {
                return this.blockStatement;
            }
            set
            {
                this.blockStatement = value;
            }
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeUnCheckedStatement; }
        }
    }

    public class CodeLockStatement : CodeStatement
    {
        CodeStatement stmt;
        CodeExpression expression;
        public CodeExpression Exprssion
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
            }

        }
        public CodeStatement Statement
        {
            get
            {
                return this.stmt;
            }
            set
            {
                this.stmt = value;
            }
        }
        public override CodeStatementType StatementType
        {
            get { return CodeStatementType.CodeLockStatement; }
        }
    }
}