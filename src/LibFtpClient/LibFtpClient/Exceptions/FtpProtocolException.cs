using System;
using System.Runtime.Serialization;

namespace Bau.Libraries.LibFtpClient.Exceptions
{
	/// <summary>
	///		Excepción relacionada con el protocolo
	/// </summary>
	[Serializable]
	public class FtpProtocolException : FtpException
	{
		[Obsolete("Serialization-only ctor")]
		protected FtpProtocolException()
		{
		}

		public FtpProtocolException(string message, FtpReply reply) : base(message)
		{	Reply = reply;
		}

		protected FtpProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{	Reply = new FtpReply(info.GetInt32("FtpReplyCode"));
		}

		public FtpProtocolException(string message) : base(message)
		{
		}

		public FtpProtocolException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public FtpProtocolException(string message, int hresult) : base(message, hresult)
		{
		}

		/// <summary>
		///		Cuando se sobrescribe en una clase derivada, asign <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
		///	con la información de la excepción
		/// </summary>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{ // Añade el código
				info.AddValue("FtpReplyCode", Reply.Code);
			// Llama al método base
				base.GetObjectData(info, context);
		}

		/// <summary>
		///		Respuesta
		/// </summary>
		public FtpReply Reply { get; private set; }
	}
}
