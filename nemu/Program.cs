using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading;

using IniParser;
using IniParser.Model;
using MediaInfoDotNet;

namespace nemu
{
	class Program
	{
		static int Main(string[] args)
		{
			Console.WriteLine("NEMU - Network Encoder Media Utility");
			Console.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");

			LoadConfig();

			if (args.Length > 1)
			{
				DirWatch = args[1];

				if (string.Equals(args[0], "-1", IC))
				{
					for (int i = 2; i < args.Length; i++)
					{
						if (args[i][0] == '-')
						{
							if (args[i].Length > 0)
							{
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
									BitDepth = Convert.ToInt32(args[++i]);
								}
							}
						}

						if (args[i][0] == '(')
						{
							while (args[++i][0] != ')')
							{
								EncArgs += $"\"{args[i]}\" ";
							}
						}
					}

					if (string.IsNullOrEmpty(DirWatch))
					{
						Console.WriteLine("Error: Watch folder not set!");
						return 1;
					}

					if (string.IsNullOrEmpty(Input))
					{
						Console.WriteLine("Error: Input not set!");
						return 1;
					}

					if (string.IsNullOrEmpty(Encoder))
					{
						Console.WriteLine("Error: Encoder not set!");
						return 1;
					}

					if (BitDepth == 0)
					{
						Console.WriteLine("Error: Bit Depth not set!");
						return 1;
					}

					return Master();
				}
				else
				{
					return Slave(Convert.ToInt32(args[0]));
				}
			}
			else
			{
				Help();
			}

			return 0;
		}

		static void Help()
		{
			Console.WriteLine();
			Console.WriteLine("Usage: nemu <id> <folder> [option_master]");
			Console.WriteLine();
			Console.WriteLine("Main option:");
			Console.WriteLine("  ID            Host unique id, -1 mean master, 0 or above mean slave");
			Console.WriteLine("  FOLDER        Folder path that slave server can see");
			Console.WriteLine();
			Console.WriteLine("Master option:");
			Console.WriteLine("  -i <file>     Input video or AviSynth");
			Console.WriteLine("  -e x264|x265  Use x264 or x265 encoder");
			Console.WriteLine("  -b 8|10|12    Set a Bit-Depth, 12bit x265 only");
			Console.WriteLine("  ( args )      Encoder argument, enclosed with parentheses, mind the space!");
			Console.WriteLine();
			Console.WriteLine("NEMU home page: <https://x265.github.io/>");
			Console.WriteLine("Report bugs to: <https://github.com/Anime4000/NEMU/issues>");
			Console.WriteLine("Facebook page : <https://fb.com/internetfriendlymediaencoder/>");
		}

		static int Master()
		{
			Console.WriteLine("Checking how many slave server is running");
			int count = 0;
			foreach (var item in Directory.GetFiles(DirWatch, "*.id"))
			{
				if (count != Convert.ToInt32(Path.GetFileNameWithoutExtension(item)))
				{
					Console.WriteLine($"Slave id {count} is missing!");
					Console.WriteLine($"You need start slave id from 0 and increment, non drop number.");
					return 1;
				}

				count++; // this will touch at last iteration, it has extra
			}

			string filename;
			if (string.Equals(Path.GetExtension(Input), ".avs", IC))
			{
				filename = Path.GetFileNameWithoutExtension(Input) + ".mp4";

				Console.WriteLine("Rendering AviSynth before proceed, this may take a while...");
				TaskManager.Run($"\"{BinAvs2pipe}\" video \"{Input}\" | \"{BinFFmpeg}\" -i - -c:v libx264 -preset {RenderPreset} -crf {RenderCRF} -y {Path.Combine(DirWatch, filename)}");
            }
			else
			{
				filename = Path.GetFileName(Input);

				Console.WriteLine("Copying file to watch folder, please wait...");
				File.Copy(Input, Path.Combine(DirWatch, filename), true);
			}

			// Read frame and split
			MediaFile AVI = new MediaFile(Path.Combine(DirWatch, filename));
			int frames = AVI.Video[0].frameCount;
			int each = frames / count;
			int last = frames % count;

			Console.WriteLine($"This input has {frames} frames");

			var job = new IniData();
			job.Sections.AddSection("master");
			job.Sections["master"].AddKey("file", Path.Combine(DirWatch, filename));
			job.Sections["master"].AddKey("enc", Encoder);
			job.Sections["master"].AddKey("arg", EncArgs);
			job.Sections["master"].AddKey("yuv", "420");
			job.Sections["master"].AddKey("bit", $"{BitDepth}");

			for (int i = 0; i < count; i++)
			{
				job.Sections.AddSection($"{i}");
				job.Sections[$"{i}"].AddKey("start", $"{(i == 0 ? 0 : (each * i) + 1)}");
				job.Sections[$"{i}"].AddKey("end", $"{(i == count - 1 ? each * (i + 1) + last : each * (i + 1))}");
			}

			new FileIniDataParser().WriteFile(Path.Combine(DirWatch, "job.ini"), job);

			// Read slave progress

			return 0;
		}

