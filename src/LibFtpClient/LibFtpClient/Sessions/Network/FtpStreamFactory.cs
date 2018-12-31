using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Abre un stream <see cref="FtpActiveStream"/> o <see cref="FtpPassiveStream"/> dependiendo del modo de transferencia
	/// </summary>
	internal class FtpStreamFactory
	{
		/// <summary>
		///		Abre un stream de datos
		/// </summary>
		internal FtpStream OpenDataStream(FtpConnection connection, FtpClient.FtpTransferMode mode)
		{ 
			// Comprueba la protección existente
			connection.CheckProtection(Parameters.FtpClientParameters.FtpProtection.DataChannel);
			// Cambia el modo de transferencia
			SetTransferMode(connection, mode);
			// Abre el stream pasivo (o no)
			if (connection.Client.ClientParameters.Passive)
				return new FtpPasiveStreamFactory().Open(connection);
			else
				return new FtpActiveStreamFactory().Open(connection);
		}

		/// <summary>
		///		Cambia el modo de transferencia
		/// </summary>
		private void SetTransferMode(FtpConnection connection, FtpClient.FtpTransferMode mode)
		{
			if (mode != connection.TransferMode)
			{ 
				// Envía el comando del modo de transferencia
				new Commands.Streams.FtpTransferModeCommand(connection, mode).Send();
				// Indica que se ha cambiado el modo de transferencia
				connection.TransferMode = mode;
			}
		}
	}
}
