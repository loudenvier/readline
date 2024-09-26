using Internal.ReadLine.Abstractions;

using System;
using System.Collections.Generic;
using System.Text;

namespace Internal.ReadLine
{
    internal class KeyHandler
    {
        private int _cursorPos;
        private int _cursorLimit;
        private readonly StringBuilder _text;
        private readonly List<string> _history;
        private int _historyIndex;
        private ConsoleKeyInfo _keyInfo;
        private readonly Dictionary<string, Action> _keyActions;
        private string[] _completions;
        private int _completionStart;
        private int _completionsIndex;
        private readonly IConsole console;

        private bool IsStartOfLine() => _cursorPos == 0;

        private bool IsEndOfLine() => _cursorPos == _cursorLimit;
        private bool IsLastChar() => _cursorPos == _cursorLimit || _cursorPos == _cursorLimit - 1;
        private bool IsBlank() => _cursorPos >= _cursorLimit || _text[_cursorPos] == ' ';

        private bool IsStartOfBuffer() => console.CursorLeft == 0;

        private bool IsEndOfBuffer() => console.CursorLeft == console.BufferWidth - 1;
        private bool IsInAutoCompleteMode() => _completions != null;

        private void MoveCursorLeft()
        {
            if (IsStartOfLine())
                return;

            if (IsStartOfBuffer())
                console.SetCursorPosition(console.BufferWidth - 1, console.CursorTop - 1);
            else
                console.SetCursorPosition(console.CursorLeft - 1, console.CursorTop);

            _cursorPos--;
        }

        private void MoveCursorHome()
        {
            while (!IsStartOfLine())
                MoveCursorLeft();
        }

        private string BuildKeyInput() => _keyInfo.Modifiers == 0
            ? $"{_keyInfo.Key}"
            : $"{_keyInfo.Modifiers}{_keyInfo.Key}";

        private void MoveCursorRight()
        {
            if (IsEndOfLine())
                return;

            if (IsEndOfBuffer())
                console.SetCursorPosition(0, console.CursorTop + 1);
            else
                console.SetCursorPosition(console.CursorLeft + 1, console.CursorTop);

            _cursorPos++;
        }

        private void MoveCursorEnd()
        {
            while (!IsEndOfLine())
                MoveCursorRight();
        }

        private void ClearScreen() {
            ClearLine();
            console.Clear();
            console.WritePrompt();
        }

        private void ClearLine()
        {
            MoveCursorEnd();
            while (!IsStartOfLine())
                Backspace();
        }

        private void SkipBlanks(bool backwards = false) {
            Action moveCursor = backwards ? MoveCursorLeft : MoveCursorRight;
            moveCursor();
            while (!IsStartOfLine() && !IsEndOfLine() && IsBlank())
                moveCursor();
        }

        private void WriteNewString(string str)
        {
            ClearLine();
            foreach (char character in str)
                WriteChar(character);
        }

        private void WriteString(string str)
        {
            foreach (char character in str)
                WriteChar(character);
        }

        private void WriteChar() {
            // solves bug when typing things like ControlLeftArrow...
            // maybe we should only write printable characters...
            if (_keyInfo.KeyChar !=  '\0')
                WriteChar(_keyInfo.KeyChar);
        }

        private void WriteChar(char c)
        {
            if (IsEndOfLine())
            {
                _text.Append(c);
                console.Write(c.ToString());
                _cursorPos++;
            }
            else
            {
                int left = console.CursorLeft;
                int top = console.CursorTop;
                string str = _text.ToString().Substring(_cursorPos);
                _text.Insert(_cursorPos, c);
                console.Write(c.ToString() + str);
                console.SetCursorPosition(left, top);
                MoveCursorRight();
            }

            _cursorLimit++;
        }

        private void ReplaceChar(char c) {
            if (IsEndOfLine()) return;
            _text[_cursorPos++] = c;
            Console.Write($"{c}");
        }

        private void Backspace()
        {
            if (IsStartOfLine())
                return;

            MoveCursorLeft();
            int index = _cursorPos;
            _text.Remove(index, 1);
            string replacement = _text.ToString().Substring(index);
            int left = console.CursorLeft;
            int top = console.CursorTop;
            console.Write(string.Format("{0} ", replacement));
            console.SetCursorPosition(left, top);
            _cursorLimit--;
        }

        private void Delete()
        {
            if (IsEndOfLine())
                return;

            int index = _cursorPos;
            _text.Remove(index, 1);
            string replacement = _text.ToString().Substring(index);
            int left = console.CursorLeft;
            int top = console.CursorTop;
            console.Write(string.Format("{0} ", replacement));
            console.SetCursorPosition(left, top);
            _cursorLimit--;
        }

        private void TransposeChars()
        {
            // local helper functions
            bool almostEndOfLine() => (_cursorLimit - _cursorPos) == 1;
            int incrementIf(Func<bool> expression, int index) =>  expression() ? index + 1 : index;
            int decrementIf(Func<bool> expression, int index) => expression() ? index - 1 : index;

            if (IsStartOfLine()) { return; }

            var firstIdx = decrementIf(IsEndOfLine, _cursorPos - 1);
            var secondIdx = decrementIf(IsEndOfLine, _cursorPos);

            (_text[firstIdx], _text[secondIdx]) = (_text[secondIdx], _text[firstIdx]);
            var left = incrementIf(almostEndOfLine, console.CursorLeft);
            var cursorPosition = incrementIf(almostEndOfLine, _cursorPos);

            WriteNewString(_text.ToString());

            console.SetCursorPosition(left, console.CursorTop);
            _cursorPos = cursorPosition;

            MoveCursorRight();
        }

