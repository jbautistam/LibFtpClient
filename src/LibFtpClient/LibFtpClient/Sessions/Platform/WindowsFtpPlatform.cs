using System;
using System.Text.RegularExpressions;

namespace Bau.Libraries.LibFtpClient.Sessions.Platform
{
	/// <summary>
	///		Clase específica para la plataforma Windows IIS
	/// </summary>
	internal class WindowsFtpPlatform : FtpPlatform
	{ // Variables privadas
		private static readonly Regex RegExWindows = new Regex(
																	@"\s*"
																	+ @"(?<month>\d{2})\-"
																	+ @"(?<day>\d{2})\-"
																	+ @"(?<year>\d{2,4})\s+"
																	+ @"(?<hour>\d{2})\:"
																	+ @"(?<minute>\d{2})"
																	+ @"((?<am>AM)|(?<pm>PM))\s+"
																	+ @"((?<dir>\<DIR\>)|(?<size>\d+))\s+"
																	+ @"(?<name>.*)",
																	RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>
		///		Interpreta la línea
		/// </summary>
		protected override FtpEntry ParseLine(string pathLine, FtpPath pathParent)
		{
			FtpEntry entry = null;
			Match match = RegExWindows.Match(pathLine);

				// Interpreta la línea
				if (match.Success)
				{
					string name = match.Groups["name"].Value;
					DateTime createdAt = ParseDateTime(match);
					string strLiteralSize = match.Groups["size"].Value;
					long size = 0;
					FtpEntry.FtpEntryType type = FtpEntry.FtpEntryType.File;

						// Obtiene el tamaño del archivo. Si no tiene tamaño, se le considera un directorio
						if (!string.IsNullOrEmpty(strLiteralSize))
							size = long.Parse(strLiteralSize);
						else
							type = FtpEntry.FtpEntryType.Directory;
						// Obtine la entrada
						entry = new FtpEntry(pathParent, name, size, type, createdAt, null);
				}
				// Devuelve la entrada
				return entry;
		}
	}
}