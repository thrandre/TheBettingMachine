using System;
using System.Net;

namespace TheBettingMachine.App.Data.Sources
{
	internal class WebDataSource : IStringDataSource
	{
		private readonly Uri _url;

		public WebDataSource(Uri url)
		{
			_url = url;
		}

		public string Read()
		{
			return new WebClient().DownloadString(_url);
		}
	}
}