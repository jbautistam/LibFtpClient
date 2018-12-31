using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands
{
	/// <summary>
	///		Comando para vaciar el stream de códigos (no hace nada, simplemente vacía el stream con los 
	///	resultados esperados
	/// </summary>
	internal class FtpEmptyStreamCommand : FtpCommand
	{
		internal FtpEmptyStreamCommand(FtpConnection connection, int[] expectedCodes) : base(connection)
		{
			ExpectedCodes = expectedCodes;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{ 
			// Trata los códigos
			Expect(ExpectedCodes);
			// Devuelve los datos necesarios para cumplir con la interface
			return new FtpReply(0);
		}

		/// <summary>
		///		Códigos que espera el comando
		/// </summary>
		internal int[] ExpectedCodes { get; }
	}
}
