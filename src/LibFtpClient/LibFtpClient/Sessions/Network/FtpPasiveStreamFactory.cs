using System;
using System.Net;
using System.Net.Sockets;

namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Factory para la creación de <see cref="FtpPassiveStream"/>
	/// </summary>
	internal class FtpPasiveStreamFactory
	{
		/// <summary>
		///		Abre un stream de datos en modo pasivo
		/// </summary>
		internal FtpPassiveStream Open(FtpConnection connection)
		{
			string host = null;
			int port = 0;
			bool passive = false;

				// Intenta pasar a modo pasivo (EPSV o PSV)
				if (connection.Server.Features.HasFeature("EPSV"))
					passive = TransferExtendedPassiveMode(connection, ref host, ref port);
				else
					passive = TransferPassiveMode(connection, ref host, ref port);
				// Si no ha podido pasar a modo pasivo o no se ha interpretado correctamente el host lanza una excepción
				if (!passive)
					throw new Exceptions.FtpException("No se ha podido pasar a modo pasivo");
				if (string.IsNullOrEmpty(host))
					throw new Exceptions.FtpException("No se pudo obtener la dirección del host al pasar a modo pasivo");
				// Asigna el proxy
				if (connection.Client.ClientParameters.ProxyConnect != null)
				{
					Socket socket = connection.Client.ClientParameters.ProxyConnect(new DnsEndPoint(host, port));

						if (socket != null)
							return new FtpPassiveStream(connection, socket);
				}
				// Abre la conexión de datos
				return OpenDataStream(connection, host, port);
		}

		/// <summary>
		///		Transfiere a modo pasivo (EPSV)
		/// </summary>
		private bool TransferExtendedPassiveMode(FtpConnection connection, ref string host, ref int port)
		{
			bool passive = false;

				// Intenta pasar a modo pasivo
				for (int loops = 0; !passive && loops < 5; loops++)
				{
					FtpReply reply;
					Commands.Streams.FtpChangePassiveCommand command;

						// Envía el comando
						command = new Commands.Streams.FtpChangePassiveCommand(connection, true);
						reply = command.Send();
						// Si responde con código 229, recoge el puerto, si no, lo intenta de nuevo (puede haber dado
						// un error de tipo "421 - Demasiadas conexiones abiertas")
						if (reply.Code == 229)
						{
							host = command.Host;
							port = command.Port;
							passive = command.IsPassive;
						}
				}
				// Devuelve el valor que indica si se ha podido pasar a modo pasivo
				return passive;
		}

		/// <summary>
		///		Transfiere a modo pasivo (PASV)
		/// </summary>
		private bool TransferPassiveMode(FtpConnection connection, ref string host, ref int port)
		{
			bool passive = false;

				// Intenta pasar a modo pasivo
				for (int loops = 0; !passive && loops < 5; loops++)
				{
					Commands.Streams.FtpChangePassiveCommand command;
					FtpReply reply;

						// Envía el comando y obtiene la respuesta
						command = new Commands.Streams.FtpChangePassiveCommand(connection, false);
						reply = command.Send();
						// Si se encuentra el código 227, recoge el host y puerto, si no, lo intenta de nuevo (puede haber dado un
						// error del tipo "421 - Demasiadas conexiones abiertas")
						if (reply.Code == 227)
						{
							host = command.Host;
							port = command.Port;
							passive = command.IsPassive;
						}
				}
				// Devuelve el valor que indica si ha podido pasar a modo pasivo
				return passive;
		}

		/// <summary>
		///		Abre el stream de datos en modo pasivo directo
		/// </summary>
		private FtpPassiveStream OpenDataStream(FtpConnection connection, string host, int port)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				// Asigna los timeout de envío y recepción
				socket.SendTimeout = socket.ReceiveTimeout = (int) connection.Client.ClientParameters.ReadWriteTimeout.TotalMilliseconds;
				// Conecta al socket
				socket.Connect(host, port, connection.Client.ClientParameters.ConnectTimeout);
				// Si no se ha podido conectar, devuelve una excepción
				if (!socket.Connected)
					throw new Exceptions.FtpTransportException($"Socket error to {host}");
				// Devuelve el stream pasivo
				return new FtpPassiveStream(connection, socket);
		}
	}
}
