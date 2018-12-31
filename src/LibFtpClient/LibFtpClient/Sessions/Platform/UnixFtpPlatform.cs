using System;
using System.Linq;

namespace Bau.Libraries.LibFtpClient.Sessions.Platform
{
	/// <summary>
	///		Implementaci�n predeterminada para plataformas Unix
	/// </summary>
	internal class UnixFtpPlatform : FtpPlatform
	{
		/// <summary>
		///		Escapa el directorio
		/// </summary>
		public override string EscapePath(string path)
		{
			return EscapePath(path, " []()");
		}

		/// <summary>
		///		Escapa el directorio. Se hace a este nivel para que lo compartan las diferentes clases
		/// </summary>
		private string EscapePath(string path, string escapeCharacters)
		{
			if (!escapeCharacters.Any(path.Contains))
				return path;
			else
			{
				System.Text.StringBuilder builder = new System.Text.StringBuilder();

					// Recorre los caracteres cambiando los caracteres de escape
					foreach (char pathChar in path)
					{ 
						// A�ade el separador para los caracteres de escape
						if (escapeCharacters.Contains(pathChar))
							builder.Append('\\');
						// A�ade el car�cter
						builder.Append(pathChar);
					}
					// Devuelve el directorio
					return builder.ToString();
			}
		}
	}
}