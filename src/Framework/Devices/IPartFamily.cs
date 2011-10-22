using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework.Devices
{
	public interface IPartFamily
	{
		string ShortName { get; } // eg acr2
		string Name { get; } // eg Automotive CoolRunner2

		// The parts for this family
		IList<IPart> Parts { get; }

		// The valid Packages for the family
		IEnumerable<IPartPackage> Packages { get; }

		// The valid Speeds for the parts in the family
		IEnumerable<IPartSpeed> Speeds { get; }

		// Create a package/speed if it does not exist
		IPartPackage CreatePackage(string name);
		IPartSpeed CreateSpeed(string name);

		// Find a package/speed
		IPartPackage FindPackage(string name);
		IPartSpeed FindSpeed(string name);
	}
}
