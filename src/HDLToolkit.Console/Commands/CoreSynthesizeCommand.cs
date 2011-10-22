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
using HDLToolkit.Framework;
using HDLToolkit.Xilinx.Synthesis;
using System.IO;

namespace HDLToolkit.Console.Commands
{
	[Command("coresyn")]
	public class CoreSynthesizeCommand : BaseCommand
	{
		[Argument(ShortName = "o", LongName = "output")]
		public string OutputPath { get; set; }

		[Argument(Position = 0)]
		public string[] Cores { get; set; }

		[Argument(ShortName = "t", LongName = "top")]
		public string TopModule { get; set; }

		public override void Execute()
		{
			base.Execute();

			if (!string.IsNullOrEmpty(TopModule))
			{
				PrjFile prj = new PrjFile(Program.Repository);

				foreach (string core in Cores)
				{
					Logger.Instance.WriteVerbose("Selecting Core: {0}", core);
					prj.AddAllInLibrary(prj.Environment.GetLibrary(core));
				}

				// Select top module
				string[] splitModule = TopModule.Split('.');

				ILibrary library = prj.Environment.GetLibrary(splitModule[0]);
				if (library != null)
				{
					IModule module = library.Modules.First((m) => string.Compare(m.Name, splitModule[1], true) == 0);
					if (module != null)
					{
						Logger.Instance.WriteVerbose("Selected module '{0}' in library '{1}'", module.Name, library.Name);

						string workingDirectory = XilinxSynthesizer.GenerateWorkingDirectory();
						Logger.Instance.WriteVerbose("Starting Build");
						XilinxSynthesizer.BuildResult result = XilinxSynthesizer.BuildProject(workingDirectory, prj, module);
						Logger.Instance.WriteVerbose("Build Complete");

						Logger.Instance.WriteDebug(result.BuildLog);

						Logger.Instance.WriteVerbose("Cleaning temporary directory");
						Directory.Delete(workingDirectory, true);
					}
					else
					{
						Logger.Instance.WriteError("Top Level module does not existing in library '{0}'", library.Name);
					}
				}
				else
				{
					Logger.Instance.WriteError("Top Level module library does not exist in the repository");
				}
			}
			else
			{
				Logger.Instance.WriteError("Top Level Module not specified, terminating...");
			}
		}
	}
}
