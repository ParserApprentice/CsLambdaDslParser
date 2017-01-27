//MIT, 2015-2017, ParserApprentice
using System; 
using System.Collections.Generic;

namespace Parser.ParserKit
{

    class FirstTokenInfoCollectionDic
    {

        Dictionary<TokenDefinition, List<SymbolSequence>> firstTokensDic = new Dictionary<TokenDefinition, List<SymbolSequence>>();
        List<TokenDefinition> foundConflictList;
        NTDefinition ownerNT;
        TokenDefinition[] cachedFirstTokens;

#if DEBUG
        static int dbugTotalId = 0;
        public int dbugId = dbugTotalId++;
#endif
        public FirstTokenInfoCollectionDic(NTDefinition ownerNT)
        {
            this.ownerNT = ownerNT;
        }
        public NTDefinition OwnerNT
        {
            get
            {
                return this.ownerNT;
            }
        }
        public void SnapTokenDic()
        {

            this.IsChanged = false;
        }
        public void FillDataWithAnotherIfNotExist(SymbolSequence sq, FirstTokenInfoCollectionDic anotherNTSymbolCollectionDic)
        {
            if (this == anotherNTSymbolCollectionDic)
            {
                throw new NotSupportedException();
            }

            if (anotherNTSymbolCollectionDic.HasConflicts)
            {

            }

            foreach (var kp in anotherNTSymbolCollectionDic.firstTokensDic)
            {
                this.AddSequence(kp.Key, sq);
            }
        }
        public void Clear()
        {
            this.firstTokensDic.Clear();
            if (foundConflictList != null)
            {
                foundConflictList.Clear();
            }
        }
        public bool HasConflicts
        {
            get
            {
                return foundConflictList != null;
            }
        }
        public TokenDefinition[] GetAllFirstTokens()
        {

            if (cachedFirstTokens != null)
            {
                return cachedFirstTokens;
            }

            TokenDefinition[] ess = new TokenDefinition[firstTokensDic.Count];
            int i = 0;
            foreach (var kp in firstTokensDic.Keys)
            {
                ess[i] = kp;
                i++;
            }
            return this.cachedFirstTokens = ess;

        }

        public bool AddTokenIfNotExist(TokenDefinition tkinfo)
        {
            if (!firstTokensDic.ContainsKey(tkinfo))
            {
                firstTokensDic.Add(tkinfo, new List<SymbolSequence>());
                this.IsChanged = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsChanged
        {
            get;
            private set;

        }

        public void AddSequence(TokenDefinition startTokenInfo, SymbolSequence s)
        {
            List<SymbolSequence> existingList;
            if (!firstTokensDic.TryGetValue(startTokenInfo, out existingList))
            {

                existingList = new List<SymbolSequence>();
                existingList.Add(s);
                firstTokensDic.Add(startTokenInfo, existingList);
                this.IsChanged = true;
            }
            else
            {

                //at here : there are some conflicts in this collection
                //a start symbol  has 2 sequence
                //check if s is not in this existingList
                foreach (SymbolSequence sq in existingList)
                {
                    if (sq == s)
                    {

                        return;
                    }
                }

                if (foundConflictList == null)
                {
                    foundConflictList = new List<TokenDefinition>();
                }
                existingList.Add(s);
                //found conflicts
                foundConflictList.Add(startTokenInfo);
            }
        }
        /// <summary>
        /// get sq from start token , we may find more than 1 sq , in case of conflict         
        /// </summary>
        /// <param name="startToken"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public List<SymbolSequence> TryGetSequence(TokenDefinition tkinfo)
        {
            List<SymbolSequence> foundList;
            firstTokensDic.TryGetValue(tkinfo, out foundList);
            return foundList;
        }

    }
}