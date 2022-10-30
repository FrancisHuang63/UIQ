using System.Text.Encodings.Web;
using UIQ.Services.Interfaces;

namespace UIQ.Services
{
	public class EncodeService : IEncodeService
	{
		private readonly UrlEncoder _urlEncoder;
		private readonly HtmlEncoder _htmlEncoder;

		public EncodeService(UrlEncoder urlEncoder, HtmlEncoder htmlEncoder)
		{
			_urlEncoder = urlEncoder;
			_htmlEncoder = htmlEncoder;
		}

		public string UrlEncode(string input)
		{
			if (input == null) return null;

			return _urlEncoder.Encode(input);
		}

		public string HtmlEncode(string input)
		{
			if (input == null) return null;

			return _htmlEncoder.Encode(input);
		}
	}
}