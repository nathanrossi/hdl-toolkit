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
using System.Xml.Linq;
using HDLToolkit.Framework;

namespace HDLToolkit.Xilinx
{
	public class XilinxProjectFile : IProjectFile
	{
		public IRepository Environment { get; private set; }
		public ICollection<IModule> Modules { get; private set; }

		public XilinxProjectFile(IRepository repository)
		{
			Environment = repository;
			Modules = new HashSet<IModule>();
		}

		public IModule AddModule(IModule module)
		{
			Modules.Add(module);
			return module;
		}

		public ILibrary AddAllInLibrary(ILibrary library)
		{
			foreach (IModule module in library.Modules)
			{
				AddModule(module);
			}
			return library;
		}

		private const string templateHeader = 
			"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\n" +
			"<project xmlns=\"http://www.xilinx.com/XMLSchema\" xmlns:xil_pn=\"http://www.xilinx.com/XMLSchema\">\n" +
			"  <header/>\n" +
			"  <version xil_pn:ise_version=\"13.1\" xil_pn:schema_version=\"2\"/>\n";

		private const string templateFileStart =
			"  <files>\n";

		private const string templateFileElementStart =
			"    <file xil_pn:name=\"{0}\" xil_pn:type=\"{1}\">\n" +
			"      <library xil_pn:name=\"{2}\"/>\n";

		private const string templateFileElementSimulationElement = 
			"      <association xil_pn:name=\"BehavioralSimulation\" xil_pn:seqID=\"0\"/>\n";

		private const string templateFileElementSynthesisElement = 
			"      <association xil_pn:name=\"Implementation\" xil_pn:seqID=\"0\"/>\n";

		private const string templateFileElementEnd =
			"    </file>\n";

		private const string templateFileEnd =
			"  </files>\n";

		private const string templateDefaultProps =
			"  <properties>\n" +
			"    <property xil_pn:name=\"Preferred Language\" xil_pn:value=\"VHDL\" xil_pn:valueState=\"non-default\"/>\n" +
			"    <property xil_pn:name=\"Property Specification in Project File\" xil_pn:value=\"Store non-default values only\" xil_pn:valueState=\"non-default\"/>\n" +
			"    <property xil_pn:name=\"Working Directory\" xil_pn:value=\".\" xil_pn:valueState=\"non-default\"/>\n" +
			// Device
			"    <property xil_pn:name=\"Device Family\" xil_pn:value=\"Spartan6\" xil_pn:valueState=\"non-default\"/>\n" +
			"    <property xil_pn:name=\"Device\" xil_pn:value=\"xc6slx9\" xil_pn:valueState=\"non-default\"/>\n" +
			"    <property xil_pn:name=\"Package\" xil_pn:value=\"csg225\" xil_pn:valueState=\"default\"/>\n" +
			"    <property xil_pn:name=\"Speed Grade\" xil_pn:value=\"-3\" xil_pn:valueState=\"default\"/>\n" +
			"  </properties>\n";

		private const string templateLibrariesStart =
			"  <libraries>\n";

		private const string templateLibrariesElement =
			"    <library xil_pn:name=\"{0}\"/>\n";

		private const string templateLibrariesEnd =
			"  </libraries>\n";

		private const string templateFooter =
			"  <bindings/>\n" +
			"  <autoManagedFiles/>\n" +
			"</project>";

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			HashSet<string> libraries = new HashSet<string>();

			builder.Append(templateHeader);
			builder.Append(templateFileStart);

			foreach (IModule module in ReferenceHelper.GetAllModules(Modules))
			{
				builder.Append(IModuleToProjectFileElement(module));

				libraries.Add(module.Parent.Name);
			}

			builder.Append(templateFileEnd);
			builder.Append(templateDefaultProps);
			builder.Append(templateLibrariesStart);

			foreach (string library in libraries)
			{
				builder.Append(string.Format(templateLibrariesElement, library));
			}

			builder.Append(templateLibrariesEnd);
			builder.Append(templateFooter);

			return builder.ToString();
		}

		private static string IModuleToProjectFileElement(IModule module)
		{
			string type = null;
			if (module.Type == ModuleType.Vhdl)
			{
				type = "FILE_VHDL";
			}
			else if (module.Type == ModuleType.Verilog)
			{
				throw new NotSupportedException();
				//type = "verilog";
			}

			if (type != null)
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendFormat(templateFileElementStart, module.FileLocation, type, module.Parent.Name);

				// Append the Simulation element if the module supports the Simulation execution type
				if (EnumHelpers.ExecutionTypeMatchesRequirement(ExecutionType.SimulationOnly, module.Execution))
				{
					builder.AppendFormat(templateFileElementSimulationElement);
				}
				// Append the Synthesis element if the module supports the Synthesis execution type
				if (EnumHelpers.ExecutionTypeMatchesRequirement(ExecutionType.SynthesisOnly, module.Execution))
				{
					builder.AppendFormat(templateFileElementSynthesisElement);
				}

				builder.AppendFormat(templateFileElementEnd);
				return builder.ToString();
			}
			return "";
		}
	}
}
