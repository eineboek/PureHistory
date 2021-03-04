﻿using System;
using static System.Console;

namespace PureHistory
{
    /// <summary>
    /// Used to display a meu to the console with multiple selection choices
    /// </summary>
    internal class Menu
    {
        private int selectedIndex;
        private string[] _options;
        private string _prompt;

        /// <summary>
        /// Creates a new instance of the Menu class
        /// </summary>
        /// <param name="_prompt">The line of text to be displayed at the top of the menu</param>
        /// <param name="_options">The available options</param>
        public Menu(string prompt, string[] options)
        {
            _prompt = prompt;
            _options = options;
            selectedIndex = 0;
        }

        /// <summary>
        /// Draws the menu to the console with color accentuation for what item is selected
        /// </summary>
        private void Draw()
        {
            WriteLine(_prompt);
            for (int i = 0; i < _options.Length; i++)
            {
                string currentOption = _options[i];
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

        /// <summary>
        /// Initializes the options screen
        /// </summary>
        /// <returns>The index of the item that the user has clicked enter at</returns>
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
                        selectedIndex = _options.Length - 1;
                    }
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    selectedIndex++;
                    if (selectedIndex == _options.Length)
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