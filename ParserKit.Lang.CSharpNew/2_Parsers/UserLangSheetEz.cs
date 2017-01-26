//MIT, 2015-2017, ParserApprentice

using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parser.ParserKit
{


    /// <summary>
    /// operator precedence
    /// </summary>
    public enum prec
    {
        //low
        Unknown,
        Lambda,
        Assignment,
        Conditional,
        NullCoalescing,
        ConditionOR,
        ConditionAND,
        LogicalOR,
        LogicalXOR,
        LogicalAND,
        Equality,
        RelationalAndTypeTesting,
        Shift,
        Additive,
        Multiplicative,
        Unary,
        Primary
    }

    public struct UserSequenceHelper
    {
        UserSymbolSequence seq;
        public UserSequenceHelper(UserSymbolSequence seq)
        {
            this.seq = seq;
        }

        public UserSequenceHelper Rd(SeqReductionDel del)
        {
            seq.SetParserReductionNotifyDel(del);
            return this;
        }
        public UserSequenceHelper Sh(ParserNotifyDel parserNotifyDel)
        {
            seq.SetParserNotifyDel(ParseEventKind.Shift, parserNotifyDel);
            return this;
        }
        public UserSequenceHelper SR(ParserNotifyDel shiftDel, SeqReductionDel reduceDel)
        {
            seq.SetParserNotifyDel(ParseEventKind.Shift, shiftDel);
            seq.SetParserReductionNotifyDel(reduceDel);
            return this;
        }

    }
   
    
    public abstract class UserGrammarSheetEz : UserLangGrammarSheet
    {
        string currentSqName;
        protected int currentSqPrecedence;
        protected UserNTDefinition currentUserNT = null;
        protected UserSymbolSequence currentSq = null;
        public UserGrammarSheetEz()
        { 
        }
        public TokenDefinition GetTokenDefinition(string grammarSymbolInfo)
        {
            return tokenInfoCollection.GetTokenInfo(grammarSymbolInfo);
        }
        public void ClearAllTks()
        {
            this.tokenInfoCollection = new TokenInfoCollection();
        }
        public void define_term(params string[] terms)
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
        public void define_special_token(string specialTkName, string presentationString)
        { 
            this.tokenInfoCollection.AddNonkeywordToken(specialTkName, presentationString);
        } 
       
        
        /// <summary>
        /// create terminal list from space_sep_terms definition
        /// </summary>
        /// <param name="terminal_list"></param>
        /// <returns></returns>
        protected List<TokenDefinition> GetTerminals(string space_sep_terms)
        {
            string[] terms = space_sep_terms.Split(' ');
            int j = terms.Length;
            List<TokenDefinition> foundList = new List<TokenDefinition>(j);
            for (int i = 0; i < j; ++i)
            {
                foundList.Add(GetTokenInfo(terms[i]));
            }
            return foundList;
        }
        protected TokenDefinition GetTokenInfo(string grammarSymbolString)
        {
            return this.tokenInfoCollection.GetTokenInfo(grammarSymbolString);
        }
        /// <summary>
        /// create new nt, and set it to current nt
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UserNTDefinition nt(string name)
        {
            this.currentSqName = null;
            this.currentSqPrecedence = 0;
            this.currentUserNT = new UserNTDefinition(name);
            this.allUserNTs.AddNT(this.currentUserNT);
            return this.currentUserNT;
        }
      
        /// <summary>
        /// create user symbol from specific string
        /// </summary>
        /// <param name="fromstr"></param>
        /// <returns></returns>
        UserExpectedSymbol CreateUserSymbolFromStringPattern(string fromstr)
        {


            string symbolStr = fromstr;
            bool isOptional = false;
            if (fromstr.Length > 1)
            {
                int colon_index = fromstr.IndexOf(':');
                if (colon_index > -1)
                {

                    //this is nonterminal -> wait for resolving
                    string[] details = fromstr.Split(':');
                    symbolStr = details[0];
                    int j = details.Length;
                    for (int i = 1; i < j; ++i)
                    {
                        switch (details[i])
                        {
                            case "opt":
                                {
                                    isOptional = true;
                                } break;
                            default:
                                {

                                } break;
                        }
                    }
                }
            }

            //get defined terminal 
            TokenDefinition terminal = GetTokenInfo(symbolStr);
            if (terminal != null)
            {

                if (isOptional)
                {
                    //?
                }
                return new UserExpectedSymbol(terminal, isOptional);
            }
            else
            {
                //unresolve nt
                return new UserExpectedSymbol(symbolStr, isOptional);
            }
        }

        public void N(string setCurrentSqName)
        {
            this.currentSqName = setCurrentSqName;
            this.currentSqPrecedence = 0;
        }
        public void N(string setCurrentSqName, int precedence)
        {
            this.currentSqName = setCurrentSqName;
            this.currentSqPrecedence = precedence;
        }
        /// <summary>
        /// create seq
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public UserSequenceHelper I(params string[] words_str)
        {
            int j = words_str.Length;
            UserExpectedSymbol[] words = new UserExpectedSymbol[j];
            for (int i = j - 1; i >= 0; --i)
            {
                words[i] = CreateUserSymbolFromStringPattern(words_str[i]);
            }

            UserSymbolSequence sq = new UserSymbolSequence(this.currentUserNT);
            sq.SeqName = this.currentSqName;
            if (this.currentSqPrecedence > 0)
            {
                sq.Precedence = this.currentSqPrecedence;
            }
            for (int i = 0; i < j; ++i)
            {
                sq.AppendLast(words[i]);
            }
            this.currentSq = sq;
            this.currentUserNT.AddSymbolSequence(sq);

            return new UserSequenceHelper(sq);
        }
        /// <summary>
        /// assign on shift notification
        /// </summary>
        /// <param name="del"></param>
        public void Sh(ParserNotifyDel del)
        {
            if (this.currentSq != null)
            {
                this.currentSq.SetParserNotifyDel(ParseEventKind.Shift, del);
            }
        }
        /// <summary>
        /// assign on reduce notification
        /// </summary>
        /// <param name="del"></param>
        public void Rd(SeqReductionDel del)
        {
            if (this.currentSq != null)
            {

                this.currentSq.SetParserReductionNotifyDel(del);
            }
        }
        public UserExpectedSymbol oneof(string space_sep_str)
        {
            return oneof(0, space_sep_str.Split(' '));
        }
        public UserExpectedSymbol oneof(int prec, params string[] tokenlist)
        {

            //oneof is auto-generated nt
            UserNTDefinition autoGenOneOf = new UserNTDefinition("one_of_" + GetNewAutoNtSuffixNumber());
            autoGenOneOf.IsAutoGen = true;
            autoGenOneOf.NTPrecedence = prec;
            this.allUserNTs.AddNT(autoGenOneOf);

            int j = tokenlist.Length;
            for (int i = 0; i < j; ++i)
            {
                UserSymbolSequence sq = new UserSymbolSequence(autoGenOneOf, CreateUserSymbolFromStringPattern(tokenlist[i]));
                sq.Precedence = prec;
                autoGenOneOf.AddSymbolSequence(sq);
            }
            return new UserExpectedSymbol(autoGenOneOf);
        }
        public UserExpectedSymbol itemlist(string itemName, string sep, bool isOptional)
        {

            string list_name = "list_of_" + itemName + GetNewAutoListOfSuffixNumber();

            UserNTDefinition autoGenOneOf = new UserNTDefinition(list_name);
            autoGenOneOf.IsAutoGen = true;
            this.allUserNTs.AddNT(autoGenOneOf);

            //2 sequences
            UserSymbolSequence sq1 = new UserSymbolSequence(autoGenOneOf, CreateUserSymbolFromStringPattern(itemName));
            autoGenOneOf.AddSymbolSequence(sq1);
            UserSymbolSequence sq2 = new UserSymbolSequence(autoGenOneOf, CreateUserSymbolFromStringPattern(itemName));
            autoGenOneOf.AddSymbolSequence(sq2);

            sq2.AppendLast(CreateUserSymbolFromStringPattern(sep));
            sq2.AppendLast(CreateUserSymbolFromStringPattern(list_name));

            return new UserExpectedSymbol(autoGenOneOf, isOptional);
        }


        int autoNTSuffixNum = 0;
        int autoListOfSuffixNum = 0;
        int GetNewAutoNtSuffixNumber()
        {
            return autoNTSuffixNum++;
        }
        int GetNewAutoListOfSuffixNumber()
        {
            return autoListOfSuffixNum++;
        }

        [DebuggerHidden]
        [DebuggerStepThrough]
        [DebuggerStepperBoundary]
        static string br()
        {
            Debugger.Break();
            return null;
        }
        [DebuggerHidden]
        public void doc(int page)
        {
        }


    }




}