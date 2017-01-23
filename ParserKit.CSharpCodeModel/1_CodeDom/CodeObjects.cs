//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Parser.CodeDom
{

    public interface ICodeParseNode
    {
        bool IsTerminalNode
        {
            get;
        }
    }

    public enum CodeObjectKind
    {

        Unknown,

        Class, Struct, Delegate, Enum, Interface,
        Field, DefineClass,

        Method, ObjectConstructor, Destructor, ClassConstructor,
        TypeConstraintCollection, TypeConstraint,

        Event, Property, Indexer,

        GenericTypeParameter, GenericMethodParameter,

        Directive,
        //-------------------------------------------------------------------
        //ParserKit  Extensions
        AttributeDecl, AttributeArg, CodeObjectCollection,
        AttributeCollection,
        Comment, Statement, Expression, SQLExpression,
        StatementCollection, Namespace, NamespaceImport, NamedItem, SimpleName,
        TypeParameter, ExpressionCollection,
        ConditionBlock, ElseBlock,
        SwitchLabelBlock, DefaultLabelBlock, StatementBlock,
        TryClause, CatchClause, FinallyClause,
        ParameterExpressionCollection,
        DefineFieldExpressionCollection,

        InternalCodeObjectCollection,
        NamedItemCollection,

        Operator,

        CodeCommentStatementCollection,
        CodeCommentCollection,

        //----------------------------------------------------------------
        CodeXmlDoc, CodeXmlElement, CodeXmlAttribute, CodeXmlName,
        CodeXmlComment, CodeXmlProcessInstruction, CodeXmlAtomicValue,
        CodeXmlTextNode,
        //----------------------------------------------
        XcmlStatement, XcmlExpression,
        XcmlCreateNewContextExpression,
        XcmlHoldAsCurrentExpression,
        //----------------------------------------------
        VariableDeclarator,
        //----------------------------------------------
        AstCompilationUnit,
        ExternAlias,
        ExternAliasCollection,
        CodeUsingAliasDirective,
        CodeUsingNamespaceDirective,
        //-----------
        CodeArgument,
        CodeArgumentList
    }
    public enum CodeReplacingResult
    {
        NotSupport = 0,
        Ok
    }

    public abstract class CodeObject
    {

        LocationCodeArea sourceLocation = LocationCodeArea.Empty;
        CodeObject parentCodeObject;
        CodeObjectRoles codeObjectRole;
        CodeCommentCollection comments;
        ICodeParseNode syntaxNode;
#if DEBUG
        static int dbugTotalCodeObjectCount = 0;
        public int dbugCodeObjecId = 0;
#endif

        public CodeObject()
        {

#if DEBUG

            this.dbugCodeObjecId = ++dbugTotalCodeObjectCount;

#endif
        }


        public ICodeParseNode SyntaxNode
        {
            get
            {
                return this.syntaxNode;
            }
        }

        public void SetSyntaxNode(ICodeParseNode syntaxNode)
        {
            this.syntaxNode = syntaxNode;
        }

        public CodeCommentCollection Comments
        {
            get
            {
                return comments;
            }
            set
            {
                comments = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeCommentCollection_CommentObject);
                }
            }
        }
        public void AddComment(CodeComment cmt)
        {
            if (comments == null)
            {
                this.Comments = new CodeCommentCollection();
            }
            comments.AddCodeObject(cmt);
        }
        public CodeObject ParentCodeObject
        {
            get
            {
                return parentCodeObject;
            }
        }
        public CodeObjectRoles CodeObjectRole
        {
            get
            {
                return codeObjectRole;
            }

        }
        public abstract CodeObjectKind Kind
        {
            get;

        }
        public LocationCodeArea SourceLocation
        {
            get
            {
                return sourceLocation;
            }
        }
        public static void SetCodeArea(CodeObject codeObject,
            LocationCodeArea sourceLocation)
        {
            codeObject.sourceLocation = sourceLocation;
        }

        internal static void TransferSourceLocation(CodeObject fromCodeObject, CodeObject toCodeObject)
        {
            LocationCodeArea loca = fromCodeObject.sourceLocation;
            toCodeObject.sourceLocation = loca;
        }

        protected void AcceptChild(CodeObject childCodeObject, CodeObjectRoles codeObjFunc)
        {
            childCodeObject.parentCodeObject = this;
            childCodeObject.codeObjectRole = codeObjFunc;
        }

        internal static void ClearChildFunction(CodeObject childCodeObject)
        {
#if DEBUG
            //childCodeObject.isUnassign = true;
#endif

            childCodeObject.parentCodeObject = null;
            childCodeObject.codeObjectRole = CodeObjectRoles.CodeObject_UnAssigned;
        }
        public static void CompilerModify(CodeExpression oldCodeExpression, CodeExpression newCodeExpression)
        {
            newCodeExpression.IsCompilerGen = true;

            Modify(oldCodeExpression, newCodeExpression);
            if (!oldCodeExpression.IsCompilerGen)
            {

                CodeExpression.StoreOriginalUserCodeExpression(newCodeExpression, oldCodeExpression);
            }
            else
            {

                CodeExpression.StoreOriginalUserCodeExpression(newCodeExpression, oldCodeExpression);
            }
        }
        public static void CompilerReplaceMethodBody(CodeMethodDeclaration originalMethod, CodeStatementCollection newMethodBody)
        {

            if (originalMethod.UserOriginalMethodBody == null)
            {
                originalMethod.UserOriginalMethodBody = originalMethod.Body;
            }
            originalMethod.Body = newMethodBody;
        }

        static void Modify(CodeObject oldCodeObject, CodeObject newCodeObject)
        {
            CodeObject parentCodeObj = oldCodeObject.ParentCodeObject;
            CodeReplacingResult replacingResult = CodeReplacingResult.NotSupport;
            //--------------------------------------------
            if (parentCodeObj == null)
            {
                throw new NotSupportedException();
            }

            if (oldCodeObject is CodeLambdaExpression)
            {

                if (newCodeObject is CodeExpression)
                {
                    CodeExpression.SetCompilerGenerateExpressionForEmit((CodeExpression)oldCodeObject, (CodeExpression)newCodeObject);
                    replacingResult = CodeReplacingResult.Ok;
                }
                else
                {

                }
            }
            if (parentCodeObj is CodeExpression && newCodeObject is CodeExpression)
            {
                CodeExpression parentCodeExpr = (CodeExpression)parentCodeObj;

                EnsureSameCodeObjectFunctionValue(oldCodeObject, newCodeObject);
                replacingResult = parentCodeExpr.ReplaceChildExpression((CodeExpression)oldCodeObject, (CodeExpression)newCodeObject);

            }
            else if (parentCodeObj is CodeTypeMember)
            {
                if (parentCodeObj is CodeFieldDeclaration && oldCodeObject.CodeObjectRole == CodeObjectRoles.CodeField_InitExpression)
                {
                    CodeFieldDeclaration codeFieldDecl = (CodeFieldDeclaration)parentCodeObj;
                    codeFieldDecl.InitExpression = (CodeExpression)newCodeObject;
                    replacingResult = CodeReplacingResult.Ok;
                }
            }
            else if (parentCodeObj is CodeStatement)
            {

                if (newCodeObject is CodeExpression)
                {
                    CodeStatement codestm = (CodeStatement)parentCodeObj;
                    EnsureSameCodeObjectFunctionValue(oldCodeObject, newCodeObject);
                    replacingResult = codestm.ReplaceChildExpression((CodeExpression)newCodeObject);
                }

            }
            else if (parentCodeObj is CodeExpressionCollection)
            {
                CodeExpressionCollection exprCollection = (CodeExpressionCollection)parentCodeObj;
                replacingResult = exprCollection.ReplaceExpressionWith((CodeExpression)oldCodeObject, (CodeExpression)newCodeObject);

            }
            else if (parentCodeObj is CodeStatementCollection)
            {

                CodeStatementCollection stmCollection = (CodeStatementCollection)parentCodeObj;

                if (newCodeObject is CodeStatement)
                {
                    stmCollection.ReplaceSpecificStatement((CodeStatement)oldCodeObject, (CodeStatement)newCodeObject);
                    replacingResult = CodeReplacingResult.Ok;

                }
                else if (newCodeObject is CodeStatementCollection)
                {

                    stmCollection.ReplaceSpecificStatement((CodeStatement)oldCodeObject, (CodeStatementCollection)newCodeObject);
                    replacingResult = CodeReplacingResult.Ok;

                }
                else
                {
                    throw new NotSupportedException();
                }

            }
            else if (parentCodeObj is CodeIfBlock)
            {
                CodeIfBlock codeIfBlock = (CodeIfBlock)parentCodeObj;
                if (oldCodeObject is CodeExpression && newCodeObject is CodeExpression)
                {
                    codeIfBlock.ConditionExpression = (CodeExpression)newCodeObject;
                    replacingResult = CodeReplacingResult.Ok;
                }

            }
            else
            {
                throw new NotSupportedException();
            }
            if (replacingResult != CodeReplacingResult.Ok)
            {

                throw new NotSupportedException();
            }
            else
            {

                CodeObject.ClearChildFunction(oldCodeObject);
            }

        }
        static void EnsureSameCodeObjectFunctionValue(CodeObject oldCodeObject, CodeObject newCodeObject)
        {
            if (newCodeObject.codeObjectRole != oldCodeObject.codeObjectRole
                && newCodeObject.codeObjectRole == CodeObjectRoles.CodeObject_UnAssigned)
            {

                newCodeObject.codeObjectRole = oldCodeObject.codeObjectRole;

            }
            else if (oldCodeObject.codeObjectRole == CodeObjectRoles.CodeObject_UnAssigned)
            {
                throw new NotSupportedException();
            }
        }
    }

    public class CodeSimpleName : CodeObject
    {
        //id , not include artity ***

        string identifier;
        CodeTypeArgs typeArgs;
        CodeSimpleName nextSimpleName;
        public CodeSimpleName(string identifier)
        {
#if DEBUG
            //if (identifier.Contains("`"))
            //{
            //}
#endif
            this.identifier = identifier;

        }
        public CodeSimpleName Next
        {
            get
            {
                return this.nextSimpleName;
            }
            set
            {
                if (value == this)
                {
                }
                this.nextSimpleName = value;
            }
        }
        public CodeTypeArgs TypeArgList
        {
            get
            {
                return typeArgs;
            }
            set
            {
                typeArgs = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeSimpleName_TypeArgCollection);
                }
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.SimpleName; }
        }

        public string FullName
        {
            get
            {
                if (typeArgs == null)
                {
                    return identifier;
                }
                else
                {
                    CodeTypeArgs codeTypeargs = this.TypeArgList;

                    int j = codeTypeargs.Count;
                    StringBuilder stBuilder = new StringBuilder();
                    stBuilder.Append(identifier);// + "<");
                    stBuilder.Append('<');
                    for (int i = 0; i < j; i++)
                    {

                        CodeTypeReference typeref = codeTypeargs[i];
                        stBuilder.Append(typeref.FullName);
                        if (i < j - 1)
                        {
                            stBuilder.Append(',');
                        }
                    }
                    stBuilder.Append('>');
                    return stBuilder.ToString();
                }
            }
        }

        public string NormalName
        {
            get
            {
                if (this.typeArgs == null)
                {
                    return identifier;
                }
                else
                {

                    return identifier + "`" + TypeArgList.Count;
                }
            }
        }

        public string NameWithoutArity
        {
            get
            {
                return identifier;
            }
        }

        public bool HasTypeArgs
        {
            get
            {
                return (TypeArgList != null) && TypeArgList.Count > 0;
            }
        }
        public bool IsNameEqualTo(string anotherName)
        {
            return identifier == anotherName;
        }
