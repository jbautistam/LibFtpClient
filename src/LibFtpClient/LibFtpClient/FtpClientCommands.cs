using System;
using System.IO;

using Bau.Libraries.LibFtpClient.Sessions.Commands.Files;
using Bau.Libraries.LibFtpClient.Sessions.Commands.Streams;

namespace Bau.Libraries.LibFtpClient
{
	/// <summary>
	///		Comandos que se pueden ejecutar sobre FTP
	/// </summary>
	public class FtpClientCommands
	{
		public FtpClientCommands(FtpClient objClient)
		{
			Client = objClient;
		}

		/// <summary>
		///		Obtiene el stream para descargar un archivo
		/// </summary>
		public Stream Retrieve(string remoteFileName, FtpClient.FtpTransferMode mode = FtpClient.FtpTransferMode.Binary)
		{
			FtpRetrieveCommand command = new FtpRetrieveCommand(Client.Connection, remoteFileName, mode);

				// Devuelve el stream
				return command.GetStream();
		}

		/// <summary>
		///		Obtiene el stream para subir un archivo
		/// </summary>
		public Stream Store(string remoteFileName, FtpClient.FtpTransferMode mode = FtpClient.FtpTransferMode.Binary)
		{
			FtpStoreCommand command = new FtpStoreCommand(Client.Connection, remoteFileName, mode);

				// Devuelve el stream
				return command.GetStream();
		}

		/// <summary>
		///		Envía un archivo por FTP
		/// </summary>
		public void Upload(string fileNameLocal, string remoteFileName)
		{
			using (FileStream localData = File.OpenRead(fileNameLocal))
			{   
				// Abre un stream en el servidor para subir un archivo
				using (Stream ftpData = Store(remoteFileName, FtpClient.FtpTransferMode.Binary))
				{
					byte[] buffer = new byte[1024];
					int read;

						// Escribe los archivos en el servidor
						while ((read = localData.Read(buffer, 0, buffer.Length)) > 0)
							ftpData.Write(buffer, 0, read);
						// Envía el resto de bytes y cierra el stream
						ftpData.Flush();
						ftpData.Close();
				}
				// Cierra el stream de lectura
				localData.Close();
			}
		}

		/// <summary>
		///		Descarga un archivo por FTP
		/// </summary>
		public void Download(string remoteFileName, string fileNameLocal)
		{
			using (FileStream localData = File.Open(fileNameLocal, FileMode.OpenOrCreate, FileAccess.Write))
			{   
				// Abre un stream de descarga en el servidor
				using (Stream ftpData = Retrieve(remoteFileName, FtpClient.FtpTransferMode.Binary))
				{
					byte[] buffer = new byte[1024];
					int read;

					// Lee de la conexión con el servidor
					while ((read = ftpData.Read(buffer, 0, buffer.Length)) > 0)
						localData.Write(buffer, 0, read);
					// Envía el resto de bytes y cierra el stream de lectura
					localData.Flush();
					ftpData.Close();
				}
				// Cierra el stream de escritura
				localData.Close();
			}
		}

		/// <summary>
		///		Borra un directorio
		/// </summary>
		public bool RemoveDirectory(FtpPath path)
		{
			return new FtpDeleteCommand(Client.Connection, path, true).Send().IsSuccess;
		}

		/// <summary>
		///		Borra un archivo o directorio
		/// </summary>
		public bool Delete(FtpPath path)
		{
			return new FtpDeleteCommand(Client.Connection, path, null).Send().IsSuccess;
		}

		/// <summary>
		///		Cambia el nombre de un archivo
		/// </summary>
		public bool Rename(string source, string target)
		{
			FtpReply reply = new FtpRenameCommand(Client.Connection, source, target).Send();

				return reply.IsSuccess;
		}

