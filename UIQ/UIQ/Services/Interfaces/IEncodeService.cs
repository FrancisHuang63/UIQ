namespace UIQ.Services.Interfaces
{
	public interface IEncodeService
	{
		public string UrlEncode(string input);

		public string HtmlEncode(string input);
	}
}