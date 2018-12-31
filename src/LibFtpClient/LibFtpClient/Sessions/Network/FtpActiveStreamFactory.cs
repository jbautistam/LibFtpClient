using System;
using System.Net;
using System.Net.Sockets;

namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Factory para la creación de <see cref="FtpActiveStream"/>
	/// </summary>
	internal class FtpActiveStreamFactory
	{
		/// <summary>
		///		Abre el stream de datos en modo activo
		/// </summary>
		internal FtpActiveStream Open(FtpConnection connection)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			int port;

				// Asigna el tiempo de espera de envío y recepción
				socket.SendTimeout = socket.ReceiveTimeout = (int) connection.Client.ClientParameters.ReadWriteTimeout.TotalMilliseconds;
				// Vincula el socket al host
				socket.Bind(new IPEndPoint(connection.HostAddress, 0));
				// Obtiene el número de puerto
				port = ((IPEndPoint) socket.LocalEndPoint).Port;
				// Cambia el modo del servidor
				if (connection.Server.Features.HasFeature("EPRT"))
					new Commands.Server.FtpEprtCommand(connection, connection.HostAddress, port).Send();
				else
					new Commands.Server.FtpPortCommand(connection, connection.HostAddress, port).Send();
				// Pone el socket en modo de escucha
				socket.Listen(1);
				// Devuelve el stream en modo activo
				return new FtpActiveStream(connection, socket, connection.Client.ClientParameters.ConnectTimeout);
		}
	}
}
