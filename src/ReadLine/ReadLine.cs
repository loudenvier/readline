using Internal.ReadLine;
using Internal.ReadLine.Abstractions;

using System.Collections.Generic;

namespace System
{
    public static class ReadLine
    {
        private static List<string> _history;

        static ReadLine()
        {
            _history = [];
        }

        public static void AddHistory(params string[] text) => _history.AddRange(text);
        public static List<string> GetHistory() => _history;
        public static void ClearHistory() => _history = [];
        public static bool HistoryEnabled { get; set; }
        public static IAutoCompleteHandler AutoCompletionHandler { private get; set; }

        public static string Read(string prompt = "", string @default = "")
        {
            var console = new Console2(prompt);
            console.WritePrompt();
            //Console.Write(prompt);
            var keyHandler = new KeyHandler(console, _history, AutoCompletionHandler);
            string text = GetText(keyHandler);

            if (String.IsNullOrWhiteSpace(text) && !String.IsNullOrWhiteSpace(@default))
            {
                text = @default;
            }
            else
            {
                if (HistoryEnabled)
                    _history.Add(text);
            }

            return text;
        }

        public static string ReadPassword(string prompt = "")
        {
            var console = new Console2(prompt) { PasswordMode = true };
            //Console.Write(prompt);
            console.WritePrompt();
            var keyHandler = new KeyHandler(console, null, null);
            return GetText(keyHandler);
        }

        private static string GetText(KeyHandler keyHandler)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyHandler.Handle(keyInfo);
                keyInfo = Console.ReadKey(true);
            }

            Console.WriteLine();
            return keyHandler.Text;
        }
    }
}
