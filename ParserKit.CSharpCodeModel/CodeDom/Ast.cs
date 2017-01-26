//MIT, 2015-2017, ParserApprentice
//using System;
//using System.Text;
//using System.Collections.Generic;

//namespace Parser.CodeDom
//{
//    public abstract class AstNode
//    {

//    }

//    public enum TypeVisibility
//    {
//        Private,
//        Public, Protected, Internal,
//        Protected_Internal,
//    }
//    public enum TypeChainKind
//    {
//        Normal,
//        Abstract,
//        Sealed,
//        New,
//    }

//    public abstract class TypeMemberDecl
//    {
//    }

//    public class ClassDeclaration : AstNode
//    {

//        public bool IsStatic { get; set; }
//        public string Name { get; set; }
//        public TypeVisibility Visibility { get; set; }
//        public TypeChainKind ChainKind { get; set; }
//        public TypeParameterConstraints TypeParameterConstraints { get; set; }

//#if DEBUG
//        public override string ToString()
//        {
//            return "class " + Name + "{}";
//        }
//#endif
//    }

//    public class TypeParameterConstraints
//    {
//        public string TypeParameterName;
//        List<TypeParameterConstraint> constraits = new List<TypeParameterConstraint>();
//        public void AddConstraint(TypeParameterConstraint c)
//        {
//            constraits.Add(c);
//        }
//    }

//    public enum TypeParameterConstraintKind
//    {
//        ClassName,
//        Class,
//        Struct,
//        New
//    }

//    public class TypeParameterConstraint
//    {
//        public TypeParameterConstraintKind Kind { get; set; }
//        public string Name { get; set; }

//    }

//    public abstract class CodeExpression
//    {
//    }
//    public abstract class CodeStatement
//    {
//    }

//    public class CodeLocalVarDecl
//    {
//        public string LocalVarName;
//        public CodeExpression InitExpression;
//    }


//    public class CodeLocalVarDeclStatement : CodeStatement
//    {

//    }
//    public class CodeIfStatement : CodeStatement
//    {
//        public CodeExpression TestExpression;
//        public CodeStatement trueStatement;
//        public CodeStatement falseStatement;

//    }
//    public class CodeReturnStatement : CodeStatement
//    {
//        public CodeExpression Expressionl;
//    }
//    public class NamespaceDeclaration : AstNode
//    {
//        List<AstNode> nsMembers = new List<AstNode>();
//        public void AddMember(AstNode astNode)
//        {
//            nsMembers.Add(astNode);
//        }
//        public string Name { get; set; }

//#if DEBUG
//        public override string ToString()
//        {
//            return "namespace " + Name + "{}";
//        }
//#endif
//    }


//}