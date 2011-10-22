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
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Console.Commands
{
	[Command("listknowndevices")]
	public class ListKnownDevicesCommand : BaseCommand
	{
		[Argument(Position = 0)]
		public string SearchTerm { get; set; }

		public override void Execute()
		{
			base.Execute();

			XilinxDeviceTree devTree = new XilinxDeviceTree();
			devTree.LoadDevices();

			if (string.IsNullOrEmpty(SearchTerm))
			{
				foreach (IPartFamily family in devTree.Families)
				{
					DisplayFamilyTree(family);
				}
			}
			else
			{
				IPartFamily family = devTree.FindFamily(SearchTerm);
				if (family == null)
				{
					Logger.Instance.WriteInfo("Valid families:");
					foreach (IPartFamily f in devTree.Families)
					{
						Logger.Instance.WriteInfo("\t{0} - ({1})", f.ShortName, f.Name);
					}

					Logger.Instance.WriteError("Invalid family specified");
				}
				else
				{
					DisplayFamilyTree(family);
				}
			}
		}

		public void DisplayFamilyTree(IPartFamily family)
		{
			Logger.Instance.WriteInfo("o {0} ({1})", family.Name, family.ShortName);
			foreach (IPart part in family.Parts)
			{
				Logger.Instance.WriteInfo("  + {0}", part.Name);
				foreach (IPartDevice device in part.Devices)
				{
					Logger.Instance.WriteInfo("    + {0} ({1})", device.Name, device.Package.Name);

					StringBuilder sb = new StringBuilder();
					foreach (IPartSpeed speed in device.Speeds)
					{
						sb.Append(speed.Name);
						sb.Append(", ");
					}
					sb.Remove(sb.Length - 2, 2);

					Logger.Instance.WriteInfo("      + {0}", sb.ToString());
				}
			}
		}
	}
}
