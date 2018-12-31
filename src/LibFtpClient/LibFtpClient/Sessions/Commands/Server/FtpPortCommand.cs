using System;
using System.Net;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Server
{
	/// <summary>
	///		Comando PORT de FTP
	/// </summary>
	internal class FtpPortCommand : FtpCommand
	{
		internal FtpPortCommand(FtpConnection connection, IPAddress address, int port) : base(connection)
		{
			Address = address;
			Port = port;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			byte[] address = Connection.HostAddress.GetAddressBytes();

				// Lanza el comando
				return Expect(SendCommand(string.Format("PORT {0},{1},{2},{3},{4},{5}",
														address[0], address[1], address[2], address[3],
														Port / 256, Port % 256)),
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
