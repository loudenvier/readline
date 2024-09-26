using System;

Console.WriteLine("""
    ReadLine Library Demo
    ---------------------
    ■ Type pwd for password entry
    ■ Type an <Empty> line to quit

    """);


ReadLine.AutoCompletionHandler = new AutoCompletionHandler();
ReadLine.HistoryEnabled = true;

string input;
do {
    input = ReadLine.Read("(prompt)> ");
    Console.WriteLine(input);
    if (input == "pwd") {
        input = ReadLine.ReadPassword("Enter Password> ");
        Console.WriteLine(input);
    }
} while(input.Length > 0);

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
