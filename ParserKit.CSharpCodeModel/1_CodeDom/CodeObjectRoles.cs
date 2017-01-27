//MIT, 2015-2017, ParserApprentice
 

namespace Parser.CodeDom
{

    public enum CodeExpressionKind
    {
        UnknownExpression,
        ProxyExpresion,
        BinaryOperatorExpression,
        UnaryOperatorExpression,

        NullExpression,
        ArrayCreateExpression,
        IndexerAccessExpression,
        BaseRefererneceExpression,

        ParenthizedExpression,


        IncDecOperatorExpression,
        MemberAccessExpression,
        MethodInvokeExpression,
        ObjectCreateExpression,
        DelegateCreateExpression,


        PrimitiveExpression,
        ThisReferenceExpression,
        IdExpression,

        ParameterDeclarationExpression,
        DefineFieldDeclarationExpression,

        ExplicitConversionExpression,
        ImplicitConversionExpression,
        AsConversionExpression,


        MethodReferenceExpression,
        BaseCtorInvoke,
        ThisCtorInvoke,

        LambdaExpression,

        LinqQueryExpression,  

        JsonObjectExpression,
        NamedExpression,

        DecorationExpression,
        MeReferenceExpression,
        DynamicListExpression,

        //DynamicAssignment, 
        DirectionExpression,
        //-------------------
        CheckedExpression,
        UnCheckedExpression,
        TypeOfExprssion,
        ConditionalExpression,
        DefaultExpression,
        AnonymousMethodExpression,
        ArrayInitialization,
        VariableDeclarator,
        AnonymousObjectCreateExpression,
     
        /// <summary>
        /// experiment
        /// </summary>
        CustomExpression

    }
    //----------------------------------------------------------------------------

    public enum CodeObjectRoles
    {
        CodeObject_UnAssigned,

        CodeMethodReferenceExpression_Target,
        CodeMethodReferenceExpression_MethodName,
        CodeObjectCreateExpression_Parameters,

        CodeForEachStatement_ForType,
        CodeForEachStatement_InWhat,
        CodeForEachStatement_Body,

        CodeForLoopStatement_InitExpr,
        CodeForLoopStatement_Condition,         
        CodeForLoopStatement_IncStm,
        CodeForLoopStatement_Body,

        CodeConditionStatement_Condition,
        CodeConditionStatement_ConditionBlock,
        CodeConditionStatement_ElseBlock,

        CodeDelegateCreate_Target,
        CodeDelegateCreate_Method,

        CodeDelegateInvokeExpression_Target,
        CodeDelegateInvokeExpression_Arguments,

        CodeMethodInvokeExpression_Method,
        CodeMethodInvokeExpression_Arguments,
        CodeMethodInvokeExpression_Extension,

        CodeBinaryOperatorExpression_Left,
        CodeBinaryOperatorExpression_Right,
        CodeParenthizedExpression_Content,
        CodeArrayIndexerExpression_Target,
        CodeArrayIndexerExpression_Indexer,
        CodeExpressionStatement_Expr,

        CodeStatementCollection,

        CodeReturnStatement_ReturnExpr,
        CodeYieldReturnStatement_ReturnExpr,
        CodeSwitchStatement_SwitchExpr,
        CodeSwitchStatement_LabelBlock,

        CodeDoWhileStatement_Condition,
        CodeDoWhileStatement_Body,
        CodeWhileStatement_Condition,
        CodeWhileStatement_Body,

        CodeVariableDeclStatement_InitExpression,
        CodeVariableDeclStatement_VarName,
        CodeVariableDeclStatement_VarType,

        CodeArrayCreateExpression_InitSizeExpr,
        CodeArrayCreateExpression_Initializer,
        CodeArrayCreateExpression_ArrayType,
        CodeMemberAccessExpression_Target,
        CodeMemberAccessExpression_Member,
        CodeIncrementDecrementOperatorExpression_TargetExpr,
        CodeAttributeArgument_Value,
        //-----------------

        CodeExpressionSet_Content,
        CodeTempImmediateFieldDeclarationExpression_Constraint,


        CodeDyamicExpressionSet_Content,
        CodeDyamicExpressionSet_Member,

        CodeQueryWhereExpression_Predicate,
        CodeQueryLetExpression_LetExpr,
        CodeQueryFromClause_NameExpr,
        CodeQueryFromClause_SrcExpr,
        CodeQueryIntoClause_NameExpr,
        CodeQuerySelectManyExpression_Selector,
        CodeQueryGroupByExpression_Source,
        CodeQueryGroupByExpression_Key,
        CodeQueryGroupByExpression_ResultSelector,

        CodeQueryJoinExpression_innerSouceVar,
        CodeQueryJoinExpression_innerSource,
        CodeQueryJoinExpression_outerSource,
        CodeQueryJoinExpression_innerSourceKeySelector,
        CodeQueryJoinExpression_outerSourceKeySelector,
        CodeQueryJoinExpression_resultSelector,

        CodeCastExpression_Expression,

        //---------
        CodeField_InitExpression,
        CodeAttributeDeclaration_Attributes,
        CodeAttributeDeclaration_AttributeTypeRef,

        CodeExpressionCollection_Member,
        CodeStatementCollection_Member,
        CodeTypeMemberCollection_Member,
        CodeNamespace_Member,
        CodeAttributeArgumentCollection_Member,
        CodeAttributeDeclarationCollection_Member,
        CodeAttributeDeclaration,
        CodeAttributeDeclarationCollection,
        CodeTypeCollection_Member,
        CodeParameterExpressionCollection_Member,
        CodeDefineFieldExpressionCollection_Member,
        CodeTypeReferenceCollection_Member,


        CodeTypeDeclaration_GenericTypeParameter,
        CodeTypeDeclaration_GenericTypeParameterCollection,
        CodeMethodDeclaration_GenericTypeParameter,

        
        CodeBaseTypeReferences,

        CodeTypeReference_ArrayOfType,
        CodeTypeReference_TypeName,

        CodeDelegateCreateExpression_DelegateTypeReference,
        CodeObjectCreateExpression_CreateObjectTypeReference,
        CodeTypeReference_TargetCastType,

        CodeParameterDeclExpression_ParameterTypeReference,
        CodeParameterDeclExpression_ParameterName,


        CodeNamespaceCollection_Member,
        CodeNamespaceImportCollection_Member,
        CodeNamespaceImport_Name,
        CodeNamespace_Name,
        //---------

        CodeCommentStatement,
        CodeCommentStatement_CommentObject,
        CodeTypeMember_CommentStatementCollection,
        CodeCommentCollection_CommentObject,
        //---------
        CodeTypeMember_ReturnTypeReference,
        CodeTypeMember_Name,
        CodeDecoreExpr_TargetExpression,
        CodeDecoreExpr_DecorStatements,

        //---------
        CodeIfConditionBlock,
        CodeElseConditionBlock,

        CodeSwitchLabelBlock,
        CodeSwitchDefaultBlock,
        //---------
        CodeParameterDecl_DefaultValueExpression,

        
        CodeParameterDeclCollection,
        //---------
        CodeMethodDeclaration_TypeParameterCollection,
        CodeGenericTypeParameterCollection,
        //---------
        CodeCtorBaseCallExpressionCollection,

        CodeLambdaMethodDeclaration_LambdaExpression,
        CodeLambdaExpression_ParameterCollection,
        CodeLambdaExpression_ReturnType,
        CodeLambdaExpression_SingleLambdaExpression,
        //---------
        CodeProperty_GetMethodDecl,
        CodeProperty_SetMethodDecl,
        //-----------

        CodeIndexer_GetMethodDecl,
        CodeIndexer_SetMethodDecl,
        //-----------
        CodeEvent_ParameterList,
        CodeEvent_AddMethodDecl,
        CodeEvent_RemoveMethodDecl,
        CodeDynamicListExpression_MemberExprCollection,
        //-----------
        CodeIfBlock_ConditionExpression,
        CodeIfBlock_Body,
        CodeElseBlock_Body,
        //----------
        CodeSwitchLabelBlock_LabelExpression,
        CodeSwitchLabelBlock_Body,
        //--------
        CodeNamedItem_SimpleName,

        CodeNamedItemCollection,
        CodeNamedItemCollection_Member,

        CodeSimpleName_TypeArgCollection,

        CodeIdExpression_IdName,
        //--------
        CodeTypeParameter_Name,
        //--------
        CodeNamedExpression_Expression,
        CodeNamedExpression_Name,
        //--------
        CodeDirectionExpression_Expression,
        //--------
        CodeNamedArgumentExpression_Expression,
        CodeNamedArgumentExpression_Name,
        //-----------------------------------------
        CodeDefineStatement_FieldCollection,
        //-----------------------------------------
        //SQL code 
        SqlInsertWithExpression_TargetTable,
        SqlInsertWithExpression_InsertItemList,
        SqlInsertWithExpression_SqlSelectExpression,
        //-----------------------------------------
        SqlInsertExpression_TargetColumnList,
        SqlInsertExpression_ValueListExpression,
        SqlInsertExpression_SelectFromExpression,
        SqlInsertExpression_TargetTableExpression,
        //-----------------------------------------
        SqlAliasExpression_OriginalSqlExpression,
        SqlAliasExpression_AliasName,
        //---------------------------------
        SqlUpdateExpression_LimitNumber,
        SqlUpdateExpression_TargetColumnList,
        SqlUpdateExpression_OrderByList,
        SqlUpdateExpression_TargetTableExpression,
        SqlUpdateExpression_UpdateCondition,
        //---------------------------------
        SqlJoinExpression_TargetTableExpression,
        SqlJoinExpression_JoinCondition,
        //---------------------------------
        SqlSelectExpression_FromExpression,
        SqlSelectExpression_WhereExpression,
        SqlSelectExpression_HavingExpression,
        SqlSelectExpression_OrderByList,
        SqlSelectExpression_GroupByList,
        SqlSelectExpression_SelectionList,
        SqlSelectExpression_JoinExpressions,
        SqlSelectExpression_LimitNumber,
        //-----------------------------------
        SqlLogicalOperatorExpression_LeftExpression,
        SqlLogicalOperatorExpression_RightExpression,
        //-----------------------------------
        SqlCodeExpressionStatement_SqlExpression,
        //-----------------------------------
        CodeThrowExceptionStatement_Expression,
        //----------------------------
        CodeTryCatchFinallyStatement_TryClause,
        CodeTryCatchFinallyStatement_FinallyClause,
        CodeTryCatchFinallyStatement_CatchClause,
        //------------------------------
        CodeCatchClause_Block,
        CodeCatchClause_CatchTypeReference,
        CodeCatchClause_LocalName,
        //------------------------------
        CodeXmlNode_Name,
        CodeXmlElement_CloseTag,
        CodeXmlElement_Attribute,
        CodeXmlElement_ChildElement,
        CodeXmlElement_ChildCommentElement,
        CodeXmlElement_ChildProcInstruction,
        CodeXmlElement_ChildTextNode,
        CodeXmlAttribute_ValueExpression,
        CodeXmlDocument_RootElement,
        CodeXmlName_Prefix,
        CodeXmlName_LocalName,
        //------------------------------
        CodeDynamicAssignment_LeftExpr,
        CodeDynamicAssignment_RightExpr,

        CodeDefineFieldDeclExpression_DefaultValue,

        CodeArgumentCollection
    }
}