        private void StartAutoComplete()
        {
            while (_cursorPos > _completionStart)
                Backspace();

            _completionsIndex = 0;

            WriteString(_completions[_completionsIndex]);
        }

        private void NextAutoComplete()
        {
            while (_cursorPos > _completionStart)
                Backspace();

            _completionsIndex++;

            if (_completionsIndex == _completions.Length)
                _completionsIndex = 0;

            WriteString(_completions[_completionsIndex]);
        }

        private void PreviousAutoComplete()
        {
            while (_cursorPos > _completionStart)
                Backspace();

            _completionsIndex--;

            if (_completionsIndex == -1)
                _completionsIndex = _completions.Length - 1;

            WriteString(_completions[_completionsIndex]);
        }

        private void PrevHistory()
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                WriteNewString(_history[_historyIndex]);
            }
        }

        private void NextHistory()
        {
            if (_historyIndex < _history.Count)
            {
                _historyIndex++;
                if (_historyIndex == _history.Count)
                    ClearLine();
                else
                    WriteNewString(_history[_historyIndex]);
            }
        }

        private void ResetAutoComplete()
        {
            _completions = null;
            _completionsIndex = 0;
        }

        private void OneWordBackward() {
            SkipBlanks(backwards: true);
            while (!IsStartOfLine() && _text[_cursorPos - 1] != ' ')
                MoveCursorLeft();
        }
        private void OneWordForward() {
            while (!IsLastChar() && _text[_cursorPos + 1] != ' ')
                MoveCursorRight();
            SkipBlanks();
        }
        private void EndOfWord() {
            while (!IsEndOfLine() && !IsBlank())
                MoveCursorRight();
        }

        public string Text
        {
            get
            {
                return _text.ToString();
            }
        }

        public KeyHandler(IConsole console, List<string> history, IAutoCompleteHandler autoCompleteHandler)
        {
            this.console = console;

            _history = history ?? [];
            _historyIndex = _history.Count;
            _text = new StringBuilder();
            _keyActions = new Dictionary<string, Action> {
                ["LeftArrow"] = MoveCursorLeft,
                ["Home"] = MoveCursorHome,
                ["End"] = MoveCursorEnd,
                ["ControlA"] = MoveCursorHome,
                ["ControlB"] = MoveCursorLeft,
                ["RightArrow"] = MoveCursorRight,
                ["ControlF"] = MoveCursorRight,
                ["ControlE"] = MoveCursorEnd,
                ["Backspace"] = Backspace,
                ["Delete"] = Delete,
                ["ControlD"] = Delete,
                ["ControlH"] = Backspace,
                ["ControlL"] = ClearScreen,
                ["Escape"] = ClearLine,
                ["UpArrow"] = PrevHistory,
                ["ControlP"] = PrevHistory,
                ["DownArrow"] = NextHistory,
                ["ControlN"] = NextHistory,
                ["ControlU"] = () => {
                    while (!IsStartOfLine())
                        Backspace();
                },
                ["ControlK"] = () => {
                    int pos = _cursorPos;
                    MoveCursorEnd();
                    while (_cursorPos > pos)
                        Backspace();
                },
                ["ControlW"] = () => {
                    while (!IsStartOfLine() && _text[_cursorPos - 1] != ' ')
                        Backspace();
                },
                ["ControlT"] = TransposeChars,
                ["ControlLeftArrow"] = OneWordBackward,
                ["AltB"] = OneWordBackward,
                ["ControlRightArrow"] = OneWordForward,
                ["AltF"] = OneWordForward,
                ["AltC"] = () => {
                    // Capitalizes the current char and moves to the end of the word
                    if (IsBlank()) return;
                    ReplaceChar(char.ToUpperInvariant(_text[_cursorPos]));
                    EndOfWord();
                },
                ["AltU"] = () => {
                    // Capitalizes every char from the cursor to the end of the word
                    while(!(IsBlank() || IsEndOfLine())) 
                        ReplaceChar(char.ToUpperInvariant(_text[_cursorPos]));    
                },
                ["AltL"] = () => {
                    // Lowers the case of every char from the cursor to the end of the word
                    while (!(IsBlank() || IsEndOfLine()))
                        ReplaceChar(char.ToLowerInvariant(_text[_cursorPos]));
                },
                ["Tab"] = () => {
                    if (IsInAutoCompleteMode()) {
                        NextAutoComplete();
                    } else {
                        if (autoCompleteHandler == null || !IsEndOfLine())
                            return;

                        string text = _text.ToString();

                        _completionStart = text.LastIndexOfAny(autoCompleteHandler.Separators);
                        _completionStart = _completionStart == -1 ? 0 : _completionStart + 1;

                        _completions = autoCompleteHandler.GetSuggestions(text, _completionStart);
                        _completions = _completions?.Length == 0 ? null : _completions;

                        if (_completions == null)
                            return;

                        StartAutoComplete();
                    }
                },

                ["ShiftTab"] = () => {
                    if (IsInAutoCompleteMode()) {
                        PreviousAutoComplete();
                    }
                }
            };
        }

        public void Handle(ConsoleKeyInfo keyInfo)
        {
            _keyInfo = keyInfo;

            // If in auto complete mode and Tab wasn't pressed
            if (IsInAutoCompleteMode() && _keyInfo.Key != ConsoleKey.Tab)
                ResetAutoComplete();

            _keyActions.TryGetValue(BuildKeyInput(), out Action action);

            (action ?? WriteChar).Invoke();
        }
    }
}
