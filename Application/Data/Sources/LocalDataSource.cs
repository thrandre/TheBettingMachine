using System.IO;

namespace TheBettingMachine.App.Data.Sources
{
	internal class LocalDataSource : IStringDataSource
	{
		private readonly FileInfo _file;

		public LocalDataSource(FileInfo file)
		{
			_file = file;
		}

		public string Read()
		{
			return File.ReadAllText(_file.FullName);
		}
	}
}