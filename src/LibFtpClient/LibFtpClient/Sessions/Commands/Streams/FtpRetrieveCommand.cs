using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Streams
{
	/// <summary>
	///		Comando para descargar archivos (RETR)
	/// </summary>
	internal class FtpRetrieveCommand : FtpAbstractStreamCommand
	{
		internal FtpRetrieveCommand(FtpConnection connection, FtpPath path,
									FtpClient.FtpTransferMode mode = FtpClient.FtpTransferMode.Binary)
				: base(connection, path, mode)
		{
		}

		/// <summary>
		///		Envía el comando RETR
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand("RETR", Path.ToString()), 125, 150, 425, 550);
		}
	}
}
