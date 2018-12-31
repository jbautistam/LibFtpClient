using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

using Bau.Libraries.LibFtpClient.Exceptions;

namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Stream de transferencia activo para FTP
	/// </summary>
	internal class FtpActiveStream : FtpStream
	{ 
		// Variables privadas
		private readonly TimeSpan _connectTimeout;
		private readonly EventWaitHandle _eventWait = new ManualResetEvent(false);
		private IOException _lastException;

		internal FtpActiveStream(FtpConnection connection, Socket socket, TimeSpan connectTimeout)
							: base(connection, socket)
		{
			_connectTimeout = connectTimeout;
			socket.BeginAccept(TreatSocketAccept, socket);
		}

		/// <summary>
		///		Se asegura que tengamos un socket válido
		/// </summary>
		private void EnsureConnection()
		{
			if (InnerSocket == null || !InnerSocket.Connected)
			{
				if (!_eventWait.WaitOne(_connectTimeout))
					throw new FtpTransportException("No se puede conectar");
				if (_lastException != null)
					throw _lastException;
			}
		}

		/// <summary>
		///		Inicializa el socket
		/// </summary>
		protected override void InitSocket()
		{   // ... no hace nada. Simplemente implementa el interface del base
		}

		/// <summary>
		///		Delegado para activar el socket
		/// </summary>
		private void TreatSocketAccept(IAsyncResult asyncResult)
		{
			Socket socket = ((Socket) asyncResult.AsyncState).EndAccept(asyncResult);

				// Intenta asignar el socket
				try
				{
					SetSocket(socket);
				}
				catch (IOException exception)
				{
					_lastException = exception;
				}
				// Sale de la región crítica
				_eventWait.Set();
		}

		/// <summary>
		///		Stream interno
		/// </summary>
		protected override Stream InnerStream
		{
			get
			{   
				// Se asegura que exista una conexión
				EnsureConnection();
				// Devuelve el stream interno
				return base.InnerStream;
			}
		}
	}
}