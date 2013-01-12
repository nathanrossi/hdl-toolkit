using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HDLToolkit
{
	public class OutputPath
	{
		/// <summary>
		/// Directory of the working project.
		/// </summary>
		public string WorkingDirectory { get; set; }
		/// <summary>
		/// Temporary Directory where generated single use files should be placed.
		/// </summary>
		public string TemporaryDirectory { get; set; }
		/// <summary>
		/// Directory where logs and reports should be placed.
		/// </summary>
		public string LogDirectory { get; set; }
		/// <summary>
		/// Output directory for final artifacts (e.g. executables, netlists, bitstreams, etc.).
		/// </summary>
		public string OutputDirectory { get; set; }

		public string CopyLogFile(string source)
		{
			if (string.IsNullOrEmpty(source) || !File.Exists(source))
			{
				return null;
			}

			string targetFile = PathHelper.Combine(LogDirectory, Path.GetFileName(source));
			File.Copy(source, targetFile, true);
			Logger.Instance.WriteDebug("Copied log file '{0}'", Path.GetFileName(source));

			return targetFile;
		}

		public string CopyOutputFile(string source)
		{
			if (string.IsNullOrEmpty(source) || !File.Exists(source))
			{
				return null;
			}

			string targetFile = PathHelper.Combine(LogDirectory, Path.GetFileName(source));
			File.Copy(source, targetFile, true);
			Logger.Instance.WriteDebug("Copied output file '{0}'", Path.GetFileName(source));

			return targetFile;
		}
	}
}
