//MIT, 2015-2017, ParserApprentice

using Parser.ParserKit.SubParsers;

namespace Parser.ParserKit.SubParsers
{
    public delegate object UserExpectedSymbolShift(ParseNodeHolder r);

    //public delegate void ReductionFillSubItem(ParseNodeHolder n); //for map with 
    // public delegate void SeqReductionBreakable(ParseNodeHolder n); //for SR(),lambda for attach with syntax
    // public delegate ReductionFillSubItem ReductionFillSubItemBreakable(ParseNodeHolder n);

    public delegate void ReductionDef(); //for map with 
}

namespace Parser.ParserKit
{
    [System.Diagnostics.DebuggerNonUserCode]
    [System.Diagnostics.DebuggerStepThrough]
    public class ReductionMonitor
    {

        bool isSubItemBreakable;
        //ReductionFillSubItemBreakable reductionDel;
        //ReductionFillSubItem fillSubItem;
        int cacheHolderId = 0;
        //internal ReductionMonitor(ReductionFillSubItemBreakable reductionFillSubItemBreakable)
        //{
        //    this.reductionDel = reductionFillSubItemBreakable;
        //    isSubItemBreakable = true;
        //    //CheckIfTargetMethodIsLambda(reductionFillSubItemBreakable.Method);
        //}
        //internal ReductionMonitor(ReductionFillSubItem fillSubItem)
        //{
        //    this.fillSubItem = fillSubItem;
        //    //CheckIfTargetMethodIsLambda(reductionFillSubItemBreakable.Method);
        //}
        //public void NotifyReduction(ParseNodeHolder holder)
        //{
        //    if (isSubItemBreakable)
        //    {

        //    }
        //    else
        //    {
        //        if (fillSubItem != null)
        //        {
        //            fillSubItem(holder);
        //        }
        //        else
        //        {

        //        }
        //    }
        //}
        void CheckIfTargetMethodIsLambda(System.Reflection.MethodInfo met)
        {
            //targetMethodMaybeLambda = met.Name.Contains("<");
            //#if DEBUG
            //            if (!targetMethodMaybeLambda)
            //            {
            //            }
            //#endif
        }
    }
}