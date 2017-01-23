//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.CodeDom
{

    public enum CodeTypeDeclKind
    {
        Class,
        Struct,
        Delegate,
        Interface,
    }

    public class CodeTypeDeclaration : CodeTypeMember, INamespaceMember
    {

        string userExplicitNamespaceName;
        TypeMemberCollection members;

        CodeTypeDeclaration isMergedTo;
        CodeTypeParameterCollection typeParameters;
        CodeObjectKind typeKind;
        CodeTypeReferenceCollection baseTypes;
        CodeNamedItem baseTypeNmItem;
        List<CodeNamedItem> interfaceTypeNames;

        public CodeTypeDeclaration(CodeTypeDeclKind typedeclKind)
        {

        }

        public CodeTypeDeclaration(CompilationUnit cu, CodeObjectKind kindOfType, string maybeFullname)
            : base(cu, CodeTypeMemberKind.Type)
        {
            typeKind = kindOfType;

            members = new TypeMemberCollection(this);
            baseTypes = new CodeTypeReferenceCollection();


            int lastDotPos = maybeFullname.LastIndexOf('.');

            if (lastDotPos > -1)
            {

                SetTypeDeclationName(this, maybeFullname.Substring(0, lastDotPos),
                    maybeFullname.Substring(lastDotPos + 1));
            }
            else
            {

                SetTypeDeclationName(this, null, maybeFullname);
            }
        }

        public CodeTypeDeclaration(CompilationUnit cu, CodeObjectKind kindOfType, CodeSimpleName name)
            : base(cu, CodeTypeMemberKind.Type)
        {
            typeKind = kindOfType;

            members = new TypeMemberCollection(this);
            baseTypes = new CodeTypeReferenceCollection();

            SetTypeDeclationName(this, null, name.NormalName);
        }
        public CodeTypeDeclaration(CompilationUnit cu, CodeObjectKind kindOfType, string namespaceName, string typename)
            : base(cu, CodeTypeMemberKind.Type)
        {
            typeKind = kindOfType;

            members = new TypeMemberCollection(this);
            baseTypes = new CodeTypeReferenceCollection();
            SetTypeDeclationName(this, namespaceName, typename);
        }


        public override CodeTypeMemberSignature Signature
        {
            get
            {
                return base.Signature;
            }
        }
        public override string Name
        {
            get
            {
                return base.Name;
            }
        }
        public void SetTypeName(string name)
        {
            SetTypeDeclationName(this, "", name);
        }
        public TypeParameterConstraints TypeParameterConstraints
        {
            get;
            set;
        }
        static void SetTypeDeclationName(CodeTypeDeclaration typedecl, string namespaceName, string typeName)
        {

            CodeTypeMember.SetName(typedecl, typeName);
            typedecl.userExplicitNamespaceName = namespaceName;
        }
        internal static void SetTypeDeclationNamespace(CodeTypeDeclaration typedecl, string namespaceName)
        {
            typedecl.userExplicitNamespaceName = namespaceName;
        }


        public string Namespace
        {
            get
            {


                return this.userExplicitNamespaceName;

            }
        }
        internal static void AcceptMember(CodeTypeDeclaration parent, CodeTypeMember mb)
        {
            parent.AcceptChild(mb, CodeObjectRoles.CodeTypeMemberCollection_Member);
        }
        internal static void AcceptTypeParameter(CodeTypeDeclaration parent, CodeTypeParameter mb)
        {
            parent.AcceptChild(mb, CodeObjectRoles.CodeTypeDeclaration_GenericTypeParameter);
        }
        public override CodeObjectKind Kind
        {
            get { return typeKind; }
        }
        public string FullNameAsString
        {
            get
            {
                string fullname = Namespace;
                if (string.IsNullOrEmpty(fullname))
                {
                    fullname = this.Name;
                }
                else
                {
                    fullname += "." + this.Name;
                }
                return fullname;
            }
        }


        public CodeTypeReference CreateCodeTypeReference()
        {
            return new CodeTypeReference(this);
        }
        public TypeMemberCollection Members
        {
            get
            {
                return members;
            }
        }

        public CodeTypeReferenceCollection BaseTypes
        {
            get
            {

                return baseTypes;
            }
            set
            {
                baseTypes = value;
            }
        }
        public CodeTypeReference BaseType
        {
            get
            {
                if (baseTypes.Count > 0)
                {
                    return baseTypes[0];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                baseTypes.Insert(0, value);
            }
        }
        internal void SetBaseTypeName(CodeNamedItem baseTypeNmItem)
        {
            this.baseTypeNmItem = baseTypeNmItem;
            this.BaseType = new CodeTypeReference(baseTypeNmItem);
        }
        internal void AddInterfaceTypeName(CodeNamedItem interfaceTypeName)
        {
            if (interfaceTypeNames == null)
            {
                interfaceTypeNames = new List<CodeNamedItem>();
            }
            interfaceTypeNames.Add(interfaceTypeName);
            this.BaseTypes.Add(new CodeTypeReference(interfaceTypeName));
        }
        internal LocationCodeArea GetBaseTypeLocationCodeArea()
        {
            if (baseTypeNmItem != null)
            {
                return baseTypeNmItem.SourceLocation;
            }
            else
            {
                return LocationCodeArea.Empty;
            }
        }
        internal bool HasBaseTypeLocationArea
        {
            get
            {
                return baseTypeNmItem != null;
            }
        }
        internal LocationCodeArea[] GetInterfacesLocationAreas()
        {
            if (interfaceTypeNames != null)
            {
                int j = interfaceTypeNames.Count;
                LocationCodeArea[] areas = new LocationCodeArea[j];
                for (int i = 0; i < j; ++i)
                {
                    areas[i] = interfaceTypeNames[i].SourceLocation;
                }
                return areas;
            }
            else
            {
                return null;
            }
        }
        internal bool HasInterfaceLocationAreas
        {
            get
            {
                return interfaceTypeNames != null;
            }
        }

        public bool IsClass
        {
            get
            {
                return Kind == CodeObjectKind.Class;

            }
        }
        public bool IsDelegate
        {
            get
            {
                return Kind == CodeObjectKind.Delegate;

            }
        }
        public bool IsStruct
        {
            get
            {
                return Kind == CodeObjectKind.Struct;
            }
        }
        public bool IsInterface
        {
            get
            {
                return Kind == CodeObjectKind.Interface;
            }
        }
        public bool IsEnum
        {
            get
            {
                return Kind == CodeObjectKind.Enum;
            }
        }
        public bool IsDefine
        {
            get
            {
                return Kind == CodeObjectKind.DefineClass;
            }
        }
        public bool IsPartial
        {
            get
            {
                return this.IsPartialMember;
            }
            set
            {
                this.IsPartialMember = value;
            }
        }

        public CodeTypeDeclaration IsMergedTo
        {
            get
            {
                return isMergedTo;
            }
            set
            {
                isMergedTo = value;
            }
        }

        public CodeTypeParameterCollection TypeParameters
        {
            get
            {
                return typeParameters;
            }
            set
            {
                typeParameters = value;

                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeTypeDeclaration_GenericTypeParameterCollection);
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


    }

    public class CodeNamespaceImport : CodeObject
    {

        CodeNamedItem namespaceName;
        public CodeNamespaceImport(string namespaceName)
        {
            NamespaceName = new CodeNamedItem(namespaceName);
        }
        public CodeNamespaceImport(CodeNamedItem namespaceName)
        {

            NamespaceName = namespaceName;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.NamespaceImport; }
        }
        public CodeNamedItem NamespaceName
        {
            get
            {
                return namespaceName;
            }
            private set
            {
                namespaceName = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeNamespaceImport_Name);
                }
            }
        }

        public string FullName
        {
            get
            {
                return namespaceName.FullName;
            }
        }

#if DEBUG
        public override string ToString()
        {
            return "using " + FullName;
        }
#endif

        public List<CodeTypeDeclaration> ExportTypes
        {
            get
            {

                return null;
            }
        }
    }


    public class CodeTypeDelegate : CodeTypeDeclaration
    {

        CodeParameterExpressionCollection delegateParameters;
        CodeTypeReference delegateReturnType;
        bool isInvokeMethodCreated;

        public CodeTypeDelegate(CompilationUnit cu, string typenamespace, string typename)
            : base(cu, CodeObjectKind.Delegate, typenamespace, typename)
        {
            this.BaseType = CodeTypeReference.CreateFromAsmTypeName("System.Delegate");
        }
        public CodeTypeDelegate(CompilationUnit cu, CodeSimpleName name)
            : base(cu, CodeObjectKind.Delegate, name)
        {
            this.BaseType = CodeTypeReference.CreateFromAsmTypeName("System.Delegate");
        }
        public CodeParameterExpressionCollection Parameters
        {
            get
            {
                return delegateParameters;
            }
            set
            {

                if (isInvokeMethodCreated)
                {
                    throw new NotSupportedException();
                }
                //----------------------------------------- 
                delegateParameters = value;

                CodeMethodDeclaration invokeMethod = new CodeMethodDeclaration(this.CU, "Invoke");
                invokeMethod.ParameterList = delegateParameters;
                invokeMethod.IsVirtual = true;
                invokeMethod.IsExtern = true;
                invokeMethod.MemberAccessibility = CodeTypeMemberAccessibility.Public;
                invokeMethod.IsCompilerGenerated = true;

                //----------------------------------------- 
                if (delegateReturnType == null)
                {
                    invokeMethod.ReturnType = CodeTypeReference.CreateFromAsmTypeName("void");
                }
                else
                {
                    invokeMethod.ReturnType = delegateReturnType;
                }
                //----------------------------------------- 
                this.Members.AddMember(invokeMethod);
                //-------------------------------------------------------------
                //automatically create ctor
                CodeObjectConstructorDeclaration delegateCtor = new CodeObjectConstructorDeclaration(this.CU, false);
                delegateCtor.IsExtern = true;//***
                delegateCtor.IsCompilerGenerated = true;
                delegateCtor.MemberAccessibility = CodeTypeMemberAccessibility.Public;

                delegateCtor.ParameterList = new CodeParameterExpressionCollection(
                    new CodeParameterDeclarationExpression("@object", CodeTypeReference.CreateFromAsmTypeName("System.Object")),
                    new CodeParameterDeclarationExpression("@ftnptr", CodeTypeReference.CreateFromAsmTypeName("System.IntPtr"))
                    );
                this.Members.AddMember(delegateCtor);

                isInvokeMethodCreated = true;
                //------------------------------------------------------------- 
            }
        }
        public CodeTypeReference DelegateReturnType
        {
            get
            {
                return delegateReturnType;
            }
            set
            {
                delegateReturnType = value;
                CodeMethodDeclaration delegateInvokeMethod = this.Members.SearchSingleMethodByName("Invoke");
                if (delegateInvokeMethod != null)
                {
                    delegateInvokeMethod.ReturnType = delegateReturnType;
                }
            }
        }
#if DEBUG
        public override string ToString()
        {
            return "delegate " + this.FullNameAsString;
        }
#endif
    }




}
