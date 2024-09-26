[![Windows build status](https://ci.appveyor.com/api/projects/status/github/tonerdo/readline?branch=master&svg=true)](https://ci.appveyor.com/project/tonerdo/readline)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![NuGet version](https://badge.fury.io/nu/Loudenvier.ReadLine.svg)](https://www.nuget.org/packages/Loudenvier.ReadLine)

_This fork of tornedo's ReadLine is aimed at solving bugs (contributed as PR's to main) and adding small improvements/new features (marked as such on the readme file). It's a drop-in replacement: simply change the nuget from `ReadLine` to `Loudenvier.ReadLine` (the nuget version is based on the original ReadLine package version)_

# ReadLine

ReadLine is a [GNU Readline](https://en.wikipedia.org/wiki/GNU_Readline) like library built in pure C#. It can serve as a drop in replacement for the inbuilt `Console.ReadLine()` and brings along
with it some of the terminal goodness you get from unix shells, like command history navigation and tab auto completion.

It is cross platform and runs anywhere .NET is supported, targeting `netstandard2.0` means that it can be used with .NET and .NET Framework.

## Shortcut Guide

| Shortcut                       | Comment                           |
| ------------------------------ | --------------------------------- |
| `Ctrl`+`A` / `HOME`            | Beginning of line                 |
| `Ctrl`+`B` / `←`               | Backward one character            |
| `Ctrl`+`C`                     | Send EOF                          |
| `Ctrl`+`E` / `END`             | End of line                       |
| `Ctrl`+`F` / `→`               | Forward one character             |
| `Ctrl`+`H` / `Backspace`       | Delete previous character         |
| `Tab`                          | Command line completion           |
| `Shift`+`Tab`                  | Backwards command line completion |
| `Ctrl`+`J` / `Enter`           | Line feed                         |
| `Ctrl`+`K`                     | Cut text to the end of line       |
| `Ctrl`+`L` / `Esc`             | Clear line                        |
| `Ctrl`+`M`                     | Same as Enter key                 |
| `Ctrl`+`N` / `↓`               | Forward in history                |
| `Ctrl`+`P` / `↑`               | Backward in history               |
| `Ctrl`+`U`                     | Cut text to the start of line     |
| `Ctrl`+`W`                     | Cut previous word                 |
| `Backspace`                    | Delete previous character         |
| `Ctrl` + `D` / `Delete`        | Delete succeeding character       |
| `Ctrl` + `←` / `Alt` + `B`     | Backward one word<sup>_new_</sup> |
| `Ctrl` + `→` / `Alt` + `F`     | Forward one word<sup>_new_</sup>  |
| `Alt` + `C`                    | Capitalizes current char and moves to the end of the word<sup>_new_</sup>  |
| `Alt` + `U`                    | Capitalizes every char from the cursor's position to the end of the word<sup>_new_</sup>  |
| `Alt` + `L`                    | Lowercases every char from the cursor's position to the end of the word<sup>_new_</sup>  |


## Installation

Available on [NuGet](https://www.nuget.org/packages/Loudenvier.ReadLine/)

Visual Studio:

```powershell
PM> Install-Package Loudenvier.ReadLine
```

.NET Core CLI:

```bash
dotnet add package Loudenvier.ReadLine
```

## Loudenvier's changes

* Fixed a bug with CTRL+LEFT_ARROW/RIGHT_ARROW creating havoc in editing
* Added support to navigate to PREVIOUS/NEXT word (CTRL+LEFT/RIGHT)

## Usage

### Read input from the console

```csharp
string input = ReadLine.Read("(prompt)> ");
```

### Read password from the console

```csharp
string password = ReadLine.ReadPassword("(prompt)> ");
```

_Note: The `(prompt>)` is  optional_

### History management

```csharp
// Get command history
ReadLine.GetHistory();

// Add command to history
ReadLine.AddHistory("dotnet run");

// Clear history
ReadLine.ClearHistory();

// Disable history
ReadLine.HistoryEnabled = false;
```

_Note: History information is persisted for an entire application session. Also, calls to `ReadLine.Read()` automatically adds the console input to history_

### Auto-Completion

```csharp
class AutoCompletionHandler : IAutoCompleteHandler
{
    // characters to start completion from
    public char[] Separators { get; set; } = new char[] { ' ', '.', '/' };

    // text - The current text entered in the console
    // index - The index of the terminal cursor within {text}
    public string[] GetSuggestions(string text, int index)
    {
        if (text.StartsWith("git "))
            return new string[] { "init", "clone", "pull", "push" };
        else
            return null;
    }
}

ReadLine.AutoCompletionHandler = new AutoCompletionHandler();
```

_Note: If no "AutoCompletionHandler" is set, tab autocompletion will be disabled_

## Contributing

Contributions are highly welcome. If you have found a bug or if you have a feature request, please report them at this repository issues section.

Things you can help with:
* Achieve better command parity with [GNU Readline](https://en.wikipedia.org/wiki/GNU_Readline).
* Add more test cases.

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.
