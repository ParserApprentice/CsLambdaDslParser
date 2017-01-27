//MIT, 2015-2017, ParserApprentice
using System;
using System.Diagnostics;
namespace ParserKit.Internal
{
    public class InjAttribute : Attribute { }

    [DebuggerNonUserCode]
    [DebuggerStepThrough]
    static class InternalAPI
    {
        [DebuggerStepperBoundary]
        public static void _CallDebugger()
        {
        }
        [DebuggerStepperBoundary]
        public static bool _BreakOnLambda(Parser.ParserKit.AstWalker walker, string argName)
        {
            if (Parser.ParserKit.SubParsers.BreakMode._shouldBreakOnLambda == null)
            {
                return false;
            }
            else
            {
                return Parser.ParserKit.SubParsers.BreakMode._shouldBreakOnLambda(walker, argName);
            }

        }
        [DebuggerStepperBoundary]
        public static bool _LetsEnterMethodBody()
        {
            return Parser.ParserKit.SubParsers.BreakMode._letsEnterMethodBody;
        }
    }

}