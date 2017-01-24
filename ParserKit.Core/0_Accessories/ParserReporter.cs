//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{
    public abstract class AstWalker
    {
        public ParseNodeHolder OwnerHolder { get; set; }
        public override string ToString()
        {
            if (OwnerHolder != null && OwnerHolder.Reporter != null)
            {
                return OwnerHolder.Reporter.ToString();
            }
            else
            {
                return "";
            }
        }
    }

    public enum ParseResultKind
    {
        Error,
        OK,
    }

    public struct ParseResult
    {
        public readonly ParseNode parseNode;
        public readonly ParseResultKind resultKind;
        public readonly int parsedIndex;
        public ParseResult(ParseNode parseNode, ParseResultKind resultKind, int parsedIndex)
        {
            this.parseNode = parseNode;
            this.resultKind = resultKind;
            this.parsedIndex = parsedIndex;
        }
    }

    /// <summary>
    /// parser report 
    /// </summary> 
    public abstract class ParserReporter
    {

        //ParseNode result;
        ParseNodeHolder holder;

        public ParserReporter(ParseNodeHolder holder)
        {
            this.holder = holder;
        }

        internal void SetParseNodeStack(LR.ParseNodeStack stack)
        {
            holder.ParseNodeStack = stack;
        }
        public override string ToString()
        {

            TokenStream allTks = TokenStreamReader.OriginalTokens;
            int pos = TokenStreamReader.CurrentReadIndex;

            StringBuilder stbuilder = new StringBuilder();
            //stbuilder.Append(CurrentToken.ToString() + " $ "); 
            if (pos >= 3 && pos < allTks.Length - 3)
            {
                for (int i = pos - 3; i < pos + 3; ++i)
                {
                    if (i == pos)
                    {
                        stbuilder.Append('@');
                    }
                    stbuilder.Append(allTks.GetToken(i).ToString());
                    stbuilder.Append(' ');
                }
            }

            else if (pos >= allTks.Length - 3)
            {
                stbuilder.Append('@');
                for (int i = pos; i < allTks.Length; ++i)
                {

                    stbuilder.Append(allTks.GetToken(i).ToString());
                    stbuilder.Append(' ');
                }
            }
            else if (pos < 3)
            {
                if (allTks.Length < 3)
                {
                    for (int i = 0; i < allTks.Length; ++i)
                    {
                        if (i == pos)
                        {
                            stbuilder.Append('@');
                        }
                        stbuilder.Append(allTks.GetToken(i).ToString());
                        stbuilder.Append(' ');
                    }
                }
                else
                {
                    for (int i = 0; i < pos + 3; ++i)
                    {
                        if (i == pos)
                        {
                            stbuilder.Append('@');
                        }
                        stbuilder.Append(allTks.GetToken(i).ToString());
                        stbuilder.Append(' ');
                    }
                }
            }
            else
            {
                stbuilder.Append(allTks.GetToken(pos).ToString());
            }

            return stbuilder.ToString();
        }


        //public ParseNode Result
        //{
        //    get { return result; }
        //    set
        //    {
        //        this.result = value;
        //        holder.LatestParseNode = value;
        //    }
        //}
        public ParseNodeHolder ParseNodeHolder
        {
            get { return holder; }

        }
        internal TokenStreamReader TokenStreamReader { get; set; }
        public Token CurrentToken { get; internal set; }

    }
}