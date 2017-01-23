//MIT 2015-2017, ParserApprentice 
using System;
using System.Collections.Generic;
using System.Text;
using Parser.ParserKit;

namespace Parser.MyCs
{

    public class CsParserHelper
    {
        ParserManager parserMan;
        TokenInfoCollection tkInfoCollection;
        SubParser startSubParser;
        MyCsLexer myCsLexer;

        public CsParserHelper()
        {
            //----------------------------------------------------
            //internal steps
            //---------------------------------------------------- 

            //1. build token info collection 
            CsTokenInfoCollection csTkInfo = new CsTokenInfoCollection();
            tkInfoCollection = csTkInfo.GetSnapedTokenInfoCollection();

            //2. parser manager
            parserMan = new ParserManager(tkInfoCollection);
            parserMan.UseCache = true;

            //3. setup sub-parsers
            parserMan.Setup(new FormalParameterListParser());
            NamespaceParser nsParser = parserMan.Setup(new NamespaceParser() { GetWalker = p => ((CsParseNodeHolder)p).NamespaceWalker });
            parserMan.Setup(new ExpressionParser() { GetWalker = p => ((CsParseNodeHolder)p).ExpressionWalker });
            parserMan.Setup(new TypeArgumentListParser());
            parserMan.Setup(new TypeParser());
            parserMan.Setup(new StatementParser() { GetWalker = p => ((CsParseNodeHolder)p).StatementBuilder });
            parserMan.Setup(new StructDeclParser());
            parserMan.Setup(new ClassDeclParser() { GetWalker = p => ((CsParseNodeHolder)p).ClassWalker });
            parserMan.Setup(new MethodDeclParser() { GetWalker = p => ((CsParseNodeHolder)p).MethodWalker });
            parserMan.Setup(new PropertyDeclParser());
            parserMan.Setup(new FieldDeclParser());
            parserMan.Setup(new AttributesParser());
            parserMan.Setup(new ArrayTypeParser());
            parserMan.Setup(new ArgumentListParser());
            parserMan.Setup(new ObjectOrCollectionInitializerParser());
            parserMan.Setup(new TypeParameterConstraintsClausesParser());
            //4. prepare switch link
            parserMan.PrepareSwitchLink();
            //5. setup start parser
            startSubParser = nsParser;
            //6. build lexer
            myCsLexer = new Parser.MyCs.MyCsLexer(tkInfoCollection);
        }

        public ParseNode Parse(char[] codeBuffer)
        {
            //7.
            //create token stream & reader
            CodeText sourceText = new CodeText(codeBuffer);
            MyCsTokenStream myCsTkStream = new MyCsTokenStream(sourceText, myCsLexer);
            myCsTkStream.Lex();//partial lex 
            myCsTkStream.SetCurrentPosition(-1);//reset to first position
            TokenStreamReader reader = new TokenStreamReader(myCsTkStream);
            reader.ReadNext();

            //8.
            startSubParser.Parse(ParseNodeHolderFactory.CreateCsParseNodeHolderForAst(), reader);
            return startSubParser.FinalNode;
        }

    }
}