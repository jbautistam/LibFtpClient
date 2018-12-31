using System;
using System.Collections.Generic;
using System.Linq;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Comando STAT (recupera los parámetros del servidor)
	/// </summary>
	internal class FtpStatCommand : FtpAbstractFileCommand
	{
		internal FtpStatCommand(FtpConnection connection, FtpPath path) : base(connection, path) { }

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{ 
			// Comprueba la protección del socket antes de enviar
			Connection.CheckProtection(Parameters.FtpClientParameters.FtpProtection.ControlChannel);
			// Envía el comando
			return Expect(SendCommand("STAT", Connection.Server.ServerPlatform.EscapePath(Path.ToString())), 213, 211);
		}

		/// <summary>
		///		Envía un comando STAT al servidor e interpreta la respuesta
		/// </summary>
		internal IEnumerable<string> ParseStat()
		{
			FtpReply reply = Send();

				return reply.Lines.Skip(1).Take(reply.Lines.Length - 2);
		}
	}
}
