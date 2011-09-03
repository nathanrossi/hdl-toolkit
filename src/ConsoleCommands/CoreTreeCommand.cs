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
using HDLToolkit.Framework;
using HDLToolkit.Xilinx;
using HDLToolkit.Xilinx.Simulation;

namespace HDLToolkit.ConsoleCommands
{
	[Command("coretree")]
	public class CoreTreeCommand : BaseCommand
	{
		[Argument(ShortName = "m", LongName = "display-modules")]
		public bool DisplayModules { get; set; }

		[Argument(ShortName = "c", LongName = "display-components")]
		public bool DisplayComponents { get; set; }

		[Argument(Position = 0)]
		public string[] Cores { get; set; }

		public override void Execute()
		{
			base.Execute();

			if (DisplayComponents)
			{
				Logger.Instance.WriteWarning("Display of components currently not implemented");
			}

			foreach (string core in Cores)
			{
				ILibrary library = Program.Repository.GetLibrary(core);
				Logger.Instance.WriteInfo("");
				PrintTreeForCore(library);
			}
		}

		public void PrintTreeForCore(ILibrary library)
		{
			Logger.Instance.WriteInfo("{0} {1} (Root)", TreeNodeRoot, library.Name);
			TreePrintNodes(null, library, TreeRootEmpty, DisplayModules);
		}

		#region TreeStringHelpers
		const string TreeNodeRoot = "o";
		const string TreeRootEmpty = "";
		const string TreeNode = "\x251c-o"; // (char)0x251C
		const string TreeNodeEnd = "\x2514-o"; // (char)0x2514
		const string TreeEmpty = "\x2502 "; // (char)0x2502

		private static void TreePrintNodes(List<ILibrary> path, ILibrary library, string basePad, bool displayModules)
		{
			// Build the path for the current node in the tree
			List<ILibrary> treePath = path;
			if (treePath == null)
			{
				treePath = new List<ILibrary>();
			}
			// Add the current node to the path
			treePath.Add(library);

			List<ILibrary> references = new List<ILibrary>(library.References);
			string spacePadding = basePad + TreeEmpty;
			string emptyPadding = basePad + StringHelpers.ExpandString(" ", TreeEmpty.Length);
			string nodePadding = basePad;

			if (displayModules)
			{
				List<IModule> modules = new List<IModule>(library.Modules);
				for (int i = 0; i < modules.Count; i++)
				{
					bool lastNode = (i == modules.Count - 1) && (references.Count == 0);
					Logger.Instance.WriteInfo("{0}{1} {2} ({3})",
						nodePadding,
						(lastNode ? TreeNodeEnd : TreeNode),
						modules[i].Name,
						modules[i].RelativeLocation);
				}
			}

			for (int i = 0; i < references.Count; i++)
			{
				bool lastNode = (i == references.Count - 1);
				// Detect a possible recursive loop
				bool recursiveLoop = treePath.Contains(references[i]);

				Logger.Instance.WriteInfo("{0}{1} {2}{3}", 
					nodePadding,
					(lastNode ? TreeNodeEnd : TreeNode),
					references[i].ToString(),
					(recursiveLoop ? " (Cyclic Reference)" : ""));

				// Only print of a recurive loop does not exist
				if (!recursiveLoop)
				{
					TreePrintNodes(treePath, references[i], (lastNode ? emptyPadding : spacePadding), displayModules);
				}
			}

			// Remove the current not from the path
			treePath.RemoveAt(treePath.Count - 1);
		}
		#endregion
	}
}
