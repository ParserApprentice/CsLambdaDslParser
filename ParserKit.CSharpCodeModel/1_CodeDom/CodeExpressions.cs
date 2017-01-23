//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;
using Parser.AsmInfrastructures;

namespace Parser.CodeDom
{

    public class CodeCheckedExpression : CodeExpression
    {
        CodeExpression innerExpression;
        public CodeCheckedExpression(CodeExpression innerExpression)
        {
            this.innerExpression = innerExpression;
        }
        public CodeExpression Expression
        {
            get
            {
                return this.innerExpression;
            }
        }

        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.CheckedExpression; }
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class CodeUnCheckedExpression : CodeExpression
    {
        CodeExpression innerExpression;
        public CodeUnCheckedExpression(CodeExpression innerExpression)
        {
            this.Expression = innerExpression;
        }
        public CodeExpression Expression
        {
            get
            {
                return this.innerExpression;
            }
            set
            {
                this.innerExpression = value;
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.UnCheckedExpression; }
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class CodeConditionalExpression : CodeExpression
    {
        CodeExpression testExpression;
        CodeExpression condTrueExpression;
        CodeExpression condFalseExpression;
        public CodeExpression TestExpression
        {
            get
            {
                return this.testExpression;
            }
            set
            {
                this.testExpression = value;
            }
        }
        public CodeExpression ConditionTrueExpression
        {
            get
            {
                return this.condTrueExpression;
            }
            set
            {
                this.condTrueExpression = value;
            }
        }
        public CodeExpression ConditionFalseExpression
        {
            get
            {
                return this.condFalseExpression;
            }
            set
            {
                this.condFalseExpression = value;
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.ConditionalExpression; }
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
    }
    public class CodeTypeOfExprssion : CodeExpression
    {
        CodeNamedItem typeName;
        public CodeNamedItem TypeName
        {
            get
            {
                return this.typeName;
            }
            set
            {
                this.typeName = value;
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.TypeOfExprssion; }
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
    }
    public class CodeAsTypeConversionExpression : CodeTypeConversionExpression
    {
        public CodeAsTypeConversionExpression()
        {
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.AsConversionExpression; }
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class CodeDefaultExpression : CodeExpression
    {
        public CodeDefaultExpression(CodeTypeReference typeReference)
        {
            this.TypeReference = typeReference;
        }
        public CodeTypeReference TypeReference
        {
            get;
            private set;
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.DefaultExpression; }
        }
        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
    }
    public class CodeExplicitTypeConversionExpression : CodeTypeConversionExpression
    {

        public CodeExplicitTypeConversionExpression(CodeExpression targetExpr, CodeTypeReference convTypeRef)
        {
            this.CastedExpression = targetExpr;
            this.TargetType = convTypeRef;

        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.ExplicitConversionExpression; }
        }
        public override int OperatorPrecedence
        {
            get
            {
                return CodeOperatorPrecendence.EXPLICIT_CAST;
            }
        }

    }

    public abstract class CodeTypeConversionExpression : CodeExpression
    {

        CodeExpression castedExpr;
        CodeTypeReference targetType;


        public CodeTypeConversionExpression()
        {

        }

        public CodeExpression CastedExpression
        {
            get { return castedExpr; }
            set
            {
                this.castedExpr = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeCastExpression_Expression);
                }
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            CastedExpression = newcodeExpression;
            return CodeReplacingResult.Ok;
        }
        public CodeTypeReference TargetType
        {
            get
            {
                return targetType;
            }
            set
            {
                targetType = value;

            }
        }


    }


    //===================================================================== 
    /// <summary>
    /// code expression that has enclosed with parenthesis 
    /// </summary>
    public class CodeParenthizedExpression : CodeExpression
    {

        CodeExpression contentExpression;
        public CodeParenthizedExpression(CodeExpression content)
        {
            ContentExpression = content;
        }
        public CodeExpression ContentExpression
        {
            get
            {
                return contentExpression;
            }
            set
            {

                contentExpression = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeParenthizedExpression_Content);
                }
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (contentExpression.CodeObjectRole == newcodeExpression.CodeObjectRole)
            {
                ContentExpression = newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }

        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.ParenthizedExpression;
            }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }

        public bool CanbeCastExpression
        {
            get
            {
                return (contentExpression is CodeIdExpression) ||
                    (contentExpression is CodeMemberAccessExpression);
            }
        }
    }


    public class CodeThisReferenceExpression : CodeExpression
    {

