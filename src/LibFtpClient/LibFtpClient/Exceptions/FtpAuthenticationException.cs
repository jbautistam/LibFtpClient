using System;
using System.Runtime.Serialization;

namespace Bau.Libraries.LibFtpClient.Exceptions
{
	/// <summary>
	///		Excepción de autentificación
	/// </summary>
	[Serializable]
	public class FtpAuthenticationException : FtpProtocolException
	{
		[Obsolete("Serialization-only ctor")]
		protected FtpAuthenticationException()
		{
		}

		public FtpAuthenticationException(string message, FtpReply reply) : base(message, reply)
		{
		}

		protected FtpAuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public FtpAuthenticationException(string message) : base(message)
		{
		}

		public FtpAuthenticationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public FtpAuthenticationException(string message, int hresult) : base(message, hresult)
		{
		}
	}
}