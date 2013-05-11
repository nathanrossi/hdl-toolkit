using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HDLToolkit.Framework.Devices
{
	public class DevicePartSpeed : IXmlSerializable
	{
		public string Name
		{
			get
			{
				if (Part != null && Speed != null)
				{
					return Part.Name + Speed.Name;
				}
				return null;
			}
		}

		public string AlternateName
		{
			get
			{
				if (Part != null && Speed != null)
				{
					return Part.Parent.Name + Speed.Name + Part.Package.Name;
				}
				return null;
			}
		}

		public DevicePart Part { get; private set; }
		public DeviceSpeed Speed { get; private set; }
		// Devices can have an associated Toolchain which can provide features
		public IEnumerable<ToolchainReference> Toolchains { get { return toolchains; } }
		private HashSet<ToolchainReference> toolchains = new HashSet<ToolchainReference>();

		public DevicePartSpeed(DevicePart part)
			: this(part, null)
		{
		}

		public DevicePartSpeed(DevicePart part, DeviceSpeed speed)
		{
			Part = part;
			Speed = speed;
		}

		public void AddToolchain(ToolchainReference reference)
		{
			foreach (ToolchainReference toolchain in toolchains)
			{
				if (toolchain.Match(reference))
				{
					return;
				}
			}
			toolchains.Add(reference);
		}

		public void AddToolchain(IToolchain toolchain)
		{
			AddToolchain(new ToolchainReference(toolchain));
		}

		public virtual XElement Serialize()
		{
			XElement element = new XElement("devicepartspeed");
			element.Add(new XAttribute("speed", Speed.Name));

			// Serialize Toolchain Support
			if (toolchains.Count != 0)
			{
				XElement toolchainsElement = new XElement("toolchains");
				element.Add(toolchainsElement);
				foreach (ToolchainReference reference in Toolchains)
				{
					toolchainsElement.Add(reference.Serialize());
				}
			}

			return element;
		}

		public virtual void Deserialize(XElement element)
		{
			if (string.Compare(element.Name.ToString(), "devicepartspeed") == 0)
			{
				// Parse the package
				XAttribute speedAttr = element.Attribute("speed");
				if (speedAttr != null)
				{
					Speed = Part.Parent.Family.FindSpeed(speedAttr.Value);
				}

				// Parse the Toolchains
				XElement toolchainsElement = element.Element("toolchains");
				if (toolchainsElement != null)
				{
					foreach (XElement toolchainElement in toolchainsElement.Elements())
					{
						ToolchainReference reference = new ToolchainReference();
						reference.Deserialize(toolchainElement);
						if (!reference.IsNull)
						{
							toolchains.Add(reference);
						}
					}
				}
			}
		}
	}
}
