// Copyright 2011 Nathan Rossi - http://nathanrossi.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NConsole;
using HDLToolkit.Xilinx;
using System.IO;

namespace HDLToolkit.ConsoleCommands
{
	public class BaseCommand : ICommand
	{
		[Argument(ShortName = "r", LongName = "repo")]
		public string[] AdditionalRepositories { get; set; }

		[Argument(LongName = "disable-xilinx-repo")]
		public bool DisableXilinxRepository { get; set; }

		[Argument(LongName = "disable-environment-repo")]
		public bool DisableEnvironmentRepository { get; set; }

		[Argument(ShortName = "v", LongName = "verbose")]
		public bool VerboseOutput { get; set; }

		[Argument(LongName = "debugmode")]
		public bool DebugOutput { get; set; }

		public virtual void Execute()
		{
			if (DebugOutput)
			{
				Logger.Instance.VerbosityLevel = Logger.Verbosity.Debug;
			}
			else if (VerboseOutput)
			{
				Logger.Instance.VerbosityLevel = Logger.Verbosity.Low;
			}
			else
			{
				Logger.Instance.VerbosityLevel = Logger.Verbosity.Off;
			}

			// Get Xilinx Root Directory
			if (!DisableXilinxRepository)
			{
				Program.Repository.AddSearchPath(PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "EDK", "hw"));
			}

			// Get Enivornment Repositories
			if (AdditionalRepositories != null && AdditionalRepositories.Length >= 1)
			{
				foreach (string path in AdditionalRepositories)
				{
					string fullPath = Path.GetFullPath(path.Trim('"'));
					Logger.Instance.WriteVerbose("Repository: Adding {0}", fullPath);
					Program.Repository.AddSearchPath(fullPath);
				}
			}

			// Get Enivornment Repositories
			if (!DisableEnvironmentRepository)
			{
				string environRepos = Environment.GetEnvironmentVariable("REPOSITORY");
				if (!string.IsNullOrEmpty(environRepos))
				{
					string[] paths = environRepos.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string path in paths)
					{
						string fullPath = Path.GetFullPath(path.Trim('"'));
						Logger.Instance.WriteVerbose("Repository: Adding {0}", fullPath);
						Program.Repository.AddSearchPath(fullPath);
					}
				}
			}
		}
	}
}
