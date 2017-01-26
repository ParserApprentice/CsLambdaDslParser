//MIT, 2015-2017, ParserApprentice
using System; 
using System.Collections.Generic; 
using Parser.MyCs;


//describe switch problems
//
namespace Parser.ParserKit.LR
{
    //delegate int TestDel();
    //delegate int TestDel2(Parser.ParserKit.ParseNodeHolder x);

    //public class A : Parser.ParserKit.ReflectionSubParser
    //{

    //    public void Test()
    //    {
    //        TestDel d1 = delegate  //use delegate keyword
    //        {
    //            return 0;
    //        };

    //        TestDel d2 = () =>      //use lambda statement
    //        {
    //            return 0;
    //        };

    //        TestDel d3 = () => 0;   //use lambda expression              

    //        TestDel2 d4 = (o) =>    //use lambda statement
    //        {
    //            return 1;
    //        };

    //        TestDel2 d5 = o => 1;   //use lambda expression 

    //        d1();                   //test lambda
    //        d2(); d3();
    //        d4(null); d5(null);
    //    }

    //    protected override void Define()
    //    {
    //    }
    //    public override string GetTokenPresentationName(string fieldname)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


    public partial class Lang01 : UserGrammarSheetEz
    {


        public TokenInfoCollection RebuildCsTokenInfoCollection()
        {
            BuildCSTokenSheet();
            return tokenInfoCollection;
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

        Parser.MyCs.MyCsLexer myCsLexer;
        public TokenStreamReader Lex2(string input)
        {
            return Lex2(input.ToCharArray());
        }
        public TokenStreamReader Lex2(char[] input)
        {
            CsTokenInfoCollection csTokenInfos = new CsTokenInfoCollection();
            TokenInfoCollection tkInfoCollection = csTokenInfos.GetSnapedTokenInfoCollection();
            //lexer is reusable  
            myCsLexer = new Parser.MyCs.MyCsLexer(tkInfoCollection);
            TokenStream tkStream = new TokenStream();
            int latestPos;
            myCsLexer.Lex(input, tkStream, 0, out latestPos);
            return new TokenStreamReader(tkStream);
        }
        /// <summary>
        /// assign symbol presendence
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="precendence"></param>
        public void set_precedence(string whitespace_sep_symbol_list, int precendence)
        {
            //isRightAssoc=false
            set_precedence(whitespace_sep_symbol_list, precendence, false);
        }

        void AddRow(TestLRParsingTable table, int stateNumber, params string[] todoList)
        {
            TestLRParsingTableRow row = table.StartNewTableRow();

            int j = todoList.Length;
            for (int i = 0; i < j; ++i)
            {
                string todo = todoList[i];

                int pos = todo.IndexOf(' ');
                if (pos < 0)
                {
                    throw new NotSupportedException();
                }

                string first = todo.Substring(0, pos);
                string second = todo.Substring(pos + 1);
                TokenDefinition tokenInfo = (first == "$") ? TokenDefinition._eof : GetTokenInfo(first);

                if (tokenInfo != null)
                {

                    char c = second[0];
                    switch (c)
                    {
                        case 's':
                            {
                                row.AddShiftTask(tokenInfo, int.Parse(second.Substring(1)));
                            }
                            break;
                        case 'r':
                            {
                                row.AddReductionTask(tokenInfo, int.Parse(second.Substring(1)));
                            }
                            break;
                        case 'a':
                            {
                                row.AddAcceptTask(tokenInfo);
                            }
                            break;
                        case 'e':
                            {
                                row.AddErrorTask(tokenInfo, int.Parse(second.Substring(1)));
                            }
                            break;
                        case 'g':
                        default:
                            {
                                throw new NotSupportedException();
                            }
                    }
                }
                else
                {
                    UserNTDefinition nt = this.allUserNTs.Find(first);
                    if (nt == null || nt.GenNT == null)
                    {
                        throw new NotSupportedException();
                    }
                    char c = second[0];
                    switch (c)
                    {
                        case 'g':
                            {
                                row.AddGotoTask(nt.GenNT, int.Parse(second.Substring(1)));
                            }
                            break;
                        default:
                            {
                                throw new NotSupportedException();
                            }
                    }
                }
            }
        }

        public void TestLR0_3_4()
        {

            //define lex
            define_term("id");
            define_term("a", "b", "c", "d", "x", "y", "z", "+", "*", "(", ")", "/", "%");
            set_precedence("+", 10);
            set_precedence("*", 20);

            var simple_E = nt("E");
            {
                I("id");
                I("(", "X", ")");
            }
            var simple_M = nt("M");
            {
                I("id"); //base
                I("M", "/", "M");
                I("M", "%", "M");
            }
            NTDefinition augmentedNT = PrepareUserGrammarForAnyLR(simple_E);

            //---------------------------------------------------------------------------------
            LRParsingTable parsingTable = CreateLR1Table(augmentedNT);
            parsingTable.MakeParsingTable();
            //---------------------------------------------------------------------------------
            //string[] test01 = new string[] { "id", "+", "id", "*", "id" };
            //string[] test01 = new string[] { "(", "id", ")", "id", "%", "id" };
            string[] test01 = new string[] { "(", "id", ")" };
            //--------------------------------------------------------------------------------- 
            LRParser parser = LRParsing.CreateRunner(parsingTable);
#if DEBUG
            parser.dbugWriteParseLog = true;
#endif
            parser.Parse(LexLR2(test01));

        }
        public void TestLR0_3_4_2()
        {

            //define lex
            define_term("id");
            define_term("a", "b", "c", "d", "x", "y", "z", "+", "*", "(", ")", "/", "%");

            set_precedence("+", 10);
            set_precedence("*", 20);

            //---------------
            //Precedence and Associativity to Resolve Conflicts
            //page 279 
            //Grammar 4.3 DragonBook page 194
            var simple_E = nt("E");
            {
                I("id"); //base

                //I("E", "+", "E");
                //I("E", "*", "E");
                //I("(", "E", ")");                
                //I("(", "E", ")", "E");                    
                //-------------------
                //to another
                //I("M");
                I("(", "X", ")");
            }

            var simple_M = nt("M");
            {
                I("id"); //base
                I("M", "/", "M");
                I("M", "%", "M");
            }


            NTDefinition augmentedNT = PrepareUserGrammarForAnyLR(simple_E);

            //---------------------------------------------------------------------------------
            LRParsingTable parsingTable = CreateLR1Table(augmentedNT);
            //parsingTable.LR1ToLALR = true;
            parsingTable.MakeParsingTable();
            //---------------------------------------------------------------------------------
            //string[] test01 = new string[] { "id", "+", "id", "*", "id" };
            //string[] test01 = new string[] { "(", "id", ")", "id", "%", "id" };
            string[] test01 = new string[] { "(", "id", ")" };
            //--------------------------------------------------------------------------------- 
            LRParser parser = LRParsing.CreateRunner(parsingTable);
#if DEBUG
            parser.dbugWriteParseLog = true;
#endif
            parser.Parse(LexLR2(test01));

        }
        class SimpleParseNodeHolder : ParseNodeHolder { }

        public void TestLR0_3_5_1()
        {
            //define lex
            define_term("id");
            define_term("a", "b", "c", "d", "x", "y", "z",
                "+", "*", "(", ")",
                "[", "]",
                "/", "%");

            set_precedence("+", 10);
            set_precedence("*", 20);

            var simple_E = nt("E");
            {
                I("id");
                I("(", "M", ")");
                I("(", "M", "]");
            }

            var simple_M = nt("M");
            {
                I("id");
                I("M", "/", "M");
                I("M", "%", "M");
            }

            NTDefinition[] augmentedNTs =
                PrepareUserGrammarForAnyLR(new[] { simple_E, simple_M });

            //--------------------------------------------------------------------------------- 
            LRParsingTable parsingTable1 = CreateLR1Table(augmentedNTs[0]);
            parsingTable1.MakeParsingTable();

            LRParsingTable parsingTable2 = CreateLR1Table(augmentedNTs[1]);
            parsingTable2.MakeParsingTable();

            SimpleParseNodeHolder parseNodeHolder = new SimpleParseNodeHolder();
            LRParser parser1 = LRParsing.CreateRunner(parsingTable1);
            parser1.ParseNodeHolder = parseNodeHolder;

#if DEBUG
            parser1.dbugWriteParseLog = true;
#endif

            LRParser parser2 = LRParsing.CreateRunner(parsingTable2);
            parser2.ParseNodeHolder = parseNodeHolder;
#if DEBUG
            parser2.dbugWriteParseLog = true;
#endif
            parser1.SetSwitchHandler(swctx =>
            {
                Token current_token = swctx.CurrentToken;
                //goto another parser
                swctx.BeginSwitch();
                {
                    swctx.SwitchBackParseResult = parser2.Parse(swctx.Reader);
                }
                swctx.EndSwitch();
            });

            parser2.SetSwitchHandler(swctx =>
            {

            });

            //---------------------------------------------------------------------------------              
            //string[] test01 = new string[] { "id", "+", "id", "*", "id" };
            //string[] test01 = new string[] { "(", "id", ")", "id", "%", "id" };
            string[] test01 = new string[] { "(", "id", "/", "id", ")" };
            // string[] test01 = new string[] { "(", "id", "+", "id", "]" };
            //--------------------------------------------------------------------------------- 
            Token[] tokens = LexLR(test01);
            TokenStream tokenStream1 = new TokenStream();
            tokenStream1.AddTokens(tokens);
            parser1.Parse(new TokenStreamReader(tokenStream1));

        }

        public void TestLR0_3_5x()
        {
            //define lex 
            define_term("id",
                "+", "*", "(", ")",
                "[", "]",
                "/", "%");
            set_precedence("+", 10);
            set_precedence("*", 20);

            BeginSG();
            var E = nt("E");
            {
                I("(", "M", ")");
                I("(", "M", "]");
            }
            var M = nt("M");
            {
                I("id");
                I("M", "/", "M");
                I("M", "%", "M");
            }
            NTDefinition _E = EndSG(E);
            LRParsingTable parsingTable1 = CreateLR1Table(_E);
            parsingTable1.MakeParsingTable();


            var parseNodeHolder = new SimpleParseNodeHolder();
            LRParser parser1 = LRParsing.CreateRunner(parsingTable1);
            parser1.ParseNodeHolder = parseNodeHolder;

#if DEBUG
            parser1.dbugWriteParseLog = true;
#endif
            parser1.SetSwitchHandler(swctx =>
            {

            });
            //---------------------------------------------------------------------------------              
            //string[] test01 = new string[] { "id", "+", "id", "*", "id" };
            //string[] test01 = new string[] { "(", "id", ")", "id", "%", "id" };
            string[] test01 = new string[] { "(", "id", "/", "id", ")" };
            // string[] test01 = new string[] { "(", "id", "+", "id", "]" };
            //--------------------------------------------------------------------------------- 
            Token[] tokens = LexLR(test01);
            TokenStream tokenStream1 = new TokenStream();
            tokenStream1.AddTokens(tokens);
            tokenStream1.SetCurrentPosition(-1);

            TokenStreamReader reader = new TokenStreamReader(tokenStream1);
            reader.ReadNext();
            parser1.Parse(reader);
            ParseNode finalNode = parser1.FinalNode;


        }
        public void TestLR0_3_5()
        {
            //define lex
            define_term("id",
                "+", "*", "(", ")",
                "[", "]",
                "/", "%");
            set_precedence("+", 10);
            set_precedence("*", 20);
            //----------------------------------------------
            BeginSG();
            var E = nt("E");
            {
                I("(", "M", ")");
                I("(", "M", "]");
            }
            NTDefinition _E = EndSG(E);
            LRParsingTable parsingTable1 = CreateLR1Table(_E);
            parsingTable1.MakeParsingTable();
            //----------------------------------------------
            BeginSG();
            var M = nt("M");
            {
                I("id");
                I("M", "/", "M");
                I("M", "%", "M");
            }
            NTDefinition _M = EndSG(M);
            LRParsingTable parsingTable2 = CreateLR1Table(_M);
            parsingTable2.MakeParsingTable();
            //----------------------------------------------

            SimpleParseNodeHolder parseNodeHolder = new SimpleParseNodeHolder();
            LRParser parser1 = LRParsing.CreateRunner(parsingTable1);
            parser1.ParseNodeHolder = parseNodeHolder;
#if DEBUG
            parser1.dbugWriteParseLog = true;
#endif

            LRParser parser2 = LRParsing.CreateRunner(parsingTable2);
            parser2.ParseNodeHolder = parseNodeHolder;
#if DEBUG
            parser2.dbugWriteParseLog = true;
#endif
            parser1.SetSwitchHandler(swctx =>
            {
                Token current_token = swctx.CurrentToken;
                //goto another parser
                var switchDetail = swctx.SwitchDetail;
                swctx.BeginSwitch();
                {
                    swctx.SwitchBackParseResult = parser2.Parse(swctx);
                    var switchPair = switchDetail.GetSwPair(0);
                    swctx.SwitchBackState = switchPair.switchBackState;
                }
                swctx.EndSwitch();
            });

            parser2.SetSwitchHandler(swctx =>
            {

            });

            //---------------------------------------------------------------------------------              
            //string[] test01 = new string[] { "id", "+", "id", "*", "id" };
            //string[] test01 = new string[] { "(", "id", ")", "id", "%", "id" };
            string[] test01 = new string[] { "(", "id", "/", "id", ")" };
            // string[] test01 = new string[] { "(", "id", "+", "id", "]" };
            //--------------------------------------------------------------------------------- 
            Token[] tokens = LexLR(test01);
            TokenStream tokenStream1 = new TokenStream();
            tokenStream1.AddTokens(tokens);
            tokenStream1.SetCurrentPosition(-1);

            TokenStreamReader reader = new TokenStreamReader(tokenStream1);

            parser1.Parse(reader);
            ParseNode finalNode = parser1.FinalNode;

        }

        public void TestLR0_3_5x1()
        {
            //define lex
            define_term("id",
                "+", "*", "(", ")",
                "[", "]",
                "/", "%");
            set_precedence("+", 10);
            set_precedence("*", 20);
            //----------------------------------------------
            BeginSG();
            var E = nt("E");
            {
                I("(", "M", ")");
                I("(", "M", "]");
            }
            NTDefinition _E = EndSG(E);
            LRParsingTable parsingTable1 = CreateLR1Table(_E);
            parsingTable1.MakeParsingTable();
            //----------------------------------------------
            BeginSG();
            var M = nt("M");
            {
                I("id");
                I("M", "/", "M");
                I("M", "%", "M");
            }
            NTDefinition _M = EndSG(M);
            LRParsingTable parsingTable2 = CreateLR1Table(_M);
            parsingTable2.MakeParsingTable();
            //----------------------------------------------

            var parseNodeHolder = new SimpleParseNodeHolder();
            LRParser parser1 = LRParsing.CreateRunner(parsingTable1);
            parser1.ParseNodeHolder = parseNodeHolder;
#if DEBUG
            parser1.dbugWriteParseLog = true;
#endif
            LRParser parser2 = LRParsing.CreateRunner(parsingTable2);
            parser2.ParseNodeHolder = parseNodeHolder;
#if DEBUG
            parser2.dbugWriteParseLog = true;
#endif
            parser1.SetSwitchHandler(swctx =>
            {
                Token current_token = swctx.CurrentToken;
                var switchDetail = swctx.SwitchDetail;
                swctx.BeginSwitch();
                {
                    swctx.SwitchBackParseResult = parser2.Parse(swctx);
                    var switchPair = switchDetail.GetSwPair(0);
                    swctx.SwitchBackState = switchPair.switchBackState;
                }
                swctx.EndSwitch();
            });
            parser2.SetSwitchHandler(swctx => { });

            string[] input = new string[] { "(", "id", "/", "id", ")" };
            Token[] tokens = LexLR(input);
            var reader = new TokenStreamReader(new TokenStream(tokens));

            parser1.Parse(reader);

            ParseNode finalNode = parser1.FinalNode;
        }


        public void TestLR0_3_5_2()
        {
            //define lex
            define_term("id");
            define_term("id", "b", "c", "d", "x", "y", "z",
                "+", "*", "(", ")",
                "[", "]",
                "/", "%");

            set_precedence("+", 10);
            set_precedence("*", 20);
            var simple_E = nt("E");
            {
                I("id"); //base  
                I("id", "X");
            }

            var simple_M = nt("M");
            {
                I("+", "-"); //base 
            }

            NTDefinition[] augmentedNTs =
                PrepareUserGrammarForAnyLR(new[] { simple_E, simple_M });

            //---------------------------------------------------------------------------------
            LRParsingTable parsingTable1 = CreateLR1Table(augmentedNTs[0]);
            parsingTable1.MakeParsingTable();

            LRParsingTable parsingTable2 = CreateLR1Table(augmentedNTs[1]);
            parsingTable2.MakeParsingTable();


            //--------------------------------------------------------------------------------- 
            LRParser parser1 = LRParsing.CreateRunner(parsingTable1);
#if DEBUG
            parser1.dbugWriteParseLog = true;
#endif
            //--------------------------------------------------------------------------------- 
            LRParser parser2 = LRParsing.CreateRunner(parsingTable2);
#if DEBUG
            parser2.dbugWriteParseLog = true;
#endif
            parser1.SetSwitchHandler(sw =>
            {
                Token current_token = sw.CurrentToken;
                var strm_reader = sw.Reader;
                sw.BeginSwitch();
                {
                    //goto another parser
                    sw.SwitchBackParseResult = parser2.Parse(sw.Reader);
                }
                sw.EndSwitch();
            });

            //--------------------------------------------------------------------------------- 
            string[] test01 = new string[] { "id", "+", "-" };

            TokenStream tkStream = new TokenStream();
            tkStream.AddTokens(LexLR(test01));
            parser1.Parse(new TokenStreamReader(tkStream));

        }
        public void TestLR0_3_6()
        {

            //define lex
            define_term("id");
            define_term("class", "struct", "interface");
            define_term("public", "private", "internal", "protected");
            define_term("void", "string", "int");

            define_term(
                "+", "*", "(", ")", "{", "}",
                "[", "]",
                "/", "%");

            set_precedence("+", 10);
            set_precedence("*", 20);


            var simple_E = nt("type_decl");
            {
                I("class_decl");
                I("struct_decl");
                I("interface_decl");

            }
            nt("class_decl");
            {
                I("type_modifier", "class", "j2", "id", "{", "}");
            }
            nt("struct_decl");
            {
                I("type_modifier", "struct", "j2", "id", "{", "}");
            }
            nt("interface_decl");
            {
                I("type_modifier", "interface", "j2", "id", "{", "}");

            }
            nt("type_modifier");
            {
                I("public");
                I("private");
                I("protected");
            }


            NTDefinition[] augmentedNTs = PrepareUserGrammarForAnyLR(new[] { simple_E });
            //---------------------------------------------------------------------------------

            //---------------------------------------------------------------------------------
            LRParsingTable parsingTable1 = CreateLR1Table(augmentedNTs[0]);
            //parsingTable.LR1ToLALR = true;
            parsingTable1.MakeParsingTable();


            string[] test01 = new string[] { "public", "class", "A", "{", "}" };
            //--------------------------------------------------------------------------------- 
            LRParser parser1 = LRParsing.CreateRunner(parsingTable1);
#if DEBUG
            parser1.dbugWriteParseLog = true;
#endif
            Token[] tokens = LexLR(test01);

            parser1.SetSwitchHandler(sw =>
            {
                //handle error 
                TokenStreamReader strm_reader = sw.Reader;
                var current_token = strm_reader.CurrentToken;
                //strm_reader.WaitingParserCount++;
                //parser2.Parse(strm_reader);
                sw.SwitchBackParseResult = new ParseResult();  //input node from another
                //strm_reader.WaitingParserCount--; 
            });

            TokenStream tkstream = new TokenStream();
            tkstream.AddTokens(tokens);
            parser1.Parse(new TokenStreamReader(tkstream));

        }







        void BuildCSTokenSheet()
        {

            ClearAllTks();
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

            tokenInfoCollection.SnapAllTokenInfo();
        }

        public void TestLR_SimpleArgsB()
        {
            BuildCSTokenSheet();
            //--------------------------------------  
            string expression = "expression";
            UserNTDefinition simple_E =
            nt("argument_list");
            {
                I("argument");
                Sh(r =>
                {

                });
                Rd(r =>
                {
                });
                //----------------------------------
                I("argument_list", ",", "argument");
                Sh(r =>
                {

                });
                Rd(r =>
                {
                });
                //----------------------------------
            }
            //need root
            nt(expression);
            {
                I("id");
                Sh(r =>
                {

                });
                Rd(r =>
                {

                });
            }
            nt("argument");
            {
                I("argument_name:opt", "argument_value");
                Sh(r =>
                {

                });
                Rd(r =>
                {
                });
            }
            nt("argument_name");
            {
                I("id", ":");
                Sh(r =>
                {

                });
                Rd(r =>
                {
                });
            }
            nt("argument_value");
            {
                I(expression);
                Sh(r =>
                {

                });
                Rd(r =>
                {
                });
            }

            NTDefinition augmentedNT = PrepareUserGrammarForAnyLR(simple_E);

            LRParsingTable parsingTable = CreateLR1Table(augmentedNT);
            parsingTable.MakeParsingTable();



            //test input   
            //string teststr = "id()"; //? 
            //string teststr = "id(id)"; //? 
            //string teststr = "id(id,id)"; //? 

            //test ambiguous grammar (CS4 spec page 163)
            //string teststr = "id(id<id,id>(id))";//1.

            //string teststr = "id(id<id,id,id>(id))";//2.             
            //string teststr = "id(id<id,id>id)";//2.             
            //string teststr = "id<id>(id)";//3.             
            //string teststr = "id(id<id)";//4.  
            //string teststr = "id<id";//4.             
            //string teststr = "(id)<(id)";//4.       

            //string teststr = "id<id>+id";//1.//ok
            //string teststr = "(id<(id))";//1.//ok
            //string teststr = "id(id+id*id)";//1.//ok
            //string teststr = "id";//1.//ok
            //            string teststr = "id:id";//1.//ok
            string teststr = "id:id,id,id";//1.//ok
            //string teststr = "id=id=id";
            //string teststr = "id+id+id+id"; 

            var tkstream = Lex2(teststr);

            //runner
            var runner1 = LRParsing.CreateRunner(parsingTable);// parsingTable.CreateRunner();
            runner1.Parse(tkstream);

            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }



        public void TestLR_SimpleArgsB2()
        {

            BuildCSTokenSheet();
            ArgumentListParser argParser = new ArgumentListParser();
            argParser.Setup(this.tokenInfoCollection);
            //UserNTDefinition[] uNts = argParser.GetAllUserNts();
            //foreach (UserNTDefinition uNt in uNts)
            //{
            //    allUserNTs.AddNT(uNt);
            //} 
            //NTDefinition augmentedNT = PrepareUserGrammarForAnyLR(argParser.RootNt); 
            //LRParsingTable parsingTable = CreateLR1Table(augmentedNT);
            //parsingTable.MakeParsingTable();



            //test input   
            //string teststr = "id()"; //? 
            //string teststr = "id(id)"; //? 
            //string teststr = "id(id,id)"; //? 

            //test ambiguous grammar (CS4 spec page 163)
            //string teststr = "id(id<id,id>(id))";//1.

            //string teststr = "id(id<id,id,id>(id))";//2.             
            //string teststr = "id(id<id,id>id)";//2.             
            //string teststr = "id<id>(id)";//3.             
            //string teststr = "id(id<id)";//4.  
            //string teststr = "id<id";//4.             
            //string teststr = "(id)<(id)";//4.       

            //string teststr = "id<id>+id";//1.//ok
            //string teststr = "(id<(id))";//1.//ok
            //string teststr = "id(id+id*id)";//1.//ok
            //string teststr = "id";//1.//ok
            //            string teststr = "id:id";//1.//ok
            string teststr = "id:id,id,id";//1.//ok
            //string teststr = "id=id=id";
            //string teststr = "id+id+id+id"; 

            var tkstream = Lex2(teststr);

            //runner
            //var runner1 = LRParsing.CreateRunner(parsingTable);// parsingTable.CreateRunner();
            ParseCs(argParser, tkstream);
            //runner1.Parse(tkstream); 
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }


        public void TestLR_SimpleArgsB3()
        {

            BuildCSTokenSheet();

            var exprParser = new ExpressionParser();
            exprParser.Setup(this.tokenInfoCollection);


            var typeArgParsers = new TypeArgumentListParser();
            typeArgParsers.Setup(this.tokenInfoCollection);

            var typeParser = new TypeParser();
            typeParser.Setup(this.tokenInfoCollection);

            exprParser.SetParserSwitchHandler(sw =>
            {
                //what next parser 
                //handle error  
                Token current_token1 = sw.CurrentToken;
                //check if next parser start with current token  
                if (current_token1.GetLexeme() == "<")
                {
                    typeArgParsers.Parse(sw);
                }
            });

            typeArgParsers.SetParserSwitchHandler(sw =>
            {
                Token current_token1 = sw.CurrentToken;

                //late switch technique
                //goto another parser
                typeParser.Parse(sw);
            });

            typeParser.SetParserSwitchHandler(strm_reader =>
            {

            });

            string teststr = "id<id>";
            //string teststr = "id && id";

            var tkstream = Lex2(teststr);

            ParseCs(exprParser, tkstream);
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }
        public void TestLR_SimpleArgsB4()
        {


            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);

            ExpressionParser exprParser = parserMan.Setup(new ExpressionParser());
            TypeArgumentListParser typeArgParsers = parserMan.Setup(new TypeArgumentListParser());
            TypeParser typeParser = parserMan.Setup(new TypeParser());

            //string teststr = "id<id>";
            string teststr = "id || id || id";

            TokenStreamReader tkstream = Lex2(teststr);
            ParseCs(exprParser, tkstream);
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }
        void ParseCs(SubParser subParser, TokenStreamReader tk)
        {
            subParser.Parse(ParseNodeHolderFactory.CreateCsParseNodeHolderForAst(), tk);
        }

