using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TestLanguage.Language;
using TestLanguage.Language.Language;
using TestLanguage.Language.Runtime;

namespace TestLanguage
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Glatrix's Test Lang";
            //Lang.StartRepl();
            Lang.RunFile(".\\test.js");
            Console.ReadKey();
        }
    }
}