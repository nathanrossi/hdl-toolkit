using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using HDLToolkit;

namespace ISAGenericTestSuiteRunner
{
	public class AssertTestCommand : TestCommand
	{
		public AssertTestCommand(int addr, int cycles, string parameters)
			: base(addr, cycles, parameters)
		{
		}

		private static Regex assertionOperation = new Regex("(?<a>.*?)(?<op>(==|!=))(?<b>.*)", RegexOptions.IgnoreCase);

		public override void Execute(TestBench test, ProcessorState state)
		{
			Match m = assertionOperation.Match(Parameters);
			if (m.Success)
			{
				string a = m.Groups["a"].Value.Trim();
				string b = m.Groups["b"].Value.Trim();
				string op = m.Groups["op"].Value.Trim();
				bool passed = false;

				if (string.Compare(op, "==", true) == 0)
				{
					if (GetValueForString(a, state) == GetValueForString(b, state))
					{
						passed = true;
					}
				}
				else if (string.Compare(op, "!=", true) == 0)
				{
					if (GetValueForString(a, state) != GetValueForString(b, state))
					{
						passed = true;
					}
				}

				if (!passed)
				{
					Logger.Instance.WriteError("Assertion failed 0x{0:X4}@{1}, '{2}'", Address, CyclesAfterEvent, Parameters);
				}

				test.IncrementAssertionResult(passed);
				return;
			}

			Console.WriteLine("Malformed assertion! '{0}'", Parameters);
		}

		Regex register = new Regex(@"^r(?<index>\d{1,2})", RegexOptions.IgnoreCase | RegexOptions.Multiline);
		public int GetValueForString(string str, ProcessorState state)
		{
			Match m = register.Match(str);
			if (m.Success)
			{
				int i = int.Parse(m.Groups["index"].Value);
				return state.Registers[i];
			}

			if (str.StartsWith("sreg"))
			{
				string strToLower = str.ToLower();
				switch (strToLower)
				{
					case "sreg":
						return state.StatusRegister;
					case "sreg[c]": // carry
						return (state.StatusRegister >> 0) & 0x1;
					case "sreg[z]": // zero
						return (state.StatusRegister >> 1) & 0x1;
					case "sreg[n]": // negative
						return (state.StatusRegister >> 2) & 0x1;
					case "sreg[v]": // twos comp (v)
						return (state.StatusRegister >> 3) & 0x1;
					case "sreg[s]": // signed
						return (state.StatusRegister >> 4) & 0x1;
					case "sreg[h]": // half carry
						return (state.StatusRegister >> 5) & 0x1;
					case "sreg[t]": // temp/transfer
						return (state.StatusRegister >> 6) & 0x1;
					case "sreg[i]": // instruction
						return (state.StatusRegister >> 7) & 0x1;
					default:
						break;
				}
			}

			int value = 0;
			if (int.TryParse(str, out value))
			{
				return value;
			}

			if (str.StartsWith("0x"))
			{
				if (int.TryParse(str.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value))
				{
					return value;
				}
			}

			bool valueBool = false;
			if (bool.TryParse(str, out valueBool))
			{
				return valueBool ? 1 : 0;
			}

			throw new Exception("parsing exception?");
			// unknown
			return -1;
		}
	}
}
