using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace nemu
{
	class TaskManager
	{
		public static int Run(string cmd)
		{
			string cli;
			string arg;

			if (OS.IsWindows)
			{
				Environment.SetEnvironmentVariable("NEMU", cmd, EnvironmentVariableTarget.Process);
				cli = "cmd";
				arg = $"/c %NEMU%";
			}
			else
			{
				cli = "bash";
				arg = $"-c '{cmd}'";
			}

			var p = new Process();
			p.StartInfo = new ProcessStartInfo(cli, arg)
			{
				UseShellExecute = false,
				WorkingDirectory = Program.InstallDir
			};

			p.Start();
			p.WaitForExit();

			return p.ExitCode;
		}
	}
}
