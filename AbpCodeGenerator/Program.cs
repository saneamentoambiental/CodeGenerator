using CommandLine;
using System;
using System.Linq;
using System.Text;

namespace AbpCodeGenerator
{
    class Program
    {
        class Programa
        {
            public string[] args = new string[] { };
            public Programa(params string[] args) { } 
            public void ShowMenu()
            {

                var parser = new Parser(with => { with.EnableDashDash = false; with.HelpWriter = Console.Out; });
                do
                {
                    var result = parser.ParseArguments<Options>(args)
                        .WithParsed(options =>
                        {
                            options.ExecuteCommand();
                        });



                } while (!Quit());
            }
            private bool Quit()
            {
                Console.WriteLine("Prompt your command or q to quit");
                args = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                return args.Count() == 1 && "q".Equals(args[0], StringComparison.InvariantCultureIgnoreCase);
            }
        }
        private static void Main(params string[] args)
        {
            new Programa(args).ShowMenu();
            
        }
       
    }
}
