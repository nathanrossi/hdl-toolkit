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
using HDLToolkit.ConsoleCommands;
using System.Threading;

namespace HDLToolkit.Xilinx.Simulation
{
	public class ISimInstance
	{
		public PrjFile Project { get; private set; }
		public IModule TopModule { get; private set; }

		public bool UseGraphicalUserInterface { get; set; }

		private bool running = false;
		private string currentWorkingDirectory;
		private FuseBuild.BuildResult currentResult;
		private ISimProcess isimProcess;

		public ISimInstance(PrjFile project, IModule topModule)
		{
			Project = project;
			TopModule = topModule;
		}

		private string GenerateWorkingDirectory()
		{
			string path = PathHelper.Combine(Path.GetTempPath(), "isim-temp", Guid.NewGuid().ToString());
			Logger.Instance.WriteVerbose("Creating temporary working directory at '{0}'", path);
			Directory.CreateDirectory(path);
			return path;
		}

		/// <summary>
		/// Builds simulation files
		/// </summary>
		private void BuildSimulation()
		{
			if (currentWorkingDirectory != null)
			{
				CleanSimulation();
			}

			currentWorkingDirectory = GenerateWorkingDirectory();

			// Start Building
			currentResult = FuseBuild.BuildProject(currentWorkingDirectory, Project, TopModule);

			Logger.Instance.WriteDebug("Fuse Build Result:");
			Logger.Instance.WriteDebug(currentResult.BuildLog);
		}

		/// <summary>
		/// Cleans up temporary simulation files
		/// </summary>
		private void CleanSimulation()
		{
			Directory.Delete(currentWorkingDirectory, true);

			currentWorkingDirectory = null;
			currentResult = null;
			isimProcess = null;
		}

		public void Start()
		{
			if (running)
			{
				throw new Exception("Stop the instance before starting");
			}

			// System is now running
			running = true;

			BuildSimulation();

			// Execute the ISim Process
			Logger.Instance.WriteVerbose("Starting ISim Process at '{0}'", currentResult.ExecutableFile);
			isimProcess = new ISimProcess(currentWorkingDirectory, currentResult.ExecutableFile);
			isimProcess.RunGraphicalUserInterface = UseGraphicalUserInterface;
			isimProcess.Start();

			// Current example process
			while (isimProcess.Running)
			{
				// If using gui, dont run console prompt mode
				if (!UseGraphicalUserInterface)
				{
					isimProcess.WaitForPrompt();

					Console.Write("$ ");
					string command = Console.ReadLine();
					Console.WriteLine(isimProcess.InjectCommand(command));
				}
				else
				{
					Thread.Sleep(1000);
				}
			}

			Logger.Instance.WriteVerbose("ISim terminated");
		}

		public void Stop()
		{
			if (!running)
			{
				throw new Exception("Start the instance before stopping");
			}

			// System is not running anymore
			running = false;

			isimProcess.Stop();

			CleanSimulation();
		}
	}
}
