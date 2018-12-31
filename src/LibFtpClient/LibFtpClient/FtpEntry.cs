using System;

namespace Bau.Libraries.LibFtpClient
{
	/// <summary>
	///		Representa una entrada en el sistema de archivos
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("Archivo FTP {Name}, tipo = {Type}, tamaño = {Size}")]
	public class FtpEntry
	{
		/// <summary>
		///		Tipo de entrada
		/// </summary>
		public enum FtpEntryType
		{
			/// <summary>Archivo</summary>
			File,
			/// <summary>Directorio</summary>
			Directory,
			/// <summary>Vínculo</summary>
			Link
		}

		public FtpEntry(FtpPath parent, string name, long size, FtpEntryType type, DateTime date, FtpPath target)
		{
			if (parent != null)
				Path = parent + name;
			Name = name;
			Date = date;
			Type = type;
			Target = target;
			Size = size;
		}

		public FtpEntry(FtpPath path, long size, FtpEntryType type, DateTime date, FtpPath target)
		{
			Path = path;
			Name = path.GetFileName();
			Date = date;
			Type = type;
			Target = target;
			Size = size;
		}

		/// <summary>
		///		Nombre
		/// </summary>
		public string Name { get; }

		/// <summary>
		///		Directorio
		/// </summary>
		public FtpPath Path { get; }

		/// <summary>
		///		Destino en el caso de vínculos (null en cualquier otro caso)
		/// </summary>
		public FtpPath Target { get; }

		/// <summary>
		///		Tamaño
		/// </summary>
		public long Size { get; }

		/// <summary>
		///		Tipo de entrada
		/// </summary>
		public FtpEntryType Type { get; }

		/// <summary>
		///		Fecha de alta / modificación
		/// </summary>
		public DateTime Date { get; }
	}
}
