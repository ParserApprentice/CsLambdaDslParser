//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;
using Parser.AsmInfrastructures;


namespace Parser.CodeDom
{

    public enum CodeQueryOperation
    {
        From,
        SelectMany,
        Where,
        Groupby,
        Join,
        Let,
        Into
    }
    public abstract class CodeQueryExpression : CodeExpression
    {


        CodeQueryExpression nextExpr;
        CodeQueryExpression prevExpr;
        public CodeQueryExpression()
        {

        }
        public CodeQueryExpression NextExpression
        {
            get
            {
                return nextExpr;
            }
            set
            {
                nextExpr = value;
            }
        }
        public CodeQueryExpression PrevExpression
        {
            get
            {
                return prevExpr;
            }
            set
            {
                prevExpr = value;
            }
        }
        public sealed override CodeExpressionKind ExpressionKind
        {
            get { return CodeExpressionKind.LinqQueryExpression; }
        }
        public abstract CodeQueryOperation QueryOperationType { get; }
        public override int OperatorPrecedence
        {
            get { return 0; }
        }
    }
    public class CodeQueryWhereExpression : CodeQueryExpression
    {
        CodeExpression predicate;
        public CodeQueryWhereExpression()
        {
        }
        public CodeExpression Predicate
        {
            get
            {
                return predicate;
            }
            set
            {
                predicate = value;
                AcceptChild(predicate, CodeObjectRoles.CodeQueryWhereExpression_Predicate);
            }
        }
        public override CodeQueryOperation QueryOperationType
        {
            get { return CodeQueryOperation.Where; }
        }
    }
    public class CodeQueryLetExpression : CodeQueryExpression
    {
        CodeExpression letExpression;
        public CodeQueryLetExpression()
        {

        }
        public CodeExpression LetExpression
        {
            get
            {
                return letExpression;
            }
            set
            {
                letExpression = value;
                AcceptChild(letExpression, CodeObjectRoles.CodeQueryLetExpression_LetExpr);
            }
        }
        public override CodeQueryOperation QueryOperationType
        {
            get { return CodeQueryOperation.Let; }
        }
    }

    public class CodeQueryFromClause : CodeQueryExpression
    {

        CodeIdExpression namedItemExpression;
        CodeExpression sourceExpression;
        public CodeQueryFromClause()
        {

        }
        public CodeIdExpression NamedItemExpression
        {
            get
            {
                return namedItemExpression;
            }
            set
            {
                namedItemExpression = value;
                AcceptChild(namedItemExpression, CodeObjectRoles.CodeQueryFromClause_NameExpr);
            }
        }
        public CodeExpression SourceExpression
        {
            get
            {
                return sourceExpression;
            }
            set
            {
                sourceExpression = value;
                AcceptChild(sourceExpression, CodeObjectRoles.CodeQueryFromClause_SrcExpr);
            }

        }
        public override CodeQueryOperation QueryOperationType
        {
            get { return CodeQueryOperation.From; }
        }

#if DEBUG
        public override string ToString()
        {
            return "from " + namedItemExpression.Name + " in " + sourceExpression.ToString();
        }
#endif



    }
    public class CodeQueryIntoClause : CodeQueryExpression
    {
        CodeIdExpression namedItemExpression;
        public CodeQueryIntoClause()
        {

        }
        public CodeIdExpression NamedItemExpression
        {
            get
            {
                return namedItemExpression;
            }
            set
            {
                namedItemExpression = value;
                AcceptChild(namedItemExpression, CodeObjectRoles.CodeQueryIntoClause_NameExpr);
            }
        }
        public override CodeQueryOperation QueryOperationType
        {
            get { return CodeQueryOperation.Into; }
        }
    }
    public class CodeQuerySelectManyExpression : CodeQueryExpression
    {

        CodeExpression selector;
        public CodeQuerySelectManyExpression()
        {

        }
        public CodeExpression Selector
        {
            get
            {
                return selector;
            }
            set
            {
                selector = value;
                AcceptChild(selector, CodeObjectRoles.CodeQuerySelectManyExpression_Selector);

            }

        }
        public override CodeQueryOperation QueryOperationType
        {
            get { return CodeQueryOperation.SelectMany; }
        }

    }
    public class CodeQueryGroupByExpression : CodeQueryExpression
    {
        CodeIdExpression source;
        CodeExpression keySelector;
        CodeExpression resultSelector;


        public CodeQueryGroupByExpression()
        {

        }

        public CodeIdExpression Source
        {
            get { return source; }
            set
            {
                source = value;
                AcceptChild(source, CodeObjectRoles.CodeQueryGroupByExpression_Source);
            }
        }
        public CodeExpression KeySelector 
        {
            get
            {
                return keySelector;
            }
            set
            {
                keySelector = value;
                AcceptChild(keySelector, CodeObjectRoles.CodeQueryGroupByExpression_Key);
            }
        }
        public CodeExpression ResultSelector 
        {
            get
            {
                return resultSelector;
            }
            set
            {
                resultSelector = value;
                AcceptChild(resultSelector, CodeObjectRoles.CodeQueryGroupByExpression_ResultSelector);
            }
        }

        public override CodeQueryOperation QueryOperationType
        {
            get { return CodeQueryOperation.Groupby; }
        }


    }
    public class CodeQueryJoinExpression : CodeQueryExpression
    {
        CodeIdExpression innerSouceVar;
        CodeIdExpression innerSource;
        CodeIdExpression outerSource;
        CodeExpression innerSourceKeySelector;
        CodeExpression outerSourceKeySelector;
        CodeExpression resultSelector;

        public CodeQueryJoinExpression()
        {
           
        }
        public CodeIdExpression InnerSouceVar
        {
            get
            {
                return innerSouceVar;
            }
            set
            {
                innerSouceVar = value;
                AcceptChild(innerSouceVar, CodeObjectRoles.CodeQueryJoinExpression_innerSouceVar);
            }
        }
        public CodeIdExpression InnerSource
        {
            get
            {
                return innerSource;
            }
            set
            {
                innerSource = value;
                AcceptChild(innerSource, CodeObjectRoles.CodeQueryJoinExpression_innerSource);
            }

        }
        public CodeIdExpression OuterSource
        {
            get
            {
                return outerSource;
            }
            set
            {
                outerSource = value;
                AcceptChild(outerSource, CodeObjectRoles.CodeQueryJoinExpression_outerSource);
            }
        }
        public CodeExpression InnerSourceKeySelector
        {
            get
            {
                return innerSourceKeySelector;
            }
            set
            {
                innerSourceKeySelector = value;
                AcceptChild(innerSourceKeySelector, CodeObjectRoles.CodeQueryJoinExpression_innerSourceKeySelector);
            }

        }
        public CodeExpression OuterSourceKeySelector
        {
            get
            {
                return outerSourceKeySelector;
            }
            set
            {
                outerSourceKeySelector = value;
                AcceptChild(outerSourceKeySelector, CodeObjectRoles.CodeQueryJoinExpression_outerSourceKeySelector);
            }
        }
        public CodeExpression ResultSelector
        {
            get
            {
                return resultSelector;
            }
            set
            {
                AcceptChild(resultSelector, CodeObjectRoles.CodeQueryJoinExpression_resultSelector);
            }
        }
        public override CodeQueryOperation QueryOperationType
        {
            get { return CodeQueryOperation.Join; }
        }

    }
     
}
