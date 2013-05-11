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
using HDLToolkit.Framework.Devices;
using System.Text.RegularExpressions;
using HDLToolkit.Framework;

namespace HDLToolkit.Xilinx.Devices
{
	public static class XilinxPartGen
	{
		public static List<string> LoadFamilyList()
		{
			List<string> families = new List<string>();
			ProcessHelper.ProcessExecutionResult result = XilinxProcess.ExecuteProcess(Environment.CurrentDirectory, "partgen", null);

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

		public static DeviceFamily LoadFamily(XilinxToolchain toolchain, DeviceManufacture manufacture, string familyName)
		{
			DeviceFamily family = null;

			List<string> arguments = new List<string>();
			arguments.Add("-intstyle silent");
			arguments.Add("-arch " + familyName);
			ProcessHelper.ProcessExecutionResult result = XilinxProcess.ExecuteProcess(Environment.CurrentDirectory, "partgen", arguments);

			bool startedList = false;
			string realFamilyName = familyName;
			string defaultSpeeds = null;
			Device currentDevice = null;
			DeviceType familyType = ScanDeviceType(familyName);
			using (StringReader reader = new StringReader(result.StandardOutput))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!startedList)
					{
						startedList = true;
						realFamilyName = line.Trim(); // Picked up name
						family = new DeviceFamily(manufacture, realFamilyName, familyName, familyType);
					}
					else if (family != null)
					{
						// The first line i the part + speeds, lines afterwards are packages
						string cleanup = line.Trim();
						if (line.StartsWith("    "))
						{
							if (currentDevice != null)
							{
								// Device
								string[] splitUp = cleanup.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
								if (splitUp.Length >= 1 && !string.IsNullOrEmpty(splitUp[0]))
								{
									// Package specifier
									Logger.Instance.WriteDebug("Create/Find Package '{0}'", splitUp[0]);
									DevicePackage partPackage = family.CreatePackage(splitUp[0]);
									Logger.Instance.WriteDebug("Create/Find part with package '{0}'", partPackage.Name);
									DevicePart part = currentDevice.CreatePart(partPackage);

									// Can have an exclusive set of speeds
									ParseSpeedDetails(toolchain, family, part, (splitUp.Length > 1) ? splitUp[1] : defaultSpeeds);
								}
							}
						}
						else
						{
							string[] splitUp = cleanup.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
							if (splitUp.Length >= 3 && !string.IsNullOrEmpty(splitUp[0]))
							{
								Logger.Instance.WriteDebug("Create/Find Device '{0}'", splitUp[0]);
								currentDevice = family.CreateDevice(splitUp[0]);
								defaultSpeeds = splitUp[2]; // Set default speed for devices
							}
						}
					}
				}
			}
			return family;
		}

		/// <summary>
		/// This uses a best guess approach to determine the type, it uses the family name (e.g. 'spartan3').
		/// This approach is faster then the scanning of 'partgen -v'.
		/// </summary>
		/// <param name="familyName">Family Name to determine type of</param>
		/// <returns>Type</returns>
		private static DeviceType ScanDeviceType(string familyName)
		{
			if (Regex.IsMatch(familyName, "spartan|virtex|zynq|kintex|artix"))
			{
				return DeviceType.FPGA;
			}
			else
			{
				return DeviceType.CPLD;
			}
		}

		private static void ParseSpeedDetails(XilinxToolchain toolchain, DeviceFamily family, DevicePart part, string speedDetails)
		{
			if (!string.IsNullOrEmpty(speedDetails))
			{
				string[] splitUpSpeeds = speedDetails.Split(new string[] { "    " }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string speed in splitUpSpeeds)
				{
					// Shouldn't start with "("
					if (!speed.StartsWith("("))
					{
						DeviceSpeed familySpeed = family.CreateSpeed(speed);
						DevicePartSpeed partSpeed = part.CreateSpeed(familySpeed);
						partSpeed.AddToolchain(toolchain);
					}
				}
			}
		}
	}
}
