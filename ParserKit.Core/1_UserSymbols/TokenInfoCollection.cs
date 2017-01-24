//MIT, 2015-2017, ParserApprentice
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.ParserKit
{
    public class TokenInfoCollection
    {
        Dictionary<string, TokenDefinition> keywords = new Dictionary<string, TokenDefinition>();
        Dictionary<char, List<TokenDefinition>> multiCharTerminals = new Dictionary<char, List<TokenDefinition>>();
        Dictionary<char, TokenDefinition> singleCharTerminals = new Dictionary<char, TokenDefinition>();
        Dictionary<string, TokenDefinition> allTokens = new Dictionary<string, TokenDefinition>();


        public TokenInfoCollection()
        {
            AddSpecialToken(TokenDefinition._identifier);
            AddSpecialToken(TokenDefinition._literalString);
            AddSpecialToken(TokenDefinition._literalInteger);
            AddSpecialToken(TokenDefinition._switchToken);
        }
        public TokenInfoCollectionState CollectionState
        {
            get;
            set;
        }
        public TokenDefinition AddTokenKeyword(string grammarSymbolString, string presentationString)
        {
            if (this.CollectionState == TokenInfoCollectionState.Closed)
            {
                throw new NotSupportedException();
            }
            TokenDefinition tokenInfo = new TokenDefinition(grammarSymbolString, presentationString, BasicTokenKind.Keyword);
            this.allTokens.Add(tokenInfo.GrammarSymbolString, tokenInfo);
            this.keywords.Add(tokenInfo.GrammarSymbolString, tokenInfo);
            return tokenInfo;
        }
        public TokenDefinition AddContextualKeyword(string grammarSymbolString, string presentationString)
        {
            if (this.CollectionState == TokenInfoCollectionState.Closed)
            {
                throw new NotSupportedException();
            }
            TokenDefinition tokenInfo = new TokenDefinition(grammarSymbolString, presentationString, BasicTokenKind.ContextualKeyword);
            this.allTokens.Add(tokenInfo.GrammarSymbolString, tokenInfo);
            this.keywords.Add(tokenInfo.GrammarSymbolString, tokenInfo);
            return tokenInfo;
        }
        public TokenDefinition AddNonkeywordToken(string grammarSymbolString, string presentationString)
        {
            if (this.CollectionState == TokenInfoCollectionState.Closed)
            {
                throw new NotSupportedException();
            }
            if (grammarSymbolString.StartsWith("'"))
            {
                return null;
            }

            TokenDefinition tokenInfo = new TokenDefinition(grammarSymbolString, presentationString, BasicTokenKind.Terminal);
            this.allTokens.Add(grammarSymbolString, tokenInfo);
            
            return tokenInfo;
        }
        public TokenDefinition AddNonkeywordToken(string presentationString)
        {

            

            if (this.CollectionState == TokenInfoCollectionState.Closed)
            {
                throw new NotSupportedException();
            }
            if (presentationString.StartsWith("'"))
            {

                return null;
            }
            TokenDefinition tokenInfo = new TokenDefinition(presentationString, presentationString, BasicTokenKind.Terminal);
            this.allTokens.Add(presentationString, tokenInfo);
            //------------------------------------------------
            if (tokenInfo.PresentationStringLength > 1)
            {
                List<TokenDefinition> existingList;
                if (!multiCharTerminals.TryGetValue(tokenInfo.PresentationString[0], out existingList))
                {
                    existingList = new List<TokenDefinition>();
                    this.multiCharTerminals.Add(tokenInfo.PresentationString[0], existingList);
                }
                existingList.Add(tokenInfo);
            }
            else
            {

                singleCharTerminals.Add(tokenInfo.PresentationString[0], tokenInfo);
            }
            //------------------------------------------------
            return tokenInfo;
        }
        public char[] GetSingleTerminals()
        {
            char[] singleTerms = new char[singleCharTerminals.Count];
            int i = 0;
            foreach (char c in singleCharTerminals.Keys)
            {
                singleTerms[i++] = c;
            }
            return singleTerms;
        }
        void AddSpecialToken(TokenDefinition tokenInfo)
        {
            this.allTokens.Add(tokenInfo.GrammarSymbolString, tokenInfo);
        }
        public TokenDefinition GetTokenInfo(string grammarSymbolString)
        {
            TokenDefinition found;
            this.allTokens.TryGetValue(grammarSymbolString, out found);
            return found;
        }

        public bool NeedAdvancePosition(char c)
        {
            return multiCharTerminals.ContainsKey(c);
        }
        public bool IsInNonkeywordTokenGroup(char c)
        {
            return this.singleCharTerminals.ContainsKey(c);
        }
        public bool IsInMulticharNonKeywordGroup(string toknstr)
        {
            List<TokenDefinition> list;
            if (multiCharTerminals.TryGetValue(toknstr[0], out list))
            {
                for (int i = list.Count - 1; i > -1; --i)
                {
                    if (list[i].PresentationString == toknstr)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //----------------------------------------------------------

        TokenDefinition[] allTokenInfoArray;
        public void SnapAllTokenInfo()
        {

            allTokenInfoArray = new TokenDefinition[this.allTokens.Count + 1];
            allTokenInfoArray[0] = TokenDefinition._eof;
            int i = 1;
            foreach (TokenDefinition tkinfo in this.allTokens.Values)
            {
                allTokenInfoArray[i] = tkinfo;
                tkinfo.TokenInfoNumber = i++;
            }
            this.CollectionState = TokenInfoCollectionState.Closed;
        }
        public TokenDefinition GetTokenInfoByIndex(int index)
        {
            return this.allTokenInfoArray[index];
        }
        public int SnapAllTokenCount
        {
            get
            {
                return this.allTokenInfoArray.Length;
            }
        }

        //internal TokenDefinition[] GetSnapTokenArray() { return this.allTokenInfoArray; }
        internal IEnumerable<TokenDefinition> GetTokenDefinitionIter()
        {
            foreach (var tk in this.allTokens.Values)
            {
                yield return tk;
            }
        }

    }



    public enum TokenInfoCollectionState
    {
        Open,
        Closed
    }

}