//MIT 2015-2017, ParserApprentice  

using System;
using System.Text;
using System.Collections.Generic;

namespace Parser.ParserKit
{
    public class ParserSwitchContext
    {
        public readonly Token eof = new Token(TokenDefinition._eof);
        //---------------------------------------   
        //for LR
        ParserReporterImpl reporter;
        internal ParserSwitchContext(ParseNodeHolder holder, TokenStreamReader reader)
        {
            this.reporter = new ParserReporterImpl(holder);
            Reader = reader;
        }
        public ParseNodeHolder Holder
        {
            get; set;
        }
        public TokenStreamReader Reader
        {
            get;
            private set;
        }

        public ParseNode SwitchBackParseNode
        {
            get { return this.SwitchBackParseResult.parseNode; }

        }
        public ParseResult SwitchBackParseResult
        {
            get;
            set;
        }

#if DEBUG
        public string dbugFromSubParserName
        {
            get;
            set;
        }
#endif
        public Token CurrentToken
        {
            get { return Reader.CurrentToken; }
        }

        public SubParsers.SwitchDetail SwitchDetail
        {
            get;
            set;
        }
        public int SwitchBackState
        {
            get;
            set;
        }
       
        internal int WaitingParserCount { get; private set; }

        public void BeginSwitch()
        {
            WaitingParserCount++;
            //prepare switching context
        }
        public void EndSwitch()
        {
            WaitingParserCount--;
        }
        internal ParserReporter GetReporter()
        {
            return reporter;
        }
        internal LR.LRParsingContext GetLRParsingContext()
        {
            if (lrParsingContextStack.Count > 0)
            {
                return lrParsingContextStack.Pop();
            }
            else
            {
                return new LR.LRParsingContext();
            }
        }
        internal void RelaseLRParsingContext(LR.LRParsingContext context)
        {
            context.Clear();
            lrParsingContextStack.Push(context);
            this.CurrentLRParsingContext = null;
        }

        internal LR.LRParsingContext CurrentLRParsingContext
        {
            get;
            set;
        }

        Stack<LR.LRParsingContext> lrParsingContextStack = new Stack<LR.LRParsingContext>();
        class ParserReporterImpl : ParserReporter
        {
            public ParserReporterImpl(ParseNodeHolder holder)
                : base(holder)
            {
            }
        }
    }
}