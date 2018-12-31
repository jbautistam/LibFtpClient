using System;

namespace Bau.Libraries.LibFtpClient.EventArguments
{
	/// <summary>
	///		Argumentos de los eventos con los mensajes del protocolo
	/// </summary>
	public class ProtocolMessageEventArgs : EventArgs
	{
		public ProtocolMessageEventArgs(string request = null, string[] parameters = null, FtpReply reply = null)
		{
			RequestCommand = request;
			RequestParameters = parameters;
			Reply = reply;
		}

		/// <summary>
		///		Comando de la solicitud
		/// </summary>
		public string RequestCommand { get; }

		/// <summary>
		///		Parámetros del comando de solicitud
		/// </summary>
		public string[] RequestParameters { get; }

		/// <summary>
		///		Respuesta del servidor
		/// </summary>
		public FtpReply Reply { get; }
	}
}
