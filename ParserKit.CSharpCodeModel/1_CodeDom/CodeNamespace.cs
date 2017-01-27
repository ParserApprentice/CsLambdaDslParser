//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Parser.CodeDom
{
    //---------------------------------------------------
    public class CodeAstCompilationUnit : CodeObject
    {

        List<IUsingDirective> usingDirs = new List<IUsingDirective>();
        List<INamespaceMember> members = new List<INamespaceMember>();

        public CodeAstCompilationUnit()
        {
        }
        public void AddMember(INamespaceMember nsMember)
        {
            this.members.Add(nsMember);
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.AstCompilationUnit; }
        }

        public CodeExternAliasCollection ExternAliasCollection
        {
            get;
            set;
        }
        public void AddUsingDirective(IUsingDirective usingDirective)
        {
            this.usingDirs.Add(usingDirective);
        }
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            //1. extern alias
            //2. using direcitves
            //3. global attrs
            //4. ns member

            int j = this.usingDirs.Count;
            for (int i = 0; i < j; ++i)
            {
                stbuilder.Append(usingDirs[i].ToString());
                stbuilder.AppendLine();
            }

            j = members.Count;
            for (int i = 0; i < j; ++i)
            {
                stbuilder.Append(members[i].ToString());
            }
            return stbuilder.ToString();
        }
    }
    /// <summary>
    /// namespace or type declaration
    /// </summary>
    public interface INamespaceMember
    {
    }
    //--------------------------------
    public class CodeExternAlias : CodeObject
    {
        public CodeExternAlias()
        {
        }
        public CodeExternAlias(string iden)
        {
            this.Iden = iden;
        }
        public string Iden
        {
            get;
            set;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.ExternAlias; }
        }
        public override string ToString()
        {
            return "extern alias " + this.Iden;
        }
    }
    public class CodeExternAliasCollection : CodeObject
    {
        List<CodeExternAlias> members = new List<CodeExternAlias>();
        public void AddMember(CodeExternAlias externAlias)
        {
            this.members.Add(externAlias);
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.ExternAliasCollection; }
        }
    }
    //--------------------------------
    public interface IUsingDirective
    {

    }
    public class CodeUsingAliasDirective : CodeObject, IUsingDirective
    {
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeUsingAliasDirective; }
        }
        public string AliasName
        {
            get;
            set;
        }
        public string NamespaceName
        {
            get;
            set;
        }
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append("using ");
            stbuilder.Append(this.AliasName);
            stbuilder.Append('=');
            stbuilder.Append(this.NamespaceName);
            stbuilder.Append(';');
            return stbuilder.ToString();
        }
    }
    public class CodeUsingNamespaceDirective : CodeObject, IUsingDirective
    {
        public CodeUsingNamespaceDirective(string namespaceName)
        {
            this.Namespacename = namespaceName;
        }
        public string Namespacename
        {
            get;
            set;
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeUsingNamespaceDirective; }
        }
        public override string ToString()
        {
            return "using " + this.Namespacename + ";";
        }
    }

    public class CodeNamespace : CodeObject, INamespaceMember
    {
        List<INamespaceMember> members = new List<INamespaceMember>();
        CodeNamespaceImportCollection imports = new CodeNamespaceImportCollection();
        CodeNamedItem namespaceName;
        string namespaceNameStr;
        public CodeNamespace()
        {

        }
        public CodeNamespace(string name)
        {
            this.Name = new CodeNamedItem(name);
        }
        public CodeNamespace(CodeNamedItem nameItem)
        {
            this.Name = nameItem;
        }

        public void AddMember(INamespaceMember nsMb)
        {
            this.members.Add(nsMb);
        }
        public CodeNamespaceImportCollection Imports
        {
            get
            {
                return imports;
            }
            set
            {
                imports = value;
            }
        }

        public int Count
        {
            get
            {
                return this.members.Count;
            }
        }
        public IEnumerable<INamespaceMember> GetMemberIterForward()
        {
            foreach (INamespaceMember nmb in this.members)
            {
                yield return nmb;
            }
        }
        public INamespaceMember GetMember(int index)
        {
            return this.members[index];
        }

        public CodeNamedItem Name
        {
            get
            {
                return namespaceName;
            }
            set
            {
                namespaceName = value;
                if (value != null)
                {
                    AcceptChild(namespaceName, CodeObjectRoles.CodeNamespace_Name);
                    this.namespaceNameStr = value.FullNormalName;
                }
                else
                {
                    this.namespaceNameStr = null;
                }
            }
        }
        public void SetName(string name)
        {
            this.Name = new CodeNamedItem(name);
        }
        public string NameAsString
        {
            get
            {
                return this.namespaceNameStr;
            }
        }

#if DEBUG
        public override string ToString()
        {
            StringBuilder stBuilder = new StringBuilder();
            stBuilder.Append("namespace ");
            stBuilder.Append(this.NameAsString);
            stBuilder.Append('{');
            foreach (INamespaceMember nsMb in this.GetMemberIterForward())
            {
                stBuilder.AppendLine(nsMb.ToString());

            }
            stBuilder.Append('}');
            return stBuilder.ToString();
        }
#endif
        public override CodeObjectKind Kind
        {
            get
            {
                return CodeObjectKind.Namespace;
            }
        }


    }


}