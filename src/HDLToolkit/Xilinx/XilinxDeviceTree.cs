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
using System.IO;
using HDLToolkit.Xilinx.Devices;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Xilinx
{
	public class XilinxDeviceTree
	{
		public List<GenericPartFamily> Families { get; set; }

		private List<string> LoadFamilyList()
		{
			List<string> families = new List<string>();
			string fullPath = XilinxHelper.GetXilinxToolPath("partgen.exe");
			// Execute to dump supported families
			ProcessHelper.ProcessExecutionResult result = XilinxProcess.ExecuteProcess(Environment.CurrentDirectory, fullPath, null);

			bool startedList = false;
			using (StringReader reader = new StringReader(result.StandardOutput))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!startedList)
					{
						if (line.EndsWith("Valid architectures are:", StringComparison.InvariantCultureIgnoreCase))
						{
							startedList = true;
						}
					}
					else
					{
						// Successive lines are now device families
						string cleanup = line.Trim();
						if (!string.IsNullOrEmpty(cleanup))
						{
							families.Add(cleanup);
						}
					}
				}
			}

			return families;
		}

		private GenericPartFamily LoadFamily(string familyName)
		{
			GenericPartFamily family = null;

			string fullPath = XilinxHelper.GetXilinxToolPath("partgen.exe");
			List<string> arguments = new List<string>();
			arguments.Add("-intstyle silent");
			arguments.Add("-arch " + familyName);
			ProcessHelper.ProcessExecutionResult result = XilinxProcess.ExecuteProcess(Environment.CurrentDirectory, fullPath, arguments);

			bool startedList = false;
			string realFamilyName = familyName;
			string defaultSpeeds = null;
			GenericPart currentPart = null;
			using (StringReader reader = new StringReader(result.StandardOutput))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!startedList)
					{
						// Picked up name
						startedList = true;
						realFamilyName = line.Trim();
						// Create the Family with the real name and short name
						family = new GenericPartFamily(realFamilyName, familyName);
					}
					else if (family != null)
					{
						// The first line i the part + speeds, lines afterwards are packages
						string cleanup = line.Trim();
						if (line.StartsWith("    "))
						{
							if (currentPart != null)
							{
								// Device
								string[] splitUp = cleanup.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
								if (splitUp.Length >= 1 && !string.IsNullOrEmpty(splitUp[0]))
								{
									// Package specifier
									IPartPackage partPackage = family.CreatePackage(splitUp[0]);
									// Device
									GenericPartDevice device = currentPart.CreateDevice(partPackage);

									// Can have an exclusive set of speeds
									if (splitUp.Length > 1)
									{
										ParseSpeedDetails(family, device, splitUp[1]);
									}
									else
									{
										ParseSpeedDetails(family, device, defaultSpeeds);
									}
								}
							}
						}
						else
						{
							string[] splitUp = cleanup.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
							if (splitUp.Length >= 3 && !string.IsNullOrEmpty(splitUp[0]))
							{
								// Create part
								currentPart = family.CreatePart(splitUp[0]);
								// Set default speed for devices
								defaultSpeeds = splitUp[2];
							}
						}
					}
				}
			}
			return family;
		}

		private void ParseSpeedDetails(GenericPartFamily family, GenericPartDevice device, string speedDetails)
		{
			if (!string.IsNullOrEmpty(speedDetails))
			{
				string[] splitUpSpeeds = speedDetails.Split(new string[] { "    " }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string speed in splitUpSpeeds)
				{
					// Shouldn't start with "("
					if (!speed.StartsWith("("))
					{
						IPartSpeed familySpeed = family.CreateSpeed(speed);
						device.Speeds.Add(familySpeed);
					}
				}
			}
		}

		public void LoadDevices()
		{
			Logger.Instance.WriteVerbose("Loading Xilinx Part Library...");

			Families = new List<GenericPartFamily>();
			List<string> families = LoadFamilyList();
			//List<string> families = new List<string>();
			//families.Add("spartan3e");
			foreach (string family in families)
			{
				Logger.Instance.WriteDebug("Loading Xilinx Part for the '{0}' family", family);
				Families.Add(LoadFamily(family));
			}
		}

		public GenericPartFamily FindFamily(string name)
		{
			foreach (GenericPartFamily family in Families)
			{
				if (family.ShortName.CompareTo(name) == 0)
				{
					return family;
				}
			}
			return null;
		}
	}
}
