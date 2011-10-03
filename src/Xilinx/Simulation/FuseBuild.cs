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
using HDLToolkit.Framework;
using System.IO;

namespace HDLToolkit.Xilinx.Simulation
{
	public static  class FuseBuild
	{
		public class BuildResult
		{
			public string WorkingDirectory { get; set; }
			public string ExecutableFile { get; set; }

			public bool Built { get; set; }

			public string BuildLog { get; set; }
		}

		public static BuildResult BuildProject(string workingDirectory, PrjFile projectFile, IModule topModule)
		{
			string fusePath = XilinxHelper.GetXilinxToolPath("fuse.exe");
			if (string.IsNullOrEmpty(fusePath))
			{
				fusePath = XilinxHelper.GetXilinxToolPath("fuse");
				if (string.IsNullOrEmpty(fusePath))
				{
					throw new Exception("Unable to find the fuse Executable");
				}
			}

			// Create prj file on disk
			string projectExecutablePath = PathHelper.Combine(workingDirectory, "x.exe");
			string projectFilePath = PathHelper.Combine(workingDirectory, "projectfile.prj");
			File.WriteAllText(projectFilePath, projectFile.ToString(ExecutionType.SimulationOnly));

			List<string> arguments = new List<string>();
			arguments.Add(string.Format("--prj \"{0}\"", projectFilePath));
			//arguments.Add(string.Format("-o \"{0}\"", projectExecutablePath));
			arguments.Add(string.Format("{0}.{1}", topModule.Parent.Name, topModule.Name));

			ProcessHelper.ProcessExecutionResult result = XilinxProcess.ExecuteProcess(workingDirectory, fusePath, arguments);

			BuildResult buildResult = new BuildResult();
			buildResult.BuildLog = result.StandardOutput + "\n\n\n" +  result.StandardError;
			buildResult.WorkingDirectory = workingDirectory;
			buildResult.ExecutableFile = projectExecutablePath;
			buildResult.Built = true;

			File.Delete(projectFilePath);

			return buildResult;
		}
	}
}
