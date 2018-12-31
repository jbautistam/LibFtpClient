using System;
using System.Text.RegularExpressions;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Streams
{
	/// <summary>
	///		Comando para cambiar a modo pasivo (PASV) o modo pasivo extendido (EPSV)
	/// </summary>
	internal class FtpChangePassiveCommand : FtpCommand
	{ // Variables privadas
		private static readonly Regex EpsvEx = new Regex(@".*?\(\|\|\|(?<port>\d*)\|\)", RegexOptions.Compiled);
		private static readonly Regex PasvEx = new Regex(@".*?\(\s*(?<ip1>\d*)\s*,\s*(?<ip2>\d*)\s*,\s*(?<ip3>\d*)\s*,\s*(?<ip4>\d*)\s*,\s*(?<portHi>\d*)\s*,\s*(?<portLo>\d*)\s*\)", 
														 RegexOptions.Compiled);

		internal FtpChangePassiveCommand(FtpConnection connection, bool extended) : base(connection)
		{
			IsExtended = extended;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply;

				// Cambia a modo pasivo y obtiene los parámetros
				if (IsExtended)
				{ 
					// Envía el comando
					reply = Expect(SendCommand("EPSV"), 229, 421);
					// Obtiene los parámetros Epsv
					ReadParametersEpsv(reply);
				}
				else
				{ // Envía el comando
					reply = Expect(SendCommand("PASV"), 227, 421);
					// Obtiene los parámetros de PSV
					ReadParametersPsv(reply);
				}
				// Devuelve la respuesta
				return reply;
		}

		/// <summary>
		///		Lee los parámetros de EPSV
		/// </summary>
		private void ReadParametersEpsv(FtpReply reply)
		{   
			// Si responde con código 229, recoge el puerto, si no, lo intenta de nuevo (puede haber dado
			// un error de tipo "421 - Demasiadas conexiones abiertas")
			if (reply.Code == 229)
			{
				Match match = EpsvEx.Match(reply.Lines[0]);

					// Obtiene los datos
					Host = Connection.Client.Uri.Host;
					Port = int.Parse(match.Groups["port"].Value);
					// Indica que se ha pasado a modo pasivo
					IsPassive = true;
			}
		}

		/// <summary>
		///		Lee los parámetros de PSV
		/// </summary>
		private void ReadParametersPsv(FtpReply reply)
		{ 
			// Si se encuentra el código 227, recoge el host y puerto, si no, lo intenta de nuevo (puede haber dado un
			// error del tipo "421 - Demasiadas conexiones abiertas")
			if (reply.Code == 227)
			{
				Match match = PasvEx.Match(reply.Lines[0]);

					// Obtiene el host y el puerto de la expresión regular
					Host = string.Format("{0}.{1}.{2}.{3}",
										 match.Groups["ip1"], match.Groups["ip2"],
										 match.Groups["ip3"], match.Groups["ip4"]);
					Port = int.Parse(match.Groups["portHi"].Value) * 256 + int.Parse(match.Groups["portLo"].Value);
					// Indica que se ha pasado a modo pasivo
					IsPassive = true;
			}
		}

		/// <summary>
		///		Indica si la transferencia es a modo pasivo extendido (EPSV)
		/// </summary>
		internal bool IsExtended { get; }

		/// <summary>
		///		Host leído en la respuesta
		/// </summary>
		internal string Host { get; private set; }

		/// <summary>
		///		Puerto leído en la respuesta
		/// </summary>
		internal int Port { get; private set; }

		/// <summary>
		///		Indica si se ha pasado a modo pasivo
		/// </summary>
		internal bool IsPassive { get; private set; }
	}
}
