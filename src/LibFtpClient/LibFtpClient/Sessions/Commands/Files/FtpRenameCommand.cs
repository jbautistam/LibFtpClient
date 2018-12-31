using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Comando para cambiar el nombre de un archivo
	/// </summary>
	internal class FtpRenameCommand : FtpAbstractFileCommand
	{
		internal FtpRenameCommand(FtpConnection connection, FtpPath source, FtpPath target)
									: base(connection, source)
		{
			Target = target;
		}

		/// <summary>
		///		Envía los comandos RNFR y RNTO
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply = Expect(SendCommand("RNFR", Path.ToString()), 350);

				// Si ha podido enviar el comando RNFR, envía el comando RNTO
				if (reply.IsSuccess)
					reply = Expect(SendCommand("RNTO", Target.ToString()), 250);
				// Devuelve la última respuesta
				return reply;
		}

		/// <summary>
		///		Archivo / directorio destino
		/// </summary>
		internal FtpPath Target { get; }
	}
}
