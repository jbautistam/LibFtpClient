using System;
using System.Net;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Server
{
	/// <summary>
	///		Comando EPRT de FTP
	/// </summary>
	internal class FtpEprtCommand : FtpCommand
	{
		internal FtpEprtCommand(FtpConnection connection, IPAddress address, int port) : base(connection)
		{
			Address = address;
			Port = port;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand(string.Format("EPRT |{0}|{1}|{2}|",
													Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 1 : 2,
													Address, Port)),
									  200);
		}

		/// <summary>
		///		Dirección IP
		/// </summary>
		internal IPAddress Address { get; }

		/// <summary>
		///		Puerto
		/// </summary>
		internal int Port { get; }
	}
}
