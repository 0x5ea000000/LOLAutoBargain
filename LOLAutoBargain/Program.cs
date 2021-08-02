using LOLAutoBargain;
using System;
using System.Threading.Tasks;

namespace JPClientStart
{
    public class Program
    {
        private static int option;

        public static async Task Main()
        {
            Console.WriteLine("Welcome to AUTO Bargain Helper!");
            while(true)
            {
                ShowOption();
                option = int.Parse(Console.ReadLine());
                //option = 1;
                await ExecuteOptionAsync();
            }
            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();

        }

        private static void ShowOption()
        {
            Console.WriteLine("Select your option: ");
            Console.WriteLine("1. Auto enter codes");
            Console.WriteLine("2. Auto convert blue essence");
        }

        private static async Task ExecuteOptionAsync()
        {
            switch (option)
            {
                case 1:
                    await AutoBargain.Run();
                    break;
                case 2:
                    await AutoBlueEssence.Run();
                    break;
            }
        }
    }
}


   