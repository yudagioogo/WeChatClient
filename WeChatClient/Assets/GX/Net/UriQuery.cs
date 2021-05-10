#if UNITY
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System;

namespace GX.Net
{
	public class UriQuery : IEnumerable<KeyValuePair<string, string>>
	{
		private readonly List<KeyValuePair<string, string>> data;

		public string this[string key]
		{
			get
			{
				return (from i in data where i.Key == key select i.Value).FirstOrDefault();
			}
			set
			{
				var index = data.FindIndex(i => i.Key == key);
				if (value == null)
				{
					if (index >= 0)
						data.RemoveAt(index);
					return;
				}

				if (index != -1)
					data[index] = new KeyValuePair<string, string>(key, value);
				else
					data.Add(new KeyValuePair<string, string>(key, value));
			}
		}

		public UriQuery(string query = null)
		{
			data = new List<KeyValuePair<string, string>>(ParseQueryString(query));
		}

		public override string ToString()
		{
			return string.Join("&", data.Select(i => WWW.EscapeURL(i.Key) + "=" + WWW.EscapeURL(i.Value)).ToArray());
		}

		#region IEnumerable<KeyValuePair<string,string>> 成员

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return data.GetEnumerator();
		}

		#endregion

		#region IEnumerable 成员

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string query)
		{
			if (string.IsNullOrEmpty(query) == false)
			{
				foreach (var seg in query.Substring(query.IndexOf('?') + 1).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
				{
					var pair = seg.Split(new char[] { '=' }, 2);
					switch (pair.Length)
					{
						case 0:
							continue;
						case 1:
							yield return new KeyValuePair<string, string>(WWW.UnEscapeURL(pair[0]), string.Empty);
							break;
						case 2:
							yield return new KeyValuePair<string, string>(WWW.UnEscapeURL(pair[0]), WWW.UnEscapeURL(pair[1]));
							break;
						default:
							throw new NotImplementedException();
					}
				}
			}
		}
	}
}
#endif
