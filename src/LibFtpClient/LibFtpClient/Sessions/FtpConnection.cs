using System;

namespace Bau.Libraries.LibFtpClient.Sessions
{
	/// <summary>
	///		Conexión al servidor FTP
	/// </summary>
	public class FtpConnection : IDisposable
	{
		public FtpConnection(FtpClient client)
		{
			Client = client;
			Server = new FtpServer(this);
			State = new FtpSessionState(this);
		}

		/// <summary>
		///		Conecta una instancia
		/// </summary>
		internal bool Connect()
		{
			ConnectStream();
			Login();
			return true;
		}

		/// <summary>
		///		Abre la conexión al stream de transporte
		/// </summary>
		private bool ConnectStream()
		{
			Network.FtpTransportStreamFactory factory = new Network.FtpTransportStreamFactory();

				// Inicializa la codificación
				if (Client.ClientParameters.Encoding == null)
					Client.ClientParameters.Encoding = System.Text.Encoding.ASCII;
				// Obtiene el stream de transporte
				ProtocolStream = factory.Open(this);
				// ... y lo inicializa (probablemente no sea necesario cuando los métodos de SendCommand vayan sobre
				// el stream y no sobre la conexión)
				factory.Initialize(this, ProtocolStream);
				// Indica que se ha podido conectar
				return true;
		}

		/// <summary>
		///		Inicializa la sesión (log de usuario)
		/// </summary>
		private void Login()
		{
			if (new Commands.FtpLoginCommand(this).Send().IsSuccess)
				Client.RaiseEventConnectionInitialized();
		}

		/// <summary>
		///		Comprueba la protección
		/// </summary>
		internal void CheckProtection(Parameters.FtpClientParameters.FtpProtection protection)
		{
			if (Client.Protocol != FtpClient.FtpProtocol.Ftp) // ... ¿para qué vamos a comprobar en FTP normal?
			{
				if (Client.ClientParameters.ChannelProtection.HasFlag(protection))
					State["PROT"] = "P";
				else
					State["PROT"] = "C";
			}
		}

		/// <summary>
		///		Desconecta del servidor
		/// </summary>
		internal void Disconnect()
		{
			if (ProtocolStream != null)
			{ 
				// Libera el stream de datos
				try
				{
					ProtocolStream.Dispose();
				}
				catch (System.Net.Sockets.SocketException) { }
				catch (System.IO.IOException) { }
				// Libera el objeto
				ProtocolStream = null;
			}
		}

		/// <summary>
		///		Libera los recursos
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{   
				// Libera los recursos administrados
				if (disposing)
					Disconnect();
				// Indica que se ha liberado
				IsDisposed = true;
			}
		}

		/// <summary>
		///		Libera los recursos
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		///		Estado de sesión
		/// </summary>
		internal FtpSessionState State { get; }

		/// <summary>
		///		Indica si se ha liberado
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		///		Cliente FTP
		/// </summary>
		/// <value>
		public FtpClient Client { get; }

		/// <summary>
		///		Modo de transferencia
		/// </summary>
		internal FtpClient.FtpTransferMode? TransferMode { get; set; }

		/// <summary>
		///		Datos del servidor al que está conectado
		/// </summary>
		internal FtpServer Server { get; }

		/// <summary>
		///		Dirección del host
		/// </summary>
		internal System.Net.IPAddress HostAddress => Client.ClientParameters.ActiveTransferHost ?? ActualActiveTransferHost;

		/// <summary>
		///		Host de transferencia activo
		/// </summary>
		internal System.Net.IPAddress ActualActiveTransferHost { get; set; }

		/// <summary>
		///		Stream de datos
		/// </summary>
		internal Network.FtpTransportStream ProtocolStream { get; private set; }
	}
}
