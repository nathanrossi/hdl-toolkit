﻿// Copyright 2011 Nathan Rossi - http://nathanrossi.com
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

namespace HDLToolkit.Console.Commands
{
	[Command("corexisegen")]
	public class CoreXiseGenCommand : BaseCommand
	{
		[Argument(ShortName = "o", LongName = "output")]
		public string OutputPath { get; set; }

		[Argument(ShortName = "u", LongName = "ucf")]
		public string UserConstraintsFile { get; set; }

		[Argument(Position = 0)]
		public string[] Cores { get; set; }

		public override void Execute()
		{
			base.Execute();

			if (!string.IsNullOrEmpty(OutputPath))
			{
				XilinxProjectFile prj = new XilinxProjectFile(Program.Repository);
				if (!string.IsNullOrEmpty(UserConstraintsFile))
				{
					prj.UserConstraintsFile = Path.GetFullPath(UserConstraintsFile);
					Logger.Instance.WriteVerbose("Including the user constaints located at '{0}'", prj.UserConstraintsFile);
				}

				foreach (string core in Cores)
				{
					Logger.Instance.WriteVerbose("Selecting Core: {0}", core);
					prj.AddAllInLibrary(prj.Environment.GetLibrary(core));
				}

				Logger.Instance.WriteVerbose("Generating...");
				File.WriteAllText(OutputPath, prj.ToString());
				Logger.Instance.WriteVerbose("Generated!");
			}
			else
			{
				Logger.Instance.WriteError("Output Path not specified, terminating...");
			}
		}
	}
}
