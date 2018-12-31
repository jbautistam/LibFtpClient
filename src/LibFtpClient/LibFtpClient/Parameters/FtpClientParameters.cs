using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Bau.Libraries.LibFtpClient.Parameters
{
	/// <summary>
	///		Parámetros de una instancia de FtpClient
	/// </summary>
	public class FtpClientParameters
	{   
		// Variables privadas
		private Encoding _encoding = null;

		/// <summary>
		///		Niveles de protección para FTP
		/// </summary>
		[Flags]
		public enum FtpProtection
		{
			/// <summary>Protección predeterminada. En su caso, se obtiene la del protocolo</summary>
			Ftp = 0,
			/// <summary>Protección del canal de control</summary>
			ControlChannel = 0x01,
			/// <summary>Protección del canal de datos</summary>
			DataChannel = 0x02,
			/// <summary>Protección sobre todos los canales (control y datos)</summary>
			AllChannels = ControlChannel | DataChannel,
			/// <summary>Protección predeterminada para FtpEs (todos los canales)</summary>
			FtpES = AllChannels,
			/// <summary>Protección predterminada para FtpS (todos los canales)</summary>
			FtpS = AllChannels
		}
		/// <summary>
		///		Tipo de conexión para SSL
		/// </summary>
		public enum SslType
		{
			/// <summary>Sin conexión SSL</summary>
			None,
			/// <summary>Conexión SSL2</summary>
			Ssl2,
			/// <summary>Conexión SSL3</summary>
			Ssl3,
			/// <summary>Conexión TLS</summary>
			Tls,
			/// <summary>Conexión predeterminada (SSL3 + TLS)</summary>
			Default
		}

		public FtpClientParameters(bool passive, FtpProtection channelProtection = FtpProtection.Ftp,
								   SslType sslType = SslType.Default)
		{ 
			// Inicializa las propiedades
			Passive = passive;
			ChannelProtection = channelProtection;
			SslProtocol = sslType;
			// Lanza una excepción si no cumple con los parámetros
			if (!Passive && ProxyConnect != null)
				throw new InvalidOperationException("El modo de transferencia activo sólo funciona sin servidor proxy");
		}

		/// <summary>
		///		Obtiene el protocolo SSL
		/// </summary>
		internal System.Security.Authentication.SslProtocols GetSslProtocol()
		{
			switch (SslProtocol)
			{
				case SslType.None:
					return System.Security.Authentication.SslProtocols.None;
				case SslType.Ssl2:
					return System.Security.Authentication.SslProtocols.Ssl2;
				case SslType.Ssl3:
					return System.Security.Authentication.SslProtocols.Ssl3;
				case SslType.Tls:
					return System.Security.Authentication.SslProtocols.Tls;
				case SslType.Default:
					return System.Security.Authentication.SslProtocols.Ssl3 | System.Security.Authentication.SslProtocols.Tls;
				default:
					throw new ArgumentException("No se reconoce el tipo de protocolo SSL");
			}
		}

		/// <summary>
		///		Timeout de la conexión
		/// </summary>
		public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(10);

		/// <summary>
		///		Timeout de lectura / escritura
		/// </summary>
		public TimeSpan ReadWriteTimeout { get; set; } = TimeSpan.FromMinutes(10);

		/// <summary>
		///		Timeout de sesión
		/// </summary>
		public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(2);

		/// <summary>
		///		Indica si la conexión es pasiva
		/// </summary>
		public bool Passive { get; set; } = true;

		/// <summary>
		///		Host de transferencia activo (si se especifica, se utiliza esta dirección con los comandos PORT/EPRT, si no
		///	se utiliza como host el que tenemos conectado)
		/// </summary>
		public IPAddress ActiveTransferHost { get; set; }

		/// <summary>
		///		Contraseña para el usuario anónimo
		/// </summary>
		public string AnonymousPassword { get; set; } = "user@" + Environment.MachineName;

		/// <summary>
		///		Codificación predeterminada
		/// </summary>
		public Encoding Encoding
		{
			get
			{ 
				// Asegura que haya algo en la codificación
				if (_encoding == null)
					_encoding = Encoding.ASCII;
				// Devuelve la codificación
				return _encoding;
			}
			set { _encoding = value; }
		}

		/// <summary>
		///	Función utilizada para conexiones por proxy
		///	Para la función:
		///		Arg1: host
		///		Arg2: puerto
		///		Arg3: true para stream de control, false para stream de datos
		/// </summary>
		public Func<EndPoint, Socket> ProxyConnect { get; set; }

		/// <summary>
		///		Canal de protección. Si se deja a FTP se utiliza el valor predeterminado de acuerdo con el protocolo 
		/// </summary>
		public FtpProtection ChannelProtection { get; set; } = FtpProtection.Ftp;

		/// <summary>
		///		Tipo de protocolo SSL que se va a utilizar
		/// </summary>
		public SslType SslProtocol { get; set; } = SslType.None;
	}
}
