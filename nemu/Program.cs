using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nemu
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("NEMU - Network Encoder Media Utility");

			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i][0] == '-')
					{
						if (args[i].Length > 1)
						{

						}
					}
					else
					{

					}
				}
			}
			else
			{
				Help();
			}
		}

		static void Help()
		{
			Console.WriteLine("Usage: nemu [OPTION...] CONFIG.INI FOLDER");
			Console.WriteLine();
			Console.WriteLine("Mandatory arguments to long options are mandatory for short options too.");
			Console.WriteLine();
			Console.WriteLine("Option:");
			Console.WriteLine("  -h, --help                   show this help and exit");
			Console.WriteLine("  -v                           show version and exit");
			Console.WriteLine("  -m, --master                 start NEMU as master server");
			Console.WriteLine("  -s, --slave                  start NEMU as slave server (impiles -m)");
			Console.WriteLine("  CONFIG.INI                   (optional) override default path config.ini");
			Console.WriteLine("  FOLDER                       (optional) override default from config.ini");
			Console.WriteLine();
			Console.WriteLine("NEMU home page: <https://x265.github.io/>");
			Console.WriteLine("Report bugs to: <https://github.com/Anime4000/IFME/issues>");
			Console.WriteLine("IFME fb page  : <https://fb.com/internetfriendlymediaencoder/>");
		}

		static void Master()
		{

		}

		static void Slave()
		{

		}
	}
}
