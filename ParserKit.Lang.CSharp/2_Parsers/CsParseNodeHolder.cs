//MIT 2015-2017, ParserApprentice  
using System;
using Parser.CodeDom;
using Parser.ParserKit;
using System.Collections.Generic;

namespace Parser.MyCs
{


    public class CsParseNodeHolder : ParseNodeHolder
    {
        TypeParser.Walker typeParserWalker;
        ClassDeclParser.Walker classDeclWalker;
        StatementParser.Walker statementWalker;
        ExpressionParser.Walker exprWalker;
        NamespaceParser.Walker namespaceWalker;
        ArrayTypeParser.Walker arrayTypeWalker;

        PropertyDeclParser.Walker propertyWalker;
        FieldDeclParser.Walker fieldWalker;
        StructDeclParser.Walker structWalker;
        MethodDeclParser.Walker methodWalker;
        TypeArgumentListParser.Walker typeArgWalker;
        FormalParameterListParser.Walker formalParListWalker;

        public CsParseNodeHolder()
        {

            //default walker
            this.TypeArgListWalker = new TypeArgumentListParser.Walker();
            this.FormalParListWalker = new FormalParameterListParser.Walker();

        }
        void SetOwner(AstWalker walker)
        {
            if (walker != null)
            {
                walker.OwnerHolder = this;
            }
        }

        public TypeParser.Walker TypeBuilder
        {
            get { return this.typeParserWalker; }
            set
            {
                this.typeParserWalker = value; 
                SetOwner(value);
            }
        }
        public ExpressionParser.Walker ExpressionWalker
        {
            get { return this.exprWalker; }
            set
            {
                this.exprWalker = value;
                SetOwner(value);
            }
        }

        public FormalParameterListParser.Walker FormalParListWalker
        {
            get { return this.formalParListWalker; }
            set
            {
                this.formalParListWalker = value;
                SetOwner(value);
            }
        }

        public PropertyDeclParser.Walker PropertyWalker
        {
            get { return this.propertyWalker; }
            set
            {
                this.propertyWalker = value;
                SetOwner(value);
            }
        }
        public FieldDeclParser.Walker FieldWalker
        {
            get { return this.fieldWalker; }
            set
            {
                this.fieldWalker = value;
                SetOwner(value);
            }
        }
        public ArrayTypeParser.Walker ArrayTypeWalker
        {
            get { return this.arrayTypeWalker; }
            set
            {
                this.arrayTypeWalker = value;
                SetOwner(value);
            }
        }

        public ClassDeclParser.Walker ClassWalker
        {
            get { return this.classDeclWalker; }
            set
            {
                this.classDeclWalker = value;
                SetOwner(value);
            }
        }

        public MethodDeclParser.Walker MethodWalker
        {
            get { return this.methodWalker; }
            set
            {
                this.methodWalker = value;
                SetOwner(value);
            }
        }
        public StatementParser.Walker StatementBuilder
        {
            get { return this.statementWalker; }
            set
            {
                this.statementWalker = value;
                SetOwner(value);
            }
        }
        public NamespaceParser.Walker NamespaceWalker
        {
            get { return this.namespaceWalker; }
            set
            {
                this.namespaceWalker = value;
                SetOwner(value);
            }
        }
        public StructDeclParser.Walker StructWalker
        {
            get { return structWalker; }
            set
            {
                structWalker = value;
                SetOwner(value);
            }
        }
        public TypeArgumentListParser.Walker TypeArgListWalker
        {
            get { return typeArgWalker; }
            set
            {
                typeArgWalker = value;
                SetOwner(value);
            }
        }
    }

}