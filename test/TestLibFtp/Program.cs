using System;
using System.Collections.Generic;

using Bau.Libraries.LibFtpClient;

namespace TestLibFtp
{
	class Program
	{ // Constantes privadas
			private const int cnstIntPort = 21;
			private const int cnstIntPortFtps = 990;
			private const int cnstIntPortFtpEs = 21;
			private const string cnstStrPathRemote = "/dev";
			private const string cnstStrServer = "127.0.0.1";
			private const string cnstStrUser = "user";
			private const string cnstStrPassword = "password";
		// Variables privadas
			private static List<string> objColErrors = new List<string>();
			
		static void Main(string [] args)
		{ // Prueba el cliente de FTP
				TestFtpConnections(true);
				TestFtpConnections(false);
			// Muestra los errores (al imprimirse en la consola, puede que los errores anteriores los perdamos)
				if (objColErrors.Count == 0)
					Console.WriteLine("No se ha detectado ningún error");
				else
					{ Console.WriteLine("Errores:");
						foreach (string strError in objColErrors)
							Console.WriteLine(new string('-', 3) + strError);
					}
			// Espera
				Console.WriteLine();
				Console.WriteLine("Pulse una tecla ...");
				Console.ReadLine();
		}

		/// <summary>
		///		Prueba las conexiones FTP
		/// </summary>
		private static void TestFtpConnections(bool blnPassive)
		{ TestFtp(FtpClient.FtpProtocol.Ftp, cnstStrServer, cnstIntPort, cnstStrUser, cnstStrPassword, blnPassive);
			TestFtp(FtpClient.FtpProtocol.FtpS, cnstStrServer, cnstIntPortFtps, cnstStrUser, cnstStrPassword, blnPassive);
			TestFtp(FtpClient.FtpProtocol.FtpES, cnstStrServer, cnstIntPortFtpEs, cnstStrUser, cnstStrPassword, blnPassive);
		}

		/// <summary>
		///		Lanza las pruebas sobre el servidor FTP
		/// </summary> 
		private static void TestFtp(FtpClient.FtpProtocol intMode, string strServer, int intPort, string strUser, 
																string strPassword, bool blnPassive)
		{ using (FtpClient objFtpClient = new FtpClient(intMode, strServer, intPort, new System.Net.NetworkCredential(strUser, strPassword), 
																										GetClientParameters(intMode, blnPassive)))
				{ // Log
						Log(0, string.Format("Pruebas de {0}:{1} - {2} - Pasivo: {3}", strServer, intPort, intMode, blnPassive), false);
					// Asigna el manejador de eventos
						objFtpClient.ConnectionInitialized += (objSender, objEventArgs) =>
																		{	System.Diagnostics.Debug.WriteLine("Conexión inicializada", false);
																		};

						objFtpClient.IOError += (objSender, objEventArgs) =>
																		{	System.Diagnostics.Debug.WriteLine("IOError: " + objEventArgs.ToString(), false);
																		};
						objFtpClient.Reply += (objSender, objEventArgs) =>
																		{	System.Diagnostics.Debug.WriteLine("Reply: " + objEventArgs.Reply.ToString(), false);
																		};
						objFtpClient.Request += (objSender, objEventArgs) =>
																		{	System.Diagnostics.Debug.WriteLine("Request: " + objEventArgs.RequestCommand, false);
																		};
					// Conecta el cliente
						objFtpClient.Connect();
					// Crea el directorio /cnstStrPathRemote
						if (objFtpClient.Commands.MakeDir(cnstStrPathRemote))
							Log(1, "Creado el directorio " + cnstStrPathRemote, false);
					// Comprueba los cambios de directorio
						objFtpClient.Commands.ChangeDir("/");
						objFtpClient.Commands.ChangeDir(cnstStrPathRemote);
						objFtpClient.Commands.ChangeDir("/");
						if (objFtpClient.Commands.GetActualPath() != "/")
							Log(1, "No se puede pasar al directorio /", true);
						else
							Log(1, "Pruebas de paso al directorio " + cnstStrPathRemote + " superadas", false);
					// Comprueba la creación de directorios
						if (!objFtpClient.Commands.MakeDirRecursive(cnstStrPathRemote + "/hola/test/pruebas/"))
							Log(1, "No se ha podido crear el directorio recursivo", true);
						objFtpClient.Commands.ChangeDir(cnstStrPathRemote + "/hola/test/pruebas/");
						if (!objFtpClient.Commands.ChangeDir(cnstStrPathRemote + "/hola/test/pruebas/"))
							Log(1, "No se ha creado el directorio recursivo", true);
						if (objFtpClient.Commands.RemoveDirectory(cnstStrPathRemote + "/hola"))
							Log(1, "Se ha borrado un directorio intermedio. No debería pasar", true);
					// Borra los directorios creados recursivamente (excepto el DEV)
						objFtpClient.Commands.RemoveDirectory(cnstStrPathRemote + "/hola/test/pruebas/");
						objFtpClient.Commands.RemoveDirectory(cnstStrPathRemote + "/hola/test/");
						objFtpClient.Commands.RemoveDirectory(cnstStrPathRemote + "/hola");
						if (objFtpClient.Commands.ChangeDir(cnstStrPathRemote + "/hola"))
							Log(1, "No debería existir este directorio", true);
					// Obtiene el directorio del servidor
						objFtpClient.Commands.ChangeDir(cnstStrPathRemote);
						if (!objFtpClient.Commands.GetActualPath().Equals(cnstStrPathRemote, StringComparison.CurrentCultureIgnoreCase))
							Log(1, "No se ha cambiado al directorio " + cnstStrPathRemote, true);
					// Borra el directorio de pruebas (esté o no)
						objFtpClient.Commands.RemoveDirectory(cnstStrPathRemote + "/test");
					// Crea un nuevo directorio
						objFtpClient.Commands.MakeDir(cnstStrPathRemote + "/test");
						objFtpClient.Commands.ChangeDir(cnstStrPathRemote + "/test");
						if (!objFtpClient.Commands.GetActualPath().Equals(cnstStrPathRemote + "/test", StringComparison.CurrentCultureIgnoreCase))
							Log(1, "No se ha cambiado al directorio dev/test. Es posible que no se haya creado", true);
					// Intenta crear de nuevo el mismo directorio pero esta vez sin tener en cuenta las excepciones
						if (objFtpClient.Commands.MakeDir(cnstStrPathRemote + "/test"))
							Log(1, "No debería haber creado correctamente el directorio", true);
					// Borra el directorio que acabamos de crear para dejar todo como estaba
						objFtpClient.Commands.RemoveDirectory(cnstStrPathRemote + "/test");
					// Prueba las subidas y descargas
						Log(1, "Pruebas de subida y descarga", false);
						TestSaveFiles(objFtpClient, "Test.txt", false);
						TestSaveFiles(objFtpClient, "Test.txt", true);
					// Prueba de lista
						Log(1, "Prueba de comando LIST", false);
						TestList(objFtpClient.Commands.List(cnstStrPathRemote));
						Log(1, "Pureba de comando MLST", false);
						TestList(objFtpClient.Commands.MList(cnstStrPathRemote));
					// Log
						Log(0, string.Format("Fin de pruebas de {0}:{1} - {2} - Pasivo: {3}", strServer, intPort, intMode, blnPassive), false);
						Log(0, new string('-', 50), false);
						Log(0, "", false);
				}
		}

		/// <summary>
		///		Obtiene los parámetros del cliente
		/// </summary>
		private static Bau.Libraries.LibFtpClient.Parameters.FtpClientParameters GetClientParameters(FtpClient.FtpProtocol intMode, bool blnPassive)
		{ return new Bau.Libraries.LibFtpClient.Parameters.FtpClientParameters(blnPassive);
		}

