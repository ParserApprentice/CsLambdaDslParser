//MIT, 2015-2017, ParserApprentice
 
using Parser.ParserKit;

namespace Parser.MyCs
{

    public class CsTokenInfoCollection
    {
        TokenInfoCollection tokenInfoCollection = new TokenInfoCollection();
        public CsTokenInfoCollection()
        {
            //define_term("id"); 
            //single chars operator
            define_single_char_token(" +-*/%^()[]{}<>;:,.!=?&|~");
            define_multi_chars_token("++ -- >> << && || ::");
            define_multi_chars_token("<= >=");
            define_multi_chars_token("+= -= *= /= %= ^= &= |= <<= >>= -> ??");
            define_multi_chars_token("== !=");
            define_multi_chars_token("=>");

            define_multi_chars_token("// /*"); //comment token


            define_keywords("is as");
            define_keywords("true false sizeof new typeof checked unchecked");
            define_keywords("default");
            //statement
            define_keywords("if else while do for foreach in try catch throw finally return yield lock");
            define_keywords("ref out");
            define_keywords("namespace using");
            define_keywords("class struct delegate interface void null int uint short ushort byte sbyte long ulong double float decimal");
            define_keywords("bool enum char");
            define_keywords("object dynamic string");
            define_keywords("this base override virtual sealed abstract static extern");
            define_keywords("operator");
            define_keywords("private public internal protected partial");
            define_keywords("readonly");
            define_keywords("params");
            define_keywords("var const");
            define_keywords("switch case break goto");
            define_keywords("continue");
            define_keywords("get set add remove"); //contextual keyword
            define_keywords("where"); //contextual keyword
            //--------------------------------------
            //*** define ambiguos terminals
            //for spacial treatment-> ambiguis grammar handler in C# lang

            define_special_terminal("<t", "<"); //open generic 
            define_special_terminal(">t", ">"); //close generic   
            //--------------------------------------
            set_precedence("if", 2);
            set_precedence("else", 1);
            //-------------------------------------- 
            set_precedence(".", prec.Primary); //multiplicative
            set_precedence("* / %", prec.Multiplicative); //multiplicative
            set_precedence("+ -", prec.Additive);//additive    
            set_precedence("<< >>", prec.Shift);
            set_precedence("< > <= >= is as", prec.RelationalAndTypeTesting); //relational and type testing
            set_precedence("== !=", prec.Equality); //equality
            set_precedence("&", prec.LogicalAND); //logical and
            set_precedence("^", prec.LogicalXOR);//logical xor
            set_precedence("|", prec.LogicalOR);//logical or 
            set_precedence("&&", prec.ConditionAND);//conditional and
            set_precedence("||", prec.LogicalOR);//conditional or 
            set_precedence("= += -= *= /= &= |= ^= <<= >>= ??", prec.Assignment, true);
            //-------------------------------------- 

        }

        public TokenInfoCollection GetSnapedTokenInfoCollection()
        {
            tokenInfoCollection.SnapAllTokenInfo();
            return tokenInfoCollection;
        }



        //-----------------------------------------------------------------------
        void define_term(params string[] terms)
        {

            //collect terminal first
            foreach (string term in terms)
            {

                //create token info from the terminal
                char firstChar = term[0];
                if (char.IsLetter(firstChar) || firstChar == '_')
                {
                    //this is keyword
                    this.tokenInfoCollection.AddTokenKeyword(term, term);
                }
                else
                {
                    if (firstChar != ' ')
                    {
                        // non keyword ***

                        this.tokenInfoCollection.AddNonkeywordToken(term);
                    }
                }
            }
        }
        void define_special_token(string specialTkName, string presentationString)
        {
            this.tokenInfoCollection.AddNonkeywordToken(specialTkName, presentationString);
        }
        void define_term2(char sep, string termlist)
        {
            this.define_term(termlist.Split(sep));
        }
        void define_keywords(string termlist)
        {
            this.define_term2(' ', termlist);
        }
        /// <summary>
        /// special terminal has 'grammar form' and 'source code' form 
        /// </summary>
        /// <param name="specialToken"></param>
        /// <param name="basicToken"></param>
        void define_special_terminal(string specialTkName, string presentationString)
        {
            this.define_special_token(specialTkName, presentationString);

        }
        void define_single_char_token(string singleCharList)
        {
            char[] char_list = singleCharList.ToCharArray();
            int j = char_list.Length;
            string[] terms = new string[j];
            for (int i = 0; i < j; ++i)
            {
                terms[i] = char_list[i].ToString();
            }
            this.define_term(terms);
        }

        /// <summary>
        /// define multiple character operators, separate with single white space
        /// </summary>
        /// <param name="multipleCharsOperatorlist"></param>
        void define_multi_chars_token(string multipleCharsOperatorlist)
        {

            this.define_term2(' ', multipleCharsOperatorlist);
        }
        void set_precedence(string s, prec prec)
        {
            set_precedence(s, (int)prec);
        }
        public void set_precedence(string whitespace_sep_symbol_list, int precendence)
        {
            //isRightAssoc=false
            set_precedence(whitespace_sep_symbol_list, precendence, false);
        }
        void set_precedence(string s, prec prec, bool isRightAssoc)
        {
            set_precedence(s, (int)prec, isRightAssoc);
        }
        void set_precedence(string whitespace_sep_symbol_list, int precendence, bool isRightAssoc)
        {
            string[] symbols = whitespace_sep_symbol_list.Split(' ');
            foreach (string symbol in symbols)
            {
                TokenDefinition tkdef = GetTokenInfo(symbol);
                tkdef.TokenPrecedence = precendence;
                tkdef.IsRightAssoc = isRightAssoc; 
            }
        }
        TokenDefinition GetTokenInfo(string grammarSymbolString)
        {
            return this.tokenInfoCollection.GetTokenInfo(grammarSymbolString);
        }
        //-----------------------------------------------------------------------
    }
}