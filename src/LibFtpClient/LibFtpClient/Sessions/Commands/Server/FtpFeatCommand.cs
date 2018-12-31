using System;
using System.Linq;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Server
{
	/// <summary>
	///		Tratamiento del comando FEAT de FTP
	/// </summary>
	internal class FtpFeatCommand : FtpCommand
	{
		internal FtpFeatCommand(FtpConnection connection) : base(connection) { }

		/// <summary>
		///		Envía el comando FEAT y obtiene las características del servidor
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply = SendCommand("FEAT", null);

				// Si el servidor responde correctamente, interpreta las líneas
				if (reply.Code == 211)
				{
					var qryFeatures = from line in reply.Lines.Skip(1).Take(reply.Lines.Length - 2)
										select line.Trim();

						// Crea las características a partir de la consulta
						Features = new FtpServerFeatures(qryFeatures);
				}
				else
					Features = new FtpServerFeatures(new string[0]);
				// Devuelve la respuesta
				return reply;
		}

		/// <summary>
		///		Características leídas del comando
		/// </summary>
		internal FtpServerFeatures Features { get; private set; }
	}
}
