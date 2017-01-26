//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;
using Parser.CodeDom;
using Parser.ParserKit;
using Parser.ParserKit.LR;

namespace Parser.MyCs
{
    public abstract class CsSubParser2<T, P> : ReflectionSubParserV2<T>
        where T : AstWalker, new()
        where P : ReflectionSubParserV2
    {



        //has built-in CS token info 
        static protected readonly UserTokenDefinition
        _token_literal_integer = mtk("'literal_integer"),
        _token_literal_string = mtk("'literal_string"),
        _token_id = mtk("'identifier"),

        _token_semicolon = mtk(";"),
        _token_double_colon = mtk("::"),

        _token_openAng = mtk("<"),
        _token_openBrc = mtk("{"),
        _token_openBkt = mtk("["),
        _token_openPar = mtk("("),
        _token_closeAng = mtk(">"),
        _token_closeBrc = mtk("}"),
        _token_closeBkt = mtk("]"),
        _token_closePar = mtk(")"),

        _token_gen_oAng = mtk("<t"),
        _token_gen_cAng = mtk(">t"),

        _token_less_orEq = mtk("<="),
        _token_greater_orEq = mtk(">="),
        _token_eq = mtk("=="),
        _token_neq = mtk("!="),

        _token_lambda = mtk("=>"),

        _token_assign = mtk("="),
        _token_add_assign = mtk("+="),
        _token_minus_assign = mtk("-="),
        _token_mul_assign = mtk("*="),
        _token_div_assign = mtk("/="),
        _token_and_assign = mtk("&="),
        _token_or_assign = mtk("|="),
        _token_cap_assign = mtk("^="),
        _token_leftshift_assign = mtk("<<="),
        _token_rightshift_assign = mtk(">>="),


        _token_comma = mtk(","),
        _token_colon = mtk(":"),
        _token_dot = mtk("."),
        _token_and = mtk("&"),
        _token_or = mtk("|"),
        _token_quest = mtk("?"),
        _token_conditional_and = mtk("&&"),
        _token_conditional_or = mtk("||"),


        _token_plus = mtk("+"),
        _token_minus = mtk("-"),
        _token_mul = mtk("*"),
        _token_div = mtk("/"),
        _token_mod = mtk("%"),
        _token_minus_minus = mtk("--"),
        _token_plus_plus = mtk("++"),
        _token_bang = mtk("!"),
        _token_complement = mtk("~"),
        _token_cap = mtk("^"),
        _token_null_coalescing = mtk("??"),
        _token_shift_left = mtk("<<"),
        _token_shift_right = mtk(">>"),


        _token_partial,
        _token_class,
        _token_struct,
        _token_delegate,
        _token_enum,


        _token_new,
        _token_public,
        _token_protected,
        _token_internal,
        _token_private,
        _token_abstract,
        _token_sealed,
        _token_static,
        _token_readonly,
        _token_virtual,
        _token_override,
        _token_extern,

        _token_as,
        _token_is,

        _token_this,
        _token_base,
        _token_if,
        _token_else,
        _token_do,
        _token_while,

        _token_for,
        _token_foreach,
        _token_in,

        _token_switch,
        _token_case,
        _token_default,
        _token_break,
        _token_continue,
        _token_goto,
        _token_void,
        _token_namespace,
        _token_var,
        _token_const,
        _token_return,
        _token_try,
        _token_catch,
        _token_finally,
        _token_throw,
        _token_lock,
        _token_using,
        _token_yield,

        _token_typeof,
        _token_checked,
        _token_unchecked,

        _token_true,
        _token_false,
        _token_ref,
        _token_out,
        _token_params,


        _token_where,
        _token_get, //contextual
        _token_set  //contextual 
       ;

        static CsSubParser2()
        {

            //init all default values
            //1. set at base type
            SetDefaultFieldValues(typeof(CsSubParser2<T, P>));
            //2. and this type (P)
            SetDefaultFieldValues(typeof(P));

        }
        static void SetDefaultFieldValues(Type tt)
        {

            System.Reflection.FieldInfo[] allStaticFields =
                tt.GetFields(System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.DeclaredOnly);
            int j = allStaticFields.Length;
            TokenInfoCollection tkInfoCollection = s_tkInfoCollection;

            for (int i = 0; i < j; ++i)
            {
                System.Reflection.FieldInfo field = allStaticFields[i];
                if (field.FieldType == typeof(UserTokenDefinition))
                {
                    var fieldValue = field.GetValue(null) as UserTokenDefinition;

                    if (fieldValue == null)
                    {
                        //no init value 
                        field.SetValue(null, new UserTokenDefinition(tkInfoCollection.GetTokenInfo(GetPresentationName2(field.Name))));
                    }
                    else
                    {
                        //use presentation string 
                        fieldValue.TkDef = tkInfoCollection.GetTokenInfo(fieldValue.GrammarString);
                    }
                }
                else if (field.FieldType == typeof(UserNTDefinition))
                {
                    //create dummy user nt def
                    field.SetValue(null, new UserNTDefinition(GetPresentationName2(field.Name)));
                }
                else if (field.FieldType == typeof(TokenDefinition))
                {
                    var fieldValue = field.GetValue(null) as TokenDefinition;
                    if (fieldValue == null)
                    {
                        field.SetValue(null, tkInfoCollection.GetTokenInfo(GetPresentationName2(field.Name)));
                    } 
                }

            }
        }
        static string GetPresentationName2(string fieldname)
        {
            //convention here
            if (fieldname.StartsWith("_token_"))
            {
                return fieldname.Substring(7);
            }
            else
            {
                return fieldname;
            }
        }
        protected static bool Begin()
        {
            return true;
        }
        static UserTokenDefinition mtk(string grammarString)
        {
            return new UserTokenDefinition(grammarString);
        }

        public sealed override string GetTokenPresentationName(string fieldname)
        {

            switch (fieldname)
            {

                //case "_token_id":
                //    return "'identifier";
                //case "_token_minus_minus": return "--";
                //case "_token_plus_plus": return "++";
                //case "_token_literal_integer":
                //    return "'literal_integer";
                //case "_token_literal_string":
                //    return "'literal_string";
                //case "_token_dot": return ".";
                //case "_token_conditional_and": return "&&";
                //case "_token_conditional_or": return "||";
                //case "_token_quest": return "?";
                //case "_token_null_coalescing": return "??";
                //case "_token_colon": return ":";
                //case "_token_semicolon": return ";";
                //case "_token_comma": return ",";
                //case "_token_double_colon": return "::";
                //case "_token_less_orEq": return "<=";
                //case "_token_greater_orEq": return ">=";
                //case "_token_eq": return "==";
                //case "_token_neq": return "!=";
                //case "_token_lambda": return "=>";

                //case "_token_add_assign": return "+=";
                //case "_token_minus_assign": return "-=";
                //case "_token_mul_assign": return "*=";
                //case "_token_div_assign": return "/=";
                //case "_token_and_assign": return "&=";
                //case "_token_or_assign": return "|=";
                //case "_token_cap_assign": return "^=";
                //case "_token_leftshift_assign": return "<<=";
                //case "_token_rightshift_assign": return ">>=";


                //case "_token_openPar": return "(";
                //case "_token_closePar": return ")";
                //case "_token_openAng": return "<";
                //case "_token_closeAng": return ">";
                //case "_token_openBrc": return "{";
                //case "_token_closeBrc": return "}";
                //case "_token_openBkt": return "[";
                //case "_token_closeBkt": return "]";
                //case "_token_assign": return "=";
                //case "_token_gen_oAng": return "<t";
                //case "_token_gen_cAng": return ">t";
                //case "_token_shift_left": return "<<";
                //case "_token_shift_right": return ">>";


                //case "_token_and":
                //    return "&";
                //case "_token_or":
                //    return "|";
                //case "_token_this":
                //    return "this";
                //case "_token_gen_oAngle":
                //    return "<t";
                //case "_token_gen_cAngle":
                //    return ">t";
                //case "_token_plus":
                //    return "+";
                //case "_token_minus":
                //    return "-";
                //case "_token_mul":
                //    return "*";
                //case "_token_div":
                //    return "/";
                //case "_token_mod":
                //    return "%";
                //case "_token_bang":
                //    return "!";
                //case "_token_complement":
                //    return "~";
                //case "_token_cap":
                //    return "^";


                default:
                    //not found in switch table 
                    //then use convention 
                    if (fieldname.StartsWith("_token_"))
                    {
                        return fieldname.Substring(7);
                    }
                    else
                    {
                        return fieldname;
                    }
            }
        }
        protected static ParserKit.SubParsers.ListSymbol list_c(UserNTDefinition nt)
        {
            return list(nt, _token_comma);
        }
        protected static ParserKit.SubParsers.ListSymbol list_c(TokenDefinition tk)
        {
            return list(tk, _token_comma);
        }

    }

}