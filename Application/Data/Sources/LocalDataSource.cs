using System.IO;

namespace TheBettingMachine.App.Data.Sources
{
	internal class LocalDataSource : IStringDataSource
	{
		private readonly string _path;

		public LocalDataSource(string path)
		{
			_path = path;
		}

		public string Read()
		{
			return File.ReadAllText(_path);
		}
	}
}