﻿//MIT, 2016, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LayoutFarm.TextBreaker
{
    public class CustomBreaker
    {
        //default for latin breaking engine
        EngBreakingEngine engBreakingEngine = new EngBreakingEngine();
        //current lang breaking engine
        BreakingEngine breakingEngine;
        List<BreakingEngine> otherEngines = new List<BreakingEngine>();

        WordVisitor visitor;
        int textLength;

        public CustomBreaker()
        {
            visitor = new WordVisitor(this);
        }
        public void AddBreakingEngine(BreakingEngine engine)
        {
            //TODO: make this accept more than 1 engine
            otherEngines.Add(engine);
            breakingEngine = engine;
        }
        BreakingEngine SelectEngine(char c)
        {
            if (breakingEngine.CanHandle(c))
            {
                return breakingEngine;
            }
            else
            {
                //find other engine
                foreach (BreakingEngine engine in otherEngines)
                {
                    if (engine != breakingEngine
                        && engine.CanHandle(c))
                    {
                        return engine;
                    }
                }
                return engBreakingEngine;
            }
        }
        public void BreakWords(char[] charBuff, int startAt)
        {
            //conver to char buffer 
            int j = charBuff.Length;
            textLength = j;
            visitor.LoadText(charBuff, 0);
            //----------------------------------------
            char c = charBuff[startAt];
            BreakingEngine currentEngine = SelectEngine(c);
            breakingEngine = currentEngine;
            //----------------------------------------

            //----------------------------------------
            //select breaking engine
            bool continueParse = true;
            while (continueParse)
            {
                //----------------------------------------
                currentEngine.BreakWord(visitor, charBuff, startAt, charBuff.Length - startAt);
                switch (visitor.State)
                {
                    default: throw new NotSupportedException();

                    case VisitorState.End:
                        {
                            //ok
                            continueParse = false;
                        }
                        break;
                    case VisitorState.OutOfRangeChar:
                        {
                            //find proper breaking engine for current char
                            c = visitor.Char;
                            BreakingEngine anotherEngine = SelectEngine(c);
                            if (anotherEngine == currentEngine)
                            {
                                throw new NotSupportedException();
                            }
                            else
                            {
                                currentEngine = anotherEngine;
                                startAt = visitor.CurrentIndex;
                            }
                        }
                        break;
                }
            }
        }

        public void BreakWords(string inputstr)
        {
            BreakWords(inputstr.ToCharArray(), 0);
        }

        public bool CanBeStartChar(char c)
        {
            return breakingEngine.CanBeStartChar(c);
        }
        public void LoadBreakAtList(List<int> outputList)
        {
            outputList.AddRange(visitor.GetBreakList());
        }
        public IEnumerable<BreakSpan> GetBreakSpanIter()
        {
            List<int> breakAtList = visitor.GetBreakList();
            int c_index = 0;
            int i = 0;
            foreach (int breakAt in breakAtList)
            {
                BreakSpan sp = new BreakSpan();
                sp.startAt = c_index;
                sp.len = breakAtList[i] - c_index;
                c_index += sp.len;
                i++;
                yield return sp;
            }
            //-------------------
            if (c_index < textLength)
            {
                BreakSpan sp = new BreakSpan();
                sp.startAt = c_index;
                sp.len = textLength - c_index;
                yield return sp;
            }
        }
    }


}