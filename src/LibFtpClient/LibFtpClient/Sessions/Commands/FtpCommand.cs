using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands
{
	/// <summary>
	///		Comando de FTP
	/// </summary>
	internal abstract class FtpCommand
	{
		internal FtpCommand(FtpConnection connection)
		{
			Connection = connection;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal abstract FtpReply Send();

		/// <summary>
		///		Comprueba el código especificado en una respuesta
		/// </summary>
		protected void Expect(params int[] codes)
		{
			Expect(Connection.ProtocolStream.ReadReply(), codes);
		}

		/// <summary>
		///		Comprueba los códigos espeficcados en una respuesta
		/// </summary>
		protected FtpReply Expect(FtpReply reply, params int[] codes)
		{
			// Si no existe ninguno de los códigos especificados
			if (!ExistsCode(reply.Code, codes))
			{
				// Cuando se encuentra el código 214 no se trata, puede que se dé una inconsistencia entre
				// comandos y respuestas así que lo mejor es desconectar para reiniciar las parejas comando / respuesta
				if (reply.Code == 214)
					Connection.Disconnect();
				// Lanza la excepción adecuada
				ThrowException(reply);
			}
			// Si todo ha ido bien, devuelve la respuesta leída
			return reply;
		}

		/// <summary>
		///		Comprueba si existe un código en un array
		/// </summary>
		private bool ExistsCode(int code, int[] expectedCodes)
		{ 
			// Recorre el array
			foreach (int intExpectedCode in expectedCodes)
				if (intExpectedCode == code)
					return true;
			// Si ha llegado hasta aquí es porque no ha encontrado nada
			return false;
		}

		/// <summary>
		///		Lanza la excepción adecuada para una respuesta
		/// </summary>
		protected void ThrowException(FtpReply reply)
		{
			if (reply.Class == FtpReply.FtpReplyCodeClass.Filesystem)
				throw new Exceptions.FtpFileException($"File error. Code={reply.Code} ('{reply.Lines[0]}')", reply);
			if (reply.Class == FtpReply.FtpReplyCodeClass.Connections)
				throw new Exceptions.FtpTransportException($"Connection error. Code={reply.Code} ('{reply.Lines[0]}')");
			throw new Exceptions.FtpProtocolException($"Expected other reply than {reply.Code} ('{reply.Lines[0]}')", reply);
		}

		/// <summary>
		///		Envía un comando
		/// </summary>
		protected FtpReply SendCommand(string command, params string[] parameters)
		{
			return Process(() => Connection.ProtocolStream.SendCommand(command, parameters),
						   "sending FTP request", command, parameters);
		}

		/// <summary>
		///		Lee la respuesta del stream
		/// </summary>
		protected FtpReply ReadReply()
		{
			return Process(() => Connection.ProtocolStream.ReadReply(), "reading FTP reply", "(ReadReply)");
		}

		/// <summary>
		///		Ejecuta una función en un contexto en el que se traducen las excepciones
		/// </summary>
		protected TResult Process<TResult>(Func<TResult> fncFunction, string commandDescription,
										   string requestCommand = null, string[] parameters = null)
		{
			try
			{
				return fncFunction();
			}
			catch (Exceptions.FtpProtocolException)
			{
				throw;
			}
			catch (System.Net.Sockets.SocketException exception)
			{
				Connection.Client.RaiseEventIOError(new EventArguments.ProtocolMessageEventArgs(requestCommand, parameters));
				Connection.Disconnect();
				throw new Exceptions.FtpTransportException($"Excepción de Socket. Comando: {commandDescription}", exception);
			}
			catch (System.IO.IOException exception)
			{
				Connection.Client.RaiseEventIOError(new EventArguments.ProtocolMessageEventArgs(requestCommand, parameters));
				Connection.Disconnect();
				throw new Exceptions.FtpTransportException($"Excepción de entrada salida. Comando: {commandDescription}", exception);
			}
		}

		/// <summary>
		///		Conexión
		/// </summary>
		protected FtpConnection Connection { get; }
	}
}
