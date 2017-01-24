using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;


namespace ParserKit.TestParsers
{
    class SourceFile
    {
        string[] sourceLines;
        public SourceFile()
        {
        }
        public void LoadFrom(string filename)
        {
            Filename = filename;
            sourceLines = File.ReadAllLines(filename);
        }
        public string Filename
        {
            get;
            set;
        }

    }
    static class AsmBreakInjector
    {
        public static void InjectBreakPoint(string asmFilename)
        {
            string onlyFilename = Path.GetFileNameWithoutExtension(asmFilename);
            string pdbFilename = onlyFilename + ".pdb";
            var readerParameters = new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = new Mono.Cecil.Pdb.PdbReaderProvider() };

            if (File.Exists(pdbFilename))
            {
                pdbFs = new FileStream(pdbFilename,
                    FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                readerParameters.SymbolStream = pdbFs;
            }
            AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(asmFilename, readerParameters);
            if (IsBridgeGeneratedAsm(asmDef))
            {
                return;
            }
            //---------------------------------------------------



            //---------------------------------------------------
            ScanTypesAndMethods(asmDef);
            SetBreakMethodParameterTypes(
                new[]{
                typeof(Parser.ParserKit.ParserReporter), 
                typeof(Parser.ParserKit.ParseNodeHolder),
                typeof(Parser.ParserKit.AstWalker)});
            InjectDebugCode(asmDef);

            string tmpDllName = onlyFilename + "_tmp.dll";
            SaveAndClose(asmDef, tmpDllName);
            File.Delete(asmFilename); //original filename
            File.Copy(tmpDllName, asmFilename);
            File.Delete(onlyFilename + ".pdb"); //delete original 
            File.Copy(onlyFilename + "_tmp.pdb", onlyFilename + ".pdb");

        }
        static MethodReference _callDebugger;
        static MethodReference _BreakOnLambda;
        static TypeDefinition _injAttrType;
        static MethodDefinition _bridgeGeneratedAttrCtor;
        static FileStream pdbFs;

        static void SaveAndClose(AssemblyDefinition asmdef, string outputFilename)
        {

            //save then close

            var writeParams = new WriterParameters();
            writeParams.SymbolWriterProvider = new Mono.Cecil.Pdb.PdbWriterProvider();
            writeParams.WriteSymbols = true;
            var ms = new MemoryStream();
            writeParams.SymbolStream = ms;
            asmdef.Write(outputFilename, writeParams);

            if (pdbFs != null)
            {
                pdbFs.Close();
                pdbFs.Dispose();
                pdbFs = null;
            }

        }
        static bool IsBridgeGeneratedAsm(AssemblyDefinition asmdef)
        {
            return GetCustomAttribute(asmdef, "ParserKit.Internal.InjAttribute") != null;
        }
        static CustomAttribute GetCustomAttribute(AssemblyDefinition asmdef,
          string attrTypeName)
        {
            if (asmdef.HasCustomAttributes)
            {
                foreach (CustomAttribute customAttr in asmdef.CustomAttributes)
                {
                    if (customAttr.AttributeType.FullName == attrTypeName)
                    {
                        return customAttr;
                    }
                }
            }
            return null;
        }
        static CustomAttribute FindCustomAttribute(TypeDefinition typedef, string fullCustomAttrName)
        {
            foreach (var customAttr in typedef.CustomAttributes)
            {
                if (customAttr.AttributeType.FullName == fullCustomAttrName)
                {
                    return customAttr;
                }
            }
            return null;
        }
        static void ScanTypesAndMethods(AssemblyDefinition asmDef)
        {

            //find specific method
            var allTypes = asmDef.MainModule.GetTypes();

            foreach (var type in allTypes)
            {

                //only top level type,
                //no nested type
                if (type.FullName == "ParserKit.Internal.InternalAPI")
                {
                    foreach (MethodDefinition met in type.Methods)
                    {
                        if (met.IsStatic)
                        {
                            switch (met.Name)
                            {
                                case "_CallDebugger":
                                    _callDebugger = met;
                                    break;
                                case "_BreakOnLambda":
                                    _BreakOnLambda = met;
                                    break;
                            }
                        }
                    }
                }
                else if (type.FullName == "ParserKit.Internal.InjAttribute")
                {
                    _injAttrType = type;
                    foreach (MethodDefinition met in type.Methods)
                    {
                        if (met.IsConstructor)
                        {
                            _bridgeGeneratedAttrCtor = met;
                            break;
                        }
                    }
                }
            }
        }