        public CodeThisReferenceExpression()
        {

        }

        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.ThisReferenceExpression;
            }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }

    }


    public class CodeBaseReferenceExpression : CodeExpression
    {


        public CodeBaseReferenceExpression()
        {

        }
#if DEBUG
        public override string ToString()
        {
            return "base";
        }
#endif
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.BaseRefererneceExpression;
            }
        }
    }



    public class CodeUnaryOperatorExpression : CodeBinaryOperatorExpression
    {
        //left operand is always 'empty expression'
        //eg!z,++a,--h,-i,

        public CodeUnaryOperatorExpression(CodeBinaryOperatorName binopName)
            : base(binopName)
        {

        }
        public CodeUnaryOperatorExpression(CodeBinaryOperatorName binopName, CodeExpression expr)
            : base(binopName)
        {
            this.RightExpression = expr;
        }
        public CodeExpression Expression
        {
            get
            {
                return RightExpression;
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {

            if (RightExpression.CodeObjectRole == newcodeExpression.CodeObjectRole)
            {
                RightExpression = newcodeExpression;
                return CodeReplacingResult.Ok;
            }
            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }
        public override CodeExpressionKind ExpressionKind
        {
            get
            {
                return CodeExpressionKind.UnaryOperatorExpression;
            }
        }

    }

    public enum CodeBinaryOperatorName
    {
        Assign,
        Plus,
        Minus,
        Multiply,
        Div,
        Percent,
        Caret,
        PlusPlus,
        MinusMinus,
        Bang,
        Tile,
        LeftShift,
        RightShift,


        PlusAssign,
        MinusAssign,
        ModuloAssign,
        MultiplyAssign,
        DivAssign,
        CaretAssign,
        LeftShiftAssign,
        RightShiftAssign,
        ConditionalAssign,
        AndAssign,
        OrAssign,

        BitwiseOr,
        BitwiseAnd,
        LogicalOr,
        LogicalAnd,

        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,


        Quest,
        DoubleQuest,//double quest ??, null coalese

        Is,
        As,

    }
    public class CodeBinaryOperatorExpression : CodeExpression
    {
        CodeExpression left;
        CodeExpression right;
        CodeBinaryOperatorName binOp;
        bool isDynamicOperation;
        public CodeBinaryOperatorExpression()
        {
        }
        public CodeBinaryOperatorExpression(CodeBinaryOperatorName binOp)
        {
            this.binOp = binOp;
        }
        public CodeBinaryOperatorExpression(CodeBinaryOperatorName binOp, CodeExpression leftExpr)
        {
            this.binOp = binOp;
            this.LeftExpression = leftExpr;
        }
        public CodeBinaryOperatorExpression(CodeExpression leftExpr, CodeBinaryOperatorName binOp, CodeExpression rightExpr)
        {
            this.binOp = binOp;
            this.LeftExpression = leftExpr;
            this.RightExpression = rightExpr;
        }

        public bool IsDynamicOperation
        {
            get
            {

                return isDynamicOperation;
            }
            set
            {

                this.isDynamicOperation = value;
            }
        }
        public CodeExpression LeftExpression
        {
            get
            {
                return left;
            }
            set
            {

                left = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeBinaryOperatorExpression_Left);
                }
            }
        }
        public CodeExpression RightExpression
        {
            get
            {
                return right;
            }
            set
            {
                //--------------------------------------
                if (right != null)
                {

                    CodeObject.ClearChildFunction(right);
                }
                //--------------------------------------

                right = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeBinaryOperatorExpression_Right);
                }
            }
        }
        public CodeBinaryOperatorName BinaryOp
        {
            get
            {
                return binOp;
            }
            set
            {
                binOp = value;
            }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            if (left.CodeObjectRole == newcodeExpression.CodeObjectRole)
            {
                LeftExpression = newcodeExpression;

                return CodeReplacingResult.Ok;
            }
            else if (right.CodeObjectRole == newcodeExpression.CodeObjectRole)
            {
                RightExpression = newcodeExpression;
                return CodeReplacingResult.Ok;
            }

            return base.ReplaceChildExpression(oldcodeExpression, newcodeExpression);
        }


        public override CodeExpressionKind ExpressionKind
        {
            get
            {
                return CodeExpressionKind.BinaryOperatorExpression;
            }
        }

        public override int OperatorPrecedence
        {
            get
            {
                return binaryExpPrecedence;
            }
        }
        int binaryExpPrecedence;

        public static CodeBinaryOperatorExpression CreateAssignmentExpression(CodeExpression left, CodeExpression right)
        {
            CodeBinaryOperatorExpression binOpAssign = new CodeBinaryOperatorExpression(CodeBinaryOperatorName.Assign);
            binOpAssign.LeftExpression = left;
            binOpAssign.RightExpression = right;
            return binOpAssign;
        }
    }

    public class CodeIsOperatorExpression : CodeBinaryOperatorExpression
    {
        public CodeIsOperatorExpression()
            : base(CodeBinaryOperatorName.Is)
        {
        }
        public override int OperatorPrecedence
        {
            get
            {
                return 10;// same eq expression
            }
        }
    }

    public class CodeAsOperatorExpression : CodeBinaryOperatorExpression
    {
        public CodeAsOperatorExpression()
            : base(CodeBinaryOperatorName.As)
        {
        }
        public override int OperatorPrecedence
        {
            get
            {
                return 10;// same eq expression
            }
        }
    }
    public enum PrimitiveTokenName
    {
        Unknown,
        True,
        False,
        Character,
        String,
        Integer,
        Double,
        Float,
        Decimal

    }
    public class CodePrimitiveExpression : CodeExpression
    {

        PrimitiveTokenName primTokenName;
        string primValue;


        public CodePrimitiveExpression(PrimitiveTokenName tkName, string value)
        {

            this.primTokenName = tkName;
            this.primValue = value;

        }



        public string Value
        {
            get
            {
                return primValue;
            }
        }
        public override string ToString()
        {
            return this.Value;
        }
        public PrimitiveTokenName PrimitiveTokenName
        {
            get
            {
                return primTokenName;
            }

        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }

        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.PrimitiveExpression;
            }
        }
    }


    public class CodeParameterDeclarationExpression : CodeExpression
    {

        //eg  int index = -1;  
        CodeTypeReference parTypeRef;
        CodeExpression defaultValueExpression;
        CodeSimpleName parName;
        FieldDirection fieldDirection;
        CodeStatement compilerReplaceStatement;

        CodeAttributeDeclarationCollection customAttrs;
        bool useImplicitType;

        public CodeParameterDeclarationExpression()
        {

        }
        public CodeParameterDeclarationExpression(string parName, CodeTypeReference typeref)
        {
            this.ParameterName = new CodeSimpleName(parName);
            this.parTypeRef = typeref;

        }
        public CodeParameterDeclarationExpression(CodeSimpleName parName, CodeTypeReference typeref)
        {
            this.ParameterName = parName;
            this.parTypeRef = typeref;
        }
        public CodeParameterDeclarationExpression(CodeTypeReference typeref)
        {

            this.parTypeRef = typeref;
        }

        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            CodeObject.ClearChildFunction(oldcodeExpression);
            this.DefaultValueExpression = newcodeExpression;
            return CodeReplacingResult.Ok;

        }
        public CodeStatement CompilerReplaceStatement
        {
            get
            {
                return this.compilerReplaceStatement;
            }
            set
            {
                this.compilerReplaceStatement = value;
            }
        }

        public bool IsSpecialContextVariable
        {
            get;
            set;
        }
        public bool UseImplicitType
        {
            get
            {
                return this.useImplicitType;
            }
            set
            {
                this.useImplicitType = value;
            }
        }
        public FieldDirection FieldDirection
        {
            get
            {
                return fieldDirection;
            }
            set
            {
                fieldDirection = value;
            }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get
            {
                return CodeExpressionKind.ParameterDeclarationExpression;
            }
        }
        public bool IsImplicitParameterType
        {
            get;
            set;
        }
        public CodeExpression DefaultValueExpression
        {
            get
            {
                return defaultValueExpression;
            }
            set
            {
                defaultValueExpression = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeParameterDecl_DefaultValueExpression);
                    IsOptional = true;
                }
                else
                {
                    IsOptional = false;
                }
            }
        }
        public bool IsOptional
        {
            get;
            set;
        }
        public bool MarkedWithParams
        {
            get;
            set;
        }
        public bool MarkedWithThisKeyword
        {
            get;
            set;
        }
        public CodeTypeReference ParameterType
        {
            get
            {
                return parTypeRef;
            }
        }
        public void SetParameterTypeReference(CodeTypeReference typeref)
        {
            this.parTypeRef = typeref;

        }
        public string ParameterNameAsString
        {
            get
            {
                return parName.NormalName;
            }
        }
        public CodeSimpleName ParameterName
        {
            get
            {
                return parName;
            }
            set
            {
                parName = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeParameterDeclExpression_ParameterName);
                }
            }
        }
        public CodeAttributeDeclarationCollection CustomAttributes
        {
            get
            {
                return this.customAttrs;
            }
            set
            {
                this.customAttrs = value;
            }
        }
    }


    public class CodeDefineFieldDeclarationExpression : CodeExpression
    {

        CodeTypeReference fieldTypeRef;
        CodeExpression defaultValueExpression;
        CodeSimpleName fieldName;
        bool isOptional;
        bool maybeMultipleField;
        CodeAttributeDeclarationCollection customAttributes;


        public CodeDefineFieldDeclarationExpression()
        {

        }
        public CodeDefineFieldDeclarationExpression(string parName, CodeTypeReference typeref)
        {
            this.FieldName = new CodeSimpleName(parName);
            this.fieldTypeRef = typeref;

        }
        public CodeDefineFieldDeclarationExpression(CodeSimpleName parName, CodeTypeReference typeref)
        {
            this.FieldName = parName;
            this.fieldTypeRef = typeref;
        }
        public CodeDefineFieldDeclarationExpression(CodeTypeReference typeref)
        {

            this.fieldTypeRef = typeref;
        }

        public CodeAttributeDeclarationCollection CustomAttributes
        {
            get
            {

                return customAttributes;
            }
            set
            {
                customAttributes = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeAttributeDeclarationCollection);
                }
            }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get
            {
                return CodeExpressionKind.DefineFieldDeclarationExpression;
            }
        }


        public CodeExpression DefaultValueExpression
        {
            get
            {
                return defaultValueExpression;
            }
            set
            {
                defaultValueExpression = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeParameterDecl_DefaultValueExpression);
                }
            }
        }
        public bool IsOptional
        {
            get
            {
                return isOptional;
            }
            set
            {
                isOptional = value;
            }
        }

        public CodeTypeReference FieldType
        {
            get
            {
                return fieldTypeRef;
            }
        }
        public void SetFieldTypeReference(CodeTypeReference typeref)
        {
            this.fieldTypeRef = typeref;
        }
        public string FieldNameAsString
        {
            get
            {
                return fieldName.NormalName;
            }
        }
        public CodeSimpleName FieldName
        {
            get
            {
                return fieldName;
            }
            set
            {
                fieldName = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeDefineFieldDeclExpression_DefaultValue);
                }
            }
        }
        public bool MayBeMultipleFields
        {
            get
            {
                return maybeMultipleField;
            }
            set
            {
                maybeMultipleField = value;
            }
        }

    }


    public sealed class CodeIdExpression : CodeExpression
    {

        //SemanticSymbol mapToSemanticSymbol;
        CodeSimpleName simpleNamedItem;

        public CodeIdExpression(string name)
        {


            this.SimpleName = new CodeSimpleName(name);
        }
        public CodeIdExpression(CodeSimpleName codeSimpleName)
        {
            this.SimpleName = codeSimpleName;
            SetCodeArea(this, codeSimpleName.SourceLocation);
        }
        public CodeSimpleName SimpleName
        {
            get
            {
                return simpleNamedItem;
            }
            private set
            {
                this.simpleNamedItem = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeIdExpression_IdName);
                }
            }
        }
        public CodeSimpleName Name
        {
            get
            {
                return simpleNamedItem;
            }
        }
        public string NameAsString
        {
            get
            {
                return simpleNamedItem.NameWithoutArity;
            }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
        public override string ToString()
        {
            //need in release version
            return simpleNamedItem.FullName;
        }
        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.IdExpression;
            }
        }

    }
    public enum CodeTypeReferenceOptions
    {

        GlobalReference,
        GenericTypeParameter,
        SpecialMap,
    }


    public class CodeAnonymousObjectCreationExpression : CodeExpression
    {
        public CodeObjectInitializer objInitializer;
        public override CodeExpressionKind ExpressionKind
        {
            get
            {
                return CodeExpressionKind.AnonymousObjectCreateExpression;
            }
        }
        public override int OperatorPrecedence
        {
            get
            {
                return 10;
            }
        }
    }
    public class CodeObjectInitializer
    {
        public CodeMemberDeclarators memberDecls;
    }

    public class CodeMemberDeclarators
    {
        public List<CodeMemberDeclarator> declarators = new List<CodeMemberDeclarator>();

    }
    public class CodeMemberDeclarator
    {
        public object id;
        public CodeExpression expr;
    }

    public class CodeBaseCtorInvoke : CodeMethodReferenceExpression
    {
        CodeExpressionCollection ctorArgs;
        public CodeBaseCtorInvoke()
            : base(new CodeBaseReferenceExpression(), ".ctor")
        {
        }
        public CodeExpressionCollection Arguments
        {

            get
            {

                return ctorArgs;
            }
            set
            {
                ctorArgs = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeMethodInvokeExpression_Arguments);
                }
            }
        }
        public int ParameterCount
        {
            get
            {
                if (ctorArgs != null)
                {
                    return ctorArgs.Count;
                }
                return 0;
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.BaseCtorInvoke; }
        }
    }

    public class CodeThisCtorInvokeExpression : CodeMethodReferenceExpression
    {
        CodeExpressionCollection ctorArgs;
        public CodeThisCtorInvokeExpression()
            : base(new CodeThisReferenceExpression(), ".ctor")
        {
        }
        public CodeExpressionCollection Arguments
        {
            get
            {

                return ctorArgs;
            }
            set
            {
                ctorArgs = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeMethodInvokeExpression_Arguments);
                }
            }
        }
        public int ParameterCount
        {
            get
            {
                if (ctorArgs != null)
                {
                    return ctorArgs.Count;
                }
                return 0;
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.ThisCtorInvoke; }
        }
    }




    public class CodeLambdaExpression : CodeExpression
    {

        CodeParameterExpressionCollection parameters;

        //body of a lambda may be in the form of
        //1. single expression or,
        //2. method body
        CodeExpression singleExpression;
        CodeStatementCollection methodBody;

        CodeTypeReference returnType;
        public CodeLambdaExpression()
        {

        }
        internal string LambdaMethodDeclName
        {
            get;
            set;
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.LambdaExpression; }
        }
        public void AddParameter(CodeParameterDeclarationExpression parameterDeclExpr)
        {
            parameters.AddCodeObject(parameterDeclExpr);
        }
        public bool ContainsImplicitParameter()
        {


            if (parameters == null)
            {
                return false;
            }
            else
            {

                for (int i = parameters.Count - 1; i > -1; --i)
                {
                    if (parameters[i].IsImplicitParameterType)
                    {
                        return true;
                    }
                }
                return false;
            }

        }


        public override int OperatorPrecedence
        {
            get { return CodeOperatorPrecendence.LAMBDA; }
        }

        public CodeExpression SingleExpression
        {
            get
            {
                return singleExpression;
            }
            set
            {
                singleExpression = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeLambdaExpression_SingleLambdaExpression);
                }
            }
        }

        public CodeStatementCollection MethodBody
        {
            get
            {
                return methodBody;
            }
            set
            {
                methodBody = value;
                if (value != null)
                {

                    AcceptChild(value, CodeObjectRoles.CodeStatementCollection);
                }
            }
        }

        /// <summary>
        /// return type of this lambda
        /// </summary>
        public CodeTypeReference ReturnType
        {
            get
            {
                return returnType;
            }
            set
            {
                this.returnType = value;
            }
        }


        public void SetReturnTypeName(CodeNamedItem nameItem)
        {
            this.ReturnType = new CodeTypeReference(nameItem);
        }
        public CodeParameterExpressionCollection ParameterList
        {
            get
            {
                return parameters;
            }
            set
            {
                this.parameters = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeLambdaExpression_ParameterCollection);
                }
            }
        }

    }

    public class CodeNullExpression : CodeExpression
    {

        public CodeNullExpression()
        {

        }

        public sealed override CodeExpressionKind ExpressionKind
        {

            get
            {
                return CodeExpressionKind.NullExpression;
            }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
    }


    public class CodeNamedExpression : CodeExpression
    {
        CodeExpression expression;
        CodeSimpleName name;
        public CodeNamedExpression(string name, CodeExpression expression)
        {
            this.Name = new CodeSimpleName(name);
            this.Expression = expression;
        }
        public CodeNamedExpression(CodeSimpleName name, CodeExpression expression)
        {
            this.Name = name;
            this.Expression = expression;
        }

        public CodeExpression Expression
        {
            get
            {
                return expression;
            }
            set
            {
                expression = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeNamedExpression_Expression);
                }
            }
        }
        public string NameAsString
        {
            get
            {
                return name.NormalName;
            }
        }
        public CodeSimpleName Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeNamedExpression_Name);
                }
            }

        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.NamedExpression; }
        }
        public override CodeReplacingResult ReplaceChildExpression(CodeExpression oldcodeExpression, CodeExpression newcodeExpression)
        {
            this.Expression = newcodeExpression;
            return CodeReplacingResult.Ok;
        }
    }

    /// <summary>
    /// experiment parser kit extension 
    /// </summary>
    public class CodeMeReferenceExpression : CodeExpression
    {

        public CodeMeReferenceExpression()
        {
        }

        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.MeReferenceExpression; }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
    }
    //-------------------------------------------------------------
    public enum FieldDirection
    {
        In, //An incoming field. 
        Out,// An outgoing field. 
        Ref
    }

    public enum ArgumentDirection
    {
        In,
        Out,
        Ref
    }
    public class CodeArgumentList : CodeObjectCollection<CodeArgument>
    {
        public CodeArgumentList()
        {
        }
        public CodeArgumentList(CodeArgument arg)
        {
            this.AddCodeObject(arg);
        }
        public CodeArgumentList(params CodeExpression[] exprs)
        {
            if (exprs != null)
            {
                int j = exprs.Length;
                for (int i = 0; i < j; ++i)
                {
                    this.AddCodeObject(new CodeArgument(exprs[i]));
                }
            }
        }
        public CodeArgumentList(CodeExpressionCollection exprs)
        {
            if (exprs != null)
            {
                int j = exprs.Count;
                for (int i = 0; i < j; ++i)
                {
                    this.AddCodeObject(new CodeArgument(exprs[i]));
                }
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeArgumentList; }
        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeArgumentCollection; }
        }
    }
    public class CodeArgument : CodeObject
    {

        public string Name;
        public CodeExpression Value;
        public ArgumentDirection Direction;
        public CodeArgument()
        {
        }
        public CodeArgument(CodeExpression expr)
        {
            this.Value = expr;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeArgument; }
        }
    }

    public class CodeDirectionExpression : CodeExpression
    {
        CodeExpression expression;
        FieldDirection fieldDirection;
        public CodeDirectionExpression(CodeExpression expression, FieldDirection fieldDirection)
        {
            this.expression = expression;
            this.fieldDirection = fieldDirection;
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
        public FieldDirection FieldDirection
        {
            get
            {
                return fieldDirection;
            }
            set
            {
                fieldDirection = value;
            }
        }
        public CodeExpression Expression
        {
            get
            {
                return expression;
            }
            set
            {
                expression = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeDirectionExpression_Expression);
                }
            }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.DirectionExpression; }
        }
    }
    //------------------------------------------------------------------------------

    public class CodeAnonymousMethodExpression : CodeExpression
    {
        //C# anonymous method expression
        CodeFunctionSignature sig;
        CodeBlockStatement block;

        public override int OperatorPrecedence
        {
            get { throw new NotImplementedException(); }
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.AnonymousMethodExpression; }
        }
        public CodeBlockStatement Block
        {
            get
            {
                return this.block;
            }
            set
            {
                this.block = value;
            }
        }
        public CodeFunctionSignature Signature
        {
            get
            {
                return this.sig;
            }
            set
            {
                this.sig = value;
            }
        }

    }

    public enum CodeFunctionParameterDirection
    {
        In,
        Out,
        Ref
    }
    public class CodeFunctionSignature
    {
        List<CodeFunctionParameter> funcParameters = new List<CodeFunctionParameter>();

        public void AddParameter(CodeFunctionParameter par)
        {
            this.funcParameters.Add(par);
        }

    }
    public class CodeFunctionParameter
    {
        public CodeFunctionParameter(string parName)
        {
            this.ParameterName = parName;
        }
        public string ParameterName
        {
            get;
            set;
        }
        public CodeTypeReference ParameterType
        {
            get;
            set;
        }
        public CodeFunctionParameterDirection Direction
        {
            get;
            set;
        }

    }


    /// <summary>
    /// experiment!, parserkit extension
    /// </summary>
    public class CodeDynamicListExpression : CodeExpression
    {

        CodeExpressionCollection memberExprCollection;
        public CodeDynamicListExpression(CodeExpressionCollection memberExprCollection)
        {
            this.memberExprCollection = memberExprCollection;
            this.AcceptChild(memberExprCollection, CodeObjectRoles.CodeDynamicListExpression_MemberExprCollection);
        }
        public override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.DynamicListExpression; }
        }
        public CodeExpressionCollection MemberExpressionCollection
        {
            get
            {
                return memberExprCollection;
            }
        }
        public int MemberExpressionCount
        {
            get
            {
                if (memberExprCollection != null)
                {
                    return memberExprCollection.Count;
                }
                else
                {
                    return 0;
                }
            }
        }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
    }



}