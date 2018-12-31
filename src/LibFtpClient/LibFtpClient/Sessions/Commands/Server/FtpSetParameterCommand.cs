using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands.Server
{
	/// <summary>
	///		Comando para asignar un parámetro
	/// </summary>
	internal class FtpSetParameterCommand : FtpCommand
	{
		internal FtpSetParameterCommand(FtpConnection connection, string key, string value) : base(connection)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			return Expect(SendCommand(Key, Value), 200);
		}

		/// <summary>
		///		Clave del parámetro a enviar
		/// </summary>
		internal string Key { get; }

		/// <summary>
		///		Valor del parámetro a enviar
		/// </summary>
		internal string Value { get; }
	}
}
