using System;
using System.Collections.Generic;
using System.Text;

namespace HyperCard.Scripting
{
    public static class HypertalkScripting
    {
        public class HScript
        {
            public Dictionary<string, HMethod> Methods = new Dictionary<string, HMethod>();
        }

        public class HMethod
        {
            public Dictionary<string, string> Variables = new Dictionary<string, string>();

            public List<HLine> Code = new List<HLine>();

            public string Name { get; private set; }

            public List<string> Properties = new List<string>();

            public HMethod Overload { get; set; }

            public bool Function { get; private set; }

            public HMethod(string name, string[] words)
            {
                Function = (words[0] == "function");
                Name = name;

                for (int i = 2; i < words.Length; i++)
                {
                    if (words[i].StartsWith("--"))
                        break;
                    Properties.Add(words[i]);
                }
            }

            public void Interpret(Part currentPart)
            {
                foreach (var line in Code)
                {
                    switch (line.Words[0])
                    {
                        case "go":
                            InterpretGo(line, currentPart);
                            break;
                        default:
                            Console.WriteLine("Unknown keyword " + line.Words[0]);
                            break;
                    }
                }
            }

            private string GetQuotedString(HLine line, int offset)
            {
                string word = line.Words[offset];
                if (word.StartsWith("\"") && word.EndsWith("\"")) return word.Trim(new char[] { '"' });
                else
                {
                    for (int i = offset + 1; i < line.Words.Length; i++)
                    {
                        word += " " + line.Words[i];
                        if (word.EndsWith("\"")) return word.Trim(new char[] { '"' });
                    }
                }

                return string.Empty;
            }

            private void InterpretGo(HLine line, Part currentPart)
            {
                var stack = currentPart.Parent.Stack;

                if (line.Words.Length > 2)
                {
                    int offset = 0;
                    if (line.Words[1] == "to") offset++;

                    if (line.Words[offset + 2] == "card" || line.Words[offset + 2] == "cd")
                    {
                        if (line.Words[offset + 1] == "next")
                        {
                            stack.NextCard();
                        }
                        else if (line.Words[offset + 1] == "previous" || line.Words[offset + 1] == "prev")
                        {
                            stack.PreviousCard();
                        }
                        else if (Numbers.ContainsKey(line.Words[offset + 1]))
                        {
                            int number = Numbers[line.Words[offset + 1]];
                            if (number > stack.Cards.Count)
                                throw new Exception("No such card");
                            stack.CurrentCard = stack.GetCardFromIndex(number - 1);
                        }
                    }
                    else if (line.Words[offset + 1] == "card" || line.Words[offset + 1] == "cd")
                    {
                        if (line.Words[offset + 2] == "id")
                        {
                            int id = int.Parse(Variables.ContainsKey(line.Words[offset + 3]) ? Variables[line.Words[offset + 3]] : line.Words[offset + 3]);
                            stack.CurrentCard = stack.GetCardFromID(id) ?? throw new Exception("No such card");
                        }
                        else if (line.Words[offset + 2].StartsWith("\""))
                        {
                            string cardName = GetQuotedString(line, offset + 2);
                            stack.CurrentCard = stack.GetCardFromName(cardName) ?? throw new Exception("No such card");
                        }
                        else
                        {
                            int number = int.Parse(Variables.ContainsKey(line.Words[offset + 2]) ? Variables[line.Words[offset + 2]] : line.Words[offset + 2]);
                            if (number > stack.Cards.Count)
                                throw new Exception("No such card");
                            stack.CurrentCard = stack.GetCardFromID(stack.Cards[number - 1].ID);
                        }
                    }
                    else if (line.Words[offset + 1] == "stack")
                    {
                        var stackName = GetQuotedString(line, offset + 2);
                        var targetStack = new Stack(stackName);

                        if (string.IsNullOrEmpty(targetStack.Name)) throw new Exception("No such stack");
                        stack.Renderer.SetStack(targetStack);
                    }
                }
                else if (line.Words.Length == 2)
                {
                    switch (line.Words[1])
                    {
                        case "next": stack.NextCard();
                            break;
                        case "previous":
                        case "prev":
                            stack.PreviousCard();
                            break;
                    }
                }
            }
        }

        public struct HLine
        {
            public string[] Words;

            public HLine(string[] words)
            {
                Words = words;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var word in Words) sb.Append(word + " ");
                return sb.ToString();
            }
        }

        public static Dictionary<string, int> Numbers = new Dictionary<string, int>()
        {
            { "first", 1 },
            { "second", 2 },
            { "third", 3 },
            { "fourth", 4 },
            { "fifth", 5 },
            { "sixth", 6 },
            { "seventh", 7 },
            { "eighth", 8 },
            { "ninth", 9 },
            { "tenth", 10 },
        };

        public static HScript ParseScript(string script, IPart part)
        {
            HScript result = new HScript();

            string[] lines = script.Split(new char[] { '\r' });

            char[] wordSplit = new char[] { ' ' };
            HMethod currentMethod = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim().ToLower();    // hack for now, we should find strings and keep their case sensitivity
                if (line.StartsWith("--")) continue;    // comment

                // total hack to get the Home stack working a bit better until we have a full intepreter
                if (line.StartsWith("go") && line.Contains("(the short name of me)"))
                {
                    line = line.Replace("(the short name of me)", "\"" + part.Name.ToLower() + "\"");
                }

                string[] words = line.Split(wordSplit);

                if (currentMethod == null)
                {
                    if ((words[0] == "on" || words[0] == "function") && words.Length > 1)
                    {
                        currentMethod = new HMethod(words[1], words);
                        if (result.Methods.ContainsKey(currentMethod.Name))
                        {
                            var method = result.Methods[currentMethod.Name];
                            while (method.Overload != null) method = method.Overload;
                            method.Overload = currentMethod;
                        }
                        else result.Methods.Add(currentMethod.Name, currentMethod);
                    }
                    else
                    {
                        Console.WriteLine("Unexpected " + words);
                    }
                }
                else
                {
                    if (words[0] == "end" && words.Length > 1)
                    {
                        if (currentMethod == null) throw new Exception("No matching 'on' keyword.");
                        else if (words[1] == "if" || words[1] == "repeat") currentMethod.Code.Add(new HLine(words));
                        else if (words[1] != currentMethod.Name) throw new Exception(string.Format("Wrong method closing name.  Expected {0} but got {1}.", currentMethod.Name, words[1]));
                        else
                        {
                            currentMethod = null;
                        }
                    }
                    else if (currentMethod != null)
                    {
                        currentMethod.Code.Add(new HLine(words));
                    }
                }
            }

            return result;
        }
    }
}
