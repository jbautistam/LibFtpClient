using System;
using System.Net;
using System.Net.Sockets;

namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Stream para el protocolo de transporte (cuando se inicia la conexión y aún no se ha decidido si es activo o pasivo)
	/// </summary>
	internal class FtpTransportStream : FtpStream
	{
		internal FtpTransportStream(FtpConnection connection) : base(connection, null) { }

		/// <summary>
		///		Inicializa el socket
		/// </summary>
		protected override void InitSocket()
		{ // ... no hace nada en este momento
		}

		/// <summary>
		///		Conecta el stream de transporte
		/// </summary>
		internal void Connect(TimeSpan connectTimeout, TimeSpan readWriteTimeout, bool useSsl, out string message)
		{   
			// Inicializa los argumentos de salida
			message = null;
			// Conecta al stream
			try
			{   
				// Intenta utilizar el proxy
				if (Connection.Client.ClientParameters.ProxyConnect != null)
				{ // Inicializa el socket con el socket proxy
					InnerSocket = Connection.Client.ClientParameters.ProxyConnect(new DnsEndPoint(Connection.Client.Uri.Host, Connection.Client.Port));
					// Incializa el stream sobre ese socket
					if (InnerSocket != null)
						InnerStream = new NetworkStream(InnerSocket);
				}
				// Conecta directamente (sin proxy)
				DirectConnect(readWriteTimeout, connectTimeout, ref message);
				// Inicializa el protocolo SSL si es necesario
				if (useSsl)
					base.UpgradeToSsl(Connection.Client.ClientParameters.GetSslProtocol());
			}
			catch (SocketException exception) // ... esta excepción la puede lanzar la resolución de DNS
			{
				message = exception.ToString();
				InnerStream = null;
			}
			catch (System.IO.IOException exception) // ... esta excepción la puede lanzar la conexión al proxy
			{
				message = exception.ToString();
				InnerStream = null;
			}
		}

		/// <summary>
		///		Conexión directa al socket
		/// </summary>
		private void DirectConnect(TimeSpan readWriteTimeout, TimeSpan connectTimeout, ref string message)
		{ 
			// Intenta conectar por IP4 o IP6
			try
			{
				InnerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}
			catch (SocketException)
			{
				InnerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
			}
			// Inicializa el socket
			InnerSocket.SendTimeout = InnerSocket.ReceiveTimeout = (int) readWriteTimeout.TotalMilliseconds;
			InnerSocket.Connect(Connection.Client.Uri.Host, Connection.Client.Port, connectTimeout);
			// Comprueba si está conectado
			if (!InnerSocket.Connected)
				message = "Not connected";
			// Cambia la dirección del host
			Connection.ActualActiveTransferHost = ((IPEndPoint) InnerSocket.LocalEndPoint).Address;
			// Devuelve el stream de red sobre el sockect
			InnerStream = new NetworkStream(InnerSocket, System.IO.FileAccess.ReadWrite, true);
		}
	}
}
