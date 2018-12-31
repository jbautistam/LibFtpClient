using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArxOne.Ftp.Sessions.Commands
{
	/// <summary>
	///		Comando genérico (comando / parámetros / resultados
	/// </summary>
	internal class FtpGenericCommand : FtpCommand
	{
		internal FtpGenericCommand(FtpConnection objConnection, string strCommand, string [] arrStrParameters,
															 int [] arrIntExpectedResults) : base(objConnection)
		{ Command = strCommand;
			Parameters = arrStrParameters;
			ExpectedResults = arrIntExpectedResults;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{ if (ExpectedResults != null && ExpectedResults.Length > 0)
				return Expect(SendCommand(Command, Parameters), ExpectedResults);
			else
				return SendCommand(Command, Parameters);
		}

		/// <summary>
		///		Comando
		/// </summary>
		internal string Command { get; private set; }

		/// <summary>
		///		Parámetros
		/// </summary>
		internal string [] Parameters { get; private set; }

		/// <summary>
		///		Resultados esperados
		/// </summary>
		internal int [] ExpectedResults { get; private set; }
	}
}
