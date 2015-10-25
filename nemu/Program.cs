using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using IniParser;
using IniParser.Model;

namespace nemu
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("NEMU - Network Encoder Media Utility");
			Console.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");

			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i][0] == '-')
					{
						if (args[i].Length > 1)
						{
							if (args[i][1] == '-')
							{
								if (args[i].Contains("help"))
								{
									Help();
								}

								if (args[i].Contains("master"))
								{
									IsMaster = true;
								}

								if (args[i].Contains("slave"))
								{
									IsMaster = false;
								}

								if (args[i].Contains("config"))
								{
									Config = args[++i];
								}

								if (args[i].Contains("dir"))
								{
									Folder = args[++i];
								}
							}

							for (int j = 1; j < args[i].Length; j++)
							{
								if (args[i][j] == 'h')
								{
									Help();
								}

								if (args[i][j] == 'v')
								{
									Console.WriteLine("Hi");
									return;
								}

								if (args[i][j] == 'm')
								{
									IsMaster = true;
								}

								if (args[i][j] == 's')
								{
									IsMaster = false;
								}

								if (args[i][j] == 'c')
								{
									Config = args[++i];
								}

								if (args[i][j] == 'd')
								{
									Folder = args[++i];
								}
							}
						}
					}
					else
					{
						//
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
			Console.WriteLine();
			Console.WriteLine("Usage: nemu [OPTION...] INPUT");
			Console.WriteLine();
			Console.WriteLine("Mandatory arguments to long options are mandatory for short options too.");
			Console.WriteLine();
			Console.WriteLine("Option:");
			Console.WriteLine("  -h, --help                   show this help and exit");
			Console.WriteLine("  -v                           show version and exit");
			Console.WriteLine("  -m, --master                 start NEMU as master server");
			Console.WriteLine("  -s, --slave                  start NEMU as slave server (impiles -m)");
			Console.WriteLine("  -c, --config CONFIG.INI      (optional) override default path config.ini");
			Console.WriteLine("  -d, --dir FOLDER             (optional) override default from config.ini");
			Console.WriteLine("  INPUT                        Video file, AviSynth file (windows only)");
			Console.WriteLine();
			Console.WriteLine("NEMU home page: <https://x265.github.io/>");
			Console.WriteLine("Report bugs to: <https://github.com/Anime4000/IFME/issues>");
			Console.WriteLine("Facebook page : <https://fb.com/internetfriendlymediaencoder/>");
		}

		static void Master()
		{
			// gg
		}

		static void Slave()
		{

		}

		static void LoadConfig()
		{
			IniData Data = new FileIniDataParser().ReadFile(Config, Encoding.UTF8);

			Id = int.Parse(Data["config"]["id"]);
			WatchFolder = Data["config"]["folder"];
			BinAvsPipe = Data["binary"]["avidec"];
			BinFFmpeg = Data["binary"]["ffmpeg"];
			BinHEVC08 = Data["binary"]["08bit"];
			BinHEVC10 = Data["binary"]["10bit"];
			BinHEVC12 = Data["binary"]["12bit"];

			if (!string.IsNullOrEmpty(Folder))
				WatchFolder = Folder;
		}

		static bool IsMaster { get; set; }

		static string Config { get; set; } = "config.ini";
		static string Folder { get; set; } = null;

		static int Id { get; set; }
		static string WatchFolder { get; set; }
		static string BinAvsPipe { get; set; }
		static string BinFFmpeg { get; set; }
		static string BinHEVC08 { get; set; }
		static string BinHEVC10 { get; set; }
		static string BinHEVC12 { get; set; }
	}
}
