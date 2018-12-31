using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Server
{
	/// <summary>
	///		Comando OPTS de FTP (opciones)
	/// </summary>
	internal class FtpOptsCommand : FtpCommand
	{
		internal FtpOptsCommand(FtpConnection connection, string[] options) : base(connection)
		{
			Options = options;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand("OPTS", Options), 200);
		}

		/// <summary>
		///		Opciones
		/// </summary>
		internal string[] Options { get; }
	}
}