#if DEBUG
        public override string ToString()
        {
            return FullName;
        }
#endif
    }

    public class CodeTypeArgs : CodeObject
    {
        CodeTypeReferenceCollection list = new CodeTypeReferenceCollection();
        public CodeTypeArgs()
        {
        }
        public CodeTypeReference this[int index]
        {
            get
            {
                return this.list[index];
            }
        }
        public void AddTypeArg(CodeTypeReference typeref)
        {
            this.list.Add(typeref);
        }
        public IEnumerator<CodeTypeReference> GetEnumerator()
        {

            foreach (CodeTypeReference typeref in list)
            {
                yield return typeref;
            }
        }
        public int Count
        {
            get
            {
                return list.Count;
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.NamedItemCollection; }
        }
    }
    public class CodeTypeReferenceCollection : List<CodeTypeReference>
    {
        public CodeTypeReferenceCollection()
        {
        }
    }
    public class CodeNamedItem : CodeObject
    {

        CodeSimpleName first;
        CodeSimpleName last;

        public CodeNamedItem()
        {

        }
        public CodeNamedItem(LocationCodeArea loc)
        {
            CodeObject.SetCodeArea(this, loc);
        }
        public CodeNamedItem(string name)
        {
            first = new CodeSimpleName(name);
            last = first;
            AcceptChild(first, CodeObjectRoles.CodeNamedItem_SimpleName);
        }
        public CodeNamedItem(CodeSimpleName name)
        {
            first = name;
            last = name;
            AcceptChild(name, CodeObjectRoles.CodeNamedItem_SimpleName);
        }


        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.NamedItem; }
        }
        /// <summary>
        /// only name, no following dot .
        /// </summary>
        public bool IsSimpleName
        {
            get
            {
                return first.Next == null;
            }
        }

        public CodeSimpleName AddNextQName(string name)
        {
            //generate new CodeSimpleName each time we add it
            CodeSimpleName nextQName = new CodeSimpleName(name);
            AddNextQName(nextQName);
            return nextQName;
        }
        public void AddNextQName(CodeSimpleName nextQName)
        {
            if (first == null)
            {
                first = nextQName;
                last = nextQName;
            }
            else
            {
                last.Next = nextQName;
                last = nextQName;
            }
            AcceptChild(nextQName, CodeObjectRoles.CodeNamedItem_SimpleName);
        }
        public void AddTypeArg(CodeTypeReference typeArg)
        {
            if (last.TypeArgList == null)
            {
                last.TypeArgList = new CodeTypeArgs();
            }
            last.TypeArgList.AddTypeArg(typeArg);
        }

        public bool HasTypeArgs
        {
            get
            {
                return last.TypeArgList != null;
            }
        }
        public CodeSimpleName First
        {
            get
            {
                return first;
            }
        }
        public CodeSimpleName Last
        {
            get
            {
                return last;
            }
        }


        public static CodeNamedItem ParseQualifiedName(string typename)
        {

            StringBuilder buffer = new StringBuilder();
            char[] typeNameCharArray = typename.ToCharArray();
            int j = typename.Length;
            CodeNamedItem namedItem = null;
            Stack<CodeNamedItem> namedItemStack = new Stack<CodeNamedItem>();
            int state = 0;

            for (int i = 0; i < j; i++)
            {

                char c = typeNameCharArray[i];
                switch (state)
                {
                    case 0:
                        {

                            if (c == '`')
                            {

                                state = 1;
                            }
                            else if (c == '<')
                            {

                                if (namedItem == null)
                                {
                                    namedItem = new CodeNamedItem(buffer.ToString());
                                }
                                else
                                {
                                    namedItem.AddNextQName(buffer.ToString());
                                }
                                buffer.Length = 0;//clear
                                namedItemStack.Push(namedItem);
                                namedItem = null;

                                state = 0;
                            }
                            else if (c == ',' || c == '>')
                            {

                                if (namedItem == null)
                                {
                                    namedItem = new CodeNamedItem(buffer.ToString());
                                }
                                else
                                {
                                    if (buffer.Length > 0)
                                    {
                                        namedItem.AddNextQName(buffer.ToString());
                                    }
                                }

                                namedItemStack.Peek().AddTypeArg(new CodeTypeReference(namedItem));
                                buffer.Length = 0;
                                state = 0;

                                if (c == '>')
                                {
                                    namedItem = namedItemStack.Pop();
                                }
                                else
                                {
                                    namedItem = null;
                                }
                            }
                            else if (c == '.')
                            {

                                if (namedItem == null)
                                {
                                    namedItem = new CodeNamedItem(buffer.ToString());
                                }
                                else
                                {
                                    namedItem.AddNextQName(buffer.ToString());

                                }
                                buffer.Length = 0;//Clear
                            }
                            else
                            {
                                buffer.Append(c);
                            }
                        } break;
                    case 1:
                        {
                            if (c == '<')
                            {
                                if (namedItem == null)
                                {
                                    namedItem = new CodeNamedItem(buffer.ToString());
                                }
                                else
                                {
                                    namedItem.AddNextQName(buffer.ToString());
                                }
                                buffer.Length = 0;
                                namedItemStack.Push(namedItem);
                                namedItem = null;

                                state = 0;
                            }
                        } break;
                }
            }


            if (buffer.Length > 0)
            {
                if (namedItem != null)
                {
                    namedItem.AddNextQName(buffer.ToString());
                }
                else
                {
                    namedItem = new CodeNamedItem(buffer.ToString());
                }
            }
            return namedItem;
        }

        public static CodeNamedItem CreateCodeNameItem(string typenameWithoutTypeArgs, params CodeTypeReference[] typeArgs)
        {
            CodeNamedItem codeNameItem = ParseQualifiedName(typenameWithoutTypeArgs);
            if (typeArgs != null)
            {
                int j = typeArgs.Length;
                for (int i = 0; i < j; ++i)
                {
                    codeNameItem.AddTypeArg(typeArgs[i]);
                }
            }
            return codeNameItem;

        }
        public string QualifiedPart
        {
            get
            {
                if (first == last)
                {
                    return string.Empty;
                }
                else
                {
                    StringBuilder stBuilder = new StringBuilder();
                    CodeSimpleName curNode = first;
                    bool isFirst = true;
                    while (curNode != last)
                    {
                        if (!isFirst)
                        {
                            stBuilder.Append('.');
                        }
                        stBuilder.Append(curNode.FullName);
                        isFirst = false;
                        curNode = curNode.Next;
                    }
                    return stBuilder.ToString();
                }

            }
        }

        public string FullName
        {

            get
            {

                if (first == last)
                {

                    return first.FullName;
                }
                else
                {
                    StringBuilder stBuilder = new StringBuilder();

                    CodeSimpleName curNode = first;
                    bool repeat = false;
                    do
                    {
                        stBuilder.Append(curNode.FullName);
                        if (curNode.Next != null)
                        {
                            curNode = curNode.Next;
                            stBuilder.Append('.');
                            repeat = true;
                        }
                        else
                        {
                            repeat = false;
                        }

                    } while (repeat);
                    return stBuilder.ToString();
                }
            }
        }

        public string FullNormalName
        {
            get
            {
                StringBuilder stBuilder = new StringBuilder();

                CodeSimpleName curNode = first;
                bool repeat = false;
                bool enterNestedTypeMode = false;

                do
                {

                    stBuilder.Append(curNode.NormalName);

                    if (curNode.Next != null)
                    {

                        if (!enterNestedTypeMode && curNode.HasTypeArgs)
                        {
                            enterNestedTypeMode = true;
                        }

                        curNode = curNode.Next;

                        if (!enterNestedTypeMode)
                        {
                            stBuilder.Append('.');
                        }
                        else
                        {
                            stBuilder.Append('+');
                        }
                        repeat = true;
                    }
                    else
                    {
                        repeat = false;
                    }

                } while (repeat);
                return stBuilder.ToString();
            }
        }
        public override string ToString()
        {
            return FullName;
        }
    }

    //-----------------------------
    public class CodeTypeMemberSignature
    {

        CodeObjectKind codeObjectKind;
        CodeParameterExpressionCollection methodArgs;
        string name;
        public CodeTypeMemberSignature(string name, CodeObjectKind codeObjectKind)
        {
            this.codeObjectKind = codeObjectKind;
            methodArgs = new CodeParameterExpressionCollection();
            this.name = name;
        }

        public CodeTypeMemberSignature(string name, CodeParameterExpressionCollection methodArgs, CodeObjectKind codeObjectKind)
        {
            //used when create CodeTypeMemberSignature from MethodDeclaration
            this.codeObjectKind = codeObjectKind;
            this.methodArgs = methodArgs;
            this.name = name;
        }

        public CodeTypeMemberSignature(string name, CodeTypeReferenceCollection args, CodeObjectKind codeObjectKind)
        {
            //used when create CodeTypeMemberSignature from  expression

            this.codeObjectKind = codeObjectKind;
            methodArgs = new CodeParameterExpressionCollection();
            int i = 0;
            foreach (CodeTypeReference typeref in args)
            {
                CodeParameterDeclarationExpression codeParamDeclExpr = new CodeParameterDeclarationExpression(i.ToString(), typeref);

                methodArgs.AddCodeObject(codeParamDeclExpr);
                i++;
            }

            this.name = name;
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        public CodeParameterExpressionCollection MethodArgs
        {
            get
            {
                return methodArgs;
            }
            set
            {
                methodArgs = value;
            }
        }

        public override string ToString()
        {

            string methodSig = name;
            if (methodArgs != null && methodArgs.Count > 0)
            {
                methodSig += "(" + methodArgs.Signature + ")";
            }
            else
            {
                if (codeObjectKind == CodeObjectKind.Method
                    || codeObjectKind == CodeObjectKind.ObjectConstructor)
                {
                    methodSig += "()";
                }
            }
            return methodSig;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class CodeTypeParameter : CodeObject
    {

        //not complete here
        //TODO: review this again

        // * The CodeTypeParameter class represents a type parameter 
        // * in the declaration of a generic type or method. 
        // A generic type or method declaration contains one or more 
        // * unspecified types known as type parameters. 
        // * A type parameter name stands for the type within 
        // * the body of the generic declaration. 
        // * For example, the generic declaration for 
        // * the List<(Of <(T>)>) class contains the type parameter T.        
        // * */

        string name;
        public CodeTypeParameter(string name)
        {
            this.name = name;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.TypeParameter; }
        }
        public string Name
        {
            get
            {
                return this.name;
            }

        }
#if DEBUG
        public override string ToString()
        {
            return this.name;
        }
#endif

    }
    public abstract class CodeObjectCollection<T> : CodeObject where T : CodeObject
    {
        List<T> collection = null;
        public CodeObjectCollection()
        {
            collection = new List<T>();
        }

        public void AddCodeObject(T codeObject)
        {

            if (codeObject == null)
            {
            }
            collection.Add(codeObject);
            AcceptChild(codeObject, CollectionMemberFunction);
        }

        public abstract CodeObjectRoles CollectionMemberFunction { get; }

        public void AddRange(CodeObjectCollection<T> codeObjects)
        {

            foreach (T co in codeObjects)
            {
                AddCodeObject(co);
            }

        }
        public void Insert(int index, T codeObject)
        {
            collection.Insert(index, codeObject);
            if (codeObject != null)
            {
                AcceptChild(codeObject, CollectionMemberFunction);
            }
        }
        public T this[int index]
        {

            get
            {
                return collection[index];
            }
            set
            {
                collection[index] = value;
                AcceptChild(value, CollectionMemberFunction);
            }
        }
        public int Count
        {
            get
            {
                return collection.Count;
            }
        }
        public void Clear()
        {
            collection.Clear();
        }

        public T GetItem(int index)
        {

            return collection[index];
        }
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T t in collection)
            {
                yield return t;
            }
        }
        public void RemoveLastItem()
        {

            if (collection.Count > 0)
            {
                collection.RemoveAt(collection.Count - 1);
            }
        }
        public void RemoveAt(int index)
        {
            collection.RemoveAt(index);
        }
        internal static List<T> GetInteralList(CodeObjectCollection<T> codeObjectCollection)
        {
            return codeObjectCollection.collection;
        }

    }

}
