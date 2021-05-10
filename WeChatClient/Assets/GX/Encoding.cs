namespace GX
{
	public class Encoding
	{
		private static readonly System.Text.Encoding encoding = System.Text.Encoding.UTF8;
		public static byte[] GetBytes(string s)
		{
			return encoding.GetBytes(s);
		}
		public static string GetString(byte[] bytes)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			return encoding.GetString(bytes, 0, bytes.Length);
#else
			return encoding.GetString(bytes);	
#endif
		}
	}
}