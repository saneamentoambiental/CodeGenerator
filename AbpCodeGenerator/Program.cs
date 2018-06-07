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
			args = new string[] { "-h","-e Pessoa -m" };
			//args = new string[] { "-h" };
			//args = new string[] { "tar -x -v -z -f file.tar.gz" };
			Plosson(args);
			
			Console.Read();
		}
	}
}
