using System;
using static System.Console;

namespace PureHistory
{
    class Menu
    {
        private int selectedIndex;
        private string[] options;
        private string prompt;

        public Menu(string _prompt, string[] _options)
        {
            prompt = _prompt;
            options = _options;
            selectedIndex = 0;
        }

        private void Draw()
        {
            WriteLine(prompt);
            for(int i = 0; i < options.Length; i++)
            {
                string currentOption = options[i];
                string prefix;

                if (i == selectedIndex)
                {
                    prefix = "*";
                    ForegroundColor = ConsoleColor.Black;
                    BackgroundColor = ConsoleColor.White;
                }
                else
                {
                    prefix = " ";
                    ForegroundColor = ConsoleColor.White;
                    BackgroundColor = ConsoleColor.Black;
                }
                WriteLine($"{prefix} << {currentOption} >>");
            }
            ResetColor();
        }

        public int Init()
        {
            ConsoleKey keyPressed;
            do
            {
                Clear();
                Draw();

                ConsoleKeyInfo keyInfo = ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    selectedIndex--;
                    if (selectedIndex == -1)
                    {
                        selectedIndex = options.Length - 1;
                    }
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    selectedIndex++;
                    if (selectedIndex == options.Length)
                    {
                        selectedIndex = 0;
                    }
                }
            }
            while (keyPressed != ConsoleKey.Enter);

            return selectedIndex;
        }
    }
}
