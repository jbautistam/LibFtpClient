using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Server
{
	/// <summary>
	///		Comando SYST (obtiene el tipo de sistema del servidor)
	/// </summary>
	internal class FtpSystemCommand : FtpCommand
	{
		internal FtpSystemCommand(FtpConnection connection) : base(connection) { }

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply = Expect(SendCommand("SYST", null), 215);

				// Obtiene el sistema a partir de la respuesta
				if (reply.IsSuccess)
					System = reply.Lines[0];
				// Devuelve la respuesta del servidro
				return reply;
		}

		/// <summary>
		///		Datos del sistema
		/// </summary>
		internal string System { get; private set; }

		/// <summary>
		///		Tipo de servidor
		/// </summary>
		internal Platform.FtpPlatform.FtpServerType ServerType
		{
			get
			{
				if (System.StartsWith("unix", StringComparison.InvariantCultureIgnoreCase))
					return Platform.FtpPlatform.FtpServerType.Unix;
				else if (System.StartsWith("windows", StringComparison.InvariantCultureIgnoreCase))
					return Platform.FtpPlatform.FtpServerType.Windows;
				else
					return Platform.FtpPlatform.FtpServerType.Unknown;
			}
		}
	}
}
