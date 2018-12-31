using System;
using System.Net.Sockets;

namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Stream de FTP pasivo
	/// </summary>
	internal class FtpPassiveStream : FtpStream
	{
		internal FtpPassiveStream(FtpConnection connection, Socket socket) : base(connection, socket) { }

		/// <summary>
		///		Asigna el socket
		/// </summary>
		protected override void InitSocket()
		{
			SetSocket(InnerSocket);
		}
	}
}