//MIT, 2015-2017, ParserApprentice
using System; 
using System.Collections.Generic;
using System.Text;


namespace Parser.CodeDom
{

    public class CodeExpressionCollection : CodeObjectCollection<CodeExpression>
    {


        public CodeExpressionCollection()
        {

        }
        public CodeExpressionCollection(params CodeExpression[] exprs)
        {

            foreach (CodeExpression expr in exprs)
            {
                this.AddCodeObject(expr);
            }
        }
        public CodeExpressionCollection(IEnumerable<CodeExpression> exprs)
        {
            foreach (CodeExpression expr in exprs)
            {
                this.AddCodeObject(expr);
            }

        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeExpressionCollection_Member; }
        }
#if DEBUG
        public override string ToString()
        {
            StringBuilder stBuilder = new StringBuilder();
            //stBuilder.Append("{");
            int j = Count;
            int i = 0;
            foreach (CodeExpression exp in this)
            {
                if (exp != null)
                {
                    stBuilder.Append(exp.ToString());
                }
                else
                {
                    stBuilder.Append(string.Empty);
                }
                if (i < j - 1)
                {
                    stBuilder.Append(',');
                }
                i++;
            }
            //stBuilder.Append("}");
            return stBuilder.ToString();

        }
#endif
        public CodeReplacingResult ReplaceExpressionWith(CodeExpression oldExpr, CodeExpression newExpr)
        {
            int j = Count;
            for (int i = 0; i < j; i++)
            {
                CodeExpression expr = this[i];
                if (expr == oldExpr)
                {

                    CodeObject.ClearChildFunction(oldExpr);
                    this[i] = newExpr;

                    return CodeReplacingResult.Ok;
                }
            }
            return CodeReplacingResult.NotSupport;
        }
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.ExpressionCollection;
            }
        }
    }


    public class CodeStatementCollection : CodeObject
    {

        LinkedList<CodeStatement> stmCollections = new LinkedList<CodeStatement>();
        bool hasCompleteExitPath;

        public CodeStatementCollection()
        {

        }
        public CodeStatementCollection(IEnumerable<CodeStatement> stms)
        {
            foreach (CodeStatement stm in stms)
            {
                Add(stm);
            }
        }
        public CodeStatementCollection(params CodeStatement[] stms)
        {
            int j = stms.Length;
            for (int i = 0; i < j; ++i)
            {
                Add(stms[i]);
            }
        }
        public bool HasCompleteExitPath
        {
            get
            {
                return this.hasCompleteExitPath;
            }
            set
            {
                this.hasCompleteExitPath = value;
            }
        }
        public void Add(CodeExpression expr)
        {

            CodeExpressionStatement exprStm = new CodeExpressionStatement(expr);
            stmCollections.AddLast(exprStm);
            AcceptChild(exprStm, CodeObjectRoles.CodeStatementCollection_Member);

        }
        public void Add(CodeStatement codeStatement)
        {
            stmCollections.AddLast(codeStatement);
            AcceptChild(codeStatement, CodeObjectRoles.CodeStatementCollection_Member);
        }
        public void Add(IEnumerable<CodeStatement> codeStatements)
        {
            foreach (CodeStatement stmt in codeStatements)
            {
                Add(stmt);
            }            
        }
        public void AddTop(CodeStatement codeStatement)
        {
            stmCollections.AddFirst(codeStatement);
            AcceptChild(codeStatement, CodeObjectRoles.CodeStatementCollection_Member);
        }
        public void AddTop(IEnumerable<CodeStatement> stms)
        {

            LinkedListNode<CodeStatement> insert_afterNode = stmCollections.First;

            bool isFirstNode = true;
            foreach (CodeStatement stm in stms)
            {
                if (isFirstNode)
                {

                    if (insert_afterNode != null)
                    {

                        insert_afterNode = stmCollections.AddBefore(insert_afterNode, stm);

                    }
                    else
                    {

                        insert_afterNode = stmCollections.AddFirst(stm);

                    }
                    isFirstNode = false;
                }
                else
                {

                    insert_afterNode = stmCollections.AddAfter(insert_afterNode, stm);
                }
                AcceptChild(stm, CodeObjectRoles.CodeStatementCollection_Member);
            }
        }
        public void AddAfter(CodeStatement afterStatment, CodeStatement newStatement)
        {

            LinkedListNode<CodeStatement> foundLinkedListNode = null;
            LinkedListNode<CodeStatement> checkingNode = stmCollections.First;
            while (checkingNode != null)
            {
                if (checkingNode.Value == afterStatment)
                {
                    foundLinkedListNode = checkingNode;
                    break;
                }
                else
                {

                    checkingNode = checkingNode.Next;
                }
            }
            //--

            if (foundLinkedListNode != null)
            {
                stmCollections.AddAfter(foundLinkedListNode, newStatement);
                AcceptChild(newStatement, CodeObjectRoles.CodeStatementCollection_Member);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public void AddBefore(CodeStatement beforeStm, CodeStatement newStatement)
        {

            LinkedListNode<CodeStatement> foundLinkedListNode = null;
            LinkedListNode<CodeStatement> checkingNode = stmCollections.First;
            while (checkingNode != null)
            {
                if (checkingNode.Value == beforeStm)
                {
                    foundLinkedListNode = checkingNode;
                    break;
                }
                else
                {

                    checkingNode = checkingNode.Next;
                }
            }

            if (foundLinkedListNode != null)
            {
                stmCollections.AddBefore(foundLinkedListNode, newStatement);
                AcceptChild(newStatement, CodeObjectRoles.CodeStatementCollection_Member);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        internal LinkedListNode<CodeStatement> AddAfter(LinkedListNode<CodeStatement> afterStatementNode, CodeStatement newStatement)
        {
            AcceptChild(newStatement, CodeObjectRoles.CodeStatementCollection_Member);
            return stmCollections.AddAfter(afterStatementNode, newStatement);
        }
        internal LinkedListNode<CodeStatement> AddBefore(LinkedListNode<CodeStatement> beforeStatementNode, CodeStatement newStatement)
        {
            AcceptChild(newStatement, CodeObjectRoles.CodeStatementCollection_Member);
            return stmCollections.AddBefore(beforeStatementNode, newStatement);
        }
        internal void Remove(LinkedListNode<CodeStatement> tobeRemoveNode)
        {
            stmCollections.Remove(tobeRemoveNode);
        }
        internal LinkedListNode<CodeStatement> RemoveLastStatement()
        {
            LinkedListNode<CodeStatement> lastStm = stmCollections.Last;
            stmCollections.RemoveLast();
            return lastStm;
        }
        public CodeStatement FirstStatement
        {
            get
            {
                if (stmCollections != null && stmCollections.Count > 0)
                {
                    return stmCollections.First.Value;
                }
                else
                {
                    return null;
                }
            }
        }
        public CodeStatement LastStatement
        {
            get
            {
                if (stmCollections != null && stmCollections.Count > 0)
                {
                    return stmCollections.Last.Value;
                }
                else
                {
                    return null;
                }
            }
        }
        internal CodeStatement RemoveTop()
        {
            if (stmCollections != null && stmCollections.Count > 0)
            {
                CodeStatement result = stmCollections.First.Value;
                stmCollections.RemoveFirst();
                return result;
            }
            else
            {
                return null;
            }
        }

        public IEnumerator<CodeStatement> GetEnumerator()
        {
            foreach (CodeStatement stm in stmCollections)
            {
                yield return stm;
            }
        }
        public IEnumerable<CodeStatement> GetFinalCodeStatementIter()
        {

            foreach (CodeStatement stm in this)
            {
                if (stm.CompilerReplaceStatement != null)
                {
                    yield return stm.CompilerReplaceStatement;
                }
                else
                {
                    yield return stm;
                }
            }

        }

        public int Count
        {
            get
            {
                return stmCollections.Count;
            }
        }

        public CodeStatement GetLastStatement()
        {
            if (stmCollections.Last != null)
            {
                return stmCollections.Last.Value;
            }
            else
            {
                return null;
            }
        }

        public bool ReplaceSpecificStatement(CodeStatement oldStatement, CodeStatement newStatement)
        {


            LinkedListNode<CodeStatement> searchStmNode = SearchStatementNode(oldStatement);
            if (searchStmNode != null)
            {
                stmCollections.AddBefore(searchStmNode, newStatement);
                stmCollections.Remove(searchStmNode);
                return true;
            }
            else
            {
                return false;
            }
        }
        LinkedListNode<CodeStatement> SearchStatementNode(CodeStatement searchStatement)
        {
            LinkedListNode<CodeStatement> searchNode = stmCollections.First;

            while (searchNode != null)
            {
                if (searchNode.Value == searchStatement)
                {

                    return searchNode;
                }
                searchNode = searchNode.Next;
            }
            return null;

        }
        public bool ReplaceSpecificStatement(CodeStatement oldStatement, CodeStatementCollection newStmCollections)
        {

            LinkedListNode<CodeStatement> searchStmNode = SearchStatementNode(oldStatement);
            if (searchStmNode != null)
            {

                foreach (CodeStatement newstm in newStmCollections)
                {
                    stmCollections.AddBefore(searchStmNode, newstm);
                }
                stmCollections.Remove(searchStmNode);
                return true;
            }
            else
            {
                return false;
            }

        }

#if DEBUG
        public override string ToString()
        {
            int j = Count;
            StringBuilder stBuilder = new StringBuilder();
            foreach (CodeStatement stm in stmCollections)
            {
                stBuilder.AppendLine(stm.ToString() + ";");
            }

            return stBuilder.ToString();
        }
#endif

        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.StatementCollection; }
        }
    }

    public class CodeTypeParameterCollection : CodeObject
    {
        CodeTypeDeclaration ownerType;
        CodeMethodDeclaration ownerMethod;
        List<CodeTypeParameter> collection = new List<CodeTypeParameter>();
        public CodeTypeParameterCollection()
        {

        }

        public CodeTypeParameterCollection(CodeTypeDeclaration ownerType)
        {
            this.ownerType = ownerType;
        }
        public CodeTypeParameterCollection(CodeMethodDeclaration ownerMethod)
        {
            this.ownerMethod = ownerMethod;
        }
        public void Add(CodeTypeParameter typeParameter)
        {
            collection.Add(typeParameter);
            if (ownerType != null)
            {
                CodeTypeDeclaration.AcceptTypeParameter(ownerType, typeParameter);
            }
            else if (ownerMethod != null)
            {
                CodeMethodDeclaration.AcceptTypeParameter(ownerMethod, typeParameter);
            }
        }
        public int Count
        {
            get
            {
                return collection.Count;
            }

        }
        public CodeTypeParameter SearchParameterByTypeName(string parameterTypeName)
        {

            foreach (CodeTypeParameter parameter in collection)
            {
                if (parameter.Name == parameterTypeName)
                {
                    return parameter;
                }
            }
            return null;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.InternalCodeObjectCollection; }
        }
        public CodeTypeParameter this[int index]
        {
            get
            {
                return collection[index];
            }
        }

        public IEnumerable<CodeTypeParameter> GetTypeParameterIter()
        {
            foreach (CodeTypeParameter typeParam in collection)
            {
                yield return typeParam;
            }
        }
    }

    public class TypeMemberCollection : CodeObject
    {

        List<CodeMethodDeclaration> methods;
        List<CodePropertyDeclaration> properties;
        List<CodeEventDeclaration> events;
        List<CodeFieldDeclaration> fields;
        List<CodeTypeDeclaration> nestedTypes;
        List<CodeIndexerDeclaration> indexers;
        CodeTypeDeclaration ownerType;
        public TypeMemberCollection(CodeTypeDeclaration ownerType)
        {

            this.ownerType = ownerType;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeObjectCollection; }
        }

        /// <summary>
        /// append last of this list
        /// </summary>
        /// <param name="codeTypeMember"></param>
        public void AddMember(CodeTypeMember codeTypeMember)
        {
#if(DEBUG)
            if (codeTypeMember == this.ownerType)
            {
                throw new Exception("Cyclic Error Exception");
            }
#endif

            switch (codeTypeMember.MemberKind)
            {
                case CodeTypeMemberKind.Method:
                    {
                        if (methods == null)
                        {
                            methods = new List<CodeMethodDeclaration>();
                        }
                        methods.Add((CodeMethodDeclaration)codeTypeMember);
                        CodeTypeDeclaration.AcceptMember(ownerType, codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Property:
                    {
                        if (properties == null)
                        {
                            properties = new List<CodePropertyDeclaration>();
                        }
                        properties.Add((CodePropertyDeclaration)codeTypeMember);
                        CodeTypeDeclaration.AcceptMember(ownerType, codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Field:
                    {
                        if (fields == null)
                        {
                            fields = new List<CodeFieldDeclaration>();
                        }
                        fields.Add((CodeFieldDeclaration)codeTypeMember);
                        CodeTypeDeclaration.AcceptMember(ownerType, codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Event:
                    {
                        if (events == null)
                        {
                            events = new List<CodeEventDeclaration>();
                        }
                        events.Add((CodeEventDeclaration)codeTypeMember);
                        CodeTypeDeclaration.AcceptMember(ownerType, codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Type:
                    {
                        if (nestedTypes == null)
                        {
                            nestedTypes = new List<CodeTypeDeclaration>();
                        }
                        nestedTypes.Add((CodeTypeDeclaration)codeTypeMember);
                        CodeTypeDeclaration.AcceptMember(ownerType, codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Indexer:
                    {

                        CodeIndexerDeclaration myIndexer = (CodeIndexerDeclaration)codeTypeMember;
                        if (indexers == null)
                        {
                            indexers = new List<CodeIndexerDeclaration>();
                        }
                        indexers.Add(myIndexer);
                        CodeTypeDeclaration.AcceptMember(ownerType, codeTypeMember);
                        if (myIndexer.GetDeclaration != null)
                        {
                            this.AddMember(myIndexer.GetDeclaration);
                        }
                        if (myIndexer.SetDeclaration != null)
                        {
                            this.AddMember(myIndexer.SetDeclaration);
                        }


                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

        }
        public void Remove(CodeTypeMember codeTypeMember)
        {

            switch (codeTypeMember.MemberKind)
            {
                case CodeTypeMemberKind.Method:
                    {
                        methods.Remove((CodeMethodDeclaration)codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Property:
                    {
                        properties.Remove((CodePropertyDeclaration)codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Field:
                    {
                        fields.Remove((CodeFieldDeclaration)codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Event:
                    {
                        events.Remove((CodeEventDeclaration)codeTypeMember);
                    }
                    break;
                case CodeTypeMemberKind.Type:
                    {
                        nestedTypes.Remove((CodeTypeDeclaration)codeTypeMember);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
#if DEBUG
        public int dbug_FieldCount
        {
            get
            {
                if (fields != null)
                {
                    return fields.Count;
                }
                else
                {
                    return 0;
                }
            }

        }
#endif


        public IEnumerator<CodeTypeMember> GetEnumerator()
        {

            if (fields != null)
            {

                int j = fields.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return fields[i];
                }
            }
            if (properties != null)
            {
                int j = properties.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return properties[i];
                }
            }
            if (events != null)
            {
                int j = events.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return events[i];
                }
            }
            if (methods != null)
            {
                int j = methods.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return methods[i];
                }
            }

            if (nestedTypes != null)
            {
                int j = nestedTypes.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return nestedTypes[i];
                }
            }
        }

        public CodeMethodDeclaration SearchSingleMethodByName(string name)
        {
            if (methods != null)
            {
                for (int i = methods.Count - 1; i > -1; i--)
                {
                    if (methods[i].Name == name)
                    {
                        return methods[i];
                    }
                }
            }

            return null;
        }

        public List<CodeMethodDeclaration> SearchMethodsByName(string name, int parameterCount)
        {

            List<CodeMethodDeclaration> list = null;
            if (name == null)
            {
                throw new NotSupportedException();
            }
            if (methods != null)
            {
                if (parameterCount < 0)
                {

                    for (int i = methods.Count - 1; i > -1; i--)
                    {
                        if (methods[i].Name == name)
                        {
                            if (list == null)
                            {
                                list = new List<CodeMethodDeclaration>();
                            }
                            list.Add(methods[i]);
                        }
                    }
                }
                else
                {

                    for (int i = methods.Count - 1; i > -1; i--)
                    {
                        if (methods[i].Name == name)
                        {
                            if (list == null)
                            {
                                list = new List<CodeMethodDeclaration>();
                            }

                            CodeMethodDeclaration met = methods[i];
                            if (parameterCount == 0)
                            {
                                if (met.ParameterList == null
                                    || met.ParameterList.Count == 0)
                                {
                                    list.Add(met);
                                }
                            }
                            else
                            {
                                if (met.ParameterList.Count == parameterCount)
                                {
                                    list.Add(met);
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }
        public CodeEventDeclaration SearchEventByName(string name)
        {
            if (events != null)
            {
                for (int i = events.Count - 1; i > -1; i--)
                {
                    if (events[i].Name == name)
                    {
                        return events[i];
                    }
                }
            }

            return null;
        }
        public CodePropertyDeclaration SearchPropertyByName(string name)
        {
            foreach (CodePropertyDeclaration propDecl in this.PropertyIter)
            {
                if (propDecl.Name == name)
                {
                    return propDecl;
                }
            }
            return null;
        }

        internal CodeFieldDeclaration GetFirstInstanceField()
        {
            if (fields != null)
            {
                int j = fields.Count;
                for (int i = 0; i < j; i++)
                {
                    if (!fields[i].IsStatic)
                    {
                        return fields[i];
                    }
                }
            }
            return null;
        }
        internal CodeFieldDeclaration GetLastInstanceField()
        {

            if (fields != null)
            {
                for (int i = fields.Count - 1; i > -1; i--)
                {
                    if (!fields[i].IsStatic)
                    {
                        return fields[i];
                    }
                }

            }
            return null;
        }
        public CodeFieldDeclaration SearchFieldByName(string name)
        {

            if (fields != null)
            {
                for (int i = fields.Count - 1; i > -1; i--)
                {
                    if (fields[i].Name == name)
                    {
                        return fields[i];
                    }
                }
            }
            return null;
        }
        public CodeMethodDeclaration SearchSingleMethodBySignature(string methodsig)
        {
            if (methods != null)
            {
                for (int i = methods.Count - 1; i > -1; i--)
                {
                    if (methods[i].Signature.ToString() == methodsig)
                    {

                        return methods[i];
                    }
                }
            }
            return null;
        }


        internal IEnumerable<CodeMethodDeclaration> GetExtensionMethodIter()
        {
            foreach (CodeTypeMember typemb in this)
            {
                switch (typemb.Kind)
                {
                    case CodeObjectKind.Method:
                        {
                            CodeMethodDeclaration method = (CodeMethodDeclaration)typemb;
                            if (method.IsExtensionMethod)
                            {
                                yield return method;
                            }
                        }
                        break;
                }
            }
        }
        internal IEnumerable<CodeMethodDeclaration> MethodPartIter
        {
            get
            {
                foreach (CodeTypeMember typemb in this)
                {

                    switch (typemb.Kind)
                    {
                        case CodeObjectKind.Method:
                            {
                                yield return (CodeMethodDeclaration)typemb;
                            }
                            break;
                        case CodeObjectKind.ObjectConstructor:
                            {
                                yield return (CodeMethodDeclaration)typemb;
                            }
                            break;
                        case CodeObjectKind.Property:
                            {
                                CodePropertyDeclaration propDecl = (CodePropertyDeclaration)typemb;
                                if (propDecl.GetDeclaration != null)
                                {
                                    yield return propDecl.GetDeclaration;
                                }
                                if (propDecl.SetDeclaration != null)
                                {
                                    yield return propDecl.SetDeclaration;
                                }

                            }
                            break;
                        case CodeObjectKind.Event:
                            {
                                CodeEventDeclaration eventDecl = (CodeEventDeclaration)typemb;
                                if (eventDecl.AddDeclaration != null)
                                {
                                    yield return eventDecl.AddDeclaration;
                                }
                                if (eventDecl.RemoveDeclaration != null)
                                {
                                    yield return eventDecl.RemoveDeclaration;
                                }

                            }
                            break;
                    }

                }
            }
        }
        internal void GetConstructors(List<CodeObjectConstructorDeclaration> output)
        {
            if (methods != null)
            {
                int j = methods.Count;
                for (int i = 0; i < j; i++)
                {
                    if (methods[i].Kind == CodeObjectKind.ObjectConstructor)
                    {
                        output.Add((CodeObjectConstructorDeclaration)methods[i]);
                    }
                }
            }
        }

        internal IEnumerable<CodePropertyDeclaration> PropertyIter
        {
            get
            {
                if (properties != null)
                {
                    int j = properties.Count;
                    for (int i = 0; i < j; i++)
                    {
                        yield return properties[i];
                    }
                }
            }
        }

        public int NestedTypeCount
        {
            get
            {
                if (nestedTypes == null)
                {
                    return 0;
                }
                else
                {
                    return nestedTypes.Count;
                }
            }
        }
        internal CodeTypeDeclaration GetNestedType(int index)
        {
            if (nestedTypes == null)
            {
                return null;
            }
            else
            {
                return nestedTypes[index];
            }
        }
        internal IEnumerable<CodeTypeDeclaration> NestedTypeIter
        {
            get
            {
                if (nestedTypes != null)
                {
                    int j = nestedTypes.Count;
                    for (int i = 0; i < j; i++)
                    {
                        yield return nestedTypes[i];
                    }
                }
            }
        }
        internal void GetLayoutInstanceFields(List<CodeFieldDeclaration> output)
        {
            if (fields != null)
            {

                int j = fields.Count;
                for (int i = 0; i < j; i++)
                {

                    if (!fields[i].IsStatic)
                    {
                        output.Add(fields[i]);
                    }
                }
            }
        }
        internal void GetStaticFields(List<CodeFieldDeclaration> output)
        {
            if (fields != null)
            {

                //order of field may affect layout 
                int j = fields.Count;
                for (int i = 0; i < j; i++)
                {

                    if (fields[i].IsStatic)
                    {
                        output.Add(fields[i]);
                    }
                }
            }
        }

        internal IEnumerable<CodeEventDeclaration> EventIter
        {
            get
            {
                if (events != null)
                {
                    int j = events.Count;
                    for (int i = 0; i < j; i++)
                    {
                        yield return (CodeEventDeclaration)events[i];
                    }
                }
            }
        }

        public static List<CodeMethodDeclaration> GetAllMethods(TypeMemberCollection mbs)
        {
            return mbs.methods;
        }

        internal static List<CodePropertyDeclaration> GetAllProperties(TypeMemberCollection mbs)
        {
            return mbs.properties;
        }

        internal static List<CodeFieldDeclaration> GetAllFields(TypeMemberCollection mbs)
        {
            return mbs.fields;
        }


        internal static List<CodeEventDeclaration> GetAllEvents(TypeMemberCollection mbs)
        {
            return mbs.events;
        }
        internal static List<CodeTypeDeclaration> GetAllNestedTypes(TypeMemberCollection mbs)
        {
            return mbs.nestedTypes;
        }
    }


    public class CodeDefineFieldExpressionCollection : CodeObjectCollection<CodeDefineFieldDeclarationExpression>
    {
        string defineCollectionName;
        public CodeDefineFieldExpressionCollection()
        {
        }
        public string DefineCollectionName
        {
            get
            {
                return defineCollectionName;
            }
            set
            {
                this.defineCollectionName = value;
            }
        }

        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeDefineFieldExpressionCollection_Member; }
        }
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.DefineFieldExpressionCollection;
            }
        }


        public void Drive(CodeDomWalker visitor)
        {

            foreach (CodeDefineFieldDeclarationExpression defFieldExpr in this)
            {


            }

        }
#if DEBUG
        public override string ToString()
        {
            int j = Count;
            string t = "";
            for (int i = 0; i < j; i++)
            {
                CodeDefineFieldDeclarationExpression context = this[i];
                t += context.ToString();
                if (i < j - 1)
                {
                    t += ",";
                }
            }
            return t;
        }
#endif


    }

    public class CodeParameterExpressionCollection : CodeObjectCollection<CodeParameterDeclarationExpression>
    {

        public CodeParameterExpressionCollection()
        {

        }
        public CodeParameterExpressionCollection(params CodeParameterDeclarationExpression[] parameterDecls)
        {

            foreach (CodeParameterDeclarationExpression parameter in parameterDecls)
            {
                if (parameter.ParameterType == null)
                {
                    parameter.IsImplicitParameterType = true;
                }

                this.AddCodeObject(parameter);
            }
        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeParameterExpressionCollection_Member; }
        }

        bool markedForExtensionMethod;

        public bool MarkedForExtensionMethod
        {
            get
            {
                return markedForExtensionMethod;
            }
            set
            {
                markedForExtensionMethod = value;
            }
        }
#if DEBUG
        public override string ToString()
        {
            int j = Count;
            string t = "";
            for (int i = 0; i < j; i++)
            {
                CodeParameterDeclarationExpression context = this[i];
                t += context.ToString();
                if (i < j - 1)
                {
                    t += ",";
                }
            }
            return t;
        }
#endif
        public string Signature
        {
            get
            {

                int j = Count;
                StringBuilder stBuilder = new StringBuilder();
                for (int i = 0; i < j; i++)
                {
                    CodeParameterDeclarationExpression varDecl = this[i];

                    stBuilder.Append(varDecl.ParameterType.FullName);
                    if (i < j - 1)
                    {
                        stBuilder.Append(',');
                    }
                }
                return stBuilder.ToString();
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.ParameterExpressionCollection; }
        }
    }
    public class CodeCommentStatementCollection : CodeObjectCollection<CodeCommentStatement>
    {
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeCommentStatementCollection; }
        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeCommentStatement; }
        }
    }


    public class CodeNamespaceCollection : CodeObjectCollection<CodeNamespace>
    {

        public CodeNamespaceCollection()
        {

        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeCommentStatement; }
        }

        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.InternalCodeObjectCollection; }
        }
#if DEBUG
        public override string ToString()
        {
            StringBuilder stBuilder = new StringBuilder();
            foreach (CodeNamespace ns in this)
            {
                stBuilder.AppendLine(ns.ToString());
            }
            return stBuilder.ToString();
        }
#endif

    }

    public class CodeNamespaceImportCollection : CodeObjectCollection<CodeNamespaceImport>
    {
        public CodeNamespaceImportCollection()
        {
        }
        public override CodeObjectRoles CollectionMemberFunction
        {
            get { return CodeObjectRoles.CodeNamespaceImportCollection_Member; }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.InternalCodeObjectCollection; }
        }
        public IEnumerable<string> GetImportNamespaceNameIter()
        {
            foreach (CodeNamespaceImport ns in this)
            {
                yield return ns.NamespaceName.ToString();
            }
        }
#if DEBUG
        public override string ToString()
        {
            StringBuilder stBuilder = new StringBuilder();
            foreach (CodeNamespaceImport ns in this)
            {
                stBuilder.AppendLine(ns.ToString());
            }
            return stBuilder.ToString();
        }
#endif

    }

}
