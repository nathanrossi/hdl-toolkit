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
using System.IO;

namespace HDLToolkit.Console.Commands
{
	[Command("clearcache")]
	public class ClearCacheCommand : BaseCommand
	{
		public override void Execute()
		{
			base.Execute();

			Logger.Instance.WriteInfo("Clearing cache..");

			// TODO: use a cache manager?
			string path = SystemHelper.GetCacheDirectory();
			Logger.Instance.WriteVerbose("Deleting all files in '{0}'", path);
			Directory.Delete(path, true);

			Logger.Instance.WriteInfo("Done.");
		}
	}
}
