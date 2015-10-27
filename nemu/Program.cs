using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;

using IniParser;
using IniParser.Model;
using MediaInfoDotNet;

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
				if (string.Equals(args[0], "master", IC))
				{
					for (int i = 1; i < args.Length; i++)
					{
						if (args[i][0] == '-')
						{
							if (args[i].Length > 0)
							{
								if (args[i][1] == 'w')
								{
									DirWatch = args[++i];
								}

								if (args[i][1] == 'i')
								{
									Input = args[++i];
								}

								if (args[i][1] == 'e')
								{
									Encoder = args[++i];
								}

								if (args[i][1] == 'b')
								{
									BitDepth = Convert.ToInt32(args[i]);
								}
							}
						}

						if (args[i][0] == '(')
						{
							while (args[i][0] != ')')
							{
								EncArgs += $"\"{args[i++]}\" ";
							}
						}
					}

					Master();
				}
				else
				{
					
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
			Console.WriteLine("Usage: nemu [master|slave] -w [folder] -i [input] -e [x264|x265] -b [8|10|12] ( [encoder args] )");
			Console.WriteLine();
			Console.WriteLine("Main option:");
			Console.WriteLine("  master                       Start NEMU as master server (implies slave)");
			Console.WriteLine("  slave                        Start NEMU as salve server");
			Console.WriteLine("  -w                           Folder path that slave server can see");
			Console.WriteLine();
			Console.WriteLine("Master option:");
			Console.WriteLine("  -i                           Input video or AviSynth");
			Console.WriteLine("  -e                           Use x264 or x265 encoder");
			Console.WriteLine("  -b                           Set a Bit-Depth 8, 10 or 12");
			Console.WriteLine("  encoder args                 All possible x264 or x265 argument");
			Console.WriteLine();
			Console.WriteLine("Example:");
			Console.WriteLine($"  nemu master -w {(OS.IsLinux ? "/mnt/nas" : "d:\\share")} -i {(OS.IsLinux ? "~/" : "d:\\")}video.mkv -e x265 -b 10 ( --preset veryslow --tune ssim --crf 26 )");
			Console.WriteLine($"  nemu slave -w {(OS.IsLinux ? "/mnt/nas" : "z:\\")}");
			Console.WriteLine();
			Console.WriteLine("Due security reasons, NEMU is relying on Network Drive or Shared Folder to avoid attack/exploit");
			Console.WriteLine();
			Console.WriteLine("NEMU home page: <https://x265.github.io/>");
			Console.WriteLine("Report bugs to: <https://github.com/Anime4000/NEMU/issues>");
			Console.WriteLine("Facebook page : <https://fb.com/internetfriendlymediaencoder/>");
		}

		static void Master()
		{
			if (string.Equals(Path.GetExtension(Input), ".avs", IC))
			{

			}
			else
			{

			}
		}

		static void Slave()
		{

		}

		static void LoadConfig()
		{
			IniData data = new FileIniDataParser().ReadFile(FileConfig);

			BinFFmpeg = data["binary"]["ffmpeg"];
			BinAvs2pipe = data["binary"]["avs2pipe"];
			BinAVC08 = data["x264"]["08"];
			BinAVC10 = data["x264"]["10"];
			BinHEVC08 = data["x265"]["08"];
			BinHEVC10 = data["x265"]["10"];
			BinHEVC12 = data["x265"]["12"];
		}

		static readonly StringComparison IC = StringComparison.InvariantCultureIgnoreCase;
		static readonly string InstallDir = OS.IsLinux ? Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) : AppDomain.CurrentDomain.BaseDirectory;

		static string FileConfig { get; set; } = Path.Combine(InstallDir, "nemu.ini");
		static string DirWatch { get; set; }
		static string Input { get; set; }
		static string Encoder { get; set; }
		static int BitDepth { get; set; }
		static string EncArgs { get; set; }

		static string BinFFmpeg { get; set; }
		static string BinAvs2pipe { get; set; }
		static string BinAVC08 { get; set; }
		static string BinAVC10 { get; set; }
		static string BinHEVC08 { get; set; }
		static string BinHEVC10 { get; set; }
		static string BinHEVC12 { get; set; }
	}
}
