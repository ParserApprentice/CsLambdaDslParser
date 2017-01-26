//MIT, 2015-2017, ParserApprentice
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using Parser.ParserKit.LR;
using Parser.ParserKit.SubParsers;

namespace Parser.ParserKit
{
    public abstract class ReflectionSubParser : SubParser
    {   
        public static TokenInfoCollection s_tkInfoCollection;
        List<UserNTDefinition> _initUserNts;

        protected abstract UserNTDefinition GetRegisteredProxyUserNt(System.Reflection.FieldInfo fieldInfo);


        public abstract string GetTokenPresentationName(string fieldname);

        protected static OptSymbol opt(TokenDefinition tk)
        {
            return new OptSymbol(tk);
        }
        protected static OptSymbol opt(UserNTDefinition nt)
        {
            return new OptSymbol(nt);
        }
        protected static OptSymbol opt(ListSymbol listSymbol)
        {
            return new OptSymbol(listSymbol);
        }
        protected static OptSymbol opt(OneOfSymbol oneofSymbol)
        {
            return new OptSymbol(oneofSymbol);
        }

        protected static ListSymbol list(UserNTDefinition nt)
        {
            return new ListSymbol(nt);
        }
        protected static ListSymbol list(TokenDefinition tk)
        {
            return new ListSymbol(tk);
        }
        protected static ListSymbol list(UserNTDefinition nt, TokenDefinition sep)
        {
            return new ListSymbol(nt, sep);
        }
        protected static ListSymbol list(TokenDefinition tk, TokenDefinition sep)
        {
            return new ListSymbol(tk, sep);
        }

        //------
        protected static OneOfSymbol oneof(params object[] symbols)
        {
            return new OneOfSymbol(symbols);
        }



        internal SyncSymbol skip(TokenDefinition begin, TokenDefinition end)
        {
            //ignor this pair
            return new SyncSymbol(begin, end, SyncSymbolKind.Ignor);
        }

