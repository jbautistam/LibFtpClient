using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Bau.Libraries.LibFtpClient.EventArguments
{
	/// <summary>
	///		Argumentos del evento CheckCertificate
	/// </summary>
	public class CheckCertificateEventArgs : EventArgs
	{
		public CheckCertificateEventArgs(X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
		{
			Certificate = x509Certificate;
			Chain = x509Chain;
			SslPolicyErrors = sslPolicyErrors;
			IsValid = true;
		}

		/// <summary>
		///		Invalida la instancia
		/// </summary>
		public void Invalidate()
		{
			IsValid = false;
		}

		/// <summary>
		///		Certificado
		/// </summary>
		public X509Certificate Certificate { get; }

		/// <summary>
		///		Cadena del certificado
		/// </summary>
		public X509Chain Chain { get; }

		/// <summary>
		///		Errores de SSL
		/// </summary>
		public SslPolicyErrors SslPolicyErrors { get; }

		/// <summary>
		///		Indica si es válido
		/// </summary>
		public bool IsValid { get; private set; }
	}
}
