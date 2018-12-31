using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Files
{
	/// <summary>
	///		Clase abstracta para los comandos sobre archivos o directorios
	/// </summary>
	internal abstract class FtpAbstractFileCommand : FtpCommand
	{
		internal FtpAbstractFileCommand(FtpConnection connection, FtpPath path) : base(connection)
		{
			Path = path;
		}

		/// <summary>
		///		Directorio sobre el que se opera
		/// </summary>
		internal FtpPath Path { get; }
	}
}
