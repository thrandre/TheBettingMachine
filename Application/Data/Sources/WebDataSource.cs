using System;
using System.Net;

namespace TheBettingMachine.App.Data.Sources
{
	internal class WebDataSource : IStringDataSource
	{
		private readonly string _url;

		public WebDataSource(string url)
		{
			_url = url;
		}

		public string Read()
		{
			return new WebClient().DownloadString(_url);
		}
	}
}