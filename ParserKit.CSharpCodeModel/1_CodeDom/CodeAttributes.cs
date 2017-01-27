//MIT, 2015-2017, ParserApprentice
 

namespace Parser.CodeDom
{
    public class CodeAttributeDeclaration : CodeObject
    {
        CodeTypeReference attributeType;
        CodeNamedItem attributeTypeName;
        CodeAttributeArgumentCollection arguments;

        public CodeAttributeDeclaration(CodeTypeReference attributeType)
        {
            this.attributeType = attributeType;
        }
        public CodeAttributeDeclaration(CodeNamedItem attributeTypeName)
        {
            this.attributeTypeName = attributeTypeName;
            if (!attributeTypeName.Last.FullName.EndsWith("Attribute"))
            {
                attributeTypeName = CodeNamedItem.ParseQualifiedName(attributeTypeName.ToString() + "Attribute");
            }
            this.attributeType = new CodeTypeReference(attributeTypeName);
        }

        public string NameAsString
        {
            get
            {
                return attributeType.FullName;
            }
        }
        public CodeTypeReference AttributeType
        {
            get
            {
                return attributeType;
            }
        }
        public CodeAttributeArgumentCollection Arguments
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
                    AcceptChild(arguments, CodeObjectRoles.CodeAttributeDeclaration_Attributes);
                }
            }
        }

        public CodeTypeMember OwnerTypeMember
        {
            get;
            set;
        }
        public bool IsCompilerGenerated
        {
            get;
            set;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.AttributeDecl; }
        } 
    }

    public class CodeAttributeArgument : CodeObject
    {
        private readonly string name;
        private readonly CodeExpression value;

        public CodeAttributeArgument(CodeExpression value)
        {
            this.value = value;
            AcceptChild(value, CodeObjectRoles.CodeAttributeArgument_Value); 
            if (value is CodeBinaryOperatorExpression)
            { 
                CodeBinaryOperatorExpression binOpExpr = (CodeBinaryOperatorExpression)value;
                if (binOpExpr.BinaryOp == CodeBinaryOperatorName.Assign)
                {

                    if (binOpExpr.LeftExpression is CodeIdExpression)
                    {
                        this.name = ((CodeIdExpression)binOpExpr.LeftExpression).ToString();
                        this.value = binOpExpr.RightExpression;
                    }
                }
            }
        }

        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.AttributeArg; }
        }
        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
        }
        public CodeExpression Value
        {
            get
            {
                return this.value;
            }
        }

    }

    public class CodeAttributeArgumentCollection : CodeObjectCollection<CodeAttributeArgument>
    {
        public CodeAttributeArgumentCollection()
        {
        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeAttributeArgumentCollection_Member; }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.InternalCodeObjectCollection; }
        }
    }
    public class CodeAttributeDeclarationCollection : CodeObjectCollection<CodeAttributeDeclaration>
    {
        public CodeAttributeDeclarationCollection()
        {
        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeAttributeDeclarationCollection_Member; }
        }
        public CodeAttributeDeclaration FindCustomAttributeByName(string name)
        {
            //TODO: review here if it is nessesary
            foreach (CodeAttributeDeclaration customAttr in this)
            {
                if (customAttr.NameAsString == name)
                {

                    return customAttr;
                }
            }
            return null;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.AttributeCollection; }
        }
    }
}