        static FieldInfo[] GetFields(Type instanceType)
        {
            //get field from instance Type and its base
            List<FieldInfo> list = new List<FieldInfo>();
            list.AddRange(instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
            //
            Type baseType = instanceType.BaseType;
            if (baseType != null)
            {
                baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                list.AddRange(baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
            }
            return list.ToArray();
        }

        protected override void InternalSetup(TokenInfoCollection tkInfoCollection)
        {
            _initUserNts = new List<UserNTDefinition>();
            //get all static user nt              
            List<UserNTDefinition> lateNts = null;
            FieldInfo[] fields = GetFields(this.GetType());
            foreach (FieldInfo f in fields)
            {
                if (f.FieldType == typeof(UserNTDefinition))
                {
                    UserNTDefinition unt = (UserNTDefinition)f.GetValue(null);
                    if (this._rootNtDef == null)
                    {
                        //TODO: review here again
                        _rootNtDef = unt;
                    }

                    if (unt.Name == null)
                    {
                        unt.SetName(f.Name);
                    }

                    UserNTDefinition proxyNtDef = GetRegisteredProxyUserNt(f);
                    ((ProxyUserNTDefinition)proxyNtDef).SetActualImplementation(unt);


                    unt.OwnerSubParser = this;
                    _initUserNts.Add(unt);

                    List<UserNTDefinition> lateUserNts = unt.GetLateNts();
                    if (lateUserNts != null)
                    {
                        if (lateNts == null)
                        {
                            lateNts = new List<UserNTDefinition>();
                        }
                        foreach (UserNTDefinition lateNt in lateUserNts)
                        {
                            lateNts.Add(lateNt);
                        }
                    }
                }
                else if (f.FieldType == typeof(UserTokenDefinition))
                {
                    var fieldValue = f.GetValue(this) as UserTokenDefinition;
                    if (fieldValue == null)
                    {
                        //no init value 
                        f.SetValue(this, new UserTokenDefinition(tkInfoCollection.GetTokenInfo(GetTokenPresentationName(f.Name))));
                    }
                    else
                    {
                        //use presentation string 
                        fieldValue.TkDef = tkInfoCollection.GetTokenInfo(fieldValue.GrammarString);
                    }
                }
                else if (f.FieldType == typeof(TokenDefinition))
                {
                    //get token from grammar sheet
                    //get existing value 
                    var fieldValue = f.GetValue(this) as TokenDefinition;
                    if (fieldValue == null)
                    {
                        f.SetValue(this, tkInfoCollection.GetTokenInfo(GetTokenPresentationName(f.Name)));
                    }
                }
                else if (f.FieldType == typeof(ProxyUserNTDefinition))
                {

                }
            }
            if (lateNts != null)
            {
                _initUserNts.AddRange(lateNts);
            }

            //check if all user nt is define
            for (int i = _initUserNts.Count - 1; i >= 0; --i)
            {
                UserNTDefinition unt = _initUserNts[i];
                if (unt.UserSeqCount == 0)
                {
                    //this nt is not defined!
                    unt.MarkedAsUnknownNT = true;
                }
            }

            //---------------------------------------
            _miniGrammarSheet = new MiniGrammarSheet();
            _miniGrammarSheet.LoadTokenInfo(tkInfoCollection);
            _miniGrammarSheet.LoadUserNts(_initUserNts);
            //--------------------------------------- 
            _augmentedNTDefinition = _miniGrammarSheet.PrepareUserGrammarForAnyLR(this.RootNt);
            _parsingTable = _miniGrammarSheet.CreateLR1Table(_augmentedNTDefinition);
            //can use cache table 
            //---------------------------------------    
            //sync parser table
            //if (_syncNts != null && _syncNts.Count > 0)
            //{
            //    _syncParser = new SyncParser();
            //    _syncParser.Setup(tkInfoCollection, this._syncNts);
            //}
        }

    }




    public abstract class ReflectionSubParser<T> : ReflectionSubParser
    {
        protected static Dictionary<System.Reflection.FieldInfo, UserNTDefinition>
                      proxyUserNts = new Dictionary<System.Reflection.FieldInfo, UserNTDefinition>();
        static GetWalkerDel<T> getWalkerDel;
        public GetWalkerDel<T> GetWalker
        {
            get { return getWalkerDel; }
            set { getWalkerDel = value; }
        }

        internal override List<SyncSequence> GetSyncSeqs()
        {
            return _syncSeqs;
        }
        static internal List<SyncSequence> _syncSeqs;
        protected static bool sync(params TokenDefinition[] syncTks)
        {
            if (_syncSeqs == null)
            {
                _syncSeqs = new List<SyncSequence>();
            }

            int j = syncTks.Length;
            SeqSyncCmd[] syncCmds = new SeqSyncCmd[j];
            for (int i = 0; i < j; ++i)
            {
                syncCmds[i] = new SeqSyncCmd(SyncCmdName.Match, syncTks[i]);
            }

            _syncSeqs.Add(new SyncSequence(syncCmds));
            return true;
        }
        protected static bool sync_start(TokenDefinition startSync)
        {
            if (_syncSeqs == null)
            {
                _syncSeqs = new List<SyncSequence>();
            }
            SeqSyncCmd[] syncCmds = new SeqSyncCmd[]{
                 new SeqSyncCmd(SyncCmdName.First, startSync)
            };
            _syncSeqs.Add(new SyncSequence(syncCmds));
            return true;
        }
        protected override UserNTDefinition GetRegisteredProxyUserNt(System.Reflection.FieldInfo fieldInfo)
        {
            UserNTDefinition u;
            if (proxyUserNts.TryGetValue(fieldInfo, out u))
            {
                return u;
            }
            else
            {
                return null;
            }
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel, UserExpectedSymbolDef<T> s1)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2 });
        }

        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3 });
        }

        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
             UserExpectedSymbolDef<T> s1,
             UserExpectedSymbolDef<T> s2,
             UserExpectedSymbolDef<T> s3,
             UserExpectedSymbolDef<T> s4,
             UserExpectedSymbolDef<T> s5)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4,
            UserExpectedSymbolDef<T> s5,
            UserExpectedSymbolDef<T> s6)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7, s8 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4,
            UserExpectedSymbolDef<T> s5,
            UserExpectedSymbolDef<T> s6,
            UserExpectedSymbolDef<T> s7,
            UserExpectedSymbolDef<T> s8,
            UserExpectedSymbolDef<T> s9,
            UserExpectedSymbolDef<T> s10
            )
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9,
           UserExpectedSymbolDef<T> s10,
           UserExpectedSymbolDef<T> s11
           )
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11 });
        }
        protected static NtDefAssignSet<T> _(BuilderDel3<T> reductionDel,
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9,
           UserExpectedSymbolDef<T> s10,
           UserExpectedSymbolDef<T> s11,
           params UserExpectedSymbolDef<T>[] _else
           )
        {
            int j = _else.Length;
            UserExpectedSymbolDef<T>[] total = new UserExpectedSymbolDef<T>[j + 11];
            total[0] = s1; total[1] = s2; total[2] = s3; total[3] = s4; total[4] = s5;
            total[5] = s6; total[6] = s7; total[7] = s8; total[8] = s9; total[9] = s10;
            total[10] = s11;

            for (int i = 0; i < j; ++i)
            {
                total[i + 11] = _else[i];
            }

            return new NtDefAssignSet<T>(getWalkerDel, null, reductionDel, total);
        }

        protected static NtDefAssignSet<T> _oneof(NtDefAssignSet<T> c1, NtDefAssignSet<T> c2, params NtDefAssignSet<T>[] others)
        {
            //at least 2 choices
            int j = others.Length;
            NtDefAssignSet<T>[] choices = new NtDefAssignSet<T>[j + 2];
            choices[0] = c1;
            choices[1] = c2;
            for (int i = 0; i < j; ++i)
            {
                choices[2 + i] = others[i];
            }
            return new NtDefAssignSet<T>(getWalkerDel, choices);
        }


        protected static NtDefAssignSet<T> _oneof(
         UserExpectedSymbolDef<T> c1,
         UserExpectedSymbolDef<T> c2)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 })
                });
        }
        protected static NtDefAssignSet<T> _oneof(
            UserExpectedSymbolDef<T> c1,
            UserExpectedSymbolDef<T> c2,
            UserExpectedSymbolDef<T> c3)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 })
                });
        }
        protected static NtDefAssignSet<T> _oneof(
           UserExpectedSymbolDef<T> c1,
           UserExpectedSymbolDef<T> c2,
           UserExpectedSymbolDef<T> c3,
           UserExpectedSymbolDef<T> c4)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 })
                });
        }
        protected static NtDefAssignSet<T> _oneof(
          UserExpectedSymbolDef<T> c1,
          UserExpectedSymbolDef<T> c2,
          UserExpectedSymbolDef<T> c3,
          UserExpectedSymbolDef<T> c4,
          UserExpectedSymbolDef<T> c5)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c5 })
                });
        }
        protected static NtDefAssignSet<T> _oneof(
          UserExpectedSymbolDef<T> c1,
          UserExpectedSymbolDef<T> c2,
          UserExpectedSymbolDef<T> c3,
          UserExpectedSymbolDef<T> c4,
          UserExpectedSymbolDef<T> c5,
          UserExpectedSymbolDef<T> c6)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c5 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c6 }),
                });
        }
        protected static NtDefAssignSet<T> _oneof(
         UserExpectedSymbolDef<T> c1,
         UserExpectedSymbolDef<T> c2,
         UserExpectedSymbolDef<T> c3,
         UserExpectedSymbolDef<T> c4,
         UserExpectedSymbolDef<T> c5,
         UserExpectedSymbolDef<T> c6,
         UserExpectedSymbolDef<T> c7)
        {

            return new NtDefAssignSet<T>(getWalkerDel, new NtDefAssignSet<T>[] {
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c5 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c6 }),
                     new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c7 }),
                });
        }
        protected static NtDefAssignSet<T> _oneof(
         UserExpectedSymbolDef<T> c1,
         UserExpectedSymbolDef<T> c2,
         UserExpectedSymbolDef<T> c3,
         UserExpectedSymbolDef<T> c4,
         UserExpectedSymbolDef<T> c5,
         UserExpectedSymbolDef<T> c6,
         UserExpectedSymbolDef<T> c7,
         params UserExpectedSymbolDef<T>[] others)
        {

            int j = others.Length;
            NtDefAssignSet<T>[] choices = new NtDefAssignSet<T>[j + 7];
            choices[0] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c1 });
            choices[1] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c2 });
            choices[2] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c3 });
            choices[3] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c4 });
            choices[4] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c5 });
            choices[5] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c6 });
            choices[6] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { c7 });

            for (int i = 0; i < j; ++i)
            {
                choices[7 + i] = new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { others[i] });
            }
            return new NtDefAssignSet<T>(getWalkerDel, choices);
        }



        protected static NtDefAssignSet<T> _(UserExpectedSymbolDef<T> s1)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1 });
        }

        protected static NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2 });
        }

        protected static NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3 });
        }

        protected static NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4 });
        }
        protected static NtDefAssignSet<T> _(
             UserExpectedSymbolDef<T> s1,
             UserExpectedSymbolDef<T> s2,
             UserExpectedSymbolDef<T> s3,
             UserExpectedSymbolDef<T> s4,
             UserExpectedSymbolDef<T> s5)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5 });
        }
        protected static NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4,
            UserExpectedSymbolDef<T> s5,
            UserExpectedSymbolDef<T> s6)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6 });
        }
        protected static NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7 });
        }
        protected static NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7, s8 });
        }
        protected static NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9)
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9 });
        }
        protected static NtDefAssignSet<T> _(
            UserExpectedSymbolDef<T> s1,
            UserExpectedSymbolDef<T> s2,
            UserExpectedSymbolDef<T> s3,
            UserExpectedSymbolDef<T> s4,
            UserExpectedSymbolDef<T> s5,
            UserExpectedSymbolDef<T> s6,
            UserExpectedSymbolDef<T> s7,
            UserExpectedSymbolDef<T> s8,
            UserExpectedSymbolDef<T> s9,
            UserExpectedSymbolDef<T> s10
            )
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 });
        }
        protected static NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9,
           UserExpectedSymbolDef<T> s10,
           UserExpectedSymbolDef<T> s11
           )
        {
            return new NtDefAssignSet<T>(getWalkerDel, null, null, new[] { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11 });
        }
        protected static NtDefAssignSet<T> _(
           UserExpectedSymbolDef<T> s1,
           UserExpectedSymbolDef<T> s2,
           UserExpectedSymbolDef<T> s3,
           UserExpectedSymbolDef<T> s4,
           UserExpectedSymbolDef<T> s5,
           UserExpectedSymbolDef<T> s6,
           UserExpectedSymbolDef<T> s7,
           UserExpectedSymbolDef<T> s8,
           UserExpectedSymbolDef<T> s9,
           UserExpectedSymbolDef<T> s10,
           UserExpectedSymbolDef<T> s11,
           params UserExpectedSymbolDef<T>[] _else
           )
        {
            int j = _else.Length;
            UserExpectedSymbolDef<T>[] total = new UserExpectedSymbolDef<T>[j + 11];
            total[0] = s1; total[1] = s2; total[2] = s3; total[3] = s4; total[4] = s5;
            total[5] = s6; total[6] = s7; total[7] = s8; total[8] = s9; total[9] = s10;
            total[10] = s11;

            for (int i = 0; i < j; ++i)
            {
                total[i + 11] = _else[i];
            }

            return new NtDefAssignSet<T>(getWalkerDel, null, null, total);
        }
    }

}