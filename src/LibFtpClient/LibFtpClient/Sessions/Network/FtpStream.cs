using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Stream de FTP
	/// </summary>
	internal abstract class FtpStream : Stream
	{   // Constantes privadas
		private const string Eol = "\r\n"; // Fin de l�nea para FTP
												  // Variables privadas
		// Variables privadas
		private static byte[] _eolB;
		private readonly byte[] _lineBuffer = new byte[10 << 10];

		internal FtpStream(FtpConnection connection, Socket socket)
		{
			Connection = connection;
			InnerSocket = socket;
			InitSocket();
		}

		/// <summary>
		///		Inicializa el socket interno
		/// </summary>
		protected abstract void InitSocket();

		/// <summary>
		///		Asigna el socket
		/// </summary>
		protected void SetSocket(Socket socket)
		{   
			// Crea el stream de datos
			InnerStream = CreateDataStream(socket);
			// Inicializa el socket
			InnerSocket = socket;
			InnerSocket.SendBufferSize = 1492;
		}

		/// <summary>
		///		Crea un stream de datos y opcionalmente lo asegura
		/// </summary>
		private Stream CreateDataStream(Socket socket)
		{
			Stream stmData = new NetworkStream(socket, true);

				// Si hay que asegurar el stream, lo pasa a SSL
				if (Connection.Client.ClientParameters.ChannelProtection.HasFlag(Parameters.FtpClientParameters.FtpProtection.DataChannel))
					stmData = UpgradeToSsl(stmData, Connection.Client.ClientParameters.GetSslProtocol());
				// Devuelve el stream de datos
				return stmData;
		}

		/// <summary>
		///		Protege el stream con SSL
		/// </summary>
		public void UpgradeToSsl(SslProtocols protocols)
		{
			InnerStream = UpgradeToSsl(InnerStream, protocols);
		}

		/// <summary>
		///		Protege un stream con SSL
		/// </summary>
		private Stream UpgradeToSsl(Stream data, SslProtocols protocols)
		{
			if (data is SslStream)
				return data;
			else
			{
				SslStream ssl = new SslStream(data, false, CheckCertificateHandler);

					// Autentifica el stream
					ssl.AuthenticateAsClient(Connection.Client.Uri.Host, null, protocols, false);
					// Devuelve el stream SSL
					return ssl;
			}
		}

		/// <summary>
		///		Delegado para comprobar el certificado
		/// </summary>
		private bool CheckCertificateHandler(object sender, X509Certificate certificate, X509Chain chain,
																				 SslPolicyErrors sslPolicyErrors)
		{
			EventArguments.CheckCertificateEventArgs evntArgs = new EventArguments.CheckCertificateEventArgs(certificate, chain, sslPolicyErrors);

				// Llama al manejador del evento
				Connection.Client.RaiseEventCheckCertificate(evntArgs);
				// Devuelve el valor que indica si el certificado es correcto
				return evntArgs.IsValid;
		}

		/// <summary>
		///		Abandona el protocolo SSL
		/// </summary>
		/// <remarks>
		///		A�n no est� desarrollado
		/// </remarks>
		public bool LeaveSsl()
		{
			return false;
		}

		/// <summary>
		///		Aborta la instancia
		/// </summary>
		///	<remarks>
		///		Utilizarlo con precauci�n porque no env�a informaci�n al canal de control como Dispose()
		/// </remarks>
		public void Abort()
		{
			Release(false);
		}

		/// <summary>
		///		Indica que el stream va a funcionar como un stream validado (requiere una respuesta del servidor 
		///	cuando hace el Dispose)
		/// </summary>
		public FtpStream Validated()
		{   
			// Modifica el valor que indica si se debe esperar el final de respuesta
			ExpectEndReply = true;
			// Devuelve el stream
			return this;
		}

		/// <summary>
		///		Procesa una acci�n
		/// </summary>
		protected void Process(Action action)
		{
			Process(delegate
								{
									action();
									return 0;
								});
		}

		/// <summary>
		///		Procesa una funci�n tratando las excepciones
		/// </summary>
		protected TResult Process<TResult>(Func<TResult> function)
		{
			try
			{
				return function();
			}
			catch (SocketException exception)
			{
				throw new Exceptions.FtpTransportException("Excepci�n de Socket sobre el stream de FTP", exception);
			}
			catch (IOException exception)
			{
				throw new Exceptions.FtpTransportException("Excepci�n de entrada salida sobre el stream de FTP", exception);
			}
		}

		/// <summary>
		///		Limpia el buffer de escritura del stream
		/// </summary>
		public override void Flush()
		{
			Process(() => InnerStream.Flush());
		}

		/// <summary>
		///		Lee la respuesta de un stream
		/// </summary>
		internal FtpReply ReadReply()
		{
			FtpReply reply = new FtpReply();
			bool end = false;

				// Lee las l�neas recibidas
				while (!end)
				{
					string line = ReadLine();

						// Si no ha recibido nada, se desconecta
						if (line == null)
						{
							Connection.Disconnect();
							throw new Exceptions.FtpTransportException($"Error cuando se procesaba la respuesta a ({reply})");
						}
						// Si no puede interpretar la l�nea, sale del bucle
						end = !reply.ParseLine(line);
				}
				// Lanza el evento con la respuesta
				Connection.Client.RaiseEventReply(new EventArguments.ProtocolMessageEventArgs(null, null, reply));
				// Devuelve la respuesta
				return reply;
		}

		/// <summary>
		///		Env�a un comando
		/// </summary>
		internal FtpReply SendCommand(string command, string[] parameters = null)
		{   
			// Lanza el evento de solicitud
			Connection.Client.RaiseEventRequest(new EventArguments.ProtocolMessageEventArgs(command,
																							command == "PASS" ? new string[] { "****" } : parameters));
			// Escribe la l�nea
			WriteLine(GetCommandLine(command, parameters));
			// y devuelve la respuesta
			return ReadReply();
		}

		/// <summary>
		///		Lee un byte del stream
		/// </summary>
		public override int ReadByte()
		{
			byte[] buffer = new byte[1];
			int read = Read(buffer, 0, 1);

				// Devuelve el byte le�do
				if (read == 0)
					return -1;
				else
					return buffer[0];
		}

		/// <summary>
		///		Obtiene la l�nea de comando
		/// </summary>
		private string GetCommandLine(string command, params string[] parameters)
		{
			System.Text.StringBuilder line = new System.Text.StringBuilder(command);

				// A�ade los par�metros
				if (parameters != null)
					foreach (string strParameter in parameters)
					{
						line.Append(' ');
						line.Append(strParameter);
					}
				// Devuelve la l�nea de comando
				return line.ToString();
		}

		/// <summary>
		///		Lee una l�nea
		/// </summary>
		private string ReadLine()
		{
			byte[] arrBytEolB = EolB;
			int index = 0;
			byte[] buffer = _lineBuffer;
			bool end = false;

				// Lee los bytes del stream y los deja en una l�nea
				while (!end)
				{
					int read = ReadByte();

						if (read == -1)
							end = true;
						else
						{ 
							// A�ade el byte le�do al buffer
							buffer[index++] = (byte) read;
							// Comprueba si se ha terminado
							end = EndsWith(buffer, index, arrBytEolB) || index >= buffer.Length;
						}
				}
				// Obtiene la cadena de salida
				if (buffer.Length < arrBytEolB.Length)
					return "";
				else
					return Connection.Client.ClientParameters.Encoding.GetString(buffer, 0, buffer.Length - arrBytEolB.Length);
		}

		/// <summary>
		///		Comprueba si un array de bytes termina por una serie de bytes
		/// </summary>
		private bool EndsWith(byte[] buffer, int bufferLength, byte[] end)
		{ 
			// Si no tiene longitud suficiente, no sigue comparando
			if (bufferLength < end.Length)
				return false;
			// Comprueba los �ltimos bytes
			for (int index = 0; index < end.Length; index++)
				if (buffer[bufferLength - end.Length + index] != end[index])
					return false;
			// Si ha llegado hasta aqu� es porque finaliza correctamente
			return true;
		}

		/// <summary>
		///		Escribe una l�nea en el stream de datos
		/// </summary>
		private void WriteLine(string line)
		{
			Write(line + Eol);
		}

		/// <summary>
		///		Escribe una cadena en el stream de datos
		/// </summary>
		private void Write(string line)
		{
			byte[] buffer = Connection.Client.ClientParameters.Encoding.GetBytes(line);

				// Escribe el buffer de bytes
				Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		///		Lee datos del stream
		/// </summary>
		public override int Read(byte[] buffer, int offset, int count)
		{
			return Process(() => BaseRead(buffer, offset, count));
		}

		/// <summary>
		///		Lee datos del stream
		/// </summary>
		private int BaseRead(byte[] buffer, int offset, int size)
		{
			return InnerStream.Read(buffer, offset, size);
		}

		/// <summary>
		///		Escribe datos en el stream
		/// </summary>
		public override void Write(byte[] buffer, int offset, int count)
		{
			Process(() => BaseWrite(buffer, offset, count));
		}

		/// <summary>
		///		Escribe en el stream
		/// </summary>
		private void BaseWrite(byte[] buffer, int offset, int size)
		{
			InnerStream.Write(buffer, offset, size);
		}

		/// <summary>
		///		Cambia la posici�n en el stream
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return InnerStream.Seek(offset, origin);
		}

		/// <summary>
		///		Asigna el tama�o requerido para el stream
		/// </summary>
		public override void SetLength(long value)
		{
			InnerStream.SetLength(value);
		}

		/// <summary>
		///		Libera los recursos no utilizados de <see cref="T:System.Net.Sockets.NetworkStream"/> y libera los
		///	recursos administrados
		/// </summary>
		protected override void Dispose(bool disposing)
		{ // Llama al base
			base.Dispose(disposing);
			// Libera los recursos
			if (disposing && !IsDisposed)
			{
				bool connected = true;

					// Indica que se ha liberado
					IsDisposed = true;
					// Libera los recursos
					try
					{ 
						// Libera el socket
						if (InnerSocket != null)
						{ 
							// Guarda el valor que indica si estaba conectado
							connected = InnerSocket.Connected;
							// Libera el socket
							if (connected)
								Process(delegate
												{
													InnerSocket.Shutdown(SocketShutdown.Both);
													InnerSocket.Close(300000);
												}
										);
						}
						// Libera el stream
						Process(delegate
										{
											if (InnerStream != null)
												InnerStream.Dispose();
										}
								);
					}
					finally
					{
						Release(ExpectEndReply);
					}
					// Si no estaba conectado, lanza una excepci�n
					if (!connected)
						throw new Exceptions.FtpTransportException("Stream cerrado anteriormente por el servidor");
			}
		}

		/// <summary>
		///		Libera la instancia
		/// </summary>
		private void Release(bool waitReply)
		{
			FtpConnection connection = Connection;

				if (connection != null)
					try
					{
						if (waitReply)
						{   
							// Espera los resultados:	
							//		226: ACK predeterminado
							//		150: Si el stream se abri� pero nunca se ha enviado nada, podemos salir sin problema
							Process(() => new Commands.FtpEmptyStreamCommand(connection, new int[] { 226, 150 }).Send());
						}
					}
					finally
					{ // en transferencias largas, puede que se haya cerrado el socket, sin embargo, 
					  // lo debemos se�alar en el cliente
					}
		}

		/// <summary>
		///		Conexi�n al servidor FTP
		/// </summary>
		internal FtpConnection Connection { get; }

		/// <summary>
		///		Indica si se debe esperar un final de respuesta o no
		/// </summary>
		protected bool ExpectEndReply { get; private set; }

		/// <summary>
		///		Indica si est� cerrado
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		///		Indica si se puede leer del stream
		/// </summary>
		public override bool CanRead
		{
			get { return InnerStream.CanRead; }
		}

		/// <summary>
		///		Indica si el stream acepta b�squedas
		/// </summary>
		public override bool CanSeek
		{
			get { return InnerStream.CanSeek; }
		}

		/// <summary>
		///		Indica si se puede escribir sobre el stream
		/// </summary>
		public override bool CanWrite
		{
			get { return InnerStream.CanWrite; }
		}

		/// <summary>
		///		Tama�o del stream
		/// </summary>
		public override long Length
		{
			get { return InnerStream.Length; }
		}

		/// <summary>
		///		Posici�n actual en el stream
		/// </summary>
		public override long Position
		{
			get { return InnerStream.Position; }
			set { InnerStream.Position = value; }
		}

		/// <summary>
		///		Array de bytes de salto de l�nea
		/// </summary>
		private static byte[] EolB
		{
			get
			{   // Si no se hab�a definido el array de bytes de salto de l�nea, se define
				if (_eolB == null)
					_eolB = System.Text.Encoding.UTF8.GetBytes(Eol);
				// Devuelve el array de bytes del salto de l�nea
				return _eolB;
			}
		}

		/// <summary>
		///		Stream interno
		/// </summary>
		protected virtual Stream InnerStream { get; set; }

		/// <summary>
		///		Socket interno
		/// </summary>
		protected Socket InnerSocket { get; set; }
	}
}