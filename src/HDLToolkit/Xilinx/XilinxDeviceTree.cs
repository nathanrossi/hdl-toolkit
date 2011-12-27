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
using System.Xml.Linq;

namespace HDLToolkit.Xilinx
{
	public class XilinxDeviceTree : DeviceManufacture
	{
		public bool AllowCaching { get; set; }

		public override string Name
		{
			get { return "Xilinx"; }
		}

		public XilinxDeviceTree()
			: base()
		{
			AllowCaching = true;
		}

		public void Load()
		{
			if (AllowCaching && File.Exists(GetCacheFile()))
			{
				LoadCache();
			}
			else
			{
				LoadPartGen();
				if (AllowCaching)
				{
					SaveCache();
				}
			}
		}

		private static string GetCacheFile()
		{
			string path = SystemHelper.GetCacheDirectory();
			path = PathHelper.Combine(path, "devices", "xilinx");
			Directory.CreateDirectory(path);
			path = PathHelper.Combine(path, string.Format("cache-{0}.xml", XilinxHelper.GetCurrentXilinxVersion().UniqueId));
			Logger.Instance.WriteDebug("XilinxDeviceTree cache file located at '{0}' for this xilinx version", path);
			return path;
		}

		private void LoadCache()
		{
			Logger.Instance.WriteVerbose("Loading Xilinx Part Library from cache");
			string path = GetCacheFile();
			XDocument document;
			using (FileStream stream = new FileStream(path, FileMode.Open))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					document = XDocument.Load(reader);
				}
			}
			Deserialize(document.Elements().First());
		}

		private void SaveCache()
		{
			Logger.Instance.WriteVerbose("Saving Xilinx Part Library from cache");
			string path = GetCacheFile();
			XDocument document = new XDocument(Serialize());
			using (FileStream stream = new FileStream(path, FileMode.CreateNew))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					document.Save(writer);
				}
			}
		}

		private void LoadPartGen()
		{
			Logger.Instance.WriteVerbose("Loading Xilinx Part Library...");
			foreach (string family in XilinxPartGen.LoadFamilyList())
			{
				Logger.Instance.WriteDebug("Loading Xilinx Part for the '{0}' family", family);
				Families.Add(XilinxPartGen.LoadFamily(this, family));
			}
		}
	}
}
