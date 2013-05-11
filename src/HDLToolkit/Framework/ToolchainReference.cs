using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HDLToolkit.Framework
{
	public class ToolchainReference : IXmlSerializable
	{
		public string Id { get; private set; }

		public bool IsNull
		{
			get { return string.IsNullOrEmpty(Id); }
		}

		public ToolchainReference()
		{
		}

		public ToolchainReference(string id)
		{
			Id = id;
		}

		public ToolchainReference(IToolchain toolchain)
		{
			Id = toolchain.UniqueId;
		}

		public IToolchain Toolchain(ToolchainManager manager)
		{
			return manager.FindToolchainById(Id);
		}

		public bool Match(IToolchain toolchain)
		{
			return (string.Compare(toolchain.UniqueId, Id, true) == 0);
		}

		public bool Match(ToolchainReference reference)
		{
			return (string.Compare(reference.Id, Id, true) == 0);
		}

		public virtual XElement Serialize()
		{
			XElement toolchainElement = new XElement("toolchain");
			toolchainElement.Add(new XAttribute("id", Id));

			return toolchainElement;
		}

		public virtual void Deserialize(XElement element)
		{
			if (string.Compare(element.Name.ToString(), "toolchain") == 0)
			{
				XAttribute toolchainId = element.Attribute("id");
				if (toolchainId != null)
				{
					Id = toolchainId.Value;
				}
			}
		}
	}
}
