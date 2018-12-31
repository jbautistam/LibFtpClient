using System;
using System.Collections.Generic;

namespace Bau.Libraries.LibFtpClient.Sessions
{
	/// <summary>
	///		Estado de la sesi�n
	/// </summary>
	internal class FtpSessionState
	{
		internal FtpSessionState(FtpConnection connection)
		{
			Connection = connection;
		}

		/// <summary>
		///		Obtiene un par�metro de sesi�n
		/// </summary>
		private string GetParameter(string name)
		{
			// Obtiene el valor del diccionario
			States.TryGetValue(name, out string value);
			// Devuelve el valor
			return value;
		}

		/// <summary>
		///		Asigna un par�metro al diccionario. Si no estaba, se lo env�a al servidor
		/// </summary>
		private void AssignParameter(string name, string value)
		{
			// Obtiene el valor del diccionario
			States.TryGetValue(name, out string current);
			// Asigna el valor al diccionario si no estaba ya
			if (current != value)
			{ 
				// Asigna el valor
				States[name] = value;
				// Env�a el cambio de par�metro al servidor
				new Commands.Server.FtpSetParameterCommand(Connection, name, value).Send();
			}
		}

		/// <summary>
		///		Conexi�n a la que se asocia el estado
		/// </summary>
		private FtpConnection Connection { get; }

		/// <summary>
		///		Diccionario de estados / par�metros en el servidor
		/// </summary>
		private IDictionary<string, string> States { get; } = new Dictionary<string, string>();

		/// <summary>
		///   Indizador sobre los par�metros por nombre
		/// </summary>
		internal string this[string name]
		{
			get { return GetParameter(name); }
			set { AssignParameter(name, value); }
		}
	}
}
