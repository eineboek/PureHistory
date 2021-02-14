using System;
using System.Globalization;
using System.Threading;
using static System.Console;

namespace PureHistory
{
    class Program
    {
        static CultureInfo selectedCulture;
        static void Main(string[] args)
        {
            Title = "PureHistory Installer";
            WriteLine(Resources.ModVersion + " - " + Resources.About + "\n");
            WriteLine("Navigation :");
            WriteLine(Resources.NavigationHelp1);
            WriteLine(Resources.NavigationHelp2);
            WriteLine(Resources.NavigationHelp3 + "\n");
            WriteLine(Resources.PressAnyKey);
            ReadKey();
            LanguageSelection();
            WriteLine(Resources.ExitProgram);
            ReadKey();
        }

        static private void LanguageSelection()
        {
            string prompt = Resources.SelectLanguage;
            string[] options = { "English", "Deutsch" };
            Menu selectLanguageMenu = new Menu(prompt, options);
            int selectedIndex = selectLanguageMenu.Init();

            switch (selectedIndex)
            {
                case 0:
                    selectedCulture = CultureInfo.CreateSpecificCulture("en-US");
                    break;
                case 1:
                    selectedCulture = CultureInfo.CreateSpecificCulture("de-DE");
                    break;
            }

            Thread.CurrentThread.CurrentCulture = selectedCulture;
            Thread.CurrentThread.CurrentUICulture = selectedCulture;
        }
    }
}
