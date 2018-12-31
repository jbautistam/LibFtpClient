using System;
using System.Net;

namespace Bau.Libraries.LibFtpClient
{
	/// <summary>
	///		Cliente de FTP
	/// </summary>
	public class FtpClient : IDisposable
	{   
		// Eventos
		public event EventHandler<EventArguments.CheckCertificateEventArgs> CheckCertificate;
		public event EventHandler ConnectionInitialized;
		public event EventHandler<EventArguments.ProtocolMessageEventArgs> Request;
		public event EventHandler<EventArguments.ProtocolMessageEventArgs> Reply;
		public event EventHandler<EventArguments.ProtocolMessageEventArgs> IOError;
		/// <summary>
		///		Protocolo de FTP
		/// </summary>
		public enum FtpProtocol
		{
			/// <summary>FTP estándar sin SSL</summary>
			Ftp,
			/// <summary>FTP con SSL implícito</summary>
			FtpS,
			/// <summary>FTP con SSL explícito</summary>
			FtpES
		}
		/// <summary>
		///		Modo de transferencia
		/// </summary>
		public enum FtpTransferMode
		{
			/// <summary>Modo de transferencia ASCII</summary>
			ASCII = 'A',
			/// <summary>Modo de transferencia binario</summary>
			Binary = 'I'
		}

		public FtpClient(FtpProtocol protocol, string host, int port, NetworkCredential credentials,
						 Parameters.FtpClientParameters parameters)
		{   
			// Inicializa los objetos
			Commands = new FtpClientCommands(this);
			// Asigna los parámetros
			Credential = credentials;
			Protocol = protocol;
			Port = port;
			Uri = new UriBuilder { Scheme = GetScheme(protocol), Host = host, Port = port }.Uri;
			// Inicializa los parámetros de cliente
			if (parameters == null)
				ClientParameters = new Parameters.FtpClientParameters(true);
			else
				ClientParameters = parameters;
			// Crea la conexión
			Connection = new Sessions.FtpConnection(this);
		}

		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		private string GetScheme(FtpProtocol protocol)
		{
			switch (protocol)
			{
				case FtpProtocol.Ftp:
					return Uri.UriSchemeFtp;
				case FtpProtocol.FtpS:
					return "ftps";
				case FtpProtocol.FtpES:
					return "ftpes";
				default:
					throw new ArgumentOutOfRangeException(nameof(protocol));
			}
		}

		/// <summary>
		///		Conecta al servidor
		/// </summary>
		public void Connect()
		{
			IsConnected = Connection.Connect();
		}

		/// <summary>
		///		Lanza el evento <see cref="ConnectionInitialized"/>
		/// </summary>
		internal void RaiseEventConnectionInitialized()
		{
			ConnectionInitialized?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		///		Lanza el evento <see cref="CheckCertificate"/>
		/// </summary>
		internal void RaiseEventCheckCertificate(EventArguments.CheckCertificateEventArgs evntArgs)
		{
			CheckCertificate?.Invoke(this, evntArgs);
		}

		/// <summary>
		///		Lanza un evento <see cref="Request"/>
		///	</summary>
		internal void RaiseEventRequest(EventArguments.ProtocolMessageEventArgs evntArgs)
		{
			Request?.Invoke(this, evntArgs);
		}

		/// <summary>
		///		Lanza un evento <see cref="Reply"/>
		///	</summary>
		internal void RaiseEventReply(EventArguments.ProtocolMessageEventArgs evntArgs)
		{
			Reply?.Invoke(this, evntArgs);
		}

		/// <summary>
		///		Lanza un evento <see cref="IOError"/>
		///	</summary>
		internal void RaiseEventIOError(EventArguments.ProtocolMessageEventArgs evntArgs)
		{
			IOError?.Invoke(this, evntArgs);
		}

		/// <summary>
		///		Libera los recursos
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{   
				// Libera
				if (disposing)
				{   
					// Cierra la conexión
					if (Connection != null)
						Connection.Dispose();
				}
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
		///		Parámetros de cliente
		/// </summary>
		public Parameters.FtpClientParameters ClientParameters { get; }

		/// <summary>
		///		Obtiene la URI raíz
		/// </summary>
		public Uri Uri { get; }

		/// <summary>
		///		Credenciales
		/// </summary>
		public NetworkCredential Credential { get; }

		/// <summary>
		///		Puerto
		/// </summary>
		public int Port { get; }

		/// <summary>
		///		Protocolo
		/// </summary>
		public FtpProtocol Protocol { get; }

		/// <summary>
		///		Comandos que se pueden ejecutar 
		/// </summary>
		public FtpClientCommands Commands { get; }

		/// <summary>
		///		Indica si está conectado al servidor
		/// </summary>
		public bool IsConnected { get; private set; }

		/// <summary>
		///		Conexión
		/// </summary>
		internal Sessions.FtpConnection Connection { get; }

		/// <summary>
		///		Indica si se ha cerrado el sistema
		/// </summary>
		public bool IsDisposed { get; private set; }
	}
}