		static int Slave(int id)
		{
			string fileuid = Path.Combine(DirWatch, $"{id}.id");
			string filejob = Path.Combine(DirWatch, "job.ini");

			Console.CancelKeyPress += delegate
			{
				Console.WriteLine("Operation has been cancel!");
				File.Delete(fileuid);
			};

			if (File.Exists(fileuid))
			{
				Console.WriteLine("This id cannot be use, try different id");
				return 1;
			}

			Console.WriteLine($"Starting as slave server, #{id}");
			File.WriteAllText(fileuid, $"{id}");

			while (true)
			{
				if (File.Exists(filejob))
				{
					// read job file
					IniData data = new FileIniDataParser().ReadFile(filejob);
					string video = data["master"]["file"];
					string enc = data["master"]["enc"];
					string arg = data["master"]["arg"];
					int yuv = Convert.ToInt32(data["master"]["yuv"]);
					int bit = Convert.ToInt32(data["master"]["bit"]);
					int start = Convert.ToInt32(data[$"{id}"]["start"]);
					int end = Convert.ToInt32(data[$"{id}"]["end"]);
					int frames = (end - start) + 1;

					IniData xenc = new FileIniDataParser().ReadFile(FileConfig);
					string encoder = xenc[enc][$"{bit:00}"];

					TaskManager.Run($"\"{BinFFmpeg}\" -i \"{video}\" -vf \"select=between(n\\,{start}\\,{end}),setpts=PTS-STARTPTS\" -vframes {frames} -pix_fmt yuv{yuv}p{(bit > 8 ? $"{bit}le" : "")} -strict -1 -f yuv4mpegpipe - 2> {OS.Null} | \"{encoder}\" --y4m - {arg} --frames {frames} -o {Path.Combine(DirWatch, $"{id}")}.hevc");

					Console.WriteLine("Finished!");
					while (File.Exists(filejob)) { Thread.Sleep(1000); } // block until job file get deleted
				}
			}
		}

		static void LoadConfig()
		{
			IniData data = new FileIniDataParser().ReadFile(FileConfig);

			BinFFmpeg = data["binary"]["ffmpeg"];
			BinAvs2pipe = data["binary"]["avs2pipe"];

			RenderPreset = data["render"]["preset"];
			RenderCRF = data["render"]["crf"];

			BinAVC08 = data["x264"]["08"];
			BinAVC10 = data["x264"]["10"];
			BinHEVC08 = data["x265"]["08"];
			BinHEVC10 = data["x265"]["10"];
			BinHEVC12 = data["x265"]["12"];

			// Path Fix
			if (OS.IsLinux)
			{
				BinFFmpeg = BinFFmpeg.Replace("\\", "/");
				BinAvs2pipe = BinAvs2pipe.Replace("\\", "/");
				BinAVC08 = BinAVC08.Replace("\\", "/");
				BinAVC10 = BinAVC10.Replace("\\", "/");
				BinHEVC08 = BinHEVC08.Replace("\\", "/");
				BinHEVC10 = BinHEVC10.Replace("\\", "/");
				BinHEVC12 = BinHEVC12.Replace("\\", "/");
			}
			else
			{
				BinFFmpeg = BinFFmpeg.Replace("/", "\\");
				BinAvs2pipe = BinAvs2pipe.Replace("/", "\\");
				BinAVC08 = BinAVC08.Replace("/", "\\");
				BinAVC10 = BinAVC10.Replace("/", "\\");
				BinHEVC08 = BinHEVC08.Replace("/", "\\");
				BinHEVC10 = BinHEVC10.Replace("/", "\\");
				BinHEVC12 = BinHEVC12.Replace("/", "\\");
			}
		}

		public static readonly string InstallDir = OS.IsLinux ? Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) : AppDomain.CurrentDomain.BaseDirectory;
		public static readonly StringComparison IC = StringComparison.InvariantCultureIgnoreCase;

		static string FileConfig { get; set; } = Path.Combine(InstallDir, "nemu.ini");
		static string DirWatch { get; set; }
		static string Input { get; set; }
		static string Encoder { get; set; }
		static int BitDepth { get; set; } = 0;
		static string EncArgs { get; set; }

		static string RenderPreset { get; set; }
		static string RenderCRF { get; set; }

		static string BinFFmpeg { get; set; }
		static string BinAvs2pipe { get; set; }
		static string BinAVC08 { get; set; }
		static string BinAVC10 { get; set; }
		static string BinHEVC08 { get; set; }
		static string BinHEVC10 { get; set; }
		static string BinHEVC12 { get; set; }
	}
}
