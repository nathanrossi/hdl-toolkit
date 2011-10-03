using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Xilinx.Devices
{
	public class GenericPartFamily : IPartFamily
	{
		public string ShortName { get; set; }
		public string Name { get; set; }

		public IList<IPart> Parts { get; private set; }

		List<IPartSpeed> speeds;
		List<IPartPackage> packages;
		public IEnumerable<IPartSpeed> Speeds { get { return speeds; } }
		public IEnumerable<IPartPackage> Packages { get { return packages; } }

		public GenericPartFamily(string name, string shortname)
		{
			Name = name;
			ShortName = shortname;

			Parts = new List<IPart>();
			speeds = new List<IPartSpeed>();
			packages = new List<IPartPackage>();
		}

		public GenericPart CreatePart(string name)
		{
			GenericPart part = new GenericPart(this, name);
			this.Parts.Add(part);
			return part;
		}

		public IPartPackage CreatePackage(string name)
		{
			IPartPackage create = FindPackage(name);
			if (create == null)
			{
				create = new GenericPartPackage(this, name);
				packages.Add(create);
			}
			return create;
		}

		public IPartSpeed CreateSpeed(string name)
		{
			IPartSpeed create = FindSpeed(name);
			if (create == null)
			{
				create = new GenericPartSpeed(this, name);
				speeds.Add(create);
			}
			return create;
		}

		public IPartPackage FindPackage(string name)
		{
			foreach (IPartPackage package in packages)
			{
				if (package.Name.CompareTo(name) == 0)
				{
					return package;
				}
			}
			return null;
		}

		public IPartSpeed FindSpeed(string name)
		{
			foreach (IPartSpeed speed in speeds)
			{
				if (speed.Name.CompareTo(name) == 0)
				{
					return speed;
				}
			}
			return null;
		}
	}
}