		/// <summary>
		///		Prueba de subida y descarga de archivos
		/// </summary>
		private static void TestSaveFiles(FtpClient objFtpClient, string strFileName, bool blnWithDelete)
		{	string strFileSource = System.IO.Path.Combine(GetPathBase(), strFileName);
			string strFileTarget = GetFileNameTarget(strFileName);
			string strFileDownload = System.IO.Path.Combine(GetPathBase(), "FileDownload.xlsx");

				// Elimina el archivo de descarga (por si hemos dejado alguno antiguo)
					KillFile(strFileDownload);
				// Sube un archivo
					Log(2, "Subir el archivo " + strFileSource, false);
					objFtpClient.Commands.Upload(strFileSource, strFileTarget);
				// Comprueba si existe el archivo en el destino
					if (!ExistsFile(objFtpClient, strFileTarget))
						Log(3, "No se ha subido correctamente " + strFileTarget, true);
					else
						Log(3, "Subido correctamente", false);
				// Descarga el archivo
					Log(2, "Descargar el archivo " + strFileTarget, false);
					objFtpClient.Commands.Download(strFileTarget, strFileDownload);
				// Compara los tamaños de los archivos
					if (!System.IO.File.Exists(strFileDownload))
						Log(3, "No se ha descargado el archivo " + strFileDownload, true);
					else if (new System.IO.FileInfo(strFileSource).Length != new System.IO.FileInfo(strFileDownload).Length)
						Log(3, "El tamaño del fichero descargado no es igual al tamaño del fichero inicial", true);
					else
						Log(3, "Archivo descargado correctamente", false);
				// Si son pruebas con borrado, elimina el archivo en el destino
					if (blnWithDelete)
						{ // Log
								Log(2, "Prueba de borrado", false);
							// Borra el archivo destino
								objFtpClient.Commands.Delete(strFileTarget);
							// Comprueba si existe el archivo en el destino
								if (ExistsFile(objFtpClient, strFileTarget))
									Log(3, "No se ha borrado correctamente " + strFileTarget, true);
								else
									Log(3, "Se ha borrado el archivo enviado al servidor", false);
							}
				// Elimina el archivo descargado
					KillFile(strFileDownload);
		}

		/// <summary>
		///		Elimina un archivo
		/// </summary>
		private static void KillFile(string strFile)
		{ try
				{ System.IO.File.Delete(strFile);
				}
			catch {}
		}

		/// <summary>
		///		Log
		/// </summary>
		private static void Log(int intIndent, string strMessage, bool blnError)
		{ Console.WriteLine("{0} {1}", new string(' ', intIndent * 2), strMessage);
			if (blnError)
				objColErrors.Add(strMessage);
		}

		/// <summary>
		///		Obtiene el nombre del archivo en el servidor de FTP
		/// </summary>
		private static string GetFileNameTarget(string strFileName)
		{ return System.IO.Path.Combine(cnstStrPathRemote, 
																		string.Format("{0:yyyy.MM.dd.HH.mm.ss.fff}{1}", 
																									DateTime.Now, System.IO.Path.GetExtension(strFileName)));
		}

		/// <summary>
		///		Comprueba si existe un archivo en el servidor
		/// </summary>
		private static bool ExistsFile(FtpClient objFtpClient, string strFile)
		{ IList<FtpEntry> objColFiles = objFtpClient.Commands.List(cnstStrPathRemote);

				// Quita el directorio del nombre de archivos
					if (cnstStrPathRemote.Length > 0)
						strFile = strFile.Substring(cnstStrPathRemote.Length + 1);
				// Comprueba si existe el archivo
					foreach (FtpEntry objFile in objColFiles)
						if (objFile.Name.Equals(strFile, StringComparison.CurrentCultureIgnoreCase))
							return true;
				// Si ha llegado hasta aquí es porque no existe el archivo
					return false;
		}

		/// <summary>
		///		Muestra la lista de archivos
		/// </summary>
		private static void TestList(IList<FtpEntry> objColFiles)
		{ foreach (FtpEntry objFile in objColFiles)
				Log(2, string.Format("{0} - {1} - {2} - {3}", objFile.Name, objFile.Type == FtpEntry.FtpEntryType.Directory ? "Directorio" : "Archivo",
														 objFile.Size, objFile.Date), false);
		}

		/// <summary>
		///		Obtiene el directorio base de la aplicación
		/// </summary>
		private static string GetPathBase()
		{ return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
		}
	}
}
