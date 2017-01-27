//MIT, 2015-2017, ParserApprentice
 
using System.Collections.Generic;
using System.Text; 
namespace Parser.CodeDom
{   
    public enum CodeXmlNodeType
    {
        XmlShortElementNode,
        XmlOpenElementNode,
        XmlCloseElementNode,
        XmlAttributeNode,
        XmlTextNode,
        XmlCommentNode,
        XmlProcessInstructionNode,
        XmlDocument,
    }
    public class CodeXmlName : CodeObject
    {
        CodeXmlAtomicValue prefix;
        CodeXmlAtomicValue localName;
        public CodeXmlName()
        {

        }
        public CodeXmlName(string prefix, string localName)
        {
            if (prefix != null)
            {
                this.prefix = new CodeXmlAtomicValue(prefix);
            }
            if (localName != null)
            {
                this.localName = new CodeXmlAtomicValue(localName);
            }
        }
        public CodeXmlName(string localName)
        {
            if (localName != null)
            {
                this.localName = new CodeXmlAtomicValue(localName);
            }
        }
        public CodeXmlAtomicValue Prefix
        {
            get
            {
                return prefix;
            }
            set
            {
                prefix = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeXmlName_Prefix);
                }
            }
        }
        public CodeXmlAtomicValue LocalName
        {
            get
            {
                return localName;
            }
            set
            {
                localName = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeXmlName_LocalName);
                }
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeXmlName; }
        }
        public override string ToString()
        {
            if (prefix != null)
            {
                return prefix.Value + ":" + localName.Value;
            }
            else
            {
                return localName.Value;
            }
        }
        public string PrefixAsString
        {
            get
            {
                if (prefix != null)
                {
                    return prefix.Value;
                }
                else
                {
                    return null;
                }
            }
        }
        public string LocalNameAsString
        {
            get
            {
                if (localName != null)
                {
                    return localName.Value;
                }
                else
                {
                    return null;
                }
            }
        }



    }
    public abstract class CodeXmlNode : CodeObject
    {

        CodeXmlName xmlName;
        CodeXmlDocument ownerDoc;
        CodeXmlNodeType nodeType;

        int nodeOrder;
        public CodeXmlNode(CodeXmlDocument ownerDoc)
        {
            this.ownerDoc = ownerDoc;
        }
        public CodeXmlNode(CodeXmlDocument ownerDoc, string nodeName)
        {
            this.ownerDoc = ownerDoc;
            xmlName = new CodeXmlName(nodeName);

        }

        protected static void SetNodeOrder(CodeXmlNode node, int nodeOrder)
        {
            node.nodeOrder = nodeOrder;
        }
        protected static int GetNodeOrder(CodeXmlNode node)
        {
            return node.nodeOrder;
        }


        public CodeXmlName XmlName
        {
            get
            {
                return xmlName;
            }
            set
            {
                xmlName = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeXmlNode_Name);
                }
            }
        }

        /// <summary>
        /// prefix + localname
        /// </summary>
        public string Name
        {
            get
            {
                return xmlName.ToString();
            }
        }
        public string Prefix
        {
            get
            {
                if (xmlName.Prefix != null)
                {
                    return xmlName.Prefix.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public string LocalName
        {
            get
            {
                return xmlName.LocalName.Value;
            }
        }

        public CodeXmlAtomicValue NodeLocalName
        {
            get
            {
                return xmlName.LocalName;
            }
        }
        public CodeXmlName NodeName
        {
            get
            {
                return xmlName;
            }
        }
        public CodeXmlDocument OwnerDocument
        {
            get
            {
                return ownerDoc;
            }
        }
        public CodeXmlNodeType NodeType
        {
            get
            {
                return nodeType;
            }
        }

        protected void SetNodeType(CodeXmlNodeType nodeType)
        {
            this.nodeType = nodeType;
        }


        internal virtual void ToCodeString(StringBuilder stBuilder)
        {

        }

    }

    public class CodeXmlComment : CodeXmlNode
    {
        StringBuilder content;
        public CodeXmlComment(CodeXmlDocument codeXmlDoc)
            : base(codeXmlDoc)
        {
            SetNodeType(CodeXmlNodeType.XmlCommentNode);
        }
        public StringBuilder Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }

        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeXmlComment; }
        }
        internal override void ToCodeString(StringBuilder stBuilder)
        {
            stBuilder.Append("<!--");
            stBuilder.Append(content.ToString());
            stBuilder.Append("-->");
        }
    }

    public class CodeXmlProcessInstruction : CodeXmlNode
    {
        StringBuilder content;
        public CodeXmlProcessInstruction(CodeXmlDocument codeXmlDoc)
            : base(codeXmlDoc)
        {
            SetNodeType(CodeXmlNodeType.XmlProcessInstructionNode);
        }
        internal override void ToCodeString(StringBuilder stBuilder)
        {
            stBuilder.Append("<?");
            stBuilder.Append(this.Name);
            stBuilder.Append(' ');
            stBuilder.Append(content.ToString());
            stBuilder.Append("?>");
        }
        public StringBuilder Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeXmlProcessInstruction; }
        }

    }



    public class CodeXmlTextNode : CodeXmlNode
    {
        StringBuilder content;
        public CodeXmlTextNode(CodeXmlDocument xmldoc)
            : base(xmldoc)
        {
            SetNodeType(CodeXmlNodeType.XmlTextNode);
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeXmlTextNode; }
        }
        public StringBuilder Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }
        internal override void ToCodeString(StringBuilder stBuilder)
        {
            stBuilder.Append(content.ToString());
        }
    }


    public class CodeXmlDocument : CodeXmlNode
    {
        CodeXmlElement rootElement;
        public CodeXmlDocument()
            : base(null)
        {

            SetNodeType(CodeXmlNodeType.XmlDocument);
        }
        public CodeXmlElement RootElement
        {
            get
            {
                return rootElement;
            }
            set
            {
                rootElement = value;
                if (value != null)
                {
                    AcceptChild(rootElement, CodeObjectRoles.CodeXmlDocument_RootElement);
                }
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeXmlDoc; }
        }
        public CodeXmlElement CreateElement(string elemName)
        {
            return new CodeXmlElement(this, elemName);
        }
        public CodeXmlAttribute CreateAttribute(string attrName)
        {
            return new CodeXmlAttribute(this, attrName);
        }


    }

    public class CodeXmlNodeCollection : List<CodeXmlNode>
    {


    }

    public class CodeXmlElement : CodeXmlNode
    {
        List<CodeXmlNode> xmlChildren;
        List<CodeXmlAttribute> xmlAttrs;

        CodeXmlElement closeTagElement;

        public CodeXmlElement(CodeXmlDocument ownerDoc)
            : base(ownerDoc)
        {

            SetNodeType(CodeXmlNodeType.XmlOpenElementNode);

        }
        public CodeXmlElement(CodeXmlDocument ownerDoc, string elementName)
            : base(ownerDoc, elementName)
        {

            SetNodeType(CodeXmlNodeType.XmlOpenElementNode);

        }
        public CodeXmlAttribute GetAttributeNode(string name)
        {
            if (xmlAttrs != null)
            {
                int j = xmlAttrs.Count;
                for (int i = 0; i < j; ++i)
                {
                    CodeXmlAttribute xmlattr = xmlAttrs[i];
                    if (xmlattr.XmlName.ToString() == name)
                    {

                        return xmlattr;
                    }
                }
            }
            return null;
        }

        public CodeXmlAttribute GetAttributeNodeByName(string prefix, string localName)
        {
            if (xmlAttrs != null)
            {
                int j = xmlAttrs.Count;
                for (int i = 0; i < j; ++i)
                {
                    CodeXmlAttribute xmlattr = xmlAttrs[i];
                    CodeXmlName xmlName = xmlattr.XmlName;
                    if (xmlName.PrefixAsString == prefix && xmlName.LocalNameAsString == localName)
                    {
                        return xmlattr;
                    }

                }
            }
            return null;
        }
        public CodeXmlAttribute GetAttributeNode(int index)
        {
            if (index < xmlAttrs.Count)
            {
                return xmlAttrs[index];
            }
            else
            {
                return null;
            }

        }
        public string GetAttribute(string name)
        {

            if (xmlAttrs != null)
            {
                int j = xmlAttrs.Count;
                for (int i = 0; i < j; ++i)
                {
                    CodeXmlAttribute xmlattr = xmlAttrs[i];
                    if (xmlattr.XmlName.ToString() == name)
                    {

                        return xmlattr.Value;
                    }
                }
            }
            return string.Empty;
        }


        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeXmlElement; }
        }
        public void AddXmlAttribute(CodeXmlAttribute codeXmlAttr)
        {
            if (xmlAttrs == null)
            {
                xmlAttrs = new List<CodeXmlAttribute>();
            }

            SetNodeOrder(codeXmlAttr, xmlAttrs.Count);

            xmlAttrs.Add(codeXmlAttr);

            AcceptChild(codeXmlAttr, CodeObjectRoles.CodeXmlElement_Attribute);
        }
        public void AddXmlElement(CodeXmlElement codeXmlElement)
        {
            if (xmlChildren == null)
            {
                xmlChildren = new List<CodeXmlNode>();
            }
            SetNodeOrder(codeXmlElement, xmlChildren.Count);
            xmlChildren.Add(codeXmlElement);

            AcceptChild(codeXmlElement, CodeObjectRoles.CodeXmlElement_ChildElement);
        }
        public void AddXmlCommentElement(CodeXmlComment codeXmlComment)
        {
            if (xmlChildren == null)
            {
                xmlChildren = new List<CodeXmlNode>();
            }
            SetNodeOrder(codeXmlComment, xmlChildren.Count);
            xmlChildren.Add(codeXmlComment);

            AcceptChild(codeXmlComment, CodeObjectRoles.CodeXmlElement_ChildCommentElement);
        }
        public void AddXmlProcessingInstruction(CodeXmlProcessInstruction codeXmlProcInst)
        {
            if (xmlChildren == null)
            {
                xmlChildren = new List<CodeXmlNode>();
            }

            SetNodeOrder(codeXmlProcInst, xmlChildren.Count);
            xmlChildren.Add(codeXmlProcInst);

            AcceptChild(codeXmlProcInst, CodeObjectRoles.CodeXmlElement_ChildProcInstruction);
        }
        public void AddXmlTextNode(CodeXmlTextNode codeXmlTextNode)
        {
            if (xmlChildren == null)
            {
                xmlChildren = new List<CodeXmlNode>();
            }
            SetNodeOrder(codeXmlTextNode, xmlChildren.Count);
            xmlChildren.Add(codeXmlTextNode);

            AcceptChild(codeXmlTextNode, CodeObjectRoles.CodeXmlElement_ChildTextNode);
        }


        public bool HasChildNodes
        {
            get
            {
                return xmlChildren != null;
            }
        }
        public int ChildrenCount
        {
            get
            {
                if (xmlChildren != null)
                {
                    return xmlChildren.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int AttributeCount
        {
            get
            {
                if (xmlAttrs != null)
                {
                    return xmlAttrs.Count;
                }
                else
                {
                    return 0;
                }
            }

        }
        public bool HasAttributes
        {
            get
            {
                return xmlAttrs != null;
            }
        }
        public CodeXmlNode GetChild(int index)
        {
            return xmlChildren[index];
        }

        public CodeXmlNodeCollection SelectNodes(string nodeName)
        {

            CodeXmlNodeCollection nodeCollection = new CodeXmlNodeCollection();
            SelectNodes(nodeCollection, nodeName);
            return nodeCollection;
        }
        protected void SelectNodes(CodeXmlNodeCollection outputCollection, string nodeName)
        {
            if (xmlChildren != null)
            {
                int j = xmlChildren.Count;
                for (int i = 0; i < j; ++i)
                {
                    CodeXmlNode child = xmlChildren[i];
                    if (child is CodeXmlElement && child.Name.ToString() == nodeName)
                    {
                        outputCollection.Add(child);
                    }
                }
            }
        }

        public IEnumerable<CodeXmlElement> GetXmlChildElementIter()
        {
            if (xmlChildren != null)
            {
                int j = xmlChildren.Count;
                for (int i = 0; i < j; ++i)
                {
                    CodeXmlNode chNode = xmlChildren[i];
                    if (chNode is CodeXmlElement)
                    {
                        yield return (CodeXmlElement)chNode;
                    }
                }
            }
        }
        public IEnumerable<CodeXmlNode> GetXmlChildNodeIter()
        {
            if (xmlChildren != null)
            {
                int j = xmlChildren.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return xmlChildren[i];
                }
            }
        }
        public IEnumerable<CodeXmlAttribute> GetAttributeIter()
        {
            if (xmlAttrs != null)
            {
                int j = xmlAttrs.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return xmlAttrs[i];
                }
            }
        }


        public void MarkAsCloseTagElement()
        {
            SetNodeType(CodeXmlNodeType.XmlCloseElementNode);
        }

        public void MarkAsShortTagElement()
        {
            SetNodeType(CodeXmlNodeType.XmlShortElementNode);
        }

        public CodeXmlElement CloseTagElement
        {
            get
            {
                return closeTagElement;
            }
            set
            {
                closeTagElement = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeXmlElement_CloseTag);
                }
            }
        }



        public void WriteToStingBuilder(StringBuilder stBuilder)
        {
            this.ToCodeString(stBuilder);
        }

        internal override void ToCodeString(StringBuilder stBuilder)
        {
            stBuilder.Append('<');
            if (this.NodeType == CodeXmlNodeType.XmlCloseElementNode)
            {
                stBuilder.Append('/');
                if (XmlName != null)
                {
                    stBuilder.Append(XmlName.ToString());
                }
                stBuilder.Append('>');
                return;
            }
            else
            {
                if (XmlName != null)
                {
                    stBuilder.Append(XmlName.ToString());
                }
            }
            //--------------------------------------------------------

            if (xmlAttrs != null)
            {
                int j = xmlAttrs.Count;
                for (int i = 0; i < j; ++i)
                {
                    stBuilder.Append(' ');
                    xmlAttrs[i].ToCodeString(stBuilder);
                }
            }

            if (this.NodeType == CodeXmlNodeType.XmlShortElementNode)
            {
                stBuilder.Append("/>");
            }
            else
            {
                stBuilder.Append('>');
                if (xmlChildren != null)
                {
                    int j = xmlChildren.Count;
                    for (int i = 0; i < j; ++i)
                    {
                        xmlChildren[i].ToCodeString(stBuilder);
                    }
                }
                if (closeTagElement != null)
                {
                    closeTagElement.ToCodeString(stBuilder);
                }
            }
        }