        static bool IsNestedLambdaDisplayType(TypeDefinition typedef)
        {
            if (typedef.IsNested)
            {
                return typedef.FullName.Contains("/<>") && IsInheritFromSubParser(typedef.DeclaringType);
            }
            return false;
        }
        static bool IsInheritFromSubParser(TypeDefinition typedef)
        {
            if (typedef.IsClass)
            {
                TypeReference baseType = typedef.BaseType;
                while (baseType != null)
                {

                    if (baseType.FullName.Contains("Parser.ParserKit.SubParser") ||
                        baseType.FullName.Contains("Parser.ParserKit.ReflectionSubParser"))
                    {
                        //found here 
                        return true;
                    }
                    var rrr = baseType.GetType();

                    if (baseType is TypeDefinition)
                    {
                        baseType = ((TypeDefinition)baseType).BaseType;
                    }
                    else if (baseType is GenericInstanceType)
                    {
                        baseType = ((GenericInstanceType)baseType).ElementType;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        static bool IsBreakMethodParameterType(TypeReference typeRef)
        {
            //not exact
            for (int i = breakParameterTypeNames.Length - 1; i >= 0; --i)
            {
                if (typeRef.FullName == breakParameterTypeNames[i])
                {
                    return true;
                }
            }

            TypeDefinition typedef = (TypeDefinition)typeRef;
            if (typedef != null)
            {
                string baseTypeName = ((TypeDefinition)typeRef).BaseType.FullName;

                for (int i = breakParameterTypeNames.Length - 1; i >= 0; --i)
                {
                    if (baseTypeName == breakParameterTypeNames[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        static void ModifyTypeDef(TypeDefinition typedef)
        { 
            //we insert special code
            if (typedef.IsAbstract)
            {
                return;
            }
            if (IsInheritFromSubParser(typedef) || IsNestedLambdaDisplayType(typedef))
            { 
                MethodDefinition defineMethod = null;
                foreach (MethodDefinition met in typedef.Methods)
                {
                    if (met.IsConstructor)
                    {
                        continue;
                    }
                    if (met.Name == "Define")
                    {
                        defineMethod = met;
                    } 
                    if (met.Name.Contains("<") && met.HasBody)
                    {
                        //this is lambda method 
                        MethodBody body = met.Body;
                        //only method with  
                        var pars = met.Parameters;
                        //only 1 pa
                        if (pars.Count != 1)
                        {
                            continue;
                        }
                        //with some method only
                        var par0Type = pars[0].ParameterType;
                        if (!IsBreakMethodParameterType(pars[0].ParameterType))
                        {
                            continue;
                        } 
                        var bodyInst = body.Instructions;
                        Instruction firstInst = bodyInst[0];
                        Instruction lastInst = bodyInst[bodyInst.Count - 1]; 
                        List<Instruction> insts = new List<Instruction>();
                        //--------------------------------------------------------                         
                        insts.Add(met.IsStatic ?
                            Instruction.Create(OpCodes.Ldarg_0) ://static method 
                            Instruction.Create(OpCodes.Ldarg_1));//instance method 
                        insts.Add(Instruction.Create(OpCodes.Ldstr, pars[0].Name));
                        insts.Add(Instruction.Create(OpCodes.Call, _BreakOnLambda));
                        insts.Add(Instruction.Create(OpCodes.Brfalse, firstInst));
                        insts.Add(Instruction.Create(OpCodes.Break)); 
                        //-------------------------------------------------------- 
                        for (int i = insts.Count - 1; i >= 0; --i)
                        {
                            bodyInst.Insert(0, insts[i]);
                        }
                    }
                }

            }

            foreach (TypeDefinition subtype in typedef.NestedTypes)
            {
                ModifyTypeDef(subtype);
            }
        }

        static SequencePoint FindNearestILWithLocation(MethodBody metBody, int startAt)
        {
            var ils = metBody.Instructions;
            for (int i = startAt; i >= 0; --i)
            {
                Instruction il = ils[i];
                if (il != null && il.SequencePoint != null)
                {
                    return il.SequencePoint;
                }
            }
            return null;

        }


        static string[] breakParameterTypeNames;
        static Type[] s_breakParameterTypes;
        static void SetBreakMethodParameterTypes(Type[] breakParameterTypes)
        {
            s_breakParameterTypes = breakParameterTypes;
            int j = breakParameterTypes.Length;
            breakParameterTypeNames = new string[j];
            for (int i = 0; i < j; ++i)
            {
                breakParameterTypeNames[i] = breakParameterTypes[i].FullName;
            }
        }
        static void InjectDebugCode(AssemblyDefinition asmDef)
        {
            var module = asmDef.MainModule;
            //----
            //add sepcial attr to asmdef

            var brigeGenAttr = module.Import(_injAttrType);
            //import
            var ctorRef = module.Import(_bridgeGeneratedAttrCtor);
            CustomAttribute customAttr = new CustomAttribute(ctorRef);
            asmDef.CustomAttributes.Add(customAttr);
            //-----

            foreach (TypeDefinition typedef in module.Types)
            {
                ModifyTypeDef(typedef);
            }

        }
    }
}