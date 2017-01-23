//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;
namespace Parser.CodeDom
{
   



    public enum CodeTypeMemberAccessibility
    {
        Private,
        Family, //internal
        Assembly, //assembly
        FamilyAndAssembly, //family and assembly
        Public
    }


    /// <summary>
    ///collection of type parameter constraints 
    /// </summary>
    public class CodeTypeParameterConstraintCollection : CodeObject
    {
        List<CodeTypeParameterConstraint> collection = new List<CodeTypeParameterConstraint>();
        public void Add(CodeTypeParameterConstraint constraint)
        {
            collection.Add(constraint);
        }
        public List<CodeTypeParameterConstraint> GetList()
        {
            return this.collection;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.TypeConstraintCollection; }
        }
    }
    /// <summary>
    /// type parameter constraint
    /// </summary>
    public class CodeTypeParameterConstraint : CodeObject
    {

        public string Name;
        List<CodeExpression> constraints = new List<CodeExpression>();

        public CodeTypeParameterConstraint()
        {

        }
        public void AddConstraint(CodeExpression nameItem)
        {
            this.constraints.Add(nameItem);
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.TypeConstraint; }
        }
        public List<CodeExpression> GetConstraintItems()
        {
            return this.constraints;
        }

    }


    public abstract partial class CodeTypeMember : CodeObject
    {

        protected const int _IS_STATIC = 1 << (1 - 1);
        //----------------------------------------------------------

        protected const int _IS_INHERIT = 1 << (6 - 1);
        protected const int _IS_VIRTUAL = 1 << (7 - 1);
        protected const int _IS_ABSTRACT = 1 << (8 - 1);
        protected const int _IS_OVERRIDE = 1 << (9 - 1);
        protected const int _IS_SEALED = 1 << (10 - 1);
        protected const int _IS_CONST = 1 << (11 - 1);
        protected const int _IS_READONLY = 1 << (12 - 1);
        protected const int _IS_EXTERN = 1 << (13 - 1);
        protected const int _IS_RT_SPECIAL = 1 << (14 - 1);
        protected const int _HAS_CUSTOM_ATTR = 1 << (15 - 1);
        protected const int _IS_COMPILER_GEN = 1 << (16 - 1);
        //----------------------------------------------------------
        protected const int _IS_EXTENSION_METHOD = 1 << (17 - 1);
        protected const int _IS_PARTIAL_MEMBER = 1 << (18 - 1);

        protected const int _IS_SPEICIAL_NAME = 1 << (19 - 1);
        protected const int _IS_NEW = 1 << (20 - 1);
#if DEBUG

        protected const int _DEBUG_RESERVED_30 = 1 << (30 - 1);
        protected const int _DEBUG_RESERVED_31 = 1 << (31 - 1);
        protected const int _DEBUG_RESERVED_32 = 1 << (32 - 1);
#endif
    }


    public abstract partial class CodeTypeMember : CodeObject
    {

        int typeMemberFlags = 0;

        CodeTypeReference returnType;
        CodeTypeMemberAccessibility memberAccessiblity;
        CodeAttributeDeclarationCollection customAttributes;
        CodeTypeParameterConstraintCollection typeConstraints;
        string memberName;
        readonly CompilationUnit cu;
        CodeTypeMemberKind codeTypeMemberKind;

        protected CodeTypeMember()
        {
        }
        public CodeTypeMember(CompilationUnit cu, CodeTypeMemberKind codeTypeMemberKind)
        {
            this.cu = cu;
            this.codeTypeMemberKind = codeTypeMemberKind;
#if DEBUG
            if (cu == null)
            {

            }
#endif
        }
        public CodeTypeMemberKind MemberKind
        {
            get
            {
                return this.codeTypeMemberKind;
            }
        }
        public CodeTypeMemberAccessibility MemberAccessibility
        {
            get
            {
                return memberAccessiblity;
            }
            set
            {
                if (memberAccessiblity == CodeTypeMemberAccessibility.Family && value == CodeTypeMemberAccessibility.Assembly)
                {
                    memberAccessiblity = CodeTypeMemberAccessibility.FamilyAndAssembly;
                }
                else if (memberAccessiblity == CodeTypeMemberAccessibility.Assembly && value == CodeTypeMemberAccessibility.Family)
                {
                    memberAccessiblity = CodeTypeMemberAccessibility.FamilyAndAssembly;
                }
                else
                {
                    memberAccessiblity = value;
                }
            }
        }
        public CompilationUnit CU
        {
            get
            {
                return this.cu;
            }
        }

        public CodeTypeParameterConstraintCollection TypeParameterConstraints
        {
            get
            {
                return this.typeConstraints;
            }
            set
            {
                this.typeConstraints = value;
            }
        }
        internal static int GetTypeMemberFlags(CodeTypeMember typeMember)
        {
            return typeMember.typeMemberFlags;
        }
        internal static void SetTypeMemberFlags(CodeTypeMember typeMember, int flags)
        {
            typeMember.typeMemberFlags = flags;
        }
        public CodeTypeDeclaration TypeDeclaration
        {
            get
            {

                if (ParentCodeObject is CodeTypeDeclaration)
                {
                    return (CodeTypeDeclaration)ParentCodeObject;
                }
                else if (ParentCodeObject is CodeTypeMember)
                {
                    return ((CodeTypeMember)ParentCodeObject).TypeDeclaration;
                }
                else
                {
                    return null;
                }
            }
        }


        public CodeTypeReference ReturnType
        {
            get
            {
                return returnType;
            }
            set
            {
                returnType = value;

            }
        }
        public void SetReturnTypeName(CodeNamedItem returnTypeName)
        {

            this.ReturnType = new CodeTypeReference(returnTypeName);

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
                    typeMemberFlags |= _HAS_CUSTOM_ATTR;
                    AcceptChild(value, CodeObjectRoles.CodeAttributeDeclarationCollection);
                }
                else
                {
                    typeMemberFlags &= ~_HAS_CUSTOM_ATTR;
                }

            }
        }
        public bool HasCustomAttributes
        {
            get
            {
                return (typeMemberFlags & _HAS_CUSTOM_ATTR) != 0;

            }
        }
        public bool IsVirtual
        {
            get
            {
                return (typeMemberFlags & _IS_VIRTUAL) != 0;
            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_VIRTUAL;
                }
                else
                {
                    typeMemberFlags &= ~_IS_VIRTUAL;
                }
            }
        }
        protected bool IsPartialMember
        {
            get
            {
                return (typeMemberFlags & _IS_PARTIAL_MEMBER) != 0;
            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_PARTIAL_MEMBER;
                }
                else
                {
                    typeMemberFlags &= ~_IS_PARTIAL_MEMBER;
                }

            }
        }
        public bool IsExtern
        {
            get
            {
                return (typeMemberFlags & _IS_EXTERN) != 0;

            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_EXTERN;
                }
                else
                {
                    typeMemberFlags &= ~_IS_EXTERN;
                }

            }
        }
        public bool IsRuntimeSpecial
        {
            get
            {
                return (typeMemberFlags & _IS_RT_SPECIAL) != 0;

            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_RT_SPECIAL;
                }
                else
                {
                    typeMemberFlags &= ~_IS_RT_SPECIAL;
                }
            }
        }
        public bool IsSpecialName
        {
            //eg. .cctor,.ctor ,.get_ ,.set, .add_ ,.remove 
            get
            {
                return (typeMemberFlags & _IS_SPEICIAL_NAME) != 0;
            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_SPEICIAL_NAME;
                }
                else
                {
                    typeMemberFlags &= ~_IS_SPEICIAL_NAME;
                }
            }
        }

        public bool IsCompilerGenerated
        {

            get
            {
                return (typeMemberFlags & _IS_COMPILER_GEN) != 0;

            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_COMPILER_GEN;
                }
                else
                {
                    typeMemberFlags &= ~_IS_COMPILER_GEN;
                }
            }
        }
        public bool IsOverride
        {
            get
            {
                return (typeMemberFlags & _IS_OVERRIDE) != 0;

            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_OVERRIDE;
                }
                else
                {
                    typeMemberFlags &= ~_IS_OVERRIDE;
                }
            }
        }
        public bool IsSealed
        {
            get
            {
                return (typeMemberFlags & _IS_SEALED) != 0;
            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_SEALED;
                }
                else
                {
                    typeMemberFlags &= ~_IS_SEALED;
                }
            }

        }
        public bool IsNew
        {
            get
            {
                return (typeMemberFlags & _IS_NEW) != 0;
            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_NEW;
                }
                else
                {
                    typeMemberFlags &= ~_IS_NEW;
                }
            }

        }
        public bool IsAbstract
        {
            get
            {
                return (typeMemberFlags & _IS_ABSTRACT) != 0;

            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_ABSTRACT;
                }
                else
                {
                    typeMemberFlags &= ~_IS_ABSTRACT;
                }

            }
        }

        public bool IsConst
        {
            get
            {
                return (typeMemberFlags & _IS_CONST) != 0;
            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_CONST;
                }
                else
                {
                    typeMemberFlags &= ~_IS_CONST;
                }

            }
        }
        public bool IsReadonly
        {
            get
            {
                return (typeMemberFlags & _IS_READONLY) != 0;
            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_READONLY;
                }
                else
                {
                    typeMemberFlags &= ~_IS_READONLY;
                }
            }

        }
        public bool IsPublic
        {
            get
            {
                return memberAccessiblity == CodeTypeMemberAccessibility.Public;
            }


        }
        public bool IsProtected
        {
            get
            {
                return memberAccessiblity == CodeTypeMemberAccessibility.Family;
            }

        }
        public bool IsStatic
        {
            get
            {
                return (typeMemberFlags & _IS_STATIC) != 0;

            }
            set
            {
                if (value)
                {
                    typeMemberFlags |= _IS_STATIC;
                }
                else
                {
                    typeMemberFlags &= ~_IS_STATIC;
                }
            }
        }
        public bool IsPrivate
        {
            get
            {
                return memberAccessiblity == CodeTypeMemberAccessibility.Private;
            }

        }
        public bool IsInternal
        {
            get
            {
                return memberAccessiblity == CodeTypeMemberAccessibility.Assembly;
            }

        }


