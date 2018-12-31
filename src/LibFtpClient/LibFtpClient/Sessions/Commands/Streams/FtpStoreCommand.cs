using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Streams
{
	/// <summary>
	///		Comando para subir archivos (STOR)
	/// </summary>
	internal class FtpStoreCommand : FtpAbstractStreamCommand
	{
		internal FtpStoreCommand(FtpConnection connection, FtpPath path,
								 FtpClient.FtpTransferMode mode = FtpClient.FtpTransferMode.Binary)
				: base(connection, path, mode)
		{
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand("STOR", Path.ToString()), 125, 150, 425, 550);
		}
	}
}
