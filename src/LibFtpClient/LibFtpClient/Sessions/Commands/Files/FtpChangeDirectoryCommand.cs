using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Cambia el directorio actual
	/// </summary>
	internal class FtpChangeDirectoryCommand : FtpAbstractFileCommand
	{
		internal FtpChangeDirectoryCommand(FtpConnection connection, FtpPath path) : base(connection, path) { }

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand("CWD", new string[] { Path.ToString() }), 250, 550);
		}
	}
}
