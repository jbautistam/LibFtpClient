using System;
using System.IO;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Comando para listar archivos / directorio (LST)
	/// </summary>
	internal class FtpListCommand : FtpAbstractFileCommand
	{
		internal FtpListCommand(FtpConnection connection, FtpPath path) : base(connection, path) { }

		/// <summary>
		///		Envía el comando LIST al servidor
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply = new FtpReply(0);
			Platform.FtpPlatform platform;

				// Obtiene la plataforma antes que nada porque puede que envíe un comando SYST por otro canal
				platform = Connection.Server.ServerPlatform;
				// Se cambia de stream (para enviar un comando PORT)
				using (Network.FtpStream data = new Network.FtpStreamFactory().OpenDataStream(Connection, FtpClient.FtpTransferMode.Binary))
				{   
					// Envía el comando LIST
					reply = Expect(SendCommand("LIST", platform.EscapePath(Path.ToString())), 125, 150, 425);
					// Si se ha respondido correctamente, se obtienen los datos
					if (reply.IsSuccess)
					{
						using (StreamReader reader = new StreamReader(data.Validated(), Connection.Client.ClientParameters.Encoding))
						{
							string line;

							// Lee las líneas creando los archivos
							while ((line = reader.ReadLine()) != null)
								Files.Add(platform.Parse(line, Path));
						}
					}
					else // ... y si no, se cierra el stream y se lanza una excepción
					{
						data.Abort();
						ThrowException(reply);
					}
				}
				// Devuelve la respuesta
				return reply;
		}

		/// <summary>
		///		Archivos (convertidos por la plataforma)
		/// </summary>
		internal System.Collections.Generic.List<FtpEntry> Files { get; } = new System.Collections.Generic.List<FtpEntry>();
	}
}
