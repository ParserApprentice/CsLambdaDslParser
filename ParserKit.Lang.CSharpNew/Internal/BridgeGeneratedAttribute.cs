using System;
namespace ParserKit.Internal
{
    public class InjAttribute : Attribute { }

    [System.Diagnostics.DebuggerNonUserCode]
    [System.Diagnostics.DebuggerStepThrough]
    static class InternalAPI
    {
        [System.Diagnostics.DebuggerStepperBoundary]
        public static void _CallDebugger()
        {
        }
        [System.Diagnostics.DebuggerStepperBoundary]
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
        [System.Diagnostics.DebuggerStepperBoundary]
        public static bool _LetsEnterMethodBody()
        {
            return Parser.ParserKit.SubParsers.BreakMode._letsEnterMethodBody;
        }
    }

}