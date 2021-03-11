using System;
using System.Diagnostics;
using static System.Console;

namespace PureHistory
{
    /// <summary>
    /// Used to display a meu to the console with multiple selection choices
    /// </summary>
    internal class Menu : ConsoleLog
    {
        private int selectedIndex;
        private string[] options;
        private string[] consoleContent;

        /// <summary>
        /// Creates a new instance of the Menu class
        /// </summary>
        /// <param name="_title">The line of text to be displayed at the top of the menu</param>
        /// <param name="_options">The available options</param>
        public Menu(string[] consoleContent, string[] options)
        {
            this.consoleContent = consoleContent;
            this.options = options;
            selectedIndex = 0;
        }

        /// <summary>
        /// Draws the menu to the console with color accentuation for what item is selected
        /// </summary>
        private void Draw()
        {
            foreach (string line in consoleContent)
            {
                WriteLine(line);
            }

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
                Write($"{prefix} << {currentOption} >>");
                ResetColor();
                Write("\r\n");
            }
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

            ResetColor();
            return selectedIndex;
        }
    }

    /// <summary>
    /// Slightly changed copy of the Menu class with selection for Yes/No
    /// </summary>
    internal class MultipleChoiceOption : ConsoleLog
    {
        private int selectedIndex;
        private string title;
        private string warning;
        private string[] choices;
        private bool[] choiceSelection;

        /// <summary>
        /// Creates a new Option class instance
        /// </summary>
        /// <param name="title">The line of text to be displayed at the top of the options</param>
        /// <param name="choices">The available choices</param>
        /// <param name="choiceSelection">Which of the options is selected. Standard should be : all false</param>
        public MultipleChoiceOption(string title, string warning, string[] choices, bool[] choiceSelection)
        {
            this.title = title;
            this.warning = warning;
            this.choices = choices;
            this.choiceSelection = choiceSelection;
            selectedIndex = 0;
        }

        public MultipleChoiceOption(string title, string[] choices, bool[] choiceSelection)
        {
            this.title = title;
            warning = null;
            this.choices = choices;
            this.choiceSelection = choiceSelection;
            selectedIndex = 0;
        }

        /// <summary>
        /// Updates the choice selection array after the user has toggled an option
        /// </summary>
        /// <param name="choiceSelection">The updated array</param>
        public void UpdateOptionSelection(bool[] choiceSelection)
        {
            this.choiceSelection = choiceSelection;
        }

        /// <summary>
        /// Draws the options to the console with color accentuation
        /// </summary>
        private void Draw()
        {
            WriteLine(title);

            //If there is a warning to be displayed, it is drawn in red.
            if (warning != null)
            {
                ForegroundColor = ConsoleColor.DarkRed;
                WriteLine(warning);
                ResetColor();
            }

            //Determine the White Space required to align the options
            int highestStringLength = 0;
            foreach (string choice in choices)
            {
                if (choice.Length > highestStringLength)
                {
                    highestStringLength = choice.Length;
                }
            }

            for (int i = 0; i < choices.Length; i++)
            {
                string currentChoice = choices[i];
                string prefix;

                if (i == selectedIndex)
                {
                    //Color accentuation and asterisk prefix
                    prefix = "*";
                    ForegroundColor = ConsoleColor.Black;
                    BackgroundColor = ConsoleColor.White;
                }
                else
                {
                    //No color accentuation and no prefix
                    prefix = " ";
                    ForegroundColor = ConsoleColor.White;
                    BackgroundColor = ConsoleColor.Black;
                }
                if (choiceSelection[i])
                {
                    WriteLine($"{prefix} {currentChoice} " + WhiteSpace(highestStringLength - currentChoice.Length) + ">> " + "[X]");
                }
                else if (!choiceSelection[i])
                {
                    WriteLine($"{prefix} {currentChoice} " + WhiteSpace(highestStringLength - currentChoice.Length) + ">> " + "[ ]");
                }
            }
            ResetColor();
            WriteLine();
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
                        selectedIndex = choices.Length - 1;
                    }
                }
                else if (keyPressed == ConsoleKey.DownArrow) //Down Arrow Key : Navigate down between choices
                {
                    selectedIndex++;
                    if (selectedIndex == choices.Length)
                    {
                        selectedIndex = 0;
                    }
                }
                else if (keyPressed == ConsoleKey.Enter) //ENTER key : Toggle option - returns the selected option as an integer
                {
                    ResetColor();
                    return new MultipleChoiceResponse(false, false, selectedIndex);
                }
                else if (keyPressed == ConsoleKey.LeftArrow) //Left Arrow Key : Go back to previous page
                {
                    ResetColor();
                    return new MultipleChoiceResponse(true, false, null);
                }
                else if (keyPressed == ConsoleKey.RightArrow) //Right Arrow Key : Continue to next page
                {
                    ResetColor();
                    return new MultipleChoiceResponse(false, true, null);
                }
            }
        }

        /// <summary>
        /// Returns a set amount of white space in a string
        /// </summary>
        /// <param name="amount">The amount of white spaces</param>
        /// <returns></returns>
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

    /// <summary>
    /// The class that holds Properties to evaluate the response of the User in a Multiple Choice Option
    /// </summary>
    internal class MultipleChoiceResponse
    {
        public bool ReturnToPrevious { get; private set; } //Determines if the user has pressed the left arrow key

        public bool ContinueToNext { get; private set; } //Determines if the user has pressed the right arrow key

        public int? ToggleSelectedIndex { get; private set; } //Determines the index at which the user has pressed ENTER. If ENTER hasnt been pressed, it is set to null

        public MultipleChoiceResponse(bool returnToPrevious, bool continueToNext, int? toggleSelectedIndex)
        {
            ReturnToPrevious = returnToPrevious;
            ContinueToNext = continueToNext;
            ToggleSelectedIndex = toggleSelectedIndex;
        }
    }
}