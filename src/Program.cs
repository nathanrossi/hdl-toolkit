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
using HDLToolkit.Xilinx;
using HDLToolkit.Framework;
using HDLToolkit.Xilinx.Simulation;
using NConsole;
using HDLToolkit.ConsoleCommands;
using System.Windows.Forms;

namespace HDLToolkit
{
	class Program
	{
		public static XilinxRepository Repository { get; private set; }

		static int Main(string[] args)
		{
			Repository = new XilinxRepository();

			ConsoleController controller = new ConsoleController();
			// Main Commands
			controller.Register(typeof(HelpCommand));

			// Core Commands
			controller.Register(typeof(CoreTreeCommand));
			controller.Register(typeof(CoreXiseGenCommand));
			controller.Register(typeof(CorePrjGenCommand));
			controller.Register(typeof(CoreISimCommand));

			controller.SetDefaultCommand(typeof(HelpCommand));
			return controller.Execute(args);
		}
	}
}