        public void TestLR_SimpleArgsB5()
        {


            BuildCSTokenSheet();

            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            ExpressionParser exprParser = parserMan.Setup(new ExpressionParser());
            TypeArgumentListParser typeArgParsers = parserMan.Setup(new TypeArgumentListParser());
            TypeParser typeParser = parserMan.Setup(new TypeParser());
            StatementParser statementParser = parserMan.Setup(new StatementParser());
            ClassDeclParser classDeclParser = parserMan.Setup(new ClassDeclParser());
            AttributesParser attrParser = parserMan.Setup(new AttributesParser());
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());

            //string teststr = "id<id>";
            //string teststr = "id;";
            string teststr = "namespace id {class id{}}";

            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(nsParser, tkstream);
            ParseNode finalNode = nsParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }



        public void TestLR_SimpleArgsB6()
        {



            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());
            ExpressionParser exprParser = parserMan.Setup(new ExpressionParser());
            TypeArgumentListParser typeArgParsers = parserMan.Setup(new TypeArgumentListParser());
            TypeParser typeParser = parserMan.Setup(new TypeParser());
            StatementParser statementParser = parserMan.Setup(new StatementParser());
            ClassDeclParser classDeclParser = parserMan.Setup(new ClassDeclParser());
            AttributesParser attrParser = parserMan.Setup(new AttributesParser());
            //-------------------------------------- 

