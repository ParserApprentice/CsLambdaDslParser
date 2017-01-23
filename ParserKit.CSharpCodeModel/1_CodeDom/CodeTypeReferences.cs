//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.CodeDom
{

    public class CodeTypeReference
    {
        CodeTypeReference elementTypeRef = null;
        /// <summary>
        /// 0 = unknown, 1= asm typeinfo,2 =code type decl , 3= code  named item
        /// </summary>
        byte createFrom;

        byte elementTypeRefKind = 0;

        const byte CREATED_FROM_UNKNOWN = 0;
        const byte CREATED_FROM_ASMTYPE_INFO = 1;
        const byte CREATED_FROM_CODENAMED_ITEM = 3;
        const byte CREATED_FROM_CODETYPE_DECL = 2;
        const byte CREATED_FROM_CODETYPE_EXTENED = 4;

        public const byte ELEMENT_TYPE_NORMAL = 0;
        public const byte ELEMENT_TYPE_ARRAY = 1;
        public const byte ELEMENT_TYPE_REF = 2;
        public const byte ELEMENT_TYPE_POINTER = 3;

        CodeTypeReferenceOptions typeRefOption = CodeTypeReferenceOptions.GlobalReference;

        //1. when created from CodeNameItem
        CodeNamedItem codeNameItem;

        //2. when created from CodeTypeDecl
        CodeTypeDeclaration codeTypeDecl;

        CodeArrayRankSpecifier arrayRankSpecifier;

#if DEBUG
        public bool dbugMimic;

        static int dbugTotalId;
        int dbugId;
        void dbugInitId()
        {
            dbugTotalId++;
            this.dbugId = dbugTotalId;
            //if (this.dbugId == 15)
            //{
            //}
        }
#endif
        protected CodeTypeReference()
        {
#if DEBUG
            dbugInitId();
#endif

            this.createFrom = CREATED_FROM_CODETYPE_EXTENED;
        }
        public CodeTypeReference(CodeNamedItem codeNameItem)
        {
#if DEBUG
            dbugInitId();
#endif
            this.codeNameItem = codeNameItem;
            this.createFrom = CREATED_FROM_CODENAMED_ITEM;
        }

        public CodeTypeReference(CodeNamedItem codeNameItem, bool maybeSpecialMap)
            : this(codeNameItem)
        {
            this.MaybeSpecialMap = maybeSpecialMap;
        }


        public CodeTypeReference(CodeTypeDeclaration codeTypeDeclaration)
        {
#if DEBUG
            dbugInitId();
#endif
            this.codeTypeDecl = codeTypeDeclaration;
            this.createFrom = CREATED_FROM_CODETYPE_DECL;
        }


        protected void LateSetCodeTypeDeclation(CodeTypeDeclaration codeTypeDeclaration)
        {
            this.codeTypeDecl = codeTypeDeclaration;

        }

        public string OnlyTypeName
        {
            get
            {
                if (codeTypeDecl != null)
                {
                    return codeTypeDecl.Name;
                }
                else
                {
                    return this.codeNameItem.Last.NormalName;
                }
            }
        }

        public static CodeTypeReference CreateFromAsmTypeName(string asmTypeName)
        {
            return new CodeTypeReference(CodeNamedItem.ParseQualifiedName(asmTypeName));
        }

        public CodeArrayRankSpecifier ArrayRankSpecifier
        {
            get
            {
                return this.arrayRankSpecifier;
            }
        }
        public bool IsInferenceType
        {
            get;
            set;
        }

        public CodeTypeReference MakeArrayTypeReference()
        {

            CodeTypeReference newTypeRef = new CodeTypeReference();
            newTypeRef.elementTypeRefKind = ELEMENT_TYPE_ARRAY;
            newTypeRef.elementTypeRef = this;
            return newTypeRef;
        }
        public CodeTypeReference MakeArrayTypeReference(CodeArrayRankSpecifier arrayRankSpecifier)
        {

            CodeTypeReference newTypeRef = new CodeTypeReference();
            newTypeRef.elementTypeRefKind = ELEMENT_TYPE_ARRAY;
            newTypeRef.arrayRankSpecifier = arrayRankSpecifier;
            newTypeRef.elementTypeRef = this;


            return newTypeRef;
        }
        public CodeTypeReference MakeByRefTypeReference()
        {
            CodeTypeReference newTypeRef = new CodeTypeReference();
            newTypeRef.elementTypeRefKind = ELEMENT_TYPE_REF;
            newTypeRef.elementTypeRef = this;
            return newTypeRef;


        }
        public int ElementTypeReferenceKind
        {
            get
            {
                return elementTypeRefKind;
            }
        }

        public bool MaybeSpecialMap
        {
            get
            {
                return this.typeRefOption == CodeTypeReferenceOptions.SpecialMap;
            }
            set
            {
                if (value)
                {
                    this.typeRefOption = CodeTypeReferenceOptions.SpecialMap;
                }
                else
                {
                    this.typeRefOption = CodeTypeReferenceOptions.GlobalReference;
                }

            }
        }
        public CodeTypeReferenceOptions Options
        {
            get
            {
                return typeRefOption;
            }

        }
        public CodeTypeReference GetElementTypeReference()
        {
            //  array , byref  pointer type
            return elementTypeRef;
        }
        public bool HasElementTypeReference
        {
            get
            {
                return elementTypeRefKind != 0;
            }
        }

        public string FullName
        {

            get
            {
                //*** preserve user code
                switch (elementTypeRefKind)
                {
                    case ELEMENT_TYPE_ARRAY:
                        {
                            return elementTypeRef.FullName + "[]";
                        }
                    case ELEMENT_TYPE_POINTER:
                        {
                            return elementTypeRef.FullName + "*";
                        }
                    case ELEMENT_TYPE_REF:
                        {
                            return elementTypeRef.FullName + "&";
                        }
                    default:
                        {
                            return "";
                            //if (referToTypeInfo == null)
                            //{
                            //    if (this.codeNameItem != null)
                            //    {
                            //        return codeNameItem.FullNormalName;
                            //    }
                            //    else if (codeTypeDecl != null)
                            //    {
                            //        return codeTypeDecl.FullNameAsString;
                            //        //return asmTypeName.FullName;
                            //    }
                            //    else
                            //    {
                            //         
                            //        throw new NotSupportedException();
                            //    }
                            //}
                            //else
                            //{
                            //    return referToTypeInfo.FullName;
                            //}
                        }
                }
            }
        }


        public CodeNamedItem GetOriginalNamedItem()
        {
            return this.codeNameItem;
        }

        public CodeTypeDeclaration GetOriginalCodeTypeDecl()
        {
            return this.codeTypeDecl;
        }


#if DEBUG
        public override string ToString()
        {
            return FullName;
        }
#endif

    }


    public class CodeArrayRankSpecifier
    {
        public CodeArrayRankSpecifier()
        {
        }
        public int DimCount
        {
            get;
            set;
        }
    }
}
