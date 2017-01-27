//MIT, 2015-2017, ParserApprentice
 
namespace Parser.ParserKit
{
    public delegate void ParserNotifyDel(ParseNodeHolder r);
    public delegate void SeqReductionDel(ParseNodeHolder r);

    public delegate T GetWalkerDel<T>(ParseNodeHolder h);
   
     
    public delegate ParserKit.SubParsers.ReductionDef BuilderDel3<T>(T builder);
}