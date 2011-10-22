﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HDLToolkit.ConsoleCommands;

namespace HDLToolkit.Xilinx.Parsers
{
	public class DefaultMessageParser : IProcessListener
	{
		private static Regex regexMessage = new Regex("(?<type>error|warning|info):(?<tool>.*?)(:(?<number>.*?)|) - (?<contents>.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

		public enum MessageType
		{
			Unknown,
			Information,
			Warning,
			Error
		}

		public class Message
		{
			public string Location { get; set; }
			public string Details { get; set; }
			public string Contents { get; set; }
			public MessageType Type { get; set; }

			public override string ToString()
			{
				if (string.IsNullOrEmpty(Location))
				{
					return string.Format("{0}: {1}", Details, Contents);
				}
				return string.Format("{0}[{1}]: {2}", Details, Location, Contents);
			}

			public void WriteToLogger()
			{
				switch (Type)
				{
					case MessageType.Error:
						Logger.Instance.WriteError(this.ToString());
						break;
					case MessageType.Warning:
						Logger.Instance.WriteWarning(this.ToString());
						break;
					case MessageType.Information:
					default:
						Logger.Instance.WriteInfo(this.ToString());
						break;
				}
			}
		}

		public List<Message> Messages { get; set; }
		public event Action<Message> MessageOccured;

		public DefaultMessageParser()
		{
			Messages = new List<Message>();
		}

		private static MessageType ParseMessageType(string type)
		{
			if (string.Compare(type, "info", true) == 0)
			{
				return MessageType.Information;
			}
			else if (string.Compare(type, "error", true) == 0)
			{
				return MessageType.Error;
			}
			else if (string.Compare(type, "warning", true) == 0)
			{
				return MessageType.Warning;
			}
			return MessageType.Unknown;
		}

		private void ParseLine(string line)
		{
			if (!string.IsNullOrEmpty(line))
			{
				Match m = regexMessage.Match(line);
				if (m.Success)
				{
					Message message = new Message();
					message.Type = ParseMessageType(m.Groups["type"].Value);
					message.Details = m.Groups["tool"].Value;
					message.Contents = m.Groups["contents"].Value;
					if (m.Groups["location"] != null)
					{
						message.Location = m.Groups["location"].Value;
					}
					Messages.Add(message);
					if (MessageOccured != null)
					{
						MessageOccured(message);
					}
				}
			}
		}

		public void ProcessLine(string line)
		{
			ParseLine(line);
		}

		public void ProcessErrorLine(string line)
		{
			ParseLine(line);
		}

		public void Dispose()
		{
			// Nothing to do here
		}
	}
}
