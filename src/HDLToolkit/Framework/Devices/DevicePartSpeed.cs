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

		public DevicePart Part { get; private set; }
		public DeviceSpeed Speed { get; private set; }

		public DevicePartSpeed(DevicePart part)
			: this(part, null)
		{
		}

		public DevicePartSpeed(DevicePart part, DeviceSpeed speed)
		{
			Part = part;
			Speed = speed;
		}

		public virtual XElement Serialize()
		{
			XElement element = new XElement("devicepartspeed");
			element.Add(new XAttribute("speed", Speed.Name));
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
			}
		}

	}
}
