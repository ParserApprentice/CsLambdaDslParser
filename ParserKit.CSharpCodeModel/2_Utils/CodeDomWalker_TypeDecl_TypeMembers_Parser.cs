//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Parser.CodeDom
{

    public partial class CodeDomWalker
    {



        protected virtual void WriteCodeTypeDeclAndMemberTitles(CodeTypeDeclaration typedecl, CodeDomVisitor visitor)
        {


            CodeTypeReferenceCollection baseTypes = typedecl.BaseTypes;
            int j = 0;

            if (baseTypes != null)
            {

                j = baseTypes.Count;
                for (int i = 0; i < j; ++i)
                {
                    OnVisitCodeTypeReference(baseTypes[i]);
                }
            }
            //-----------------------------------
            if (typedecl.HasTypeParameters)
            {
                WriteTypeParameterCollection(typedecl.TypeParameters, visitor);
            }
            //-----------------------------------------
            if (typedecl is CodeTypeDelegate)
            {

                WalkParameterDeclExprCollection(((CodeTypeDelegate)typedecl).Parameters, visitor);

                return;
            }

            TypeMemberCollection mbs = typedecl.Members;
            List<CodeFieldDeclaration> fields = TypeMemberCollection.GetAllFields(mbs);
            if (fields != null)
            {

                j = fields.Count;

                for (int i = 0; i < j; ++i)
                {
                    WalkCodeFieldDeclaration(fields[i], visitor);
                }
            }

            List<CodeMethodDeclaration> methods = TypeMemberCollection.GetAllMethods(mbs);
            if (methods != null)
            {

                j = methods.Count;

                for (int i = 0; i < j; ++i)
                {
                    WalkCodeMethodDeclaration(methods[i], visitor);
                }
            }

            List<CodePropertyDeclaration> properties = TypeMemberCollection.GetAllProperties(mbs);
            if (properties != null)
            {

                j = properties.Count;

                for (int i = 0; i < j; ++i)
                {
                    WalkCodePropertyDeclaration(properties[i], visitor);
                }
            }

            List<CodeEventDeclaration> events = TypeMemberCollection.GetAllEvents(mbs);
            if (events != null)
            {

                j = events.Count;

                for (int i = 0; i < j; ++i)
                {
                    WalkCodeEventDeclaration(events[i], visitor);
                }
            }
            List<CodeTypeDeclaration> nestedTypes = TypeMemberCollection.GetAllNestedTypes(mbs);
            if (nestedTypes == null)
            {

                j = nestedTypes.Count;

                for (int i = 0; i < j; ++i)
                {
                    WriteCodeTypeDeclAndMemberTitles(nestedTypes[i], visitor);
                }
            }
            //----------------------------------------- 
        }
        protected virtual void WriteTypeParameterCollection(CodeTypeParameterCollection typeParams, CodeDomVisitor visitor)
        {
            if (typeParams != null)
            {
                throw new NotSupportedException();

                int j = typeParams.Count;
                for (int i = 0; i < j; ++i)
                {
                    //OnVisitCodeTypeReference(typeParams[i].TypeReference); 
                }
            }
        }

        protected virtual void WalkCodeMethodDeclaration(CodeMethodDeclaration methodDecl, CodeDomVisitor visitor)
        {


            OnVisitCodeTypeReference(methodDecl.ReturnType);

            WalkParameterDeclExprCollection(methodDecl.ParameterList, visitor);

            if (methodDecl.HasTypeParameters)
            {
                WriteTypeParameterCollection(methodDecl.TypeParameters, visitor);
            }

            if (methodDecl.HasCustomAttributes)
            {
                WalkCustomAttributeCollection(methodDecl.CustomAttributes, visitor);
            }
        }
        protected virtual void WalkParameterDeclExprCollection(CodeParameterExpressionCollection parDeclExprCollection, CodeDomVisitor visitor)
        {
            if (parDeclExprCollection != null)
            {
                int j = parDeclExprCollection.Count;
                for (int i = 0; i < j; ++i)
                {
                    OnVisitCodeTypeReference(parDeclExprCollection[i].ParameterType);
                }
            }
        }
        protected virtual void WalkCodePropertyDeclaration(CodePropertyDeclaration propertyDecl, CodeDomVisitor visitor)
        {


            OnVisitCodeTypeReference(propertyDecl.ReturnType);
            //-----------------------------------------
            //6. custome attr
            if (propertyDecl.HasCustomAttributes)
            {
                WalkCustomAttributeCollection(propertyDecl.CustomAttributes, visitor);
            }


            if (propertyDecl.GetDeclaration != null)
            {
                WalkCodeMethodDeclaration(propertyDecl.GetDeclaration, visitor);
            }
            if (propertyDecl.SetDeclaration != null)
            {
                WalkCodeMethodDeclaration(propertyDecl.SetDeclaration, visitor);
            }

        }

        protected virtual void WalkCodeNamespace(CodeNamespace codeNamespace, CodeDomVisitor visitor)
        {
            int count = codeNamespace.Count;
            for (int i = 0; i < count; ++i)
            {
                var nsMb = codeNamespace.GetMember(i);
                if (nsMb is CodeTypeDeclaration)
                {
                    WriteCodeTypeDeclAndMemberTitles((CodeTypeDeclaration)nsMb, visitor);
                }
                else if (nsMb is CodeNamespace)
                {
                    WalkCodeNamespace((CodeNamespace)nsMb, visitor);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        protected virtual void WalkCodeEventDeclaration(CodeEventDeclaration eventDecl, CodeDomVisitor visitor)
        {
            OnVisitCodeTypeReference(eventDecl.ReturnType);

            if (eventDecl.HasCustomAttributes)
            {
                WalkCustomAttributeCollection(eventDecl.CustomAttributes, visitor);
            }

            if (eventDecl.AddDeclaration != null)
            {
                WalkCodeMethodDeclaration(eventDecl.AddDeclaration, visitor);
            }
            if (eventDecl.RemoveDeclaration != null)
            {
                WalkCodeMethodDeclaration(eventDecl.RemoveDeclaration, visitor);
            }

        }

        protected virtual void WalkCodeFieldDeclaration(CodeFieldDeclaration fieldDecl, CodeDomVisitor visitor)
        {

            OnVisitCodeTypeReference(fieldDecl.ReturnType);

            if (fieldDecl.HasCustomAttributes)
            {
                WalkCustomAttributeCollection(fieldDecl.CustomAttributes, visitor);
            }
            if (fieldDecl.InitExpression != null)
            {
                WalkCodeExpression(fieldDecl.InitExpression, visitor);
            }

        }


    }
}