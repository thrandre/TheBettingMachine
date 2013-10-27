using TheBettingMachine.App.Data.Sources;

namespace TheBettingMachine.App.Data.Readers
{
	internal interface IStringDataReader
	{
		string Read(IStringDataSource dataSource);
	}
}