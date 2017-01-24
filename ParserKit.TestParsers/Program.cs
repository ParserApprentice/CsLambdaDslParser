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
           // AsmBreakInjector.InjectBreakPoint("ParserKit.Lang.CSharp.dll");
#endif
            //----------- 
            Application.Run(new FormLR());
        }

    }


    delegate int TestDel();
    delegate int TestDel2(int x);
    class A
    {
        void Test()
        {
            TestDel d1 = delegate
            {
                return 0;
            };
            
            TestDel d2 = () =>
            {     // lambda statement
                return 0;
            }; 
            
            TestDel d3 = () => 0; // lambda expression

            TestDel2 d4 = (x) =>
            {   // lambda statement
                return 1;
            }; 

            TestDel2 d5 = x => 1; // lambda expression 

            d1();
            d2();
            d3();
            d4(0);
            d5(0);
        }
    }







}
