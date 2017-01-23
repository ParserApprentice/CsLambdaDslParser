//MIT 2015-2017, ParserApprentice 
using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{
    public class SymbolResolutionInfo
    {
        Dictionary<ISymbolDefinition, int> symbolPrecedences = new Dictionary<ISymbolDefinition, int>();
        Dictionary<ISymbolDefinition, int> rightAssocs = new Dictionary<ISymbolDefinition, int>();

        List<string> ntDefinitionResolveMsgs = new List<string>();
        List<string> parseTableResolveMsgs = new List<string>();

        Dictionary<int, LR.RRConflictOnRow> rrConflictFounds = new Dictionary<int, LR.RRConflictOnRow>();
        Dictionary<string, NTDefinition> unknownNts = new Dictionary<string, NTDefinition>();

        internal SymbolResolutionInfo()
        {
        }
        public NTDefinition CreateUnknownNT(string ntName)
        {
            NTDefinition found;
            if (!unknownNts.TryGetValue(ntName, out found))
            {
                found = new NTDefinition(ntName, NTDefintionKind.UnknownNT);
                unknownNts.Add(ntName, found);

            }
            return found;
        }
        public void AddSymbolPrecedenceAndAssoc(ISymbolDefinition symbol, int precedenceLevel, bool isRightAssoc)
        {
            this.symbolPrecedences.Add(symbol, precedenceLevel);
            if (isRightAssoc)
            {
                rightAssocs.Add(symbol, 0);
            }
        }
        public bool IsRightAssoc(ISymbolDefinition symbol)
        {
            return rightAssocs.ContainsKey(symbol);
        }
        public int GetSymbolPrecedence(ISymbolDefinition symbol)
        {

            int output;
            if (symbolPrecedences.TryGetValue(symbol, out output))
            {
                return output;
            }

            return 0;
        }
        public void AddResolveMessage(string msg)
        {
            this.parseTableResolveMsgs.Add(msg);
        }
        public void AddNtSequenceResolveMessage(string msg)
        {
            this.ntDefinitionResolveMsgs.Add(msg);
        }
        internal LR.RRConflictOnRow GetRRConflictFoundOrCreateIfNotExit(int rowNumber)
        {
            LR.RRConflictOnRow found;
            if (!rrConflictFounds.TryGetValue(rowNumber, out found))
            {
                found = new LR.RRConflictOnRow(rowNumber);
                this.rrConflictFounds.Add(rowNumber, found);
            }
            return found;
        }



        internal void ClearPreviousRRConflictResolution()
        {
            this.rrConflictFounds.Clear();
        }
        internal bool NeedToDoRRConflictResolution
        {
            get
            {
                return this.rrConflictFounds.Count > 0;
            }
        }


    }




}