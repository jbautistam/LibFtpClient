using System;
using System.Runtime.Serialization;

namespace Bau.Libraries.LibFtpClient.Exceptions
{
	/// <summary>
	///		Excepción de transporte de FTP
	/// </summary>
	[Serializable]
	public class FtpTransportException : FtpException
	{
		[Obsolete("Serialization-only ctor")]
		protected FtpTransportException()
		{
		}

		public FtpTransportException(string message) : base(message)
		{
		}

		public FtpTransportException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected FtpTransportException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public FtpTransportException(string message, int hresult) : base(message, hresult)
		{
		}
	}
}
