using System;
using System.IO;
using System.Collections.Generic;

namespace CardMarket_Web_Core.ApiQueryLogic
{
    static public class Tokens
    {
        static public Dictionary<string, string> ReadTokens()
        {
            Dictionary<string, string> tokens = new Dictionary<string, string>();

            string workingDirectory = Environment.CurrentDirectory;

            /// https://stackoverflow.com/questions/5758633/how-should-i-handle-windows-linux-paths-in-c-sharp
            string tokensPath = Path.DirectorySeparatorChar +
                                "Tokens" +
                                Path.DirectorySeparatorChar +
                                "Tokens.txt";

            string[] lines = File.ReadAllLines(workingDirectory + tokensPath);

            foreach (string s in lines)
            {
                string[] a = s.Split('=');
                tokens.Add(a[0], a[1]);
            }
            return tokens;
        }
    }
}
