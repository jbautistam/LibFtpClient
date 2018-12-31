using System;
using System.Text.RegularExpressions;

namespace Bau.Libraries.LibFtpClient.Sessions.Platform
{
	/// <summary>
	///		Clase base para los tratamientos de operaciones específicas por plataforma
	/// </summary>
	internal class FtpPlatform
	{   // Variables privadas
		private static readonly string[] LiteralMonths = { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug",
														   "sep", "oct", "nov", "dec" };
		private static readonly Regex RegExUnix = new Regex(
																@"(?<xtype>[-dlDL])[A-Za-z\-]{9}\s+"
																+ @"\d*\s+"
																+ @"(?<owner>\S*)\s+"
																+ @"(?<group>\S*)\s+"
																+ @"(?<size>\d*)\s+"
																+ @"(?<month>[a-zA-Z]{3})\s+"
																+ @"(?<day>\d{1,2})\s+"
																+ @"(((?<hour>\d{2})\:(?<minute>\d{2}))|(?<year>\d{4}))\s+"
																+ @"(?<name>.*)",
																RegexOptions.CultureInvariant | RegexOptions.Compiled);
		// Enumerados
		/// <summary>
		///		Tipo del servidor FTP
		/// </summary>
		public enum FtpServerType
		{
			/// <summary>Desconocido</summary>
			Unknown = 0,
			/// <summary>Unix</summary>
			Unix,
			/// <summary>Windows</summary>
			Windows
		}

		/// <summary>
		///		Interpreta una línea de directorio
		/// </summary>
		public FtpEntry Parse(string pathLine, FtpPath pathParent)
		{
			return ParseLine(pathLine, pathParent);
		}

		/// <summary>
		///		Interpreta una línea en formato Unix
		/// </summary>
		protected virtual FtpEntry ParseLine(string pathLine, FtpPath pathParent)
		{
			FtpEntry entry = null;
			Match match = RegExUnix.Match(pathLine);

				// Obtiene los datos de la línea
				if (match.Success)
				{
					string extendedType = match.Groups["xtype"].Value;
					string name = match.Groups["name"].Value;
					FtpEntry.FtpEntryType type = FtpEntry.FtpEntryType.File;
					string target = null;

						// Obtiene los datos de la entrada
						if (string.Equals(extendedType, "d", StringComparison.InvariantCultureIgnoreCase))
							type = FtpEntry.FtpEntryType.Directory;
						else if (string.Equals(extendedType, "l", StringComparison.InvariantCultureIgnoreCase))
						{   
							// Indica que es un vínculo
							type = FtpEntry.FtpEntryType.Link;
							// Separa el nombre del vínculo
							SepareLink(ref name, ref target);
						}
						// Crea el objeto con los datos de la entrada
						entry = new FtpEntry(pathParent, name, long.Parse(match.Groups["size"].Value), type,
											 ParseDateTime(match), target);
				}
				// Devuelve la entrada del directorio
				return entry;
		}

		/// <summary>
		///		Separa el vínculo del nombre de archivo
		/// </summary>
		private void SepareLink(ref string name, ref string target)
		{
			const string Separator = " -> ";
			int index = name.IndexOf(Separator, StringComparison.InvariantCultureIgnoreCase);

				// Separa el destino del nombre
				if (index >= 0)
				{
					target = name.Substring(index + Separator.Length);
					name = name.Substring(0, index);
				}
		}

		/// <summary>
		///		Interpreta la fecha y hora
		/// </summary>
		protected DateTime ParseDateTime(Match match)
		{
			return ParseDateTime(match.Groups["year"].Value, match.Groups["month"].Value, match.Groups["day"].Value,
								 match.Groups["hour"].Value, match.Groups["minute"].Value, match.Groups["pm"].Value);
		}

		/// <summary>
		///		Interpreta la fecha y hora
		/// </summary>
		private DateTime ParseDateTime(string year, string month, string day, string hour, string minute, string pm)
		{
			DateTime date = GetDate(year, month, day);

				// Obtiene la fecha completa
				return new DateTime(date.Year, date.Month, date.Day,
									GetHour(hour, pm), GetMinute(minute), 0, DateTimeKind.Local);
		}

		/// <summary>
		///		Obtiene el año
		/// </summary>
		private DateTime GetDate(string year, string month, string day)
		{
			int yearDate = DateTime.Now.Year;
			int monthDate = ParseMonth(month);
			int dayDate = int.Parse(day);

				// Obtiene el año
				if (string.IsNullOrEmpty(year)) // ... si no se le ha pasado nada en el año, lo intenta con el de hoy
				{   
					// Si la fecha de año, mes, día es superior a hoy, se toma un año menos
					if (new DateTime(yearDate, monthDate, dayDate) > DateTime.Now.Date)
						yearDate--;
				}
				else
					yearDate = int.Parse(year);
				// Normaliza el año si viene sólo con dos dígitos
				if (yearDate < 100)
				{
					int century = (DateTime.Now.Year / 100) * 100;

						// Le añade el siglo al año
						yearDate += century;
				}
				// Devuelve la fecha
				return new DateTime(yearDate, monthDate, dayDate, 0, 0, 0);
		}

		/// <summary>
		///		Obtiene la hora
		/// </summary>
		private int GetHour(string hour, string pm)
		{
			int hourDate = string.IsNullOrEmpty(hour) ? 0 : int.Parse(hour);

				// Normaliza la hora si es PM
				if (!string.IsNullOrEmpty(pm))
				{
					if (hourDate < 12) // las 12PM son las 12 así que la dejamos a las 12
						hourDate += 12;
				}
				// Devuelve la hora
				return hourDate;
		}

		/// <summary>
		///		Convierte los minutos
		/// </summary>
		private int GetMinute(string minute)
		{
			if (string.IsNullOrEmpty(minute))
				return 0;
			else
				return int.Parse(minute);
		}

		/// <summary>
		///		Obtiene el mes
		/// </summary>
		private int ParseMonth(string month)
		{
			if (int.TryParse(month, out int monthDate))
				return monthDate;
			else
				return Array.IndexOf(LiteralMonths, month.ToLower()) + 1;
		}

		/// <summary>
		///		Escapa el directorio
		/// </summary>
		public virtual string EscapePath(string path)
		{
			return path;
		}

		/// <summary>
		///		Sistema asociado a la plataforma
		/// </summary>
		public string System { get; internal set; }
	}
}
