using System;

namespace UI
{
    public static class Input
    {
        public static string RequestInput()
        {
            string input = "";
            bool exit = false;
            while (!exit)
            {
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    exit = true;
                }
                else
                {
                    Console.WriteLine("Enter a non-empty string");
                }
            }
            return input;
        }

        public static int SelectMenuOption(string message, int min, int max)
        {
            Console.WriteLine(message);
            int option = Int32.MaxValue;
            bool exit = false;
            while (!exit)
            {
                string inputOption = Console.ReadLine();
                bool parseOk = int.TryParse(inputOption, out option);
                if (parseOk)
                {
                    if (option >= min && option <= max)
                    {
                        exit = true;
                    }
                    else
                    {
                        Console.WriteLine("Select a valid option");
                    }
                }
                else
                {
                    Console.WriteLine("Input must be a number");
                }
            }
            return option;
        }

    }
}