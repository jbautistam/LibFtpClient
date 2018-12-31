using System;
using System.IO;
using System.Runtime.Serialization;

namespace Bau.Libraries.LibFtpClient.Exceptions
{
	/// <summary>
	///		Excepción base para FTP
	/// </summary>
	[Serializable]
	public class FtpException : IOException
	{
		[Obsolete("Serialization-only ctor")]
		protected FtpException() 
		{
		}

		public FtpException(string message) : base(message) 
		{
		}

		public FtpException(string message, Exception innerException) : base(message, innerException) 
		{
		}

		protected FtpException(SerializationInfo info, StreamingContext context) : base(info, context) 
		{
		}

		public FtpException(string message, int hresult) : base(message, hresult)
		{
		}
	}
}
