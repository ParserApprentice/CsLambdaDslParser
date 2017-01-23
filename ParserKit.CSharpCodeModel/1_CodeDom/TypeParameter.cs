//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;
namespace Parser.CodeDom
{
    public class TypeParameterConstraints
    {
        public string TypeParameterName;
        List<TypeParameterConstraint> constraits = new List<TypeParameterConstraint>();
        public void AddConstraint(TypeParameterConstraint c)
        {
            constraits.Add(c);
        }
    }

    public enum TypeParameterConstraintKind
    {
        ClassName,
        Class,
        Struct,
        New
    }

    public class TypeParameterConstraint
    {
        public TypeParameterConstraintKind Kind { get; set; }
        public string Name { get; set; }

    }

}