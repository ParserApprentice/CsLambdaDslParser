//MIT, 2015-2017, ParserApprentice 
 
using System.Collections.Generic;

namespace Parser.ParserKit.LR
{
    partial class LRParsingTable
    {
        //switch info 
        internal List<SubParsers.SwitchDetail> swDetailRecords = new List<SubParsers.SwitchDetail>();
        internal void MakeParsingTableFromCache(SubParsers.ParserDataBinaryCache tableData)
        {
            //caching features

            this.swDetailRecords = tableData.swDetails;
            //fill data to token table
            List<List<LRItemTodo>> todoListForTks = tableData.todoListForTks;
            int j = todoListForTks.Count;
            for (int i = 0; i < j; ++i)
            {
                this.tokenTable.columns[i].cells = todoListForTks[i];
            }
            this.tokenTable.UnsafeRefreshRowCount(tableData.TableRowCount);

            //fill data to  nt table
            List<List<LRItemTodo>> todoListForNts = tableData.todoListForNts;
            j = todoListForNts.Count;
            for (int i = 0; i < j; ++i)
            {
                this.ntTable.columns[i].cells = todoListForNts[i];
            }
            this.ntTable.UnsafeRefreshRowCount(tableData.TableRowCount);
        }
        //--------------------------------

        protected virtual void OnInit()
        {
            //switch info 
            swDetailRecords.Add(null);//empty record
        }
        internal SubParsers.SwitchDetail CreateNewSwitchDetail()
        {
            SubParsers.SwitchDetail swDetail = new SubParsers.SwitchDetail(this.swDetailRecords.Count);
            this.swDetailRecords.Add(swDetail);
            return swDetail;
        }
        internal SubParsers.SwitchDetail GetSwitchDetail(int recordNumber)
        {
            return swDetailRecords[recordNumber];
        }

        void AddLRItemSwitchToTask(
           CurrentRowHolder row,
           ISymbolDefinition jumpOverSymbol,
           LRItemSet nextItemSet,
           SymbolResolutionInfo symResolutionInfo)
        {

            LRItemTodo existingTask = row.GetTodo(jumpOverSymbol);
            if (existingTask.IsEmpty())
            {
                //nextStateNumber => state after switch back
                //next name index 

                SubParsers.SwitchDetail swDetail = CreateNewSwitchDetail();
                swDetail.AddChoice(jumpOverSymbol, nextItemSet.ItemSetNumber);
                var todo = LRItemTodo.CreateUnresolvedSwitch(swDetail.Number);//this.CreateUnresolvedSwitchRecord(targetNameIndex, nextItemSet.ItemSetNumber));

                this.FillTodoForNt(row.RowNumber, todo);
                this.FillTodoForToken(row.RowNumber, todo);//only for token 

            }
            else
            {
                //conflict found
                switch (existingTask.ItemKind)
                {
                    case LRItemTodoKind.UnresolvedSwitch:
                        {
                            //resolve switch-switch conflict 
                            //create multiple target switch instruction
                            //add additional to switch table
                            SubParsers.SwitchDetail swDetail = this.GetSwitchDetail(existingTask.SwitchRecordNumber);
                            swDetail.AddChoice(jumpOverSymbol, nextItemSet.ItemSetNumber);


                        } break;
                    default:
                        { //report conflict
                            symResolutionInfo.AddResolveMessage(
                            "conflict_sw " + row.ToString());
                        } break;
                }
            }

        }

    }
}