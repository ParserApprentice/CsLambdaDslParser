//MIT, 2015-2017, ParserApprentice
 
using System.Text; 

namespace Parser.CodeDom
{
    static class CodeOperatorPrecendence
    {
        //primary
        public const int MEMBER_ACCESS = 0;
        public const int ARRAY_INDEXER = 0;
        public const int METHOD_INVOKE = 0;
        //unary
        public const int PLUS_UNARY = 1;
        public const int MINUS_UNARY = 1;
        public const int NOT = 1; // bank ,! , not
        public const int INC_DEC = 1; //increment-decrement

        public const int NEW = 1;
        public const int TILDE = 1;
        public const int EXPLICIT_CAST = 1;
        //public const int IMPLICIT_CAST = 1;

        public const int AS_TYPE_CONV = 5;
        //public const int AS_IF_TYPE_CONV = 5;

        public const int LAMBDA = 18;

    }

    //----------------------------------------------------
    public abstract class CodeExpression : CodeObject
    {

        /// <summary>
        /// store user's original code expression, 
        /// eg. when a compiler generates new CodeExpression with will store 
        /// original code expression here
        /// </summary>
        CodeExpression originalUserCodeExpression;

        /// <summary>
        /// for emiting
        /// </summary>
        CodeExpression compilerGeneratedReplaceExpression;

        CodeTypeReference typeReference;

#if DEBUG
        bool dbugPassSemanticCheck;
#endif

        public CodeExpression()
        {
        }


        public bool IsCompilerGen
        {
            get;
            set;
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
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.Expression; }
        }

#if DEBUG

        public bool dbug_IsPassSemanticCheck
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
#endif


        public abstract CodeExpressionKind ExpressionKind { get; }
        public virtual CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            //each code expression has vary details of how to replace its child expression
            return CodeReplacingResult.NotSupport;
        }

        public CodeTypeReference TypeReference
        {
            get
            {
                return typeReference;
            }
        }

        internal void SetReferingCodeTypeReference(CodeTypeReference codetyperef)
        {
            this.typeReference = codetyperef;
        }

#if DEBUG
        protected string ToCodeString()
        {

            StringBuilder stBuilder = new StringBuilder();
            Parser.AsmInfrastructures.AsmIndentTextWriter writer =
                new Parser.AsmInfrastructures.AsmIndentTextWriter(stBuilder);
            Parser.AsmInfrastructures.CodeDomToSourceCodeConverter.GenerateExpression(this, writer);
            return stBuilder.ToString();
        }
        public override string ToString()
        {
            return ToCodeString();
        }
#endif  
        public CodeStatementCollection ParentCodeBlock
        {

            get
            {
                CodeObject parentCodeObject = this.ParentCodeObject;
                if (parentCodeObject != null)
                {
                    if (parentCodeObject is CodeExpression)
                    {
                        return ((CodeExpression)parentCodeObject).ParentCodeBlock;
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


        public abstract int OperatorPrecedence
        {
            get;
        }
        public static void StoreOriginalUserCodeExpression(CodeExpression targetExpression, CodeExpression originalUserCodeExpr)
        {
            targetExpression.originalUserCodeExpression = originalUserCodeExpr;
        }

        public CodeExpression OriginalUserCodeExpression
        {
            get
            {
                return this.originalUserCodeExpression;
            }
        }

        public static void SetCompilerGenerateExpressionForEmit(CodeExpression targetExpression, CodeExpression compilerGeneratedExpression)
        {
            targetExpression.compilerGeneratedReplaceExpression = compilerGeneratedExpression;

        }

        public static CodeExpression GetCompilerGeneratedCodeExpression(CodeExpression fromExpression)
        {
            return fromExpression.compilerGeneratedReplaceExpression;
        }

        public bool HasCompilerGeneratedReplacedExpression
        {
            get
            {
                return compilerGeneratedReplaceExpression != null;
            }
        }

        public CodeExpression CreateProxyExpression()
        {
            return new CodeProxyExpression(this);
        }
        public virtual CodeExpression GetInnerExpression()
        {
            return this;
        }
        class CodeProxyExpression : CodeExpression
        {
            readonly CodeExpression original;
            public CodeProxyExpression(CodeExpression original)
            {
                this.original = original;
            }
            public CodeExpression OriginalExpression
            {
                get
                {
                    return this.original;
                }
            }
            public override CodeExpressionKind ExpressionKind
            {
                get
                {
                    return CodeExpressionKind.ProxyExpresion;
                }
            }
            public override int OperatorPrecedence
            {
                get { return this.original.OperatorPrecedence; }
            }
            public override CodeExpression GetInnerExpression()
            {
                return this.original;
            }
        }
    }

    public interface ICodeTransform
    {

    }
}
