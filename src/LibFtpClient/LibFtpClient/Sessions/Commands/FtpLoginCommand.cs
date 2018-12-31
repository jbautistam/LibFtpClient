using System;

namespace Bau.Libraries.LibFtpClient.Sessions.Commands
{
	/// <summary>
	///		Comando de inicio de sesión
	/// </summary>
	internal class FtpLoginCommand : FtpCommand
	{
		internal FtpLoginCommand(FtpConnection connection) : base(connection) { }

		/// <summary>
		///		Envía el comando
		/// </summary>
		internal override FtpReply Send()
		{
			FtpReply reply;
			string user;
			string password;

				// Obtiene las credenciales del usuario o los datos de usuario anónimo
				if (Connection.Client.Credential != null && !string.IsNullOrEmpty(Connection.Client.Credential.UserName))
				{
					user = Connection.Client.Credential.UserName;
					password = Connection.Client.Credential.Password;
				}
				else
				{
					user = "anonymous";
					password = Connection.Client.ClientParameters.AnonymousPassword;
				}
				// Envía el usuario y lanza una excepción si hay un error
				reply = Expect(SendCommand("USER", user), 331, 530);
				if (reply.Code == 530)
					throw new Exceptions.FtpAuthenticationException("No se permiten usuarios anónimos", reply);
				// Envía el usuario y lanza una excepción si hay un error
				reply = SendCommand("PASS", password);
				if (reply.Code != 230)
					throw new Exceptions.FtpAuthenticationException("Fallo en la autentificación del usuario " + user, reply);
				// Devuelve la respuesta
				return reply;
		}
	}
}
