using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Streams
{
	/// <summary>
	///		Clase base para los comandos con stream relacionados
	/// </summary>
	internal abstract class FtpAbstractStreamCommand : FtpCommand
	{
		internal FtpAbstractStreamCommand(FtpConnection connection, FtpPath path,
										  FtpClient.FtpTransferMode mode = FtpClient.FtpTransferMode.Binary)
									: base(connection)
		{
			Path = path;
			Mode = mode;
		}

		/// <summary>
		///		Obtiene el stream asociado al comando
		/// </summary>
		internal Network.FtpStream GetStream()
		{
			Network.FtpStream data = new Network.FtpStreamFactory().OpenDataStream(Connection, Mode);
			FtpReply reply = Send();

				// Comprueba el stream antes de devolverlo
				if (!reply.IsSuccess)
				{ 
					// Limpia el stream
					data.Abort();
					// Lanza la excepción
					ThrowException(reply);
					// Anula el stream de datos
					return null;
				}
				else // Devuelve el stream de datos (si se ha podido asignar)
					return data.Validated();
		}

		/// <summary>
		///		Directorio / archivo
		/// </summary>
		internal FtpPath Path { get; }

		/// <summary>
		///		Modo
		/// </summary>
		internal FtpClient.FtpTransferMode Mode { get; }
	}
}