#if DEBUG
        protected string debug_ToCodeString()
        {

            StringBuilder stBuilder = new StringBuilder();
            Parser.AsmInfrastructures.AsmIndentTextWriter writer =
                new Parser.AsmInfrastructures.AsmIndentTextWriter(stBuilder);
            Parser.AsmInfrastructures.CodeDomToSourceCodeConverter.Generate(this, writer);
            return stBuilder.ToString();
        }
        public override string ToString()
        {
            return debug_ToCodeString();
        }
#endif
        public virtual CodeTypeMemberSignature Signature
        {
            get
            {
                return new CodeTypeMemberSignature(this.memberName, Kind);
            }
        }
        public virtual string Name
        {

            get
            {
                return memberName;
            }
        }
        public static void SetName(CodeTypeMember typeMb, CodeSimpleName name)
        {
            typeMb.memberName = name.NameWithoutArity;
            typeMb.AcceptChild(name, CodeObjectRoles.CodeTypeMember_Name);
        }
        public static void SetName(CodeTypeMember typeMb, string iden)
        {
            typeMb.memberName = iden;
        }
    }

    public enum CodeTypeMemberKind
    {
        Type, Method, Event, Field, Property, Indexer
    }

    /// <summary>
    /// method declaration
    /// </summary>
    public class CodeMethodDeclaration : CodeTypeMember
    {

        CodeStatementCollection userOriginalMethodBody;
        CodeStatementCollection body = null;
        CodeTypeParameterCollection typeParameters;//generic support

        CodeParameterExpressionCollection methodParameterList = new CodeParameterExpressionCollection();
        public CodeMethodDeclaration(CompilationUnit cu, string name)
            : base(cu, CodeTypeMemberKind.Method)
        {
            SetName(this, name);
        }
        public CodeMethodDeclaration(CompilationUnit cu, CodeSimpleName name)
            : base(cu, CodeTypeMemberKind.Method)
        {
            SetName(this, name);
        }

        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.Method; }
        }
        public CodeParameterExpressionCollection ParameterList
        {
            get
            {
                return methodParameterList;
            }
            set
            {

                methodParameterList = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeParameterDeclCollection);
                }
            }
        }
        public CodeStatementCollection Body
        {
            get
            {
                return body;
            }
            set
            {

                body = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeStatementCollection);
                }
            }
        }
        public void SetMethodBody(CodeBlockStatement block)
        {
            this.Body = new CodeStatementCollection();
            this.body.Add(block.Body);

        }
        internal CodeStatementCollection UserOriginalMethodBody
        {

            get
            {
                return userOriginalMethodBody;
            }
            set
            {
                userOriginalMethodBody = value;
            }
        }


        public bool IsExtensionMethod
        {
            get
            {

                return (GetTypeMemberFlags(this) & _IS_EXTENSION_METHOD) != 0;
            }
            set
            {
                int currentFlags = GetTypeMemberFlags(this);
                if (value)
                {
                    SetTypeMemberFlags(this, currentFlags | _IS_EXTENSION_METHOD);
                }
                else
                {
                    currentFlags &= ~_IS_EXTENSION_METHOD;
                    SetTypeMemberFlags(this, currentFlags);
                }

            }
        }

        public bool IsPartialMethod
        {
            get
            {
                return base.IsPartialMember;
            }
            set
            {
                base.IsPartialMember = value;
            }
        }
        public override CodeTypeMemberSignature Signature
        {
            get
            {
                return new CodeTypeMemberSignature(this.Name, methodParameterList, CodeObjectKind.Method);
            }
        }

        public CodeTypeParameterCollection TypeParameters
        {
            //this for generic method
            get
            {
                return typeParameters;
            }
            set
            {
                typeParameters = value;
                if (value != null)
                {
                    this.AcceptChild(value, CodeObjectRoles.CodeMethodDeclaration_TypeParameterCollection);
                    foreach (CodeTypeParameter typeParam in value.GetTypeParameterIter())
                    {
                        AcceptTypeParameter(this, typeParam);
                    }
                }
            }
        }
        public bool HasTypeParameters
        {
            get
            {
                return typeParameters != null && typeParameters.Count > 0;
            }

        }
        internal static void AcceptTypeParameter(CodeMethodDeclaration method, CodeTypeParameter typeParam)
        {
            //accept
            method.AcceptChild(typeParam, CodeObjectRoles.CodeMethodDeclaration_GenericTypeParameter);
        }


    }

    public class CodeLambdaMethodDeclaration : CodeMethodDeclaration
    {

        //link to original lambda expression
        CodeLambdaExpression lambdaExpression;
        public CodeLambdaMethodDeclaration(CompilationUnit cu, string name, CodeLambdaExpression lambdaExpression)
            : base(cu, name)
        {


            this.IsCompilerGenerated = true;
            this.ParameterList = lambdaExpression.ParameterList;

            this.ReturnType = lambdaExpression.ReturnType;

            //***
            //1. arrived lamba expression may not have complete information
            //2. statement of lambda expression may not be semantic check
            //3. content of lambda expression block will be a content of new 'compiler generated' method


            if (lambdaExpression.SingleExpression != null)
            {

            }
            else
            {
                this.Body = lambdaExpression.MethodBody;
            }

            this.lambdaExpression = lambdaExpression;
            AcceptChild(this.lambdaExpression, CodeObjectRoles.CodeLambdaMethodDeclaration_LambdaExpression);
        }
#if DEBUG
        public CodeLambdaExpression dbug_LambdaExpression
        {
            get
            {
                return lambdaExpression;
            }
        }
#endif

    }

    public class CodeIndexerDeclaration : CodeTypeMember
    {

        CodeMethodDeclaration getPart;
        CodeMethodDeclaration setPart;
        CodeParameterExpressionCollection methodParameterList = new CodeParameterExpressionCollection();

        public CodeIndexerDeclaration(CompilationUnit cu, CodeTypeReference propertyType)
            : base(cu, CodeTypeMemberKind.Indexer)
        {
            SetName(this, new CodeSimpleName("this"));
            this.ReturnType = propertyType;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.Indexer; }
        }
        public CodeParameterExpressionCollection ParameterList
        {
            get
            {
                return this.methodParameterList;
            }
            set
            {
                this.methodParameterList = value;
            }
        }
        public CodeMethodDeclaration GetDeclaration
        {
            get
            {
                return getPart;
            }
            set
            {
                getPart = value;
                if (value != null)
                {

                    AcceptChild(value, CodeObjectRoles.CodeIndexer_GetMethodDecl);
                }
            }
        }
        public CodeMethodDeclaration SetDeclaration
        {
            get
            {
                return setPart;
            }
            set
            {

                setPart = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeIndexer_SetMethodDecl);
                }
            }
        }
        public static CodeMethodDeclaration CreateIndexerGetPart(CompilationUnit cu, CodeTypeReference returnType)
        {
            CodeMethodDeclaration get_Part = new CodeMethodDeclaration(cu, "get_Item");
            get_Part.IsSpecialName = true;
            get_Part.ReturnType = returnType;


            return get_Part;
        }

        public static CodeMethodDeclaration CreateIndexerSetPart(CompilationUnit cu, CodeTypeReference inputType)
        {
            CodeMethodDeclaration set_part = new CodeMethodDeclaration(cu, "set_Item");
            set_part.IsSpecialName = true;
            set_part.ReturnType = CodeTypeReference.CreateFromAsmTypeName("void");
            return set_part;
        }
    }



    public class CodePropertyDeclaration : CodeTypeMember
    {

        CodeMethodDeclaration getPart;
        CodeMethodDeclaration setPart;
        bool isExtension;
        List<CodeParameterDeclarationExpression> additionalParameters;

        public CodePropertyDeclaration(CompilationUnit cu, string name, CodeTypeReference propertyType)
            : base(cu, CodeTypeMemberKind.Property)
        {
            SetName(this, new CodeSimpleName(name));
            this.ReturnType = propertyType;
        }
        public CodePropertyDeclaration(CompilationUnit cu, CodeSimpleName name, CodeTypeReference propertyType)
            : base(cu, CodeTypeMemberKind.Property)
        {
            SetName(this, name);
            this.ReturnType = propertyType;
        }


        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.Property; }
        }
        public CodeMethodDeclaration GetDeclaration
        {
            get
            {
                return getPart;
            }
            set
            {
                getPart = value;
                if (value != null)
                {

                    AcceptChild(value, CodeObjectRoles.CodeProperty_GetMethodDecl);
                }
            }
        }
        public CodeMethodDeclaration SetDeclaration
        {
            get
            {
                return setPart;
            }
            set
            {

                setPart = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeProperty_SetMethodDecl);
                }
            }
        }
        //------------------------------------------------

        //ParserKit  extension
        //extension property
        public bool IsExtensionProperty
        {
            get
            {
                return this.isExtension;
            }
            set
            {
                this.isExtension = value;
            }
        }
        public void AddAdditionalParameter(CodeParameterDeclarationExpression par)
        {
            if (additionalParameters == null)
            {
                additionalParameters = new List<CodeParameterDeclarationExpression>();
            }
            this.additionalParameters.Add(par);
        }
        public bool HasAdditionalParameter
        {
            get
            {
                return this.additionalParameters != null;
            }
        }
        public List<CodeParameterDeclarationExpression> GetAdditionalParameters()
        {
            return this.additionalParameters;
        }


        //------------------------------------------------
        public static CodeMethodDeclaration CreatePropertyGetPart(CompilationUnit cu, string onlyPropertyName, CodeTypeReference returnType)
        {
            CodeMethodDeclaration get_Part = new CodeMethodDeclaration(cu, "get_" + onlyPropertyName);
            get_Part.IsSpecialName = true;
            get_Part.ReturnType = returnType;

            return get_Part;
        }
        public static CodeMethodDeclaration CreatePropertySetPart(CompilationUnit cu, string onlyPropertyName, CodeTypeReference inputType)
        {
            CodeMethodDeclaration set_part = new CodeMethodDeclaration(cu, "set_" + onlyPropertyName);
            set_part.IsSpecialName = true;
            set_part.ReturnType = CodeTypeReference.CreateFromAsmTypeName("void");

            set_part.ParameterList = new CodeParameterExpressionCollection();
            set_part.ParameterList.AddCodeObject(new CodeParameterDeclarationExpression(DEFAULT_INPUT_PARNAME, inputType));
            return set_part;
        }

        public const string DEFAULT_INPUT_PARNAME = "value";
    }


    public class CodeEventDeclaration : CodeTypeMember
    {

        CodeMethodDeclaration addMethod;
        CodeMethodDeclaration removeMethod;
        CodeParameterExpressionCollection delegateParameters;
        List<CodeParameterDeclarationExpression> additionalParameters;
        bool isExtension;

        public CodeEventDeclaration(CompilationUnit cu, string name, CodeTypeReference eventType)
            : base(cu, CodeTypeMemberKind.Event)
        {

            SetName(this, name);
            this.ReturnType = eventType;
        }
        public CodeEventDeclaration(CompilationUnit cu, CodeSimpleName name, CodeTypeReference eventType)
            : base(cu, CodeTypeMemberKind.Event)
        {

            SetName(this, name);
            this.ReturnType = eventType;
        }

        //------------------------------------------------
        //ParserKit  extension
        //extension property
        public bool IsExtensionEvent
        {
            get
            {
                return this.isExtension;
            }
            set
            {
                this.isExtension = value;
            }
        }
        public void AddAdditionalParameter(CodeParameterDeclarationExpression par)
        {
            if (additionalParameters == null)
            {
                additionalParameters = new List<CodeParameterDeclarationExpression>();
            }
            this.additionalParameters.Add(par);
        }
        public bool HasAdditionalParameter
        {
            get
            {
                return this.additionalParameters != null;
            }
        }
        public List<CodeParameterDeclarationExpression> GetAdditionalParameters()
        {
            return this.additionalParameters;
        }
        //------------------------------------------------

        /// <summary>
        /// ParserKit  extension
        /// </summary>
        public CodeParameterExpressionCollection Parameters
        {
            get
            {
                return delegateParameters;
            }
            set
            {
                delegateParameters = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeEvent_ParameterList);
                }
            }
        }
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.Event;
            }
        }
        public CodeMethodDeclaration AddDeclaration
        {
            get
            {
                return addMethod;
            }
            set
            {
                addMethod = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeEvent_AddMethodDecl);
                }
            }
        }
        public CodeMethodDeclaration RemoveDeclaration
        {
            get
            {
                return removeMethod;
            }
            set
            {
                removeMethod = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeEvent_RemoveMethodDecl);
                }
            }
        }


        //helper----------------------------
        public static CodeMethodDeclaration CreateAddPart(CompilationUnit cu, string eventname, CodeTypeReference voidTypeReference)
        {
            CodeMethodDeclaration add_part = new CodeMethodDeclaration(cu, "add_" + eventname);
            add_part.IsSpecialName = true;
            add_part.ReturnType = voidTypeReference;
            return add_part;
        }
        public static CodeMethodDeclaration CreateRemovePart(CompilationUnit cu, string eventname, CodeTypeReference voidTypeReference)
        {
            CodeMethodDeclaration remove_part = new CodeMethodDeclaration(cu, "remove_" + eventname);
            remove_part.IsSpecialName = true;
            remove_part.ReturnType = voidTypeReference;
            return remove_part;
        }
    }

    public class CodeFieldDeclaration : CodeTypeMember
    {

        CodeExpression initExpr;
        public CodeFieldDeclaration(CompilationUnit cu, string fieldName)
            : base(cu, CodeTypeMemberKind.Field)
        {
            SetName(this, fieldName);
        }
        public CodeFieldDeclaration(CompilationUnit cu, CodeSimpleName fieldName)
            : base(cu, CodeTypeMemberKind.Field)
        {
            SetName(this, fieldName);
        }
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.Field;
            }
        }
        public CodeExpression InitExpression
        {
            get
            {
                return initExpr;
            }
            set
            {
                initExpr = value;
                if (value != null)
                {
                    AcceptChild(initExpr, CodeObjectRoles.CodeField_InitExpression);
                }
            }
        }
    }



    public class CodeClassConstructorDeclaration : CodeMethodDeclaration
    {

        public CodeClassConstructorDeclaration(CompilationUnit cu, bool autoGenBody)
            : base(cu, ".cctor")
        {
            ReturnType = CodeTypeReference.CreateFromAsmTypeName("void");
            this.IsRuntimeSpecial = true;
            this.IsSpecialName = true;
            if (autoGenBody)
            {

                ParameterList = new CodeParameterExpressionCollection();
                Body = new CodeStatementCollection();
            }

            //always
            this.MemberAccessibility = CodeTypeMemberAccessibility.Private;
            this.IsStatic = true;
        }
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.ClassConstructor;
            }
        }
        public override CodeTypeMemberSignature Signature
        {
            get
            {
                return new CodeTypeMemberSignature(this.Name, ParameterList, CodeObjectKind.Method);
            }
        }
    }
    public class CodeObjectConstructorDeclaration : CodeMethodDeclaration
    {
        CodeMethodReferenceExpression baseOrChainCtorInvokeExpression;
        public CodeObjectConstructorDeclaration(CompilationUnit cu, bool autoGenBody)
            : base(cu, ".ctor")
        {


            ReturnType = CodeTypeReference.CreateFromAsmTypeName("void");
            this.IsRuntimeSpecial = true;
            this.IsSpecialName = true;
            if (autoGenBody)
            {

                ParameterList = new CodeParameterExpressionCollection();
                Body = new CodeStatementCollection();
                MemberAccessibility = CodeTypeMemberAccessibility.Public;
            }
        }

        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.ObjectConstructor;
            }
        }
        public CodeBaseCtorInvoke BaseCtorInvoke
        {
            get
            {
                return baseOrChainCtorInvokeExpression as CodeBaseCtorInvoke;
            }
            set
            {
                baseOrChainCtorInvokeExpression = value;
            }
        }
        public CodeThisCtorInvokeExpression ThisCtorInvoke
        {
            get
            {
                return baseOrChainCtorInvokeExpression as CodeThisCtorInvokeExpression;
            }
            set
            {
                baseOrChainCtorInvokeExpression = value;
            }
        }
        internal bool HasUserCtorInvoke
        {
            get
            {
                return baseOrChainCtorInvokeExpression != null;
            }
        }

        public override CodeTypeMemberSignature Signature
        {
            get
            {
                return new CodeTypeMemberSignature(this.Name, ParameterList, CodeObjectKind.ObjectConstructor);
            }
        }

    }


}

