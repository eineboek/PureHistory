using System;
using static System.Console;

namespace PureHistory
{
    /// <summary>
    /// Slightly changed copy of the Menu class with selection for Yes/No
    /// </summary>
    internal class MultipleChoiceOption
    {
        private int selectedIndex;
        private string[] _choices;
        private bool[] _choiceSelection;
        private string _prompt;

        /// <summary>
        /// Creates a new Option class instance
        /// </summary>
        /// <param name="prompt">The line of text to be displayed at the top of the options</param>
        /// <param name="choices">The available choices</param>
        /// <param name="choiceSelection">Which of the options is selected. Standard should be : all false</param>
        public MultipleChoiceOption(string prompt, string[] choices, bool[] choiceSelection)
        {
            _prompt = prompt;
            _choices = choices;
            _choiceSelection = choiceSelection;
            selectedIndex = 0;
        }

        /// <summary>
        /// Updates the choice selection array after the user has toggled an option
        /// </summary>
        /// <param name="choiceSelection">The updated array</param>
        public void UpdateOptionSelection(bool[] choiceSelection)
        {
            _choiceSelection = choiceSelection;
        }

        /// <summary>
        /// Draws the options to the console with color accentuation
        /// </summary>
        private void Draw()
        {
            WriteLine(_prompt + "\r\n");

            int highestStringLength = 0;
            foreach (string choice in _choices)
            {
                if (choice.Length > highestStringLength)
                {
                    highestStringLength = choice.Length;
                }
            }

            for (int i = 0; i < _choices.Length; i++)
            {
                string currentChoice = _choices[i];
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
                if (_choiceSelection[i])
                {
                    WriteLine($"{prefix} {currentChoice} " + WhiteSpace(highestStringLength - currentChoice.Length) + ">> " + Resources.Yes);
                }
                else if (!_choiceSelection[i])
                {
                    WriteLine($"{prefix} {currentChoice} " + WhiteSpace(highestStringLength - currentChoice.Length) + ">> " + Resources.No);
                }
            }
            ResetColor();
        }

        /// <summary>
        /// Initializes the options screen
        /// </summary>
        /// <returns>This method returns a MultipleChocieResponse object which stores the data</returns>
        public MultipleChoiceResponse Init()
        {
            ConsoleKey keyPressed;
            while (true)
            {
                Clear();
                Draw();

                ConsoleKeyInfo keyInfo = ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow) //Up Arrow Key : Navigate up between choices
                {
                    selectedIndex--;
                    if (selectedIndex == -1)
                    {
                        selectedIndex = _choices.Length - 1;
                    }
                }
                else if (keyPressed == ConsoleKey.DownArrow) //Down Arrow Key : Navigate down between choices
                {
                    selectedIndex++;
                    if (selectedIndex == _choices.Length)
                    {
                        selectedIndex = 0;
                    }
                }
                else if (keyPressed == ConsoleKey.Enter) //ENTER key : Toggle option - returns the selected option as an integer
                {
                    return new MultipleChoiceResponse(false, false, selectedIndex);
                }
                else if (keyPressed == ConsoleKey.LeftArrow) //Left Arrow Key : Go back to previous page
                {
                    return new MultipleChoiceResponse(true, false, null);
                }
                else if (keyPressed == ConsoleKey.RightArrow) //Right Arrow Key : Continue to next page
                {
                    return new MultipleChoiceResponse(false, true, null);
                }
            }
        }

        private string WhiteSpace(int amount)
        {
            string whiteSpace = string.Empty;

            while (whiteSpace.Length < amount)
            {
                whiteSpace += " ";
            }

            return whiteSpace;
        }
    }

    internal class MultipleChoiceResponse
    {
        public bool ReturnToPrevious { get; private set; }

        public bool ContinueToNext { get; private set; }

        public int? ToggleSelectedIndex { get; private set; }

        public MultipleChoiceResponse(bool returnToPrevious, bool continueToNext, int? toggleSelectedIndex)
        {
            ReturnToPrevious = returnToPrevious;
            ContinueToNext = continueToNext;
            ToggleSelectedIndex = toggleSelectedIndex;
        }
    }
}