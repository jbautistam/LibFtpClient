using System;


namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Factory para <see cref="FtpTransportStream"/>
	/// </summary>
	internal class FtpTransportStreamFactory
	{
		/// <summary>
		///		Abre e inicializa un stream <see cref="FtpTransportStream"/> (transporte)
		/// </summary>
		internal FtpTransportStream Open(FtpConnection connection)
		{
			FtpTransportStream data;

				// Conecta el stream
				data = new FtpTransportStream(connection);
				data.Connect(connection.Client.ClientParameters.ConnectTimeout,
							 connection.Client.ClientParameters.ReadWriteTimeout,
							 connection.Client.Protocol == FtpClient.FtpProtocol.FtpS,
							 out string message);
				// Si no se ha podido conectar lanza una excepción
				if (data == null)
					throw new Exceptions.FtpTransportException($"Socket no conectado a {connection.Client.Uri.Host}. Mensaje ={message}");
				// Devuelve el stream abierto
				return data;
		}

		/// <summary>
		///		Inicializa los parámetros del stream de transporte: necesario porque la conexión aún no tiene el stream sobre
		///	el que se envían los comandos
		/// </summary>
		internal void Initialize(FtpConnection connection, FtpTransportStream data)
		{   
			// Se espera un código 220 como respuesta del servidor
			new Commands.FtpEmptyStreamCommand(connection, new int[] { 220 }).Send();
			// Inicializa la codificación
			InitializeTransportEncoding(connection);
			// Inicializa el protocolo
			InitializeProtocol(connection, data);
		}

		/// <summary>
		///		Inicializa el stream para utilizar (o no) SSL/TLS 
		/// </summary>
		private void InitializeProtocol(FtpConnection connection, FtpTransportStream data)
		{
			switch (connection.Client.Protocol)
			{
				case FtpClient.FtpProtocol.Ftp: // Directo. Simplemente se limpian los datos
						data.LeaveSsl();
					break;
				case FtpClient.FtpProtocol.FtpS: // ... se cambia a un túnel SSL
						data.UpgradeToSsl(connection.Client.ClientParameters.GetSslProtocol());
					break;
				case FtpClient.FtpProtocol.FtpES: // informa primero sobre un canal limpio y después se cambia a SSL
						data.LeaveSsl();
						new Commands.Server.FtpAuthCommand(connection).Send();
						data.UpgradeToSsl(connection.Client.ClientParameters.GetSslProtocol());
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		///		Inicializa la codificación del canal de transporte
		/// </summary>
		private void InitializeTransportEncoding(FtpConnection connection)
		{ 
			// Si no está ya en UTF8 y el servidor puede tratarlo, cambia la codificación
			if (connection.Client.ClientParameters.Encoding == null &&
				connection.Server.Features.HasFeature("UTF8"))
			{   
				// Manda el comando al servidor
				new Commands.Server.FtpOptsCommand(connection, new string[] { "UTF8", "ON" });
				// Cambia la codificación en el servidor
				connection.Client.ClientParameters.Encoding = System.Text.Encoding.UTF8;
			}
		}
	}
}
