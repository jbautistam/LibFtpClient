using System;
using System.Globalization;
using System.IO;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Comando para listar archivos / directorio (MLSD)
	/// </summary>
	internal class FtpMlsdCommand : FtpAbstractFileCommand
	{
		internal FtpMlsdCommand(FtpConnection connection, FtpPath path) : base(connection, path)
		{
		}

		/// <summary>
		///		Envía el comando LIST al servidor
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply = new FtpReply(0);

				// Asigna la protección
				Connection.CheckProtection(Parameters.FtpClientParameters.FtpProtection.ControlChannel);
				// Obtiene los archivos
				using (Network.FtpStream data = new Network.FtpStreamFactory().OpenDataStream(Connection, FtpClient.FtpTransferMode.Binary))
				{   
					// Envía el comando
					reply = Expect(SendCommand("MLSD", Connection.Server.ServerPlatform.EscapePath(Path.ToString())), 
											   125, 150, 425);
					// Si ha contestado correctamente, se interpretan los archivos del directorio
					if (reply.IsSuccess)
					{
						using (StreamReader reader = new StreamReader(data.Validated(), Connection.Client.ClientParameters.Encoding))
						{
							string line;

								// Lee las líneas
								while ((line = reader.ReadLine()) != null)
									AddFileEntry(line);
						}
					}
					else
					{
						data.Abort();
						ThrowException(reply);
					}
				}
				// Devuelve la respuesta
				return reply;
		}

		/// <summary>
		///		Añade un archivo a la colección interpretando la línea
		/// </summary>
		/// <remarks>
		///		El formato de la línea de archivo es: type=file;modify=20160428102907;size=1695961; 2016.04.28.12.29.06.493.pdf
		///		Es decir [fact=value]* Space FileName
		///		Donde fact: "Size" / "Modify" / "Create" / "Type" / "Unique" / "Perm" / "Lang" / "Media-Type" / "CharSet" /
		/// </remarks>
		private void AddFileEntry(string file)
		{
			if (!string.IsNullOrWhiteSpace(file))
			{
				string[] parts = file.Split(';');
				string fileName = "";
				FtpEntry.FtpEntryType type = FtpEntry.FtpEntryType.File;
				long size = 0;
				DateTime updatedAt = DateTime.MinValue, createdAt = DateTime.MinValue;

				// Obtiene los datos del archivo de la cadena
				for (int index = 0; index < parts.Length; index++)
					if (!string.IsNullOrEmpty(parts[index]))
					{ 
						// Limpia los espacios
						parts[index] = parts[index].Trim();
						// Asigna el valor adecuado para la entrada
						if (index == parts.Length - 1)
							fileName = parts[index];
						else
						{
							string[] values = parts[index].Split('=');

								if (values.Length == 2 && !string.IsNullOrEmpty(values[0]) &&
									!string.IsNullOrEmpty(values[1]))
								{ 
									// Limpia los espacios
									values[0] = values[0].Trim();
									values[1] = values[1].Trim();
									// Obtiene el valor adecuado
									if (values[0].Equals("Size", StringComparison.CurrentCultureIgnoreCase))
										size = ParseLong(values[1]);
									else if (values[0].Equals("Type", StringComparison.CurrentCultureIgnoreCase))
										type = GetFileType(values[1]);
									else if (values[0].Equals("Modify", StringComparison.CurrentCultureIgnoreCase))
										updatedAt = ParseDate(values[1]);
									else if (values[1].Equals("Create", StringComparison.CurrentCultureIgnoreCase))
										createdAt = ParseDate(values[1]);
								}
						}
					}
				// Añade el archivo a la colección
				if (!string.IsNullOrEmpty(fileName))
					Files.Add(new FtpEntry(Path, fileName, size, type, updatedAt, null));
			}
		}

		/// <summary>
		///		Interpreta un valor numérico
		/// </summary>
		private long ParseLong(string value)
		{
			if (!long.TryParse(value, out long result))
				return 0;
			else
				return result;
		}

		/// <summary>
		///		Interpreta una fecha
		/// </summary>
		private DateTime ParseDate(string date)
		{
			if (DateTime.TryParseExact(date, new[] 
												{	
													"yyyyMMddhhmmss", 
													"yyyyMMddhhmmss.fff" 
												}, 
										CultureInfo.InvariantCulture,
										DateTimeStyles.AdjustToUniversal, out DateTime value))
				return value;
			else
				return DateTime.MinValue;
		}

		/// <summary>
		///		Obtiene el tipo
		/// </summary>
		private FtpEntry.FtpEntryType GetFileType(string value)
		{
			if (value.Equals("dir", StringComparison.CurrentCultureIgnoreCase) ||
					value.Equals("cdir", StringComparison.CurrentCultureIgnoreCase) ||
					value.Equals("pdir", StringComparison.CurrentCultureIgnoreCase))
				return FtpEntry.FtpEntryType.Directory;
			else if (value.Equals("os.unix=symlink", StringComparison.CurrentCultureIgnoreCase))
				return FtpEntry.FtpEntryType.Link;
			else
				return FtpEntry.FtpEntryType.File;
		}

		/// <summary>
		///		Archivos (convertidos por la plataforma)
		/// </summary>
		internal System.Collections.Generic.List<FtpEntry> Files { get; } = new System.Collections.Generic.List<FtpEntry>();
	}
}