            //string teststr = "id<id>";
            //string teststr = "id;";
            string teststr = "namespace id { class id{} class id{} namespace id{}}";

            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(nsParser, tkstream);
            ParseNode finalNode = nsParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }
        public void TestLR_SimpleArgsB7()
        {



            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());
            ExpressionParser exprParser = parserMan.Setup(new ExpressionParser());
            TypeArgumentListParser typeArgParsers = parserMan.Setup(new TypeArgumentListParser());
            TypeParser typeParser = parserMan.Setup(new TypeParser());
            StatementParser statementParser = parserMan.Setup(new StatementParser());
            StructDeclParser structDeclParser = parserMan.Setup(new StructDeclParser());
            ClassDeclParser classDeclParser = parserMan.Setup(new ClassDeclParser());
            AttributesParser attrParser = parserMan.Setup(new AttributesParser());
            //-------------------------------------- 
            parserMan.PrepareSwitchLink();

            //-------------------------------------- 
            //string teststr = "id<id>";
            //string teststr = "id;";
            string teststr = "namespace id { public class id{} public struct id{} class id{} struct id{} namespace id{}}";


            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(nsParser, tkstream);
            ParseNode finalNode = nsParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }
        public void TestLR_SimpleArgsB8()
        {



            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());
            ExpressionParser exprParser = parserMan.Setup(new ExpressionParser());
            TypeArgumentListParser typeArgParsers = parserMan.Setup(new TypeArgumentListParser());
            TypeParser typeParser = parserMan.Setup(new TypeParser());
            StatementParser statementParser = parserMan.Setup(new StatementParser());
            StructDeclParser structDeclParser = parserMan.Setup(new StructDeclParser());
            ClassDeclParser classDeclParser = parserMan.Setup(new ClassDeclParser());
            AttributesParser attrParser = parserMan.Setup(new AttributesParser());
            //-------------------------------------- 
            parserMan.PrepareSwitchLink();

            //-------------------------------------- 
            //string teststr = "id<id>";
            //string teststr = "id;";
            string teststr = "if(id){id;} else if(id){id;} else{id;}";


            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(statementParser, tkstream);
            ParseNode finalNode = statementParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }
        public void TestLR_SimpleArgsB9()
        {



            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());



            parserMan.Setup(new ExpressionParser());
            parserMan.Setup(new TypeArgumentListParser());
            parserMan.Setup(new TypeParser());
            parserMan.Setup(new StatementParser());
            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser());
            parserMan.Setup(new MethodDeclParser());
            parserMan.Setup(new PropertyDeclParser());
            parserMan.Setup(new AttributesParser());
            //-------------------------------------- 
            parserMan.PrepareSwitchLink();

            //-------------------------------------- 

            //string teststr = " class id{ void id(){}}";//pass
            //string teststr = "class id{ public id id{get;set;}}"; //pass
            string teststr = "class id{ public id id{get;set;}}"; //error + insertion auto correct
            //string teststr = "class id{ public id +-{get;set;}}"; //error + insertion auto correct
            // string teststr = "class id{ public id id id {get;set;}}";

            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(nsParser, tkstream);
            ParseNode finalNode = nsParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }

        public void TestLR_SimpleArgsB10()
        {

            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());



            parserMan.Setup(new ExpressionParser());
            parserMan.Setup(new TypeArgumentListParser());
            parserMan.Setup(new TypeParser());
            parserMan.Setup(new StatementParser());
            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser());
            parserMan.Setup(new MethodDeclParser());
            parserMan.Setup(new PropertyDeclParser());

            parserMan.Setup(new AttributesParser());
            //-------------------------------------- 
            parserMan.PrepareSwitchLink();
            //-------------------------------------- 


            //string teststr = " class id{ void id(){}}";//pass
            //string teststr = "class id{ public id id{get;set;}}"; //pass          


            //string teststr = "class id{ public id +-{get;set;}}"; //error + insertion auto correct
            // string teststr = "class id{ public id id id {get;set;}}";

            string teststr = "namespace id {class id{  public void id(){} }}"; //error + insertion auto correct

            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(nsParser, tkstream);
            ParseNode finalNode = nsParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }


        public void TestLR_SimpleArgsB11()
        {

            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());
            parserMan.Setup(new ExpressionParser());
            parserMan.Setup(new TypeArgumentListParser());
            parserMan.Setup(new TypeParser());
            parserMan.Setup(new StatementParser());
            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser());
            parserMan.Setup(new MethodDeclParser());
            parserMan.Setup(new PropertyDeclParser());
            parserMan.Setup(new FieldDeclParser());
            parserMan.Setup(new AttributesParser());
            //-------------------------------------- 
            parserMan.PrepareSwitchLink();
            //-------------------------------------- 


            //string teststr = " class id{ void id(){}}";//pass
            //string teststr = "class id{ public id id{get;set;}}"; //pass          


            //string teststr = "class id{ public id +-{get;set;}}"; //error + insertion auto correct
            // string teststr = "class id{ public id id id {get;set;}}";

            //string teststr = "namespace id {class id{  public void id(){} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id {get;set;} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id; }}";  
            //string teststr = "namespace id {class id{  public id id = id; }}";  
            string teststr = "namespace id {class id{ [id]public id id = id; }}";
            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(nsParser, tkstream);
            ParseNode finalNode = nsParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }
        public void TestLR_SimpleArgsB12()
        {

            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());
            parserMan.Setup(new ExpressionParser());
            parserMan.Setup(new TypeArgumentListParser());
            parserMan.Setup(new TypeParser());
            parserMan.Setup(new StatementParser());
            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser());
            parserMan.Setup(new MethodDeclParser());
            parserMan.Setup(new PropertyDeclParser());
            parserMan.Setup(new FieldDeclParser());
            parserMan.Setup(new AttributesParser());
            //-------------------------------------- 
            parserMan.PrepareSwitchLink();
            //-------------------------------------- 


            //string teststr = " class id{ void id(){}}";//pass
            //string teststr = "class id{ public id id{get;set;}}"; //pass          


            //string teststr = "class id{ public id +-{get;set;}}"; //error + insertion auto correct
            // string teststr = "class id{ public id id id {get;set;}}";

            //string teststr = "namespace id {class id{  public void id(){} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id {get;set;} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id; }}";  
            //string teststr = "namespace id {class id{  public id id = id; }}";  
            string teststr = "namespace id {class id{ [id]public id id = id; [id] void id(){} }}";//pass

            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(nsParser, tkstream);
            ParseNode finalNode = nsParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }
        public void TestLR_SimpleArgsB13()
        {

            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            parserMan.Setup(new FormalParameterListParser());
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());
            parserMan.Setup(new ExpressionParser());
            parserMan.Setup(new TypeArgumentListParser());
            parserMan.Setup(new TypeParser());
            parserMan.Setup(new StatementParser());
            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser());
            parserMan.Setup(new MethodDeclParser());
            parserMan.Setup(new PropertyDeclParser());
            parserMan.Setup(new FieldDeclParser());
            parserMan.Setup(new AttributesParser());

            parserMan.Setup(new ArrayTypeParser());
            //-------------------------------------- 
            parserMan.PrepareSwitchLink();
            //-------------------------------------- 


            //string teststr = " class id{ void id(){}}";//pass
            //string teststr = "class id{ public id id{get;set;}}"; //pass          


            //string teststr = "class id{ public id +-{get;set;}}"; //error + insertion auto correct
            // string teststr = "class id{ public id id id {get;set;}}";

            //string teststr = "namespace id {class id{  public void id(){} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id {get;set;} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id; }}";  
            //string teststr = "namespace id {class id{  public id id = id; }}";  
            //string teststr = "namespace id {class id{ [id]public id id = id; [id] void id(){} }}";//pass
            string teststr = "namespace id {class id{ void id(id id,id id){} }}";//pass
            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(nsParser, tkstream);
            ParseNode finalNode = nsParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner();
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }
        public void TestLR_SimpleArgsB14()
        {
            GC.Collect();
            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            //--------------------------------------  
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            ExpressionParser exprParser = parserMan.Setup(new ExpressionParser());
            sw.Stop();
            long ms = sw.ElapsedMilliseconds;

            parserMan.Setup(new FormalParameterListParser());
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());

            parserMan.Setup(new TypeArgumentListParser());
            parserMan.Setup(new TypeParser());
            StatementParser stmtParser = parserMan.Setup(new StatementParser());
            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser());
            parserMan.Setup(new MethodDeclParser());
            parserMan.Setup(new PropertyDeclParser());
            parserMan.Setup(new FieldDeclParser());
            parserMan.Setup(new AttributesParser());
            parserMan.Setup(new ArrayTypeParser());
            parserMan.Setup(new ArgumentListParser());
            parserMan.Setup(new ObjectOrCollectionInitializerParser());
            //-------------------------------------- 
            parserMan.PrepareSwitchLink();
            //-------------------------------------- 


            //string teststr = " class id{ void id(){}}";//pass
            //string teststr = "class id{ public id id{get;set;}}"; //pass          


            //string teststr = "class id{ public id +-{get;set;}}"; //error + insertion auto correct
            // string teststr = "class id{ public id id id {get;set;}}";

            //string teststr = "namespace id {class id{  public void id(){} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id {get;set;} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id; }}";  
            //string teststr = "namespace id {class id{  public id id = id; }}";  
            //string teststr = "namespace id {class id{ [id]public id id = id; [id] void id(){} }}";//pass
            // string teststr = "1 + 1";//paparsingTabless
            //string teststr = "1 + 1 + (1 * 2)";//pass
            //string teststr = "id()";//pass
            //string teststr = "id(id,id:1)";//pass
            //string teststr = "id(out id,ref id,id:1)";//pass
            //string teststr = "id.id";//pass
            //string teststr = "id[id]";//pass
            //string teststr = "new id(0,1,2)";//pass
            //string teststr = "new id<id>(0,1,2)";//pass
            //string teststr = "-id";//pass
            //string teststr = "-id+-id";//pass
            //string teststr = "-id+id";//pass
            //string teststr = "!id+id";//pass
            //string teststr = "++id";//pass
            //string teststr = "(id)id";//pass
            //string teststr = "id >=id ";//pass
            //string teststr = "id ? id:id";//pass
            //string teststr = "id=> id";//pass
            string teststr = "id = id + id * id + id";//test precedence
            var tkstream = Lex2(teststr);
            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);

            ParseCs(exprParser, tkstream);
            ParseNode finalNode = exprParser.FinalNode;
            //var fparser = LRParsing.CreateForwardRunner(parsingTable);
            //fparser.LoadTokens(tkstream);
            //fparser.AnalysisRun();

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //stepParser.LoadTokens(tkstream);
            //stepParser.EnableSyntaxCallback = true;
            ////run step-by step
            //while (stepParser.RunNextStep()) ;
        }

        public void TestLexer1()
        {
            BuildCSTokenSheet();

            Parser.MyCs.MyCsLexer lexer = new Parser.MyCs.MyCsLexer(this.tokenInfoCollection);
            //var output = lexer.Lex("using X;namespace A{}");
            //string teststr = "using X;namespace A{}";
            //string teststr = "id<id,id>()";
            string teststr = "id/*okok*/ /*aaaa*/ +1";
            //string teststr = "id//okok12345\n+1";
            TokenStream tkStream = new TokenStream();
            lexer.Lex(teststr, tkStream);
        }


        public void TestLR_SimpleArgsB15()
        {

            //1. token info
            CsTokenInfoCollection csTokenInfos = new CsTokenInfoCollection();
            TokenInfoCollection tkInfoCollection = csTokenInfos.GetSnapedTokenInfoCollection();
            //lexer is reusable 

            myCsLexer = new Parser.MyCs.MyCsLexer(tkInfoCollection);
            // string teststr = "1 + 1";//paparsingTabless
            //string teststr = "1 + 1 + (1 * 2)";//pass
            //string teststr = "id()";//pass
            //string teststr = "id(id,id:1)";//pass
            //string teststr = "id(out id,ref id,id:1)";//pass
            //string teststr = "id.id";//pass
            //string teststr = "id[id]";//pass
            //string teststr = "new id(0,1,2)";//pass
            //string teststr = "new id<id>(0,1,2)";//pass
            //string teststr = "-id";//pass
            //string teststr = "-id+-id";//pass
            //string teststr = "-id+id";//pass
            //string teststr = "!id+id";//pass
            //string teststr = "++id";//pass
            //string teststr = "(id)id";//pass
            //string teststr = "id >=id ";//pass
            //string teststr = "id ? id:id";//pass

            //-------------------------------------- 
            ReflectionSubParser.s_tkInfoCollection = tkInfoCollection;
            System.Diagnostics.Stopwatch stopW1 = new System.Diagnostics.Stopwatch();
            stopW1.Start();

            var parserMan = new ParserManager(tkInfoCollection);
            parserMan.UseCache = true;

            parserMan.Setup(new FormalParameterListParser());

            //review get walker here
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser() { GetWalker = p => ((CsParseNodeHolder)p).NamespaceWalker });
            //NamespaceParser nsParser = parserMan.Setup(new NamespaceParser());
            ExpressionParser exprParser = parserMan.Setup(new ExpressionParser() { GetWalker = p => ((CsParseNodeHolder)p).ExpressionWalker });
            //ExpressionParser exprParser = parserMan.Setup(new ExpressionParser());

            parserMan.Setup(new TypeArgumentListParser());
            parserMan.Setup(new TypeParser());
            var stmtParser = parserMan.Setup(new StatementParser() { GetWalker = p => ((CsParseNodeHolder)p).StatementBuilder });
            //var stmtParser = parserMan.Setup(new StatementParser());
            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser() { GetWalker = p => ((CsParseNodeHolder)p).ClassWalker });
            //parserMan.Setup(new ClassDeclParser());
            parserMan.Setup(new MethodDeclParser() { GetWalker = p => ((CsParseNodeHolder)p).MethodWalker });
            //parserMan.Setup(new MethodDeclParser());
            parserMan.Setup(new PropertyDeclParser());
            parserMan.Setup(new FieldDeclParser());
            parserMan.Setup(new AttributesParser());
            parserMan.Setup(new ArrayTypeParser());
            parserMan.Setup(new ArgumentListParser());
            parserMan.Setup(new ObjectOrCollectionInitializerParser());
            parserMan.Setup(new TypeParameterConstraintsClausesParser());

            parserMan.PrepareSwitchLink();

            stopW1.Stop();
            var ms = stopW1.ElapsedMilliseconds;
            Console.WriteLine("build tables =" + ms);
            //-------------------------------------- 
            GC.Collect();

            //string teststr = " class id{ void id(){}}";//pass
            //string teststr = "class id{ public id id{get;set;}}"; //pass          


            //string teststr = "class id{ public id +-{get;set;}}"; //error + insertion auto correct
            // string teststr = "class id{ public id id id {get;set;}}";

            //string teststr = "namespace id {class id{  public void id(){} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id {get;set;} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id; }}";  
            //string teststr = "namespace id {class id{  public id[] id = id; }}";
            // string teststr = "namespace id {class id { [id]public id id = id; [id] void id(){} }}";//pass
            //string teststr = "class id<id> { }";//pass


            //string teststr = "namespace AB {public class id<id,id> where id:new(),class { }}";//pass
            //string teststr = System.IO.File.ReadAllText(@"D:\projects\px02\cs_inputtest\cs_01_300.cs");


            //string teststr = System.IO.File.ReadAllText(@"D:\projects\px02\cs_inputtest\cs_02_200.cs");
            string teststr = System.IO.File.ReadAllText(@"D:\projects\px02\cs_inputtest\cs_01.cs");
            // string teststr = "class id{}";
            //string teststr = "var id=1;";//pass 
            //string teststr = "while(true){}";//pass 
            //string teststr = "for(;;){}";//pass 
            //string teststr = "do{}while(id);";//pass 

            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);
            //parserMan.BreakOnShift = true;
            //parserMan.BreakOnReduce = true;
            //parserMan.UseFastParseMode = false;


            //-------------------------------------------
            //lambda break evaluator
            SubParsers.BreakMode._shouldBreakOnLambda = ShouldStopOnLambda;
            //-------------------------------------------
            //parserMan.BreakOnShift = true;
            //parserMan.BreakOnReduce = true;
            //parserMan.UseFastParseMode = false;

            parserMan.BreakOnShift = false;
            parserMan.BreakOnReduce = false;
            parserMan.UseFastParseMode = false;

            //------------------------
            int count = 100;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            CsParseNodeHolder csParseNodeHolder = ParseNodeHolderFactory.CreateCsParseNodeHolderForAst();

            char[] codeBuffer = teststr.ToCharArray();
            WarmUp(codeBuffer, csParseNodeHolder, nsParser, parserMan);

            System.Collections.Generic.List<long> ticks = new System.Collections.Generic.List<long>();
            int walkResult = 0;
            long min = 1000000000;
            long max = 0;


            for (int i = 0; i < count; ++i)
            {
                GC.Collect();

                //-----------------------------------
                sw.Start();

                CodeText sourceText = new CodeText(codeBuffer);
                MyCsTokenStream myCsTkStream = new MyCsTokenStream(sourceText, myCsLexer);
                myCsTkStream.Lex();//partial lex 
                myCsTkStream.SetCurrentPosition(-1);

                TokenStreamReader reader = new TokenStreamReader(myCsTkStream);

                ////sw.Stop();
                ////stop and record lex time               
                //long sticks = sw.ElapsedTicks;
                //tkstream.SetIndex(0);
                reader.ReadNext();
                ParseCs(nsParser, reader);
                sw.Stop();

                ParseNode finalNode = nsParser.FinalNode;

                ParseNodeLocator locator = new ParseNodeLocator(sourceText);
                finalNode.GetLocation(locator);
                //----------------------------------------------------
                //test nt walk            
                //ParseNodeWalker.WalkNodes(finalNode, csParseNodeHolder);
                //----------------------------------- 


                long sticks = sw.ElapsedTicks;
                long e_ms2 = sw.ElapsedMilliseconds;
                ticks.Add(e_ms2);

                if (e_ms2 > max) { max = e_ms2; }
                if (e_ms2 < min) { min = e_ms2; }

                sw.Reset();
                Console.WriteLine(i);
                //Console.WriteLine("pool count" + Token.PoolCount());
            }
            //--------------------------------------------
            long total = 0;
            for (int i = 0; i < count; ++i)
            {
                total += ticks[i];
            }

            long avg = lastestAvg = total / count;
            Console.WriteLine("xavg" + avg.ToString() + ",min=" + min + ",max" + max);
        }

        //======================
        static bool ShouldStopOnLambda(AstWalker walker, string argName)
        {
            return true;
            //return false;
            //return true;
            ////return false;
            //if (argName == "x")
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        //======================
        public void TestLR_SimpleArgsB16()
        {

            //1. token info
            CsTokenInfoCollection csTokenInfos = new CsTokenInfoCollection();
            TokenInfoCollection tkInfoCollection = csTokenInfos.GetSnapedTokenInfoCollection();
            //lexer is reusable 

            myCsLexer = new Parser.MyCs.MyCsLexer(tkInfoCollection);


            //-------------------------------------- 
            System.Diagnostics.Stopwatch stopW1 = new System.Diagnostics.Stopwatch();
            stopW1.Start();

            var parserMan = new ParserManager(tkInfoCollection);
            parserMan.UseCache = true;

            parserMan.Setup(new FormalParameterListParser());

            //parser not own walker / builder part
            //it must request from current ParseNodeHolder

            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser() { GetWalker = p => ((CsParseNodeHolder)p).NamespaceWalker });
            ExpressionParser exprParser = parserMan.Setup(new ExpressionParser() { GetWalker = p => ((CsParseNodeHolder)p).ExpressionWalker });

            parserMan.Setup(new TypeArgumentListParser() { GetWalker = p => ((CsParseNodeHolder)p).TypeArgListWalker });
            parserMan.Setup(new TypeParser());
            var stmtParser = parserMan.Setup(new StatementParser() { GetWalker = p => ((CsParseNodeHolder)p).StatementBuilder });

            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser() { GetWalker = p => ((CsParseNodeHolder)p).ClassWalker });
            parserMan.Setup(new MethodDeclParser() { GetWalker = p => ((CsParseNodeHolder)p).MethodWalker });
            parserMan.Setup(new PropertyDeclParser());
            parserMan.Setup(new FieldDeclParser());
            parserMan.Setup(new AttributesParser());
            parserMan.Setup(new ArrayTypeParser());
            parserMan.Setup(new ArgumentListParser());
            parserMan.Setup(new ObjectOrCollectionInitializerParser());
            parserMan.Setup(new TypeParameterConstraintsClausesParser());

            parserMan.PrepareSwitchLink();

            stopW1.Stop();
            var ms = stopW1.ElapsedMilliseconds;
            Console.WriteLine("build tables =" + ms);
            //-------------------------------------- 
            GC.Collect();

            //string teststr = " class id{ void id(){}}";//pass
            //string teststr = "class id{ public id id{get;set;}}"; //pass          


            //string teststr = "class id{ public id +-{get;set;}}"; //error + insertion auto correct
            // string teststr = "class id{ public id id id {get;set;}}";

            //string teststr = "namespace id {class id{  public void id(){} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id {get;set;} }}"; //error + insertion auto correct
            //string teststr = "namespace id {class id{  public id id; }}";  
            //string teststr = "namespace id {class id{  public id[] id = id; }}";
            // string teststr = "namespace id {class id { [id]public id id = id; [id] void id(){} }}";//pass
            //string teststr = "class id<id> { }";//pass


            //string teststr = "namespace AB {public class id<id,id> where id:new(),class { }}";//pass
            //string teststr = System.IO.File.ReadAllText(@"D:\projects\px02\cs_inputtest\cs_01_300.cs");


            // string teststr = System.IO.File.ReadAllText(@"D:\projects\px02\cs_inputtest\cs_02_200.cs");
            //string teststr = "1 + 1";//paparsingTabless
            //string teststr = "1 + 1 + (1 * 2)";//pass
            //string teststr = "id()";//pass
            //string teststr = "id(id,id:1)";//pass
            string teststr = "id(out id,ref id,id:1)";//pass

            //string teststr = "id.id";//pass
            //string teststr = "id[id]";//pass
            //string teststr = "new id(0,1,2)";//pass
            //string teststr = "new id<id>(0,1,2)";//pass
            //string teststr = "-id";//pass
            //string teststr = "-id+-id";//pass
            //string teststr = "-id+id";//pass
            //string teststr = "!id+id";//pass
            //string teststr = "++id";//pass
            //string teststr = "(id)id";//pass
            //string teststr = "id >=id ";//pass
            //string teststr = "id ? id:id";//pass



            // string teststr = "class id{}";
            //string teststr = "var id=1;";//pass 
            //string teststr = "while(true){}";//pass 
            //string teststr = "for(;;){}";//pass 
            //string teststr = "do{}while(id);";//pass 

            //statementParser.Parse(tkstream);
            //classDeclParser.Parse(tkstream);
            //parserMan.BreakOnShift = true;
            //parserMan.BreakOnReduce = true;
            //parserMan.UseFastParseMode = false;


            //-------------------------------------------
            //lambda break evaluator
            SubParsers.BreakMode._shouldBreakOnLambda = ShouldStopOnLambda;
            //-------------------------------------------
            //parserMan.BreakOnShift = true;
            //parserMan.BreakOnReduce = true;
            //parserMan.UseFastParseMode = false;

            parserMan.BreakOnShift = true;
            parserMan.BreakOnReduce = true;
            parserMan.UseFastParseMode = false;

            //------------------------
            int count = 100;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            CsParseNodeHolder csParseNodeHolder = ParseNodeHolderFactory.CreateCsParseNodeHolderForAst();

            SubParser startParser = exprParser;

            char[] codeBuffer = teststr.ToCharArray();
            WarmUp(codeBuffer, csParseNodeHolder, exprParser, parserMan);

            System.Collections.Generic.List<long> ticks = new System.Collections.Generic.List<long>();
            int walkResult = 0;
            long min = 1000000000;
            long max = 0;


            for (int i = 0; i < count; ++i)
            {
                GC.Collect();

                //-----------------------------------
                sw.Start();

                CodeText sourceText = new CodeText(codeBuffer);
                MyCsTokenStream myCsTkStream = new MyCsTokenStream(sourceText, myCsLexer);
                myCsTkStream.Lex();//partial lex 
                myCsTkStream.SetCurrentPosition(-1);
                TokenStreamReader reader = new TokenStreamReader(myCsTkStream);

                ////sw.Stop();
                ////stop and record lex time               
                //long sticks = sw.ElapsedTicks;
                //tkstream.SetIndex(0);
                reader.ReadNext();
                ParseCs(startParser, reader);
                sw.Stop();
                ParseNode finalNode = startParser.FinalNode;
                //-----------------------------------------------------
                //test nt walk            
                //ParseNodeWalker.WalkNodes(finalNode, csParseNodeHolder);
                //----------------------------------- 


                long sticks = sw.ElapsedTicks;
                long e_ms2 = sw.ElapsedMilliseconds;
                ticks.Add(e_ms2);

                if (e_ms2 > max) { max = e_ms2; }
                if (e_ms2 < min) { min = e_ms2; }

                sw.Reset();
                Console.WriteLine(i);
                //Console.WriteLine("pool count" + Token.PoolCount());
            }
            //--------------------------------------------
            long total = 0;
            for (int i = 0; i < count; ++i)
            {
                total += ticks[i];
            }

            long avg = lastestAvg = total / count;
            Console.WriteLine("xavg" + avg.ToString() + ",min=" + min + ",max" + max);
        }
        public long lastestAvg = 0;

        void WarmUp(char[] codeBuffer, CsParseNodeHolder csParseNodeHolder, SubParser subParser, ParserManager parserMan)
        {

            //TokenStreamReader tkstream = Lex2(codeBuffer);

            //ParseCs(subParser, tkstream);
            //ParseNode finalNode = subParser.FinalNode;
            ////-----------------------------------------------------
            ////test nt walk                
            //parserMan.WalkParseTree(csParseNodeHolder, finalNode, false);
            //-----------------------------------

        }
        public void SetupParsers(LangSheetSetupDel langsheetSetupDel)
        {
            BuildCSTokenSheet();
            //-------------------------------------- 
            var parserMan = new ParserManager(this.tokenInfoCollection);
            langsheetSetupDel(parserMan);
        }
    }
    public delegate void LangSheetSetupDel(ParserManager parserManager);
    partial class Lang01
    {


        Dictionary<NTDefinition, NTDefinition> GetNtDic()
        {

            Dictionary<NTDefinition, NTDefinition> ntdic = new Dictionary<NTDefinition, NTDefinition>();
            foreach (NTDefinition nt in this.dicCoreNTs.Values)
            {
                ntdic.Add(nt, nt);
            }
            return ntdic;
        }
        public TokenStreamReader LexLR2(string[] tokenstrs)
        {
            Token[] tokens = LexLR(tokenstrs);
            TokenStream tkstream = new TokenStream();
            tkstream.AddTokens(tokens);
            return new TokenStreamReader(tkstream);
        }
        public Token[] LexLR(string[] tokenstrs)
        {
            //add eof (auto)
            int j = tokenstrs.Length;

            Token[] token01 = new Token[j + 1];
            for (int i = 0; i < j; ++i)
            {
                TokenDefinition tkinfo = GetTokenInfo(tokenstrs[i]);
                if (tkinfo == null)
                {
                    //if no tokendef -> make it an id
                    tkinfo = GetTokenInfo("id");
                }

                token01[i] = new Token(tkinfo, tokenstrs[i]);
            }
            token01[j] = new Token(TokenDefinition._eof);
            return token01;
        }

        LRParsingTable CreateLR1Table(NTDefinition augmentedNT)
        {
            tokenInfoCollection.SnapAllTokenInfo();
            LRParsingTable parsingTable = new LRParsingTable(
                LRStyle.LR1,
                augmentedNT,
                this.GetNtDic(),
                this.symResolutionInfo,
                this.tokenInfoCollection);

            return parsingTable;
        }
        LRParsingTable CreateLALR1Table(NTDefinition augmentedNT)
        {
            tokenInfoCollection.SnapAllTokenInfo();
            LRParsingTable parsingTable = new LRParsingTable(
                LRStyle.LALR,
                augmentedNT,
                this.GetNtDic(),
                this.symResolutionInfo,
                this.tokenInfoCollection);

            return parsingTable;
        }
        LRParsingTable CreateLR0Table(NTDefinition augmentedNT)
        {
            tokenInfoCollection.SnapAllTokenInfo();
            LRParsingTable parsingTable = new LRParsingTable(
                LRStyle.LR0,
                augmentedNT,
                this.GetNtDic(),
                this.symResolutionInfo,
                this.tokenInfoCollection);

            return parsingTable;
        }


        public void TestLR0_6B()
        {
            //LALR
            //define lex
            define_term("id");
            define_term("a", "i", "e");

            set_precedence("i", 1);
            set_precedence("e", 2);
            //---------------

            //Precedence and Associativity to Resolve Conflicts
            //page 279 
            //Grammar 4.3 DragonBook page 194

            UserNTDefinition simple_E =
            nt("S");
            {
                I("i", "S", "e", "S");
                I("i", "S");
                I("a");

                //nt_collapse(o =>
                //{
                //});
            }

            //---------------------------------------------------------------------------------
            NTDefinition augmentedNT = PrepareUserGrammarForAnyLR(simple_E);
            //--------------------------------------------------------------------------------- 

            //---------------------------------------------------------------------------------
            LRParsingTable parsingTable = CreateLR0Table(augmentedNT);
            parsingTable.MakeParsingTable();

            string[] test01 = new string[] { "i", "i", "a", "e", "a" };
            LRParser parser = LRParsing.CreateRunner(parsingTable);
            parser.Parse(LexLR2(test01));

        }


        public void TestLR_CS1B()
        {

            define_term("id");
            //single chars operator
            define_single_char_token(" +-*/%^()[]{}<>;:,.!=?&|~");
            define_multi_chars_token("++ -- >> << && ||");
            define_multi_chars_token("<= >=");
            define_multi_chars_token("+= -= *= /= %= ^= &= |= <<= >>= ->");
            define_multi_chars_token("== !=");
            define_keywords("is as");
            define_keywords("true false sizeof new typeof checked unchecked");
            //statement
            define_keywords("if else while do for foreach in try catch throw return yield");

            define_keywords("namespace using");
            define_keywords("class struct delegate interface void null string int uint short ushort byte sbyte long ulong double float");
            define_keywords("this base override virtual sealed");
            define_keywords("operator");
            define_keywords("private public internal protected");

            define_special_terminal("<t", "<"); //open generic 
            define_special_terminal(">t", ">"); //close generic   
            //--------------------------------------
            set_precedence("if", 2);
            set_precedence("else", 1);
            //--------------------------------------
            set_precedence(".", 12); //multiplicative
            set_precedence("* / %", 11); //multiplicative
            set_precedence("+ -", 10);//additive   
            set_precedence("<< >>", 8);
            set_precedence("< > <= >= is as", 7);
            set_precedence("== !=", 6);
            //--------------------------------------
            string expression = "expression";
            string expression_list = "expression_list";

            string statement = "statement";
            string opt = ":opt";

            UserNTDefinition simple_E = nt(expression);
            simple_E.NTPrecedence = 100;
            {


                int p = 20;

                this.currentSqPrecedence = p;
#if DEBUG
                //I<dbugMyCsInvocationExpression>().prec(p--);
#endif
                //------------------------------------------------------------------------
                ////member access expression
                //this.currentSqPrecedence = p;
                //I<MyCs.MyCsMemberAccessExpression>().prec(p--);
                ////------------------------------------------------------------------------
                //this.currentSqPrecedence = p;
                ////unary op                
                //I<MyCs.MyCsUnaryExpression>().prec(p--);
                ////------------------------------------------------------------------------
                //this.currentSqPrecedence = p;
                ////binary op                 
                //I<MyCs.MyCsBinaryMultiplicativeExpression>().prec(p--);
                //this.currentSqPrecedence = p;
                //I<MyCs.MyCsBinaryAdditiveExpression>().prec(p--);
                //this.currentSqPrecedence = p;
                //I<MyCs.MyCsBinaryShiftExpression>().prec(p--);
                //this.currentSqPrecedence = p;
                //I<MyCs.MyCsBinaryRelationalAndTypeTestingExpression>().prec(p--);
                //this.currentSqPrecedence = p;
                //I<MyCs.MyCsBinaryEqualityExpression>().prec(p--);
                //this.currentSqPrecedence = p;
                //------------------------------------------------------------------------
            }

            //NT<MyCs.MyCsSimpleName>();
            //NT<MyCs.MyCsArgsList>();
            //NT<MyCs.MyCsTypeArgList>(); 
            //--------------------------------------------------------------------------------- 
            //AutoPrepareLaterNTs();

            NTDefinition augmentedNT = PrepareUserGrammarForAnyLR(simple_E);// PrepareUserGrammarsForLL1(simple_E);
            //--------------------------------------------------------------------------------- 

            //---------------------------------------------------------------------------------
            LRParsingTable parsingTable = CreateLR1Table(augmentedNT);
            //parsingTable.LR1ToLALR = true;
            parsingTable.MakeParsingTable();

            //test input 


            string teststr = "id()"; //? 
            //string teststr = "id(id)"; //? 
            //string teststr = "id(id,id)"; //? 

            //test ambiguous grammar (CS4 spec page 163)
            //string teststr = "id(id<id,id>(id))";//1.

            //string teststr = "id(id<id,id,id>(id))";//2.             
            //string teststr = "id(id<id,id>id)";//2.             
            //string teststr = "id<id>(id)";//3.             
            //string teststr = "id(id<id)";//4.  
            //string teststr = "id<id";//4.             
            //string teststr = "(id)<(id)";//4.       

            //string teststr = "id<id>+id";//1.//ok
            //string teststr = "(id<(id))";//1.//ok
            //string teststr = "id(id+id*id)";//1.//ok

            //parsingTable.Parse(Lex(teststr).ToArray()); 

            // string teststr = "id+id+id+id";

            // parsingTable.Parse(Lex(teststr).ToArray());
            var stepParser = LRParsing.CreateRunner(parsingTable);
            var finalNode = stepParser.Parse(Lex2(teststr));

        }

        void set_precedence(string s, prec prec)
        {
            set_precedence(s, (int)prec);
        }
        void set_precedence(string s, prec prec, bool isRightAssoc)
        {
            set_precedence(s, (int)prec, isRightAssoc);
        }
        public void set_precedence(string whitespace_sep_symbol_list, int precendence, bool isRightAssoc)
        {
            string[] symbols = whitespace_sep_symbol_list.Split(' ');
            foreach (string symbol in symbols)
            {
                TokenDefinition tkdef = GetTokenInfo(symbol);
                tkdef.TokenPrecedence = precendence;
                tkdef.IsRightAssoc = isRightAssoc;

            }
        }
        public void TestLR_CS2B()
        {

            define_term("id");
            //single chars operator
            define_single_char_token(" +-*/%^()[]{}<>;:,.!=?&|~");
            define_multi_chars_token("++ -- >> << && || ::");
            define_multi_chars_token("<= >=");
            define_multi_chars_token("+= -= *= /= %= ^= &= |= <<= >>= -> ??");
            define_multi_chars_token("== !=");
            define_keywords("is as");
            define_keywords("true false sizeof new typeof checked unchecked");
            //statement
            define_keywords("if else while do for foreach in try catch throw return yield");

            define_keywords("namespace using");
            define_keywords("class struct delegate interface void null int uint short ushort byte sbyte long ulong double float");
            define_keywords("bool enum char");
            define_keywords("object dynamic string");
            define_keywords("this base override virtual sealed");
            define_keywords("operator");
            define_keywords("private public internal protected");
            //--------------------------------------

            define_special_terminal("<t", "<"); //open generic 
            define_special_terminal(">t", ">"); //close generic   
            //--------------------------------------
            set_precedence("if", 2);
            set_precedence("else", 1);
            //--------------------------------------
            int prec_01 = 20;
            set_precedence(".", prec.Primary);
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
            string expression = "expression";
            throw new NotSupportedException();

            UserNTDefinition simple_E = null;
            //nt(expression).prec(100);
            //{
            //    int p = 20;
            //    //I("id");                   
            //    I<cs.simple_name>();
            //    this.currentSqPrecedence = p;
            //    //binary op          
            //    this.currentSqPrecedence = (int)MyCs.prec.Additive;
            //    I<cs.additive_expression>();
            //    this.currentSqPrecedence = p;
            //}

            ////NT<MyCs.simple_name>();
            ////NT<MyCs.argument_list>();
            //NT<cs.type_argument_list>();
            //NT<cs.type>();
            //--------------------------------------------------------------------------------- 
            //AutoPrepareLaterNTs();

            NTDefinition augmentedNT =
            PrepareUserGrammarForAnyLR(simple_E);// PrepareUserGrammarsForLL1(simple_E);

            //--------------------------------------------------------------------------------- 

            //---------------------------------------------------------------------------------
            LRParsingTable parsingTable = CreateLR1Table(augmentedNT);
            //parsingTable.LR1ToLALR = true;
            parsingTable.MakeParsingTable();
            //test input  

            //string teststr = "id()"; //? 
            //string teststr = "id(id)"; //? 
            //string teststr = "id(id,id)"; //? 

            //test ambiguous grammar (CS4 spec page 163)
            //string teststr = "id(id<id,id>(id))";//1.

            //string teststr = "id(id<id,id,id>(id))";//2.             
            //string teststr = "id(id<id,id>id)";//2.             
            //string teststr = "id<id>(id)";//3.             
            //string teststr = "id(id<id)";//4.  
            //string teststr = "id<id";//4.             
            //string teststr = "(id)<(id)";//4.       

            //string teststr = "id<id>+id";//1.//ok
            //string teststr = "(id<(id))";//1.//ok
            //string teststr = "id(id+id*id)";//1.//ok
            //string teststr = "id=id=id";
            string teststr = "id+id+id+id";

            // parsingTable.Parse(Lex(teststr).ToArray());
            var stepParser = LRParsing.CreateRunner(parsingTable);
            var finalNode = stepParser.Parse(Lex2(teststr));


        }
        public void TestLR_CS3B()
        {

            define_term("id");
            //single chars operator
            define_single_char_token(" +-*/%^()[]{}<>;:,.!=?&|~");
            define_multi_chars_token("++ -- >> << && || ::");
            define_multi_chars_token("<= >=");
            define_multi_chars_token("+= -= *= /= %= ^= &= |= <<= >>= -> ??");
            define_multi_chars_token("== !=");
            define_keywords("is as");
            define_keywords("true false sizeof new typeof checked unchecked");
            define_keywords("default");
            //statement
            define_keywords("if else while do for foreach in try catch throw return yield");
            define_keywords("ref out");
            define_keywords("namespace using");
            define_keywords("class struct delegate interface void null int uint short ushort byte sbyte long ulong double float decimal");
            define_keywords("bool enum char");
            define_keywords("object dynamic string");
            define_keywords("this base override virtual sealed");
            define_keywords("operator");
            define_keywords("private public internal protected");
            //--------------------------------------

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
            string expression = "expression";
            throw new NotSupportedException();
            //UserNTDefinition simple_E =
            //nt(expression);
            //{

            //    //I<MyCs.simple_name>();
            //    R<cs.parenthesized_expression>();
            //    int p = 20;
            //    //------------------------------------------------------------------------                
            //    //this.currentSqPrecedence = p;
            //    I<cs.invocation_expression>().prec(p--);
            //    //------------------------------------------------------------------------
            //    //member access expression 
            //    I<cs.member_access>().prec(p--);
            //    //------------------------------------------------------------------------
            //    this.currentSqPrecedence = p;
            //    //unary op                
            //    R<cs.unary_expression>().prec(p--);
            //    //------------------------------------------------------------------------
            //    this.currentSqPrecedence = p;
            //    //binary op          

            //    this.currentSqPrecedence = (int)MyCs.prec.Multiplicative;
            //    I<cs.multiplicative_expression>();

            //    this.currentSqPrecedence = (int)MyCs.prec.Additive;
            //    I<cs.additive_expression>();

            //    this.currentSqPrecedence = (int)MyCs.prec.RelationalAndTypeTesting;
            //    I<cs.relational_and_type_testing>();

            //    this.currentSqPrecedence = (int)MyCs.prec.Assignment;
            //    I<cs.assignment>().right(); //make it right assoc***
            //    this.currentSqPrecedence = p;
            //    //------------------------------------------------------------------------
            //}


            //NT<cs.type>();
            //NT<cs.argument_list>();
            //NT<cs.type_argument_list>();
            //NT<cs.expression_list>();
            //NT<cs.rank_specifiers>();
            //NT<cs.commas>();
            //NT<cs.dim_separators>();
            ////NT<MyCs.explicit_anonymous_funtion_parameter_list>();

            //NT<cs.constant_modifier>();
            //NT<cs.secondary_constraints>();
            //NT<cs.primary_constraint>();


            //AutoPrepareLaterNTs();

            //NTDefinition augmentedNT = PrepareUserGrammarForAnyLR(simple_E);


            ////---------------------------------------------------------------------------------
            //LRParsingTable parsingTable = CreateLALR1Table(augmentedNT);
            ////LRParsingTable parsingTable = CreateLALR1Table(augmentedNT);
            ////LRParsingTable parsingTable = CreateLR0Table(augmentedNT);
            ////parsingTable.LR1ToLALR = true; 
            //parsingTable.MakeParsingTable();
            //var symb_res = this.symResolutionInfo;

            ////test input   
            ////string teststr = "id()"; //? 
            ////string teststr = "id(id)"; //? 
            ////string teststr = "id(id,id)"; //? 

            ////test ambiguous grammar (CS4 spec page 163)
            ////string teststr = "id(id<id,id>(id))";//1.

            ////string teststr = "id(id<id,id,id>(id))";//2.             
            ////string teststr = "id(id<id,id>id)";//2.             
            ////string teststr = "id<id>(id)";//3.             
            ////string teststr = "id(id<id)";//4.  
            ////string teststr = "id<id";//4.             
            ////string teststr = "(id)<(id)";//4.       

            //string teststr = "id<id>+id";//1.//ok
            ////string teststr = "(id<(id))";//1.//ok
            ////string teststr = "(id)";//1.//ok
            ////string teststr = "id(id+id*id)";//1.//ok
            ////string teststr = "id(id,id)";//1.//ok
            ////string teststr = "id=id=id";
            ////string teststr = "id+id+id+id";

            //var stepParser = LRParsing.CreateStepRunner(parsingTable);
            //var finalNode = stepParser.Parse(Lex2(teststr));
            //if (finalNode == null)
            //{

            //}

            //LRParser parser = parsingTable.Parse(Lex(teststr).ToArray());



        }

    }
}