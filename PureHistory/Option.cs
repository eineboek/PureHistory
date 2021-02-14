using System;
using static System.Console;
namespace PureHistory
{
    class Option
    {
        private int selectedIndex;
        private string[] options;
        bool[] optionSelection;
        private string prompt;

        public Option(string _prompt, string[] _options, bool[] _optionSelection)
        {
            prompt = _prompt;
            options = _options;
            optionSelection = _optionSelection;
            selectedIndex = 0;
        }

        public void UpdateOptionSelection(bool[] _optionSelection)
        {
            optionSelection = _optionSelection;
        }
        private void Draw()
        {
            WriteLine(prompt+"\n");
            for (int i = 0; i < options.Length - 1; i++)
            {
                string currentOption = options[i];
                if (currentOption == null)
                {
                    currentOption = string.Empty;
                }
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
                    if (optionSelection[i] == true)
                    {
                        WriteLine($"{prefix} {currentOption} >>" + " - " + Resources.Yes);
                    }
                    else if (optionSelection[i] == false)
                    {
                        WriteLine($"{prefix} {currentOption} >>" + " - " + Resources.No);
                    }
                
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
                else if (keyPressed == ConsoleKey.RightArrow)
                {
                    selectedIndex = -1;
                }
            }
            while (keyPressed != ConsoleKey.Enter && keyPressed != ConsoleKey.RightArrow);

            return selectedIndex;
        }
    }

}
