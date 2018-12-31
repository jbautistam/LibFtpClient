using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Comando para eliminar un directorio
	/// </summary>
	internal class FtpDeleteCommand : FtpAbstractFileCommand
	{
		internal FtpDeleteCommand(FtpConnection connection, FtpPath path, bool? isDirectory = null)
								: base(connection, path)
		{
			IsDirectory = isDirectory;
		}

		/// <summary>
		///		Envía el comando RMD
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply = null;

				// Borra el directorio o el archivo
				if (IsDirectory ?? true)
					reply = DeletePath();
				// Si era un archivo o hemos intentado borrar el directorio y no hemos podido
				if (reply == null || !reply.IsSuccess)
					reply = DeleteFile();
				// Devuelve la respuesta
				return reply;
		}

		/// <summary>
		///		Elimina un directorio
		/// </summary>
		private FtpReply DeletePath()
		{
			return Expect(SendCommand("RMD", Path.ToString()), 250, 550);
		}

		/// <summary>
		///		Envía un comando DELE (borrar archivo)
		/// </summary>
		private FtpReply DeleteFile()
		{
			return Expect(SendCommand("DELE", Path.ToString()), 250, 550);
		}

		/// <summary>
		///		Indica si lo que se va a borrar es un archivo o un directorio
		/// </summary>
		internal bool? IsDirectory { get; }
	}
}