#if DEBUG
        public override string ToString()
        {
            StringBuilder stBuilder = new StringBuilder();
            ToCodeString(stBuilder);
            return stBuilder.ToString();
        }
#endif


    }

    public class CodeXmlAtomicValue : CodeObject
    {


        string atomicValue;
        public CodeXmlAtomicValue(string xmlAttrValue)
        {
            this.atomicValue = xmlAttrValue;
        }
        public string Value
        {
            get
            {
                return atomicValue;
            }
        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeXmlAtomicValue; }
        }
        public override string ToString()
        {
            return atomicValue;
        }
        public bool Contains(string value)
        {
            return atomicValue.Contains(value);
        }
    }



    public class CodeXmlAttribute : CodeXmlNode
    {
        CodeXmlAtomicValue attributeValue;
        bool isSpecial = false;

        public CodeXmlAttribute(CodeXmlDocument ownerDoc)
            : base(ownerDoc)
        {
            SetNodeType(CodeXmlNodeType.XmlAttributeNode);
        }
        public CodeXmlAttribute(CodeXmlDocument ownerDoc, string attrName)
            : base(ownerDoc, attrName)
        {

            SetNodeType(CodeXmlNodeType.XmlAttributeNode);
        }
        public CodeXmlAttribute NextSibling
        {
            get
            {
                if (ParentCodeObject is CodeXmlElement)
                {
                    CodeXmlElement parentElement = (CodeXmlElement)ParentCodeObject;
                    return parentElement.GetAttributeNode(GetNodeOrder(this) + 1);
                }
                else
                {
                    return null;
                }
            }
        }
        public string Value
        {
            get
            {
                if (attributeValue != null)
                {

                    return attributeValue.Value;
                }

                return null;
            }
            set
            {
                attributeValue = new CodeXmlAtomicValue(value);
            }
        }

        public CodeXmlAtomicValue AttributeValue
        {
            get
            {
                return attributeValue;
            }
            set
            {
                attributeValue = value;
                if (value != null)
                {
                    AcceptChild(value, CodeObjectRoles.CodeXmlAttribute_ValueExpression);
                }
            }

        }
        public override CodeObjectKind Kind
        {
            get { return CodeObjectKind.CodeXmlAttribute; }
        }
        public CodeXmlElement ParentElement
        {
            get
            {
                if (ParentCodeObject is CodeXmlElement)
                {
                    return (CodeXmlElement)ParentCodeObject;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsSpecial
        {
            get
            {
                return isSpecial;
            }
            set
            {
                isSpecial = value;
            }
        }
#if DEBUG
        internal override void ToCodeString(StringBuilder stBuilder)
        {
            if (this.XmlName != null)
            {
                stBuilder.Append(XmlName.ToString());
            }
            if (attributeValue != null)
            {
                stBuilder.Append('=');
                stBuilder.Append("\"" + Value + "\"");
            }
        }
#endif
    }
}
