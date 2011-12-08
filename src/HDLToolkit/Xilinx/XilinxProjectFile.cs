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
using System.IO;

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

		private static XNamespace xm = "http://www.xilinx.com/XMLSchema";
		private const string xilinxNamespacePrefix = "xil_pn";
		private const string defaultSchemaVersion = "2";
		private const string defaultVersion = "13.1";

		public override string ToString()
		{
			XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
			XElement root = new XElement(xm + "project");
			document.Add(root);

			// Setup the namespaces
			root.Add(new XAttribute(XNamespace.Xmlns + xilinxNamespacePrefix, xm)); // namespace prefix
			root.Add(new XAttribute("xmlns", xm)); // default namespace
			
			// Populate the document
			root.Add(new XElement(xm + "header")); // empty header
			root.Add(new XElement(xm + "version",
				new XAttribute(xm + "ise_version", defaultVersion),
				new XAttribute(xm + "schema_version", defaultSchemaVersion)));
			//root.Add(new XElement(xm + "bindings")); // empty
			//root.Add(new XElement(xm + "partitions")); // empty

			// Files and Libraries
			XElement files;
			XElement libraries;
			HashSet<string> librariesSet = new HashSet<string>();
			root.Add(files = new XElement(xm + "files"));
			root.Add(libraries = new XElement(xm + "libraries"));
			foreach (IModule module in ReferenceHelper.GetAllModules(Modules))
			{
				if (!librariesSet.Contains(module.Parent.Name))
				{
					librariesSet.Add(module.Parent.Name);
					libraries.Add(new XElement(xm + "library",
						new XAttribute(xm + "name", module.Parent.Name)));
				}
				files.Add(IModuleToElement(module));
			}

			// Properties
			XElement properties;
			root.Add(properties = new XElement(xm + "properties",
				new XElement(xm + "property", 
					new XAttribute(xm + "name", "Preferred Language"),
					new XAttribute(xm + "value", "VHDL"),
					new XAttribute(xm + "valueState", "non-default")),
				new XElement(xm + "property", 
					new XAttribute(xm + "name", "Property Specification in Project File"),
					new XAttribute(xm + "value", "Store non-default values only"),
					new XAttribute(xm + "valueState", "non-default")),
				new XElement(xm + "property", 
					new XAttribute(xm + "name", "Working Directory"),
					new XAttribute(xm + "value", "."),
					new XAttribute(xm + "valueState", "non-default")),
				new XElement(xm + "property", 
					new XAttribute(xm + "name", "Device Family"),
					new XAttribute(xm + "value", "Spartan6"),
					new XAttribute(xm + "valueState", "non-default")),
				new XElement(xm + "property", 
					new XAttribute(xm + "name", "Device"),
					new XAttribute(xm + "value", "xc6slx9"),
					new XAttribute(xm + "valueState", "non-default")),
				new XElement(xm + "property", 
					new XAttribute(xm + "name", "Package"),
					new XAttribute(xm + "value", "csg225"),
					new XAttribute(xm + "valueState", "default")),
				new XElement(xm + "property", 
					new XAttribute(xm + "name", "Speed Grade"),
					new XAttribute(xm + "value", "-3"),
					new XAttribute(xm + "valueState", "default"))
				));

			StringWriter writer = new StringHelpers.Utf8StringWriter();
			document.Save(writer);
			return writer.ToString();
		}

		private static XElement IModuleToElement(IModule module)
		{
			XElement element = new XElement(xm + "file");
			element.SetAttributeValue(xm + "name", module.FileLocation);
			element.SetAttributeValue(xm + "type", IModuleTypeToXiseType(module.Type));
			element.Add(new XElement(xm + "library", 
				new XAttribute(xm + "name", module.Parent.Name)));

			// Determine Association for sim/synthesis
			if (EnumHelpers.ExecutionTypeMatchesRequirement(ExecutionType.SimulationOnly, module.Execution))
			{
				XElement childAssociation = new XElement(xm + "association");
				childAssociation.SetAttributeValue(xm + "name", "BehavioralSimulation");
				element.Add(childAssociation);
			}
			if (EnumHelpers.ExecutionTypeMatchesRequirement(ExecutionType.SynthesisOnly, module.Execution))
			{
				XElement childAssociation = new XElement(xm + "association");
				childAssociation.SetAttributeValue(xm + "name", "Implementation");
				element.Add(childAssociation);
			}
			return element;
		}

		private const string XilinxFileType_VHDL = "FILE_VHDL";
		private const string XilinxFileType_Verilog = "FILE_VERILOG";
		private const string XilinxFileType_Constraints = "FILE_UCF";
		private static string IModuleTypeToXiseType(ModuleType type)
		{
			switch (type)
			{
				case ModuleType.Vhdl:
					return XilinxFileType_VHDL;
				case ModuleType.Verilog:
					return XilinxFileType_Verilog;
				default:
					throw new NotSupportedException();
			}
		}
	}
}
