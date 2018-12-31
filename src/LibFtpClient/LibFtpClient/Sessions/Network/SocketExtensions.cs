using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Bau.Libraries.LibFtpClient.Sessions.Network
{
	/// <summary>
	///		Extensión de la clase Socket
	/// </summary>
	internal static class SocketExtensions
	{
		/// <summary>
		///		Conecta al socket
		/// </summary>
		internal static void Connect(this Socket socket, string host, int port, TimeSpan timeout)
		{
			AsyncConnect(socket, (s, a, o) => s.BeginConnect(host, port, a, o), timeout);
		}

		/// <summary>
		///		Conecta al socket
		/// </summary>
		internal static void Connect(this Socket socket, IPAddress[] addresses, int port, TimeSpan timeout)
		{
			AsyncConnect(socket, (s, a, o) => s.BeginConnect(addresses, port, a, o), timeout);
		}

		/// <summary>
		///		Conexión asíncrona
		/// </summary>
		private static void AsyncConnect(Action actConnect, TimeSpan timeout)
		{
			EventWaitHandle hndEventWait = new EventWaitHandle(false, EventResetMode.ManualReset);
			Thread thdConnect = new Thread(delegate ()
												{
													actConnect();
													hndEventWait.Set();
												}
											)
												{
													Name = "Socket.AsyncConnect"
												};

				// Arranca el hilo
				thdConnect.Start();
				// Espera a que termine el proceso
				hndEventWait.WaitOne(timeout);
		}

		/// <summary>
		///		Conexión asíncrona
		/// </summary>
		private static void AsyncConnect(Socket socket, Func<Socket, AsyncCallback, object, IAsyncResult> connect, TimeSpan timeout)
		{
			IAsyncResult asyncResult = connect(socket, null, null);

				if (asyncResult.AsyncWaitHandle.WaitOne(timeout))
					try
					{
						socket.EndConnect(asyncResult);
					}
					catch (SocketException) { }
					catch (ObjectDisposedException) { }
		}

		/// <summary>
		///		Conecta a un socket especificado
		/// </summary>
		internal static Socket Accept(this Socket socket, TimeSpan timeout)
		{
			return AsyncAccept(socket, (s, a, o) => s.BeginAccept(a, o), timeout);
		}

		/// <summary>
		///		Acepta la conexión de forma asíncrona
		/// </summary>
		private static Socket AsyncAccept(Socket socket, Func<Socket, AsyncCallback, object, IAsyncResult> accept, TimeSpan timeout)
		{
			IAsyncResult asyncResult = accept(socket, null, null);

				// Finaliza la aceptación del socket
				if (asyncResult.AsyncWaitHandle.WaitOne(timeout))
					try
					{
						return socket.EndAccept(asyncResult);
					}
					catch (SocketException) { }
					catch (ObjectDisposedException) { }
				// Si ha llegado hasta aquí es porque no se ha conectado correctamente
				return null;
		}
	}
}
