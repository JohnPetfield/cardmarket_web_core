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
                //Console.WriteLine(workingDirectory);
                //Console.WriteLine("1");
                /*
                // or: Directory.GetCurrentDirectory() gives the same result

                // This will get the current PROJECT bin directory (ie ../bin/)
                string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
                Console.WriteLine(projectDirectory);
                Console.WriteLine("2");

                // This will get the current PROJECT directory
                string projectDirectory2 = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
                Console.WriteLine(projectDirectory2);
                Console.WriteLine("3");

                string projectDirectory3 = AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine(projectDirectory3);
                Console.WriteLine("4");

                */
                //string startupPath = System.IO.Directory.GetCurrentDirectory();

                //string startupPath = Environment.CurrentDirectory;


                // .gitignore file now ignores the token file, 
                // so can have this located in the project
                //string[] lines = File.ReadAllLines(projectDirectory3 + @"\Tokens\Tokens.txt");

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

                /*
                Console.WriteLine(tokens["appToken"]);
                Console.WriteLine(tokens["appSecret"]);
                Console.WriteLine(tokens["accessToken"]);
                Console.WriteLine(tokens["accessSecret"]);
                */

                return tokens;
        }
    }
}
