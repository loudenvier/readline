﻿using System;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ConsoleApplication
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("ReadLine Library Demo");
            Console.WriteLine("---------------------");
            Console.WriteLine();

            string[] history = ["ls -a", "dotnet run", "git init"];
            ReadLine.AddHistory(history);

            ReadLine.AutoCompletionHandler = new AutoCompletionHandler();

            string input = ReadLine.Read("(prompt)> ");
            Console.WriteLine(input);

            input = ReadLine.ReadPassword("Enter Password> ");
            Console.WriteLine(input);
        }
    }

    class AutoCompletionHandler : IAutoCompleteHandler
    {
        public char[] Separators { get; set; } = [' ', '.', '/', '\\', ':'];
        public string[] GetSuggestions(string text, int index)
        {
            if (text.StartsWith("git "))
                return ["init", "clone", "pull", "push"];
            else
                return null;
        }
    }
}
