using System;
using System.Runtime.Serialization;

namespace Bau.Libraries.LibFtpClient.Exceptions
{
	/// <summary>
	///		Excepción de tratamiento de archivos FTP
	/// </summary>
	[Serializable]
	public class FtpFileException : FtpProtocolException
	{
		[Obsolete("Serialization-only ctor")]
		protected FtpFileException() 
		{
		}

		public FtpFileException(string message, FtpReply reply) : base(message, reply) 
		{
		}

		protected FtpFileException(SerializationInfo info, StreamingContext context) : base(info, context) 
		{
		}

		public FtpFileException(string message) : base(message)
		{
		}

		public FtpFileException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public FtpFileException(string message, int hresult) : base(message, hresult)
		{
		}
	}
}