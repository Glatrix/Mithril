using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLanguage.Language.Language;
using TestLanguage.Language.Runtime;

namespace TestLanguage.Language
{
    public class Lang
    {
        public static void RunFile(string file)
        {
            Parser p = new Parser();
            LangEnvironment env = new LangEnvironment(true);

            string code = File.ReadAllText(file);

            // Handle Comments
            code = string.Join('\n', code.Split("\n").Select((l) => l.Split("//").FirstOrDefault()));


            var pr = p.Parse(Lexer.Tokenize(code));

            RuntimeValue val = Interpreter.evaluate(pr, env);

            return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(val);
            Console.ResetColor();
        }

        public static void StartRepl()
        {
            Parser p = new Parser();

            LangEnvironment env = new LangEnvironment(true);

            Console.Write("Repl v0.1: \n> ");

            string TotalInput = "";
            
            while (true)
            {
                string input = Console.ReadLine().Trim() ?? "";

                if(input == "exit")
                {
                    return;
                }
                else if (input == "run")
                {
                    var pr = p.Parse(Lexer.Tokenize(TotalInput));
                    RuntimeValue result = Interpreter.evaluate(pr, env);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(result);
                    Console.ResetColor();
                    TotalInput = "";
                }
                else if (input == "restart" || input == "reset")
                {
                    p = new Parser();
                    env = new LangEnvironment(true);
                    TotalInput = "";

                    Console.ResetColor();
                    Console.Clear();
                    Console.Write("Repl v0.1: \n> ");
                }
                else if (input == "clear")
                {
                    Console.ResetColor();
                    Console.Clear();
                    Console.Write("Repl v0.1: \n> ");
                    continue;
                }
                else if (input.StartsWith("runfile "))
                {
                    string file = input.Substring(8);
                    if (File.Exists(file))
                    {
                        string code = File.ReadAllText(file);
                        var pr = p.Parse(Lexer.Tokenize(code));
                        RuntimeValue result = Interpreter.evaluate(pr, env);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(result);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"File Not Found: '{file}'");
                    }
                }
                else
                {
                    try
                    {
                        //var pr = p.Parse(Lexer.Tokenize(input));
                        //RuntimeValue result = Interpreter.evaluate(pr, env);
                        //Console.ForegroundColor = ConsoleColor.Green;
                        //Console.WriteLine(result);
                        //Console.ResetColor();

                        TotalInput += $"\n{input}";
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                        //Console.WriteLine(ex);
                        Console.ResetColor();
                    }

                    Console.Write(">");
                }
            }
        }
    }
}
