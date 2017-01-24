//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit.LR
{

    public delegate NTDefinition RRConflictHandler(
       TokenDefinition jumpOver,
       NTDefinition existing,
       NTDefinition candidate,
       SymbolResolutionInfo resInfo);


    //conflict resolution helper ***
    class RRConflictOnRow
    {
        Dictionary<TokenDefinition, Dictionary<int, SymbolSequence>> tokenWithSeqChoices = new Dictionary<TokenDefinition, Dictionary<int, SymbolSequence>>();
        public RRConflictOnRow(int rowNumber)
        {
            this.RowNumber = rowNumber;
        }
        public int RowNumber
        {
            get;
            private set;
        }
        public void AddChoice(TokenDefinition tkdef, SymbolSequence ss)
        {
            Dictionary<int, SymbolSequence> found;
            if (!tokenWithSeqChoices.TryGetValue(tkdef, out found))
            {

                found = new Dictionary<int, SymbolSequence>();
                this.tokenWithSeqChoices.Add(tkdef, found);
            }
            if (!found.ContainsKey(ss.TotalSeqNumber))
            {

                found.Add(ss.TotalSeqNumber, ss);
            }
        }

        public Dictionary<int, SymbolSequence> GetSeqChoices(TokenDefinition tk)
        {
            Dictionary<int, SymbolSequence> found;
            tokenWithSeqChoices.TryGetValue(tk, out found);
            return found;
        }
#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append(this.RowNumber.ToString());
            stbuilder.Append(": jump_over ");
            foreach (var kp in this.tokenWithSeqChoices)
            {
                stbuilder.Append(kp.Key.PresentationString);
                //value= possible seqs
                stbuilder.Append("  ,");//possible symbol sq
                foreach (var kp2 in kp.Value)
                {
                    stbuilder.Append(kp2.Value.ToString());
                    stbuilder.Append(' ');
                }

            }
            return stbuilder.ToString();

        }
#endif

    }


}