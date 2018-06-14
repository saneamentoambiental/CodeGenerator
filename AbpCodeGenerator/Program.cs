using System;
using System.IO;
using System.Text;
using System.Linq;
using AbpCodeGenerator.Lib;
using Plossum.CommandLine;

namespace AbpCodeGenerator
{
	class Program
	{
		private static void Plosson(string[] args)
		{
			var options = new Options();
			var parser = new CommandLineParser(options);
			parser.Parse(args, false);
			
			Console.WriteLine(parser.UsageInfo.GetHeaderAsString(78));

			if (options.Help)
			{
				Console.WriteLine(parser.UsageInfo.GetOptionsAsString(78));
				return;
			}
			else if (parser.HasErrors)
			{
				Console.WriteLine(parser.UsageInfo.GetErrorsAsString(78));
				return;
			}
			else
			{
				options.ExecuteCommand();
			}
		}
		static void Main(string[] args)
		{
			var r = "";// "Pessoa -h -m -p";
			do
			{
				args = new string[] { $"-e {r}" };
				Plosson(args);
				ShowHelp();
				Console.WriteLine("");
				Console.WriteLine("Press new Entity or Enter To Exit");
				r = Console.ReadLine();
				Console.Clear();
			} while (!string.IsNullOrWhiteSpace(r));

		}

		private static void ShowHelp()
		{
			Console.WriteLine("");
			var parser = new CommandLineParser(new Options());
			Console.WriteLine(parser.UsageInfo.GetHeaderAsString(78));
			Console.WriteLine(parser.UsageInfo.GetOptionsAsString(78));
		}
	}
}
