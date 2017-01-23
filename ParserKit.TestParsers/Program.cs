using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;


namespace ParserKit.TestParsers
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //-----------
            //inject code 
#if DEBUG
            //AsmBreakInjector.InjectBreakPoint("ParserKit.Lang.CSharp.dll");
#endif
            //----------- 
            Application.Run(new FormLR());
        }

    }




}
