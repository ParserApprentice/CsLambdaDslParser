//MIT, 2015-2017, ParserApprentice
 
using Parser.ParserKit; 
namespace Parser.MyCs
{


    public class TypeParser : CsSubParser<TypeParser.Walker, TypeParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------

        static UserNTDefinition
            //
            //top ***
            _type = _oneof(
            /*1*/
                _(
                    o => _token_id,
                    o => opt(_type_argument_list)),
            /*2*/
                _(o => _array_type)),
            //-------------------------------------------
            //
           _type_argument_list,
            //-------------------------------------------
            //
          _array_type = _(
            o => _non_array_type,
            o => list(_rank_specifier)),
            //----------------------------------------
            //
          _non_array_type = _(o => _type),
            //----------------------------------------
            //
          _rank_specifier = _(
                    o => _token_openBkt,
                    o => opt(list(_token_comma)),
                    o => _token_closeBkt
           ),
            //----------------------------------------
          _end;


        public class Walker : AstWalker
        {
            public virtual void NewRankSpecifier() { }
            public virtual void NewArrayType() { }
        }
    }




    public class ArrayTypeParser : CsSubParser<ArrayTypeParser.Walker, ArrayTypeParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------

        static UserNTDefinition
            //
        _array_type = _(                /**/ r => r.NewArrayType,
                o => _non_array_type,
                o => list(_rank_specifier)
                ),
            //-------------------------------
        _non_array_type,
        _type,
        _rank_specifier,
            //----------------------------------------
         _end;
        public class Walker : AstWalker
        {
            public virtual void NewArrayType() { }
            public virtual void NewRankSpecifier() { }
        }
    }

    public class TypeParameterConstraintsClausesParser :
        CsSubParser<TypeParameterConstraintsClausesParser.Walker, TypeParameterConstraintsClausesParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
            //
           _type_parameter_constraints_clauses = _(
               o => list(_type_parameter_constraints_clause)),
            //----------------------------------------------
            _type_parameter_constraints_clause = _(r => r.NewTypeParameterConstraints,
               o => _token_where,
               o => _type_parameter,
               o => _token_colon,
               o => list_c(_common_constraint)
                ),
            _type_parameter = _(o => _token_id),
            _type_parameter_constraints,
            _common_constraint = _oneof(
                 _(o => _type),
                 _(o => _token_class),
                 _(o => _token_struct),
                 _(o => _token_new, o => _token_openPar, o => _token_closePar)
                 ),
            _type;
        //_primary_constraint,
        //_secondary_constraints,
        //_constructor_constraint,
        //_type; 
        public class Walker : AstWalker
        {
            public virtual void NewTypeParameterConstraints() { }
        }
    }

    public class TypeArgumentListParser : CsSubParser<TypeArgumentListParser.Walker, TypeArgumentListParser>
    {
        static UserNTDefinition
          _type_argument_list = _(
               o => _token_gen_oAng,
               o => list_c(_type),
               o => _token_gen_cAng),
            //----------------------------------------------
          _type;

        public class Walker : AstWalker { }
    }



    public class ExpressionParser : CsSubParser<ExpressionParser.Walker, ExpressionParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        //operator precedence
        //     //low
        //Unknown,
        //Lambda,
        //Assignment,
        //Conditional,
        //NullCoalescing,
        //ConditionOR,
        //ConditionAND,
        //LogicalOR,
        //LogicalXOR,
        //LogicalAND,
        //Equality,
        //RelationalAndTypeTesting,
        //Shift,
        //Additive,
        //Multiplicative,
        //Unary,
        //Primary

        //primary expression
        static UserNTDefinition
            _expression = _oneof(
                 o => _simple_name_expression,
                 o => _literal,
                 o => _parenthesized_expression,
                 o => _invocation_expression,
                 o => _member_access,
                 o => _element_access,
                 o => _this_access,
                 o => _base_access,
                 o => _post_increment_decrement_expression,
                 o => _object_creation_expression,
                 o => _array_creation_expression,
                 o => _anonymous_object_creation_expression,
                 o => _typeof_expression,
                 o => _checked_expression,
                 o => _unchecked_expression,
                 o => _default_value_expression,

                //unary expression-----------------------------------
                o => _cast_expression,
                o => _pre_increment_decrement_expression,
                o => _preop_unary_expression,
            //-----------------------------------
                o => _multiplicative_expression,
                o => _additive_expression,
                o => _shift_expression,
                o => _relational_and_type_testing,
                o => _equality_expression,

                o => _and_expression,
                o => _xor_expression,
                o => _or_expression,
                o => _conditional_and_expression,
                o => _conditional_or_expression,
                o => _null_coalescing_expression,
                o => _conditional_expression,
                o => _assignment,
                o => _lambda_expression
                ),
            //--------------------------------
            _literal = _oneof(
                  _(                                /**/r => r.NewLiteralTrue,
                      o => _token_true),
                  _(                                /**/r => r.NewLiteralFalse,
                      o => _token_false),
                  _(                                /**/r => r.NewLiteralInteger,
                      o => _token_literal_integer),
                  _(                                /**/r => r.NewLiteralString,
                      o => _token_literal_string)
                ),
            //--------------------------------
            _simple_name_expression = _(
                o => _token_id,
                o => opt(_type_argument_list)
                ),
            //--------------------------------
            _additive_expression = _(r => r.NewBinOpExpression_Add_Sub,
                 o => _expression,
                 o => oneof(_token_plus, _token_minus),
                 o => _expression
                ).set_prec((int)prec.Additive),
            //--------------------------------
            _multiplicative_expression = _(
                o => _expression,
                o => oneof(_token_mul, _token_div, _token_mod),
                o => _expression).set_prec((int)prec.Multiplicative),
            //--------------------------------
            _shift_expression = _(
               o => _expression,
               o => oneof(_token_shift_left, _token_shift_right),
               o => _expression).set_prec((int)prec.Shift),
            //--------------------------------
            _and_expression = _(
                 o => _expression,
                 o => _token_and,
                 o => _expression).set_prec((int)prec.LogicalAND),
            //--------------------------------
            _xor_expression = _(
                 o => _expression,
                 o => _token_cap,
                 o => _expression
                ).set_prec((int)prec.LogicalXOR),
            //--------------------------------
            _or_expression = _(
                o => _expression,
                o => _token_or,
                o => _expression
                ).set_prec((int)prec.LogicalOR),
            //--------------------------------
            _conditional_and_expression = _(
                o => _expression,
                o => _token_conditional_and,
                o => _expression).set_prec((int)prec.LogicalAND),
            //--------------------------------
            _conditional_or_expression = _(
                o => _expression,
                o => _token_conditional_or,
                o => _expression).set_prec((int)prec.LogicalOR),
            //--------------------------------
            _null_coalescing_expression = _(
                o => _expression,
                o => _token_null_coalescing,
                o => _expression).set_prec((int)prec.NullCoalescing),
            //--------------------------------
            _conditional_expression = _(
                o => _expression,
                o => _token_quest,
                o => _expression,
                o => _token_colon,
                o => _expression).set_prec((int)prec.Conditional), //terinary expression
            //--------------------------------
            _relational_and_type_testing = _(
                o => _expression,
                o => oneof(_token_openAng, _token_closeAng, _token_closeAng, _token_less_orEq,
                      _token_greater_orEq, _token_as, _token_is),
                o => _expression).set_prec((int)prec.RelationalAndTypeTesting),
            //--------------------------------
            _equality_expression = _(
                 o => _expression,
                 o => oneof(_token_eq, _token_neq),
                 o => _expression).set_prec((int)prec.Equality),
            //--------------------------------
            _assignment = _(
                o => _expression,
            /*assignment op*/
                o => oneof(_token_assign, _token_add_assign, _token_minus_assign,
                _token_mul_assign, _token_div_assign, _token_and_assign, _token_or_assign,
                _token_cap_assign, _token_leftshift_assign, _token_rightshift_assign),

                o => _expression).set_prec((int)prec.Assignment),

            _unary_expression,
            //-------------------------------
            //doc 479
            _type_argument_list = _(
                o => _token_gen_oAng,
                o => list_c(_type),
                o => _token_gen_cAng),
            //-------------------------------
            _parenthesized_expression = _(
                o => _token_openPar,
                o => _expression,
                o => _token_closePar),
            //-------------------------------
            _invocation_expression = _(
                o => _expression,
                o => _token_openPar,
                o => opt(_argument_list),
                o => _token_closePar
             ),
            //-------------------------------
            //doc 480
            _member_access = _(
                 o => _expression,
                 o => _token_dot,
                 o => _token_id,
                 o => opt(_type_argument_list)),
            //-------------------------------
            _element_access = _(
                o => _expression,
                o => _token_openBkt,
                o => _argument_list,
                o => _token_closeBkt),
            //-------------------------------
            _this_access = _(o => _token_this),
            //-------------------------------
            _base_access = _oneof(
                    _(o => _token_base,
                      o => _token_dot,
                      o => _token_id),
                    _(o => _token_base,
                      o => _token_openBkt,
                      o => _argument_list,
                      o => _token_closeBkt)
                ),
            //-------------------------------
            _post_increment_decrement_expression = _oneof(
                _(
                   o => _expression,
                   o => _token_plus_plus),
                _(
                   o => _expression,
                   o => _token_minus_minus)),
            //-------------------------------
            _object_creation_expression = _oneof(
                _(
                   o => _token_new,             /**/
                   o => _type,
                   o => _token_openPar,
                   o => opt(_argument_list),
                   o => _token_closePar,
                   o => opt(_object_or_collection_initializer)),

                _(
                   o => _token_new,
                   o => _type,
                   o => _object_or_collection_initializer)
               ),
            //-------------------------------
            _expression_list = _(o => list_c(_expression)),

            //-------------------------------
            //_anonymous_method_expression,
            _lambda_expression = _(
                 o => _anonymous_function_signature,
                 o => _token_lambda,
                 o => _anonymous_function_body).set_prec((int)prec.Lambda),//external
            //-------------------------------
            _anonymous_function_signature = _oneof(
                o => _explicit_anonymous_function_signature,
                o => _implicit_anonymous_function_signature),
            //-------------------------------
            _explicit_anonymous_function_signature = _(
                o => _token_openPar,
                o => opt(list_c(_explicit_anonymous_function_parameter)),
                o => _token_closePar),
            //-------------------------------
            _explicit_anonymous_function_parameter = _(
                o => opt(oneof(_token_ref, _token_out)),
                o => _type,
                o => _token_id),
            //-------------------------------
            _implicit_anonymous_function_parameter = _(o => _token_id),
            //-------------------------------
            _implicit_anonymous_function_signature = _oneof(
               _(
                 o => _token_openPar,
                 o => opt(list_c(_implicit_anonymous_function_signature)),
                 o => _token_closePar),

               _(o => _implicit_anonymous_function_parameter)
               ),
            //-------------------------------
            _anonymous_function_body = _(o => _expression),
            //-------------------------------
            _array_creation_expression = _oneof(
                _(
                    o => _token_new,
                    o => _type,
                    o => _token_openBkt,
                    o => _expression_list,
                    o => _token_closeBkt,
                    o => opt(_rank_specifiers),
                    o => opt(_array_initializer)),
                _(
                    o => _token_new,
                    o => _type,
                    o => _array_initializer),
                _(
                    o => _token_new,
                    o => _rank_specifier,
                    o => _array_initializer)),
            //-------------------------------
            _rank_specifiers = _(
                o => list(_rank_specifier)),
            //-------------------------------
            _rank_specifier = _(
                o => _token_openBkt,
                o => list(_token_comma),
                o => _token_closeBkt),
            //-------------------------------
            _array_initializer = _oneof(
            /*1*/_(
                   o => _token_openBrc,
                   o => opt(list_c(_variable_initalizer)),
                   o => _token_closeBrc),
            /*2*/_(
                   o => _token_openBrc,
                   o => list_c(_variable_initalizer),
                   o => _token_comma,
                   o => _token_closeBrc)),
            //-------------------------------
            _variable_initalizer = _oneof(
                   o => _expression,
                   o => _array_initializer),
            //-------------------------------
            _anonymous_object_creation_expression = _(
            /**/r => r.NewAnonymousObject,
                   o => _token_new,
                   o => _anonymous_object_initializer),
            //-------------------------------
            _anonymous_object_initializer = _oneof(
                    _(                                /**/ r => r.NewObjectInitializer,
                        o => _token_openBrc,
                        o => opt(_member_declarators),
                        o => _token_closeBrc),

                    _(                               /**/ r => r.NewObjectInitializer,
                        o => _token_openBrc,
                        o => _member_declarators,
                        o => _token_comma,
                        o => _token_closeBrc)
                ),
            //-------------------------------
            _member_declarators = _(                /**/r => r.NewMemberDeclarators,
                    o => list_c(_member_declarator)
                ),
            //-------------------------------
            _member_declarator = _oneof(
                 _(
                    o => _simple_name_expression),

                 _(
                    o => _member_access),

                 _(                             /**/ r => r.NewMemberDeclaratorIdAssing,
                    o => _token_id,
                    o => _token_assign,
                    o => _expression)
                ),
            //-------------------------------
            _argument_list,//other parser 
            _object_or_collection_initializer, //other parser

            //----------------------------------
            _unbound_type_name = _oneof(
               _(o => _token_id,
                  o => opt(_generic_dimension_specifier)),

               _(
                o => _token_id,
                o => _token_double_colon,
                o => _token_id,
                o => opt(_generic_dimension_specifier)
                ),

               _(
                o => _token_id,
                o => _token_dot,
                o => _token_id,
                o => opt(_generic_dimension_specifier)
                )),
            //-------------------------------
            _generic_dimension_specifier = _(
              o => _token_openAng,
              o => opt(list(_token_comma)),
              o => _token_closeAng),

            //---------------------------------
            //doc 482
            _typeof_expression = _oneof(
                  _(
                    o => _token_typeof,
                    o => _token_openPar,
                    o => _type,
                    o => _token_closePar),
                  _(
                    o => _token_typeof,
                    o => _token_openPar,
                    o => _unbound_type_name,
                    o => _token_closePar),
                 _(
                    o => _token_typeof,
                    o => _token_openPar,
                    o => _token_void,
                    o => _token_closePar)
                ),
            //-----------------------------------
            _checked_expression = _(
                o => _token_checked,
                o => _token_openPar,
                o => _expression,
                o => _token_closePar),
            //------------------------------
            _unchecked_expression = _(
                o => _token_unchecked,
                o => _token_openPar,
                o => _expression,
                o => _token_closePar),
            //--------------------------
            _default_value_expression = _(
                o => _token_default,
                o => _token_openPar,
                o => _type,
                o => _token_closePar),
            //------------------------------- 
            _preop_unary_expression = _(
               o => oneof(_token_plus, _token_minus, _token_bang, _token_complement),
               o => _expression)
               .set_prec((int)prec.Unary),

            _pre_increment_decrement_expression = _(
               o => oneof(_token_plus_plus, _token_minus),
               o => _expression).set_prec((int)prec.Unary),
            //-------------------------------             
            _cast_expression = _(
                 o => _token_openPar,
                 o => _type,
                 o => _token_closePar,
                 o => _expression /*unary expression*/
                ).set_prec((int)prec.Unary),
            //-------------------------------
            _type;




        //----------------------------------------------------------
        public class Walker : AstWalker
        {
            public virtual void NewLiteralTrue() { }
            public virtual void NewLiteralFalse() { }
            public virtual void NewLiteralInteger() { }
            public virtual void NewLiteralString() { }
            public virtual void NewBinOpExpression_Add_Sub() { }
            public virtual void BinOp_AddLeft() { }
            public virtual void BinOp_AddRight() { }
            public virtual void NewObjectInitialization() { }
            public virtual void AddObjectMemberInitializer() { }
            public virtual void NewAnonymousObject() { }
            public virtual void NewObjectInitializer() { }
            public virtual void AddMemberDeclarators() { }
            public virtual void NewMemberDeclarators() { }
            public virtual void NewMemberDeclaratorIdAssing() { }
            public virtual void MemberDeclAddName() { }
            public virtual void MemberDeclAddValue() { }
        }

    }



    public class ObjectOrCollectionInitializerParser :
        CsSubParser<ObjectOrCollectionInitializerParser.Walker, ObjectOrCollectionInitializerParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
         _object_or_collection_initializer = _oneof(
               _(  /*1*/                        r => r.NewObjectInitialization,
                   o => _token_openBrc,
                   o => opt(list_c(_member_initializer)),
                   o => _token_closeBrc),
               _(  /*2*/                        r => r.NewObjectInitialization,
                   o => _token_openBrc,
                   o => list_c(_member_initializer),
                   o => _token_comma,
                   o => _token_closeBrc)
               ),
            //---------------------------------------------
         _expression_list = _(o => list_c(_expression)),
         _expression,

         _object_initializer,
            //---------------------------------------------
         _element_initializer = _oneof(
                  _(o => _expression),
                  _(
                    o => _token_openBrc,
                    o => _expression_list,
                    o => _token_closeBrc
                  )
               ),
            //---------------------------------------------
         _member_initializer = _(
                   o => _token_id,
                   o => _token_assign,
                   o => _initialize_value
               ),
            //---------------------------------------------
         _initialize_value = _oneof(
               o => _expression,
               o => _object_or_collection_initializer
           ),
            //---------------------------------------------
         _type;
        //sync_start(_token_openBrc);


        //-----------------------------------------------------------
        public class Walker : AstWalker
        {
            public virtual void NewObjectInitialization() { }
        }
    }


    public class ArgumentListParser : CsSubParser<
        ArgumentListParser.Walker, ArgumentListParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
           _argument_list = _(o => list_c(_argument)),
            //--------------------------------------------
           _argument = _(
                 o => opt(_argument_name),
                 o => _argument_value),
            //--------------------------------------------
           _argument_value = _oneof(
                   _(o => _expression),
                   _(o => _token_ref, o => _expression),
                   _(o => _token_out, o => _expression)),
            //--------------------------------------------
           _argument_name = _(
                o => _token_id,
                o => _token_colon),
            //--------------------------------------------
           _expression;

        public class Walker : AstWalker
        {
        }

    }

    public class StatementParser : CsSubParser<StatementParser.Walker, StatementParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
            //
            _statement = _oneof(
                 o => _expression_statement,
                 o => _empty_statement,
                 o => _block_statement,
                 o => _declaration_statement,

                 o => _do_statement,
                 o => _while_statement,
                 o => _for_statement,

                 o => _if_else,
                 o => _switch_statement,

                 o => _break_statement,
                 o => _continue_statement,
                 o => _goto_statement,

                 o => _return_statement,
                 o => _throw_statement,
                 o => _try_statement
               ),

               //---------------------
            _empty_statement = _(
                 o => _token_semicolon
                ),
            //---------------------
            _expression_statement = _(
                 o => _expression,
                 o => _token_semicolon
                ),
            //---------------------

            _if_else = _oneof(
            /*1*/
                 _(r => r.NewIfElseStatement,
                 o => _token_if,
                 o => _token_openPar,
                 o => _expression,      /**/ r => m.If_TestExpression,
                 o => _token_closePar,
                 o => _statement,       /**/ r => m.If_IfBodyStmt
                 ),
            /*2*/
                 _(r => r.NewIfElseStatement,
                 o => _token_if,
                 o => _token_openPar,
                 o => _expression,      /**/ r => m.If_TestExpression,
                 o => _token_closePar,
                 o => _statement,       /**/ r => m.If_IfBodyStmt,
                 o => _token_else,
                 o => _statement,       /**/ r => m.If_ElseBodyStmt
                 )
                ),
            //---------------------
            //doc 487
            _switch_statement = _(
                o => _token_switch,
                o => _token_openPar,
                o => _expression,
                o => _token_closePar,
                o => _switch_block),
            //---------------------
            _switch_block = _(
               o => _token_openBrc,
               o => opt(list(_switch_section)),
               o => _token_closeBrc),
            //---------------------
            _switch_section = _(
               o => list(_switch_label),
               o => list(_statement)),
            //---------------------
            _switch_label = _oneof(
            /*1*/
                _(o => _token_case,
                    o => _expression,
                    o => _token_colon),
            /*2*/
                _(o => _token_default,
                  o => _token_colon)),
            //------------------------------
            _do_statement = _(
                o => _token_do,
                o => _statement/*embeded statement*/,
                o => _token_while,
                o => _token_openPar,
                o => _expression,
                o => _token_closePar,
                o => _token_semicolon
            ),
            //------------------------------
            _while_statement = _(
               o => _token_while,
               o => _token_openPar,
               o => _expression,
               o => _token_closePar,
               o => _statement
              ),
            //------------------------------
            _for_statement = _(
                o => _token_for,
                o => _token_openPar,
                o => opt(_for_initializer),
                o => _token_semicolon,
                o => opt(_expression),
                o => _token_semicolon,
                o => opt(list_c(_expression)),/*statement_expression_list*/
                o => _token_closePar,
                o => _statement /*embeded statement*/
                ),
            //------------------------------
            _for_initializer,
            //------------------------------
            _foreach_statement = _(
                o => _token_foreach,
                o => _token_openPar/*embeded statement*/,
                o => oneof(_type, _token_var),
                o => _token_id,
                o => _token_in,
                o => _expression,
                o => _token_closePar,
                o => _statement
                ),
            //------------------------------
            _embeded_statement,

            //--------------------
            _block_statement = _(                /**/   r => r.NewBlockStatement,
                o => _token_openBrc,
                o => opt(list(_statement)),      /**/ o => m.BlockStatement_Content,
                o => _token_closeBrc
                ),
            //--------------------
            _break_statement = _(
                o => _token_break,
                o => _token_semicolon
                ),
            //--------------------
            _continue_statement = _(
                    o => _token_continue,
                    o => _token_semicolon
                ),
            //--------------------
            _goto_statement = _oneof(
                  _(
                      o => _token_goto,
                      o => _token_id,
                      o => _token_semicolon),
                  _(
                      o => _token_goto,
                      o => _token_case,
                      o => _expression,
                      o => _token_semicolon),
                  _(
                      o => _token_goto,
                      o => _token_default)
                ),
            //--------------------
            _return_statement = _(        /**/ o => o.NewReturnStatement,
                o => _token_return,
                o => opt(_expression),     /**/ o => m.ReturnExpression,
                o => _token_semicolon
                ),
            //--------------------
            _throw_statement = _(
                o => _token_throw,
                o => opt(_expression),
                o => _token_semicolon
                ),
            //--------------------
            _try_statement = _oneof(
                _(
                    o => _token_try,
                    o => _block_statement,
                    o => _catch_clauses),

                _(
                    o => _token_try,
                    o => _block_statement,
                    o => _finally_clause),

                _(
                    o => _token_try,
                    o => _block_statement,
                    o => _catch_clauses,
                    o => _finally_clause)
                ),
            //--------------------
            _catch_clauses = _oneof(
                _(
                    o => list(_specific_catch_clause),
                    o => opt(_generic_catch_clause)),
                _(
                    o => opt(list(_specific_catch_clause)),
                    o => _generic_catch_clause)
                ),
            //--------------------
            _specific_catch_clause = _(
                o => _token_catch,
                o => _token_openPar,
                o => _type/*_class_type*/,
                o => opt(_token_id),
                o => _token_closePar,
                o => _block_statement
                ),
            //--------------------
            _generic_catch_clause = _(
                 o => _token_catch,
                 o => _block_statement
                ),
            //--------------------
            _finally_clause = _(
                 o => _token_finally,
                 o => _block_statement
                ),
            //--------------------
            _checked_statement = _(
                o => _token_checked,
                o => _block_statement),
            //--------------------
            _unchecked_statement = _(
                o => _token_unchecked,
                o => _block_statement
                ),

            //----------------------------
            _declaration_statement = _oneof(

                    _( /*1*/
                        o => _local_variable_declaration,
                        o => _token_semicolon
                     ),

                    _( /*2*/
                        o => _local_constant_declaration,
                        o => _token_semicolon
                     )
                ),
            //----------------------------
            _local_variable_declaration = _(
                o => oneof(_type, _token_var),
                o => list_c(_local_variable_declarator)
                ),
            //----------------------------
            _local_constant_declaration = _(
                o => _token_const,
                o => _type,
                o => list_c(_constant_declarator)
                ),
            //----------------------------
            //doc 487
            _constant_declarator = _(
                o => _token_id,
                o => _token_assign,
                o => _expression/*const expression*/
                ),
            //----------------------------
            //doc 486
            _local_variable_declarator = _oneof(
                     _(   /*1*/                            /**/r => r.NewLocalVarDecl,
                        o => _token_id,                 /**/o => m.LocalVarName
                      ),

                     _( /*2*/                            /**/r => r.NewLocalVarDecl,
                      o => _token_id,                   /**/o => m.LocalVarName,
                      o => _token_assign,
                      o => _local_variable_initializer, /**/o => m.LocalVarInitValue
                      )
                ),
            //----------------------------
            //doc 487
            _local_variable_initializer = _oneof(
                    _(o => _expression),
                    _(o => _array_initializer)
                ),
            //----------------------------
            _lock_statement = _(
                o => _token_lock,
                o => _token_openPar,
                o => _expression,
                o => _token_closePar,
                o => _statement),
            //----------------------------
            _using_statement = _(
               o => _token_using,
               o => _token_openPar,
               o => oneof(_local_variable_declarator, _expression),
               o => _token_closePar,
               o => _statement/*embeded statement*/),
            //----------------------------
            _yield_statement = _oneof(
                _(o => _token_yield, o => _token_return, o => _expression, o => _token_semicolon),
                _(o => _token_yield, o => _token_break, o => _token_semicolon)
                ),
            //----------------------------
            _label_statement,

            _expression,

            //----------------------------------------
            //489
            _array_initializer = _oneof(
                    _(
                       o => _token_openBrc,
                       o => opt(list_c(_variable_initializer)),
                       o => _token_closeBrc),

                    _(
                       o => _token_openBrc,
                       o => list_c(_variable_initializer),
                       o => _token_comma,
                       o => _token_closeBrc)
                     ),
            //----------------------------------------
            _variable_initializer = _oneof(
                _(o => _expression),
                _(o => _array_initializer)
                ),
            //----------------------------------------
            _type;


        public enum m
        {
            BlockStatement_Content,

            If_TestExpression,
            If_IfBodyStmt,
            If_ElseBodyStmt,

            LocalVarName,
            LocalVarInitValue,

            ReturnExpression,
        }


        public class Walker : AstWalker
        {
            public virtual void NewBlockStatement() { }
            public virtual void AddBlockStatementContent() { }
            public virtual void NewIfElseStatement() { }
            public virtual void AddTestExpression() { }
            public virtual void IfStatementAddTrueStatement() { }
            public virtual void IfStatementAddFalseStatement() { }
            //-------------------------
            public virtual void NewLocalVarDecl() { }
            public virtual void AddLocalVarName() { }
            public virtual void AddLocalVarInitValue() { }
            public virtual void NewReturnStatement() { }
            public virtual void AddReturnExpression() { }
        }

    }


    public class AttributesParser : CsSubParser<AttributesParser.Walker, AttributesParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
            _attributes = _(o => list_c(_attribute_section)),
            _attribute_section = _(
                o => _token_openBkt,
                o => list_c(_attribute),
                o => opt(_token_comma),
                o => _token_closeBkt),

            _attribute = _(o => _attribute_name),
            _attribute_name = _(o => _token_id)
            ;
        //protected override void Define()
        //{
        //    //_attributes += _(o => list_c(_attribute_section));
        //    _attribute_section += _(
        //        o => _token_openBkt,
        //        o => list_c(_attribute),
        //        o => opt(_token_comma),
        //        o => _token_closeBkt);

        //    _attribute += _(o => _attribute_name);
        //    _attribute_name += _(o => _token_id);

        //    //----------------
        //    sync_start(_token_openBkt);
        //    //---------------- 
        //}
        public class Walker : AstWalker
        {
        }
    }





    public class ClassDeclParser : CsSubParser<ClassDeclParser.Walker, ClassDeclParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
            _class_decl = _(                              /**/ r => r.NewClassDeclaration,
              o => opt(_attributes),
              o => opt(list(_class_modifier)),
              o => opt(_token_partial),
              o => _token_class,
              o => _token_id,
              o => opt(_type_parameter_list),
              o => opt(_class_base),
              o => opt(_type_parameter_constraints_clauses),
              o => _class_body,
              o => opt(_token_semicolon)
            ),
            //-----------------------
            _attributes,
            _class_modifier = _oneof(
                o => _token_public,
                o => _token_protected,
                o => _token_internal,
                o => _token_private,
                o => _token_abstract,
                o => _token_sealed,
                o => _token_new,
                o => _token_static
                ),
            //-----------------------
            //doc 491
            _type_parameter_list = _(
                o => _token_openAng,
                o => _type_parameters,
                o => _token_closeAng),
            //-----------------------
            _class_base = _(
                o => _token_colon,
                o => list_c(_type_name)),
            //-----------------------
            _type_parameter_constraints_clauses,
            //-----------------------
            _class_body = _(
               o => _token_openBrc,
               o => opt(list(_class_member_declaration)),
               o => _token_closeBrc),
            //-----------------------
            _type_name = _(o => _namespace_or_typename),
            //-----------------------
            //doc 491
            _type_parameters = _oneof(
                 _(o => opt(_attributes),
                   o => _type_parameter),

                 _(o => _type_parameters,
                   o => _token_comma,
                   o => opt(_attributes),
                   o => _type_parameter)
            ),
            //-----------------------
            _type_parameter = _(o => _token_id),
            //-----------------------
            //doc 477
            _namespace_or_typename = _oneof(
                 _(o => _token_id,
                    o => opt(_type_argument_list)),
                 _(o => _namespace_or_typename,
                     o => _token_dot,
                     o => _token_id,
                     o => opt(_type_argument_list))
                 ),
            //-----------------------
            _type_argument_list,
            //-----------------------
            _class_member_declaration = _oneof(
                o => _method_declaration,
                o => _property_declaration,
                o => _field_declaration
                ),
            //-----------------------
            _method_declaration,
            _property_declaration,
            _field_declaration;//external define  

        bool sync1 = sync(_token_class);

        public class Walker : AstWalker
        {
            public virtual void NewClassDeclaration() { }
        }
    }

    public class PropertyDeclParser : CsSubParser<PropertyDeclParser.Walker, PropertyDeclParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
            //--------------------------------
            _property_declaration = _(
                o => opt(_attributes),
                o => opt(list(_property_modifier)),
                o => _type,
                o => _member_name,
                o => _token_openBrc,
                o => _accessors_declarations,
                o => _token_closeBrc),
            //--------------------------------
            _property_modifier = _oneof(
               o => _token_new,
               o => _token_public,
               o => _token_protected,
               o => _token_internal,
               o => _token_private,
               o => _token_static,
               o => _token_virtual,
               o => _token_abstract,
               o => _token_extern
               ),
            //--------------------------------
            _return_type,
            _attributes,
            _member_name = _(o => _token_id),
            //--------------------------------
            _accessors_declarations = _oneof(
                _(
                  o => _get_accessor_declaration,
                  o => opt(_set_accessor_declaration)),
                _(
                  o => opt(_set_accessor_declaration),
                  o => _get_accessor_declaration)),
            //--------------------------------
            _formal_parameter_list,
            _type,
            _statement,
            //--------------------------------
            _get_accessor_declaration = _(
                o => opt(_attributes),
                o => opt(_accessor_modifier),
                o => _token_get,
                o => _accessor_body),
            //--------------------------------
            _set_accessor_declaration = _(
                o => opt(_attributes),
                o => opt(_accessor_modifier),
                o => _token_set,
                o => _accessor_body),
            //--------------------------------
            _accessor_modifier = _oneof(
                _(o => _token_protected),
                _(o => _token_internal),
                _(o => _token_private),
                _(o => _token_protected, o => _token_internal),
                _(o => _token_internal, o => _token_protected)
               ),
            //--------------------------------
            _accessor_body = _oneof(
                _(
                    o => _token_openBrc,
                    o => opt(list(_statement)),
                    o => _token_closeBrc),

                _(o => _token_semicolon))
            ;
        //TODO: review here
        //what is the 'safe-point' or 'sync-point' if  an error occurs ? 
        //-------------------------------
        //sync(_token_get);
        //sync(_token_set);  
        bool sync1 = sync(_token_get);
        bool sync2 = sync(_token_set);


        public class Walker : AstWalker
        {
        }
    }


    public class FieldDeclParser : CsSubParser<FieldDeclParser.Walker, FieldDeclParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        //492
        static UserNTDefinition
            _field_declaration = _(
               o => opt(_attributes),
               o => opt(_field_modifiers),
               o => _type,
               o => list_c(_variable_declarator), //variable_declarators
               o => _token_semicolon),
            //-----------------------------------

            _attributes,
            //-----------------------------------
            _field_modifiers = _oneof(
                 o => _token_new,
                 o => _token_public,
                 o => _token_protected,
                 o => _token_internal,
                 o => _token_private,
                 o => _token_static,
                 o => _token_readonly
                ),
            //-----------------------------------
            _type,
            //-----------------------------------
            _variable_declarator = _oneof(
                 _(o => _token_id),
                 _(o => _token_id,
                   o => _token_assign,
                   o => _expression)
                ),
            //-----------------------------------
            _expression;
        //TODO: review here
        //sync(_token_semicolon);
        //   sync(_token_assign);
        //-------------------------------------------
        bool sync1 = sync(_token_semicolon);
        bool sync2 = sync(_token_assign);
        //------------------------------------------- 
        public class Walker : AstWalker
        {
        }
    }

    public class FormalParameterListParser : CsSubParser<FormalParameterListParser.Walker, FormalParameterListParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
            _formal_parameter_list = _oneof(
            /*1*/_(
                    o => _fixedParameters),

                /*2*/_(
                    o => _fixedParameters,
                    o => _token_comma,
                    o => _parameter_array),

                /*3*/_(
                    o => _parameter_array)),
            //----------------------------------
            _fixedParameters = _(o => list_c(_fixedParameter)),
            //----------------------------------
            _fixedParameter = _(
                o => opt(_attributes),
                o => opt(_parameter_modifier),
                o => _type,
                o => _token_id,
                o => opt(_default_argument)),
            //----------------------------------
            _default_argument = _(
                o => _token_assign,
                o => _expression),
            //----------------------------------
            _parameter_modifier = _oneof(
                o => _token_ref,
                o => _token_out,
                o => _token_this
                ),
            //----------------------------------
            _parameter_array = _(
                o => opt(_attributes),
                o => _token_params,
                o => _array_type,
                o => _token_id),
            //----------------------------------
            _attributes,
            _type,
            _array_type,
            _expression
            ;

        public class Walker : AstWalker
        {

        }
    }
    public class MethodDeclParser : CsSubParser<MethodDeclParser.Walker, MethodDeclParser>
    { //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static
        UserNTDefinition
            _method_declaration = _(
                o => _method_header,
                o => _method_body),
            //---------------------------------------------------
            _method_header = _(
                o => opt(_attributes),
                o => opt(list(_method_modifier)),
                o => opt(_token_partial),
                o => _return_type,
                o => _member_name,
                o => opt(_type_parameter_list),
                o => _token_openPar,
                o => opt(_formal_parameter_list),
                o => _token_closePar
            //opt<type_parameter_constraints_clauses
                ),
            //---------------------------------------------------

            _method_modifier = _oneof(
                o => _token_new,
                o => _token_public,
                o => _token_protected,
                o => _token_internal,
                o => _token_private,
                o => _token_static,
                o => _token_virtual,
                o => _token_abstract,
                o => _token_extern
                ),
            //---------------------------------------------------

            _method_body = _oneof(
                    _(
                        o => _token_openBrc,
                        o => opt(list(_statement)),
                        o => _token_closeBrc),
                    _(
                        o => _token_semicolon)
                ),
            //---------------------------------------------------
            //TODO: review method return type
            _return_type = _(o => _token_void),
            //---------------------------------------------------
            _attributes,
            //---------------------------------------------------
            _member_name = _(o => _token_id),

            _type_parameter_list,
            _formal_parameter_list,
            _type,
            _statement
            ;
        //sync(_token_openPar);



        //TODO: review here
        bool sync1 = sync(_token_openPar);

        public class Walker : AstWalker
        {

        }
    }

    public class StructDeclParser : CsSubParser<StructDeclParser.Walker, StructDeclParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        //497 
        static UserNTDefinition
            _struct_decl = _(
               o => opt(_attributes),
               o => opt(_struct_modifiers),
               o => opt(_token_partial),
               o => _token_struct,
               o => _token_id,
               o => opt(_type_parameter_list),
               o => opt(_struct_interfaces),
               o => opt(_type_parameter_constraints_clauses),
               o => _struct_body,
               o => opt(_token_semicolon)),
            //----------------------------
           _attributes,
            //----------------------------
           _struct_modifiers = _oneof(
               o => _token_public,
               o => _token_protected,
               o => _token_internal,
               o => _token_private,
               o => _token_abstract,
               o => _token_sealed,
               o => _token_new,
               o => _token_static
               ),
            //----------------------------
            //doc 491
           _type_parameter_list = _(
                 o => _token_openAng,
                 o => _type_parameters,
                 o => _token_closeAng
                ),
            //----------------------------
           _struct_interfaces = _(
                o => _token_colon,
                o => _interface_typelist),
            //----------------------------
           _type_parameter_constraints_clauses,
            //----------------------------
           _struct_body = _(
                o => _token_openBrc,
                o => _token_closeBrc),
            //----------------------------
           _type_name = _(o => _namespace_or_typename),
            //----------------------------
           _type_parameters = _oneof(
               _(
                 o => opt(_attributes),
                 o => _type_parameter),
               _(
                 o => _type_parameters,
                 o => _token_comma,
                 o => _attributes,
                 o => _type_parameter)
                ),
            //----------------------------
           _type_parameter = _(o => _token_id),
            //----------------------------
            //doc 477
           _namespace_or_typename = _oneof(
                 _(
                    o => _token_id,
                    o => opt(_type_argument_list)),
                 _(
                    o => _namespace_or_typename,
                    o => _token_dot,
                    o => _token_id,
                    o => opt(_type_argument_list)
                 )),
            //----------------------------
           _type_argument_list = _(o => list_c(_interface_type)),
            //----------------------------
           _interface_type = _(o => _type_name),
           _interface_typelist;//external define  

        //----------------------------
        bool sync1 = sync(_token_struct);

        public class Walker : AstWalker
        {

        }
    }

    public class NamespaceParser : CsSubParser<NamespaceParser.Walker, NamespaceParser>
    {
        //must call begin first
        //this tricker static base-class call
        //before we go further
        static bool init = Begin();
        //--------------------------
        static UserNTDefinition
            _compilation_unit = _(              /**/ r => r.NewCompilationUnit,
                 o => opt(list(_using_directive)),
                 o => opt(_namespace_member_decls)
                 ),
            //-----------------------------------------
            _namespace_member_decls = _(
                 o => list(_namespace_member_decl)
                ),
            //-----------------------------------------
            _namespace_member_decl = _oneof(
                _o => _namespace_decl,
                _o => _type_decl
                ),
            //-----------------------------------------
            _namespace_decl = _(             /**/ r => r.NewNamespaceDeclaration,
                 o => _token_namespace,
                 o => _token_id,
                 o => _namespace_body,
                 o => opt(_token_semicolon)
                 ),
            //-----------------------------------------

            _namespace_body = _(
                 o => _token_openBrc,
                 o => opt(_namespace_member_decls),
                 o => _token_closeBrc
                 ),
            //-----------------------------------------
            _type_decl = _oneof(
                    _(o => _class_decl),
                    _(o => _struct_decl)
                ),
            //-----------------------------------------
            _class_decl,
            _struct_decl,
            _using_directive = _(               /**/ r => r.NewUsingDirective,
                 o => _token_using,
                 o => _token_id,
                 o => _token_semicolon
             )
            ;
        //state for shift
        public enum m
        {
            _Empty,
            VisitUsingDirectiveName,
            VisitNamespaceName,
            VisitNamespaceBody,
        }

        //action for reduce
        public class Walker : AstWalker
        {
            public virtual void NewNamespaceDeclaration() { }
            public virtual void AddNamespaceToNamespaceMember() { }
            public virtual void AddTypeDeclToNamespaceMember() { }
            public virtual void NewUsingDirective() { }
            public virtual void NewCompilationUnit() { }
        }
    }
}