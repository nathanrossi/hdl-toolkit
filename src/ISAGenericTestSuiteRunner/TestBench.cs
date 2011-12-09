using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using HDLToolkit;

namespace ISAGenericTestSuiteRunner
{
	public class TestBench
	{
		public class Assertion
		{
			
		}

		private List<string> instructionsList;
		private List<TestCommand> commands;

		public int failedAssertions = 0;
		public int passedAssertions = 0;
		public bool endCalled = false;

		private class QueuedCommand
		{
			public TestCommand Command { get; set; }
			public int CycleToWait { get; set; }

			public QueuedCommand()
			{
			}
		}

		private List<QueuedCommand> queuedCommands = new List<QueuedCommand>();

		public TestBench()
		{
			instructionsList = new List<string>();
			commands = new List<TestCommand>();
		}

		public void Reset()
		{
			queuedCommands.Clear();

			failedAssertions = 0;
			passedAssertions = 0;
			endCalled = false;
		}

		public void RunAssertions(ProcessorState state)
		{
			int nextPhysicalPC = state.Pipeline[1].Value / 2;

			// find all commands to be made
			foreach (TestCommand c in commands)
			{
				if (c.Address == nextPhysicalPC)
				{
					// enqueuing the command
					Logger.Instance.WriteDebug("Enqueued command {0}::'{1}'", c.GetType().ToString(), c.Parameters);
					queuedCommands.Add(new QueuedCommand() { Command = c, CycleToWait = c.CyclesAfterEvent });
				}
			}

			// check all queued commands and execute any
			List<QueuedCommand> toRemove = new List<QueuedCommand>();
			foreach (QueuedCommand q in queuedCommands)
			{
				if (q.CycleToWait == 0)
				{
					Logger.Instance.WriteDebug("Executed command {0}::'{1}'", q.Command.GetType().ToString(), q.Command.Parameters);
					q.Command.Execute(this, state);
					toRemove.Add(q);
				}
				else
				{
					q.CycleToWait--;
				}
			}
			foreach (QueuedCommand q in toRemove)
			{
				queuedCommands.Remove(q);
			}
		}

		public void EndTest()
		{
			endCalled = true;
		}

		public bool IsTestComplete()
		{
			return endCalled;
		}

		public void IncrementAssertionResult(bool passed)
		{
			if (passed)
			{
				passedAssertions++;
			}
			else
			{
				failedAssertions++;
			}
		}

		public string GenerateAssembly()
		{
			StringBuilder builder = new StringBuilder();
			int instruction = 0;

			foreach (string i in instructionsList)
			{
				builder.AppendLine(i + "   ; INSTRUCTION " + instruction++.ToString());
			}

			return builder.ToString();
		}

		#region Parsing
		public static TestBench Load(string file)
		{
			TestBench bench = new TestBench();

			using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine().Trim();

						if (!string.IsNullOrEmpty(line))
						{
							if (line.StartsWith("#"))
							{
								TestCommand command = ParseCommand(line, bench.instructionsList.Count);
								if (command != null)
								{
									Logger.Instance.WriteDebug("Add {0}::'{1}'", command.GetType().ToString(), command.Parameters);
									bench.commands.Add(command);
								}
								else if (line.StartsWith("##todo", StringComparison.InvariantCultureIgnoreCase))
								{
									Logger.Instance.WriteWarning("{0}: {1}", Path.GetFileName(file), line);
								}
							}
							else
							{
								bench.instructionsList.Add(line);
							}
						}
					}
				}
			}

			return bench;
		}

		private static Regex commandRegex = new Regex(@"#(?<command>.*?)(@(?<cycles>.*?))?\((?<content>.*?)\)", RegexOptions.IgnoreCase);
		private static TestCommand ParseCommand(string command, int address)
		{
			Match m = commandRegex.Match(command);
			if (m.Success)
			{
				// Standard command
				// #type@cycleoffset(parameters)

				string type = m.Groups["command"].Value.ToLower();
				string content = m.Groups["content"].Value;

				int cycles = 0;
				if (!string.IsNullOrEmpty(m.Groups["cycles"].Value))
				{
					if (!int.TryParse(m.Groups["cycles"].Value, out cycles))
					{
						return null;
					}
				}

				// Determine actual command class
				switch (type)
				{
					case "end":
						return new EndTestCommand(address, cycles, content);
					case "assert":
						return new AssertTestCommand(address, cycles, content);
					default:
						return null;
				}
			}
			return null;
		}
		#endregion
	}
}
