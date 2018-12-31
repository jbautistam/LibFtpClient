using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Streams
{
	/// <summary>
	///		Cambia el modo de transferencia (TYPE)
	/// </summary>
	internal class FtpTransferModeCommand : FtpCommand
	{
		internal FtpTransferModeCommand(FtpConnection connection, FtpClient.FtpTransferMode mode) : base(connection)
		{
			Mode = mode;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand("TYPE", ((char) Mode).ToString()), 200);
		}

		/// <summary>
		///		Modo de transferencia
		/// </summary>
		internal FtpClient.FtpTransferMode Mode { get; }
	}
}
