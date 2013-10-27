using TheBettingMachine.App.Data.Sources;

namespace TheBettingMachine.App.Data.Readers
{
	internal class StringDataReader : IStringDataReader
	{
		public string Read(IStringDataSource dataSource)
		{
			return dataSource.Read();
		}
	}
}