using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Server
{
	/// <summary>
	///		Comando AUTH de FTP
	/// </summary>
	internal class FtpAuthCommand : FtpCommand
	{
		internal FtpAuthCommand(FtpConnection connection) : base(connection) { }

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand("AUTH", "TLS"), 234);
		}
	}
}
