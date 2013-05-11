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
using System.Text.RegularExpressions;
using System.IO;
using HDLToolkit.Framework;

namespace HDLToolkit.Xilinx
{
	public class XilinxVersion : IToolchainVersion
	{
		private static Regex filesetVersionRegex = new Regex(@"  version=(?<major>\d+).(?<minor>\d+)", RegexOptions.IgnoreCase);

		public string RootPath { get; private set; }
		public Version Version { get; private set; }
		public string UniqueId { get; private set; }

		internal XilinxVersion(string path, int major, int minor)
		{
			RootPath = path;
			Version = new Version(major, minor);
			UniqueId = StringHelpers.ComputeMD5Hash(GenerateUniqueId());
		}

		private string GenerateUniqueId()
		{
			return string.Format("Xilinx {0} at '{1}', {2}", Version.ToString(), RootPath, File.GetCreationTimeUtc(RootPath));
		}

		public static XilinxVersion GetVersionFromFileset(string path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				string filesetPath = PathHelper.Combine(path, "common", "fileset.txt");
				if (File.Exists(filesetPath))
				{
					string fileset = File.ReadAllText(filesetPath);
					Match match = filesetVersionRegex.Match(fileset);
					if (match.Success)
					{
						int major = 0;
						int minor = 0;
						if (int.TryParse(match.Groups["major"].Value, out major) &&
							int.TryParse(match.Groups["minor"].Value, out minor))
						{
							return new XilinxVersion(path, major, minor);
						}
					}
				}
			}
			return null;
		}

		public override string ToString()
		{
			return string.Format("{0} [{1}]", Version.ToString(), UniqueId);
		}
	}
}
