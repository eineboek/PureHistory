using System;
using static System.Console;

namespace PureHistory
{
    /// <summary>
    /// Slightly changed copy of the Menu class with selection for Yes/No
    /// </summary>
    internal class Option
    {
        private int selectedIndex;
        private string[] options;
        private bool[] optionSelection;
        private string prompt;

        /// <summary>
        /// Creates a new Option class instance
        /// </summary>
        /// <param name="_prompt">The line of text to be displayed at the top of the options</param>
        /// <param name="_options">The available options</param>
        /// <param name="_optionSelection">Which of the options is selected. Standard should be all false</param>
        public Option(string _prompt, string[] _options, bool[] _optionSelection)
        {
            prompt = _prompt;
            options = _options;
            optionSelection = _optionSelection;
            selectedIndex = 0;
        }

        /// <summary>
        /// Updates the option selection array after the user has toggled an option
        /// </summary>
        /// <param name="_optionSelection">The updated array</param>
        public void UpdateOptionSelection(bool[] _optionSelection)
        {
            optionSelection = _optionSelection;
        }

        /// <summary>
        /// Draws the options to the console with color accentuation
        /// </summary>
        private void Draw()
        {
            WriteLine(prompt + "\n");
            for (int i = 0; i < options.Length; i++)
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
                if (optionSelection[i])
                {
                    WriteLine($"{prefix} {currentOption} >>" + " - " + Resources.Yes);
                }
                else if (!optionSelection[i])
                {
                    WriteLine($"{prefix} {currentOption} >>" + " - " + Resources.No);
                }
            }
            ResetColor();
        }

        /// <summary>
        /// Initializes the options screen
        /// </summary>
        /// <returns>The index of the option that the user has clicked enter at / -1 when the right arrow key has been pressed</returns>
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