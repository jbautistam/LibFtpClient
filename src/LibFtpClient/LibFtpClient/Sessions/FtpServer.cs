using System;

namespace Bau.Libraries.LibFtpClient.Sessions
{
	/// <summary>
	///		Clase con los datos relativos al servidor
	/// </summary>
	internal class FtpServer
	{ 
		// Variables privadas
		private FtpServerFeatures _features = null;
		private Platform.FtpPlatform platform = null;

		internal FtpServer(FtpConnection connection)
		{
			Connection = connection;
		}

		/// <summary>
		///		Carga las características de la conexión
		/// </summary>
		private FtpServerFeatures LoadServerFeatures()
		{
			Commands.Server.FtpFeatCommand command = new Commands.Server.FtpFeatCommand(Connection);

				// Envía el comando
				command.Send();
				// Devuelve las características leídas
				return command.Features;
		}

		/// <summary>
		///		Asigna los datos de plataforma
		/// </summary>
		private void LoadPlatformData()
		{
			Commands.Server.FtpSystemCommand command = new Commands.Server.FtpSystemCommand(Connection);

				// Envía el mensaje
				command.Send();
				// Asigna el tipo de servidor y la plataforma
				switch (command.ServerType)
				{
					case Platform.FtpPlatform.FtpServerType.Unknown:
							ServerPlatform = new Platform.FtpPlatform();
						break;
					case Platform.FtpPlatform.FtpServerType.Unix:
							ServerPlatform = new Platform.UnixFtpPlatform();
						break;
					case Platform.FtpPlatform.FtpServerType.Windows:
							ServerPlatform = new Platform.WindowsFtpPlatform();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(command.ServerType), command.ServerType, null);
				}
				// Asigna el sistema leído a la plataforma
				platform.System = command.System;
		}

		/// <summary>
		///		Conexión a la que se asocia el servidor
		/// </summary>
		internal FtpConnection Connection { get; }

		/// <summary>
		///		Características del servidor
		/// </summary>
		internal FtpServerFeatures Features
		{
			get
			{ 
				// Carga las características si no estaban en memoria
				if (_features == null)
					_features = LoadServerFeatures();
				// Devuelve las características
				return _features;
			}
		}

		/// <summary>
		///		Obtiene la plataforma del servidor FTP
		/// </summary>
		internal Platform.FtpPlatform ServerPlatform
		{
			get
			{ 
				// Carga los datos de plataforma
				if (platform == null)
					LoadPlatformData();
				// Devuelve la plataforma
				return platform;
			}
			private set { platform = value; }
		}
	}
}
