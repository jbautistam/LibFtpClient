using System;
using System.Collections.Generic;

namespace Bau.Libraries.LibFtpClient.Sessions
{
	/// <summary>
	///		Representa las características del servidor FTP
	/// </summary>
	internal class FtpServerFeatures
	{ 
		// Variables privadas
		private readonly IDictionary<string, IList<string>> _features;

		internal FtpServerFeatures(IEnumerable<string> features)
		{   
			// Crea el diccionario
			_features = new Dictionary<string, IList<string>>(StringComparer.InvariantCultureIgnoreCase);
			// Asigna la lista de características
			foreach (string item in features)
			{
				int index = item.IndexOf(' ');

					if (index < 0)
						RegisterFeature(item);
					else
						RegisterFeature(item.Substring(0, index), item.Substring(index + 1));
			}
		}

		/// <summary>
		///		Registra una característica
		/// </summary>
		private void RegisterFeature(string feature, string parameter = null)
		{   
			// Si no existe la clave, crea una
			if (!_features.ContainsKey(feature))
				_features[feature] = new List<string>();
			// Añade el parámetro
			if (parameter != null)
				_features[feature].Add(parameter);
		}

		/// <summary>
		///		Comprueba si existe alguna característica
		/// </summary>
		internal bool HasFeature(string feature)
		{
			return _features.ContainsKey(feature);
		}

		/// <summary>
		///		Obtiene los parámetros de una característica
		/// </summary>
		internal IList<string> GetFeatureParameters(string feature)
		{
			// Obtiene el valor
			_features.TryGetValue(feature, out IList<string> parameters);
			// Devuelve el valor localizado
			return parameters;
		}

		///// <summary>
		/////		Obtiene las claves de las características
		///// </summary>
		//internal IEnumerable<string> Features 
		//{ get { return dctFeatures.Keys; } 
		//}
	}
}
