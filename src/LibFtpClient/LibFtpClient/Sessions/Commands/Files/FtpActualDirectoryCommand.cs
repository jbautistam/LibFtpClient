using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Comando para obtener el directorio actual (PWD)
	/// </summary>
	internal class FtpActualDirectoryCommand : FtpCommand
	{
		internal FtpActualDirectoryCommand(FtpConnection connection) : base(connection) { }

		/// <summary>
		///		Obtiene el directorio actual del cliente
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply = Expect(SendCommand("PWD", null), 257, 550);

				// Si ha devuelto al menos una línea, obtiene el directorio de la primera
				if (reply.Lines.Length > 0)
					ActualPath = GetPath(reply.Lines[0]);
				// Devuelve el resultado
				return reply;
		}

		/// <summary>
		///		Obtiene el directorio devuelto por el servidor (normalmente la cadena entre comillas)
		/// </summary>
		private string GetPath(string line)
		{ 
			// Obtiene el directorio
			if (!string.IsNullOrEmpty(line))
			{ 
				// Quita los espacios
				line = line.Trim();
				// Si es una cadena entre comillas, obtiene los datos entre la inicial y la final
				if (line.StartsWith("\""))
				{ 
					// Quita la comilla inicial
					line = line.Substring(1);
					// Comprueba si queda algo antes de buscar las comillas finales
					if (!string.IsNullOrEmpty(line))
					{
						int index = 0;
						bool end = false;
						string result = "";

							// Busca la comilla final (saltándose las estructuras "")
							while (index < line.Length && !end)
							{ 
								// Añade los caracteres hasta encontrarse con la " final siempre y cuando
								// esta " no vaya seguida por otra " (es decir, en FTP se escapan las comillas
								// con comillas (RFC 959)
								if (line[index] == '"')
								{
									if (index < line.Length - 1 && line[index + 1] == '"')
									{ 
										// Añade las comillas
										result += '"';
										// Pasa al carácter después de las comillas actuales
										index += 2;
									}
									else // ... son comillas que no tienen comillas posteriores
										end = true;
								}
								else
								{ 
									// Añade el carácter
									result += line[index];
									// Incrementa el índice (en este caso, cuando es " se trata en el if)
									index++;
								}
							}
							// Cambia la línea por el resultado
							line = result;
					}
				}
			}
			// Cambia las barras por la de Unix (/)
			if (!string.IsNullOrEmpty(line))
				line = line.Replace('\\', '/');
			// Si el directorio termina por / la quitamos
			if (!string.IsNullOrEmpty(line) && line.Length > 1)
				line = line.TrimEnd('/');
			// Devuelve el resultado
			return line;
		}

		/// <summary>
		///		Directorio actual
		/// </summary>
		public FtpPath ActualPath { get; private set; }
	}
}