		/// <summary>
		///		Obtiene el nombre de directorio actual
		/// </summary>
		public string GetActualPath()
		{
			FtpActualDirectoryCommand command = new FtpActualDirectoryCommand(Client.Connection);
			FtpReply reply = command.Send();

				// Obtiene el directorio
				if (reply.IsSuccess)
					return RemoveQuotes(command.ActualPath.Path);
				else
					return "";
		}

		/// <summary>
		///		Elimina las comillas de un directorio
		/// </summary>
		private string RemoveQuotes(string path)
		{ 
			// Le quita las comillas de inicio y fin
			if (!string.IsNullOrEmpty(path) && path.StartsWith("\""))
				path = path.Substring(1);
			if (!string.IsNullOrEmpty(path) && path.EndsWith("\""))
				path = path.Substring(0, path.Length - 1);
			// Devuelve el directorio
			return path;
		}

		/// <summary>
		///		Cambia el directorio
		/// </summary>
		public bool ChangeDir(string newPath)
		{
			FtpReply reply = new FtpChangeDirectoryCommand(Client.Connection, newPath).Send();

				return reply.IsSuccess;
		}

		/// <summary>
		///		Crea un directorio
		/// </summary>
		public bool MakeDir(string newPath)
		{
			bool made = false;

				// Crea el directorio
				try
				{
					FtpReply reply = new FtpMakeDirectoryCommand(Client.Connection, newPath).Send();

						// Comprueba si se ha creado el directorio
						made = reply.IsSuccess;
				}
				catch (Exception exception)
				{ 
					//TODO Aquí debería haber algún tipo de tratamiento de log
					System.Diagnostics.Debug.WriteLine($"Excepción: {exception.Message}");
				}
				// Devuelve el valor que indica si lo ha creado
				return made;
		}

		/// <summary>
		///		Crea el directorio de forma recursiva
		/// </summary>
		public bool MakeDirRecursive(string newPath)
		{
			bool made = true; // ... supone que se crea correctamente

				// Crea recursivamente los directorios
				if (!string.IsNullOrEmpty(newPath))
				{
					string actualPath = GetActualPath();
					string[] newPathParts = newPath.Split('/');
					FtpPath path = "";

					// Recorre los directorios
					for (int index = 0; index < newPathParts.Length && made; index++)
					{ 
						// Añade el subdirectorio
						path += newPathParts[index];
						// Crea el directorio
						if (path.ToString() != "/") // ... porque en la cadena inicial había una / al comienzo
						{ 
							// Crea el directorio
							MakeDir(path.ToString());
							// Pasa al directorio creado (si puede)
							made = ChangeDir(path.ToString());
						}
					}
					// Vuelve al directorio inicial
					ChangeDir(actualPath);
				}
				// Devuelve el valor que indica si se ha creado
				return made;
		}

		/// <summary>
		///		Lista los archivos de un directorio (utilizando un comando MLSD)
		/// </summary>
		public System.Collections.Generic.IList<FtpEntry> MList(string path)
		{
			FtpMlsdCommand listCommand = new FtpMlsdCommand(Client.Connection, path);
			FtpReply reply;

				// Envía el comando
				reply = listCommand.Send();
				// Devuelve la lista de archivos
				if (reply.IsSuccess)
					return listCommand.Files;
				else
					return new System.Collections.Generic.List<FtpEntry>();
		}

		/// <summary>
		///		Lista los archivos de un directorio (utilizando un comando LIST)
		/// </summary>
		public System.Collections.Generic.IList<FtpEntry> List(string path)
		{
			FtpListCommand listCommand = new FtpListCommand(Client.Connection, path);
			FtpReply reply;

				// Envía el comando
				reply = listCommand.Send();
				// Si se ha ejecutado correctamente, devuelve la lista
				if (reply.IsSuccess)
					return listCommand.Files;
				else
					return new System.Collections.Generic.List<FtpEntry>();
		}

		/// <summary>
		///		Cliente al que se asocian los comandos
		/// </summary>
		public FtpClient Client { get; }
	}
}
