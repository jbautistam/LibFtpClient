using System;

namespace Bau.Libraries.LibFtpClient
{
	/// <summary>
	///		Representa un directorio FTP
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("FTP path: {Path}")]
	public class FtpPath
	{ 
		// Constantes privadas
		private const char Separator = '/';

		public FtpPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				Path = Separator.ToString();
			else
			{ 
				// Cambia las barras de separación
				path = path.Replace('\\', Separator);
				// Añade la barra inicial
				if (path.StartsWith(Separator.ToString()))
					Path = path;
				else
					Path = Separator + path;
			}
		}

		/// <summary>
		///		Obtiene el nombre del archivo (el último tramo del nombre de archivo)
		/// </summary>
		public string GetFileName()
		{
			string fileName = "";

				// Obtiene el nombre de archivo
				if (!string.IsNullOrEmpty(Path))
				{
					string[] paths = Path.Trim().Trim(Separator).Split(Separator);

						// Si hay algún nombre de archivo
						if (paths.Length > 0)
							fileName = paths[paths.Length - 1];
				}
				// Devuelve el nombre de archivo encontrado
				return fileName;
		}

		/// <summary>
		///		Directorio / archivo
		/// </summary>
		public string Path { get; }

		/// <summary>
		///		Obtiene la cadena que representa la instancia
		/// </summary>
		public override string ToString()
		{
			return Path;
		}

		/// <summary>
		///		Conversor de cadena a FtpPath
		/// </summary>
		public static implicit operator FtpPath(string path)
		{
			return new FtpPath(path);
		}

		/// <summary>
		///		Añade una parte al directorio
		/// </summary>
		public static FtpPath operator +(FtpPath ftpPath, string fileName)
		{
			if (fileName.StartsWith(Separator.ToString())) // ... el directorio que se añade nos lleva al / del servidor
				return new FtpPath(fileName);
			else if (ftpPath.Path.EndsWith(Separator.ToString()))
				return new FtpPath(ftpPath.Path + fileName);
			else
				return new FtpPath(ftpPath.Path + Separator + fileName);
		}
	}
}
