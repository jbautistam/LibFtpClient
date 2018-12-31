using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Comando para crear un directorio
	/// </summary>
	internal class FtpMakeDirectoryCommand : FtpAbstractFileCommand
	{
		internal FtpMakeDirectoryCommand(FtpConnection connection, FtpPath path) : base(connection, path) { }

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand("MKD", Path.ToString()), 257);
		}
	}
}
