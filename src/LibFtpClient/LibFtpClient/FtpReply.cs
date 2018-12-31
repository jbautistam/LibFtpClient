using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Bau.Libraries.LibFtpClient
{
	/// <summary>
	///		Respuestas del servidor FTP 
	/// </summary>
	[DebuggerDisplay("FTP {Code} {Lines[0]}")]
	public class FtpReply
	{
		/// <summary>
		///		Clase de respuesta
		/// </summary>
		public enum FtpReplyCodeClass
		{
			/// <summary>Errores sintácticos</summary>
			Syntax = 0,
			/// <summary>Respuestas a solicitudes de información: status o help</summary>
			Information = 10,
			/// <summary>Respuestas que hacer referencia a las conexiones de control y datos</summary>
			Connections = 20,
			/// <summary>Respuestas para el proceso de login y procedimientos de cuenta</summary>
			AuthenticationAndAccounting = 30,
			/// <summary>
			///		Respuestas que indican el estado del sistema del servidor de archivos respecto a operaciones de
			///	transferencia u otras acciones sobre el sistema de archivos
			/// </summary>
			Filesystem = 50
		}
		/// <summary>
		///		Código de gravedad de las clases de respuesta
		/// </summary>
		public enum FtpReplyCodeSeverity
		{
			/// <summary>
			/// The requested action is being initiated; expect another reply before proceeding with a new command.  (The
			/// user-process sending another command before the completion reply would be in violation of protocol; but
			/// server-FTP processes should queue any commands that arrive while a preceding command is in progress.)  This
			/// type of reply can be used to indicate that the command was accepted and the user-process may now pay attention
			/// to the data connections, for implementations where simultaneous monitoring is difficult.  The server-FTP
			/// process may send at most, one 1yz reply per command.
			/// </summary>
			PositivePreliminary = 100,
			/// <summary>
			/// The requested action has been successfully completed.  A new request may be initiated.
			/// </summary>
			PositiveCompletion = 200,
			/// <summary>
			/// The command has been accepted, but the requested action is being held in abeyance, pending receipt of further
			/// information.  The user should send another command specifying this information.  This reply is used in
			/// command sequence groups.
			/// </summary>
			PositiveIntermediate = 300,
			/// <summary>
			/// The command was not accepted and the requested action did not take place, but the error condition is temporary and
			/// the action may be requested again.  The user should return to the beginning of the command sequence, if any.
			/// It is difficult to assign a meaning to "transient", particularly when two distinct sites (Server- and
			/// User-processes) have to agree on the interpretation. 
			/// Each reply in the 4yz category might have a slightly different time value, but the intent is that the
			/// user-process is encouraged to try again.  A rule of thumb in determining if a reply fits into the 4yz or the 5yz
			/// (Permanent Negative) category is that replies are 4yz if the commands can be repeated without any change in
			/// command form or in properties of the User or Server (e.g., the command is spelled the same with the same
			/// arguments used; the user does not change his file access or user name; the server does not put up a new
			/// implementation.)        
			/// /// </summary>
			TransientNegativeCompletion = 400,
			/// <summary>
			/// The command was not accepted and the requested action did not take place.  The User-process is discouraged from
			/// repeating the exact request (in the same sequence).  Even some "permanent" error conditions can be corrected, so
			/// the human user may want to direct his User-process to reinitiate the command sequence by direct action at some
			/// point in the future (e.g., after the spelling has been changed, or the user has altered his directory status.)
			/// </summary>
			PermanentNegativeCompletion = 500
		}
		// Variables privadas
		private static readonly Regex _firstLine = new Regex(@"^(?<code>\d{3})\-(?<line>.*)", RegexOptions.Compiled);
		private static readonly Regex _lastLine = new Regex(@"^(?<code>\d{3})\ (?<line>.*)", RegexOptions.Compiled);

		public FtpReply() : this(0, null) { }

		public FtpReply(IEnumerable<string> lines) : this(0, lines) { }

		public FtpReply(int code, IEnumerable<string> lines = null)
		{
			if (lines != null)
				foreach (string line in lines)
					if (!ParseLine(line))
						break;
		}

		/// <summary>
		///		Interpreta la línea
		/// </summary>
		internal bool ParseLine(string line)
		{
			bool first = false;
			Match lastMatch = _lastLine.Match(line);

				// Añade la línea
				if (lastMatch.Success)
				{
					Code = int.Parse(lastMatch.Groups["code"].Value);
					AppendLine(lastMatch.Groups["line"].Value);
				}
				else
				{
					Match firstMatch = _firstLine.Match(line);

						// Añade la línea
						if (firstMatch.Success)
							AppendLine(firstMatch.Groups["line"].Value);
						else
							AppendLine(line);
						// Indica que se ha obtenido sobre la primera línea
						first = true;
				}
				// Devuelve el valor que indica si se ha encontrado en la primera línea
				return first;
		}

		/// <summary>
		///		Añade la línea
		/// </summary>
		private void AppendLine(string line)
		{
			List<string> lines = new List<string>();

				// Obtiene las líneas iniciales
				if (Lines != null)
					lines.AddRange(Lines);
				// Añade la línea
				lines.Add(line);
				// y pasa las líneas a la propiedad
				Lines = lines.ToArray();
		}

		/// <summary>
		///		Obtiene una cadena con los datos de la respuesta
		/// </summary>
		public override string ToString()
		{
			string debug = Code.ToString();

				// Añade los parámetros
				if (Lines == null || Lines.Length == 0)
					debug += " / Sin parámetros";
				else
					foreach (string line in Lines)
						debug += $" / {line}";
				// Devuelve el código y los parámetros
				return debug;
		}

		/// <summary>
		///		Código de respuesta
		/// </summary>
		public int Code { get; private set; }

		/// <summary>
		///		Gravedad de respuesta
		/// </summary>
		public FtpReplyCodeSeverity Severity
		{
			get { return (FtpReplyCodeSeverity) ((Code / 100) * 100); }
		}

		/// <summary>
		///		Clase de respuesta
		/// </summary>
		public FtpReplyCodeClass Class
		{
			get { return (FtpReplyCodeClass) (((Code / 10) % 10) * 10); }
		}

		/// <summary>
		///		Indica si es una respuesta correcta
		/// </summary>
		public bool IsSuccess
		{
			get { return Code < (int) FtpReplyCodeSeverity.TransientNegativeCompletion; }
		}

		/// <summary>
		///		Líneas de parámetros de la respuesta
		/// </summary>
		public string[] Lines { get; private set; }
	}
}