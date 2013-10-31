using System.Collections.Generic;
using TheBettingMachine.App.Data.Readers;
using TheBettingMachine.App.Data.Sources;

namespace TheBettingMachine.App.Data
{
	internal interface IDataParser
	{
		IEnumerable<T> GetRecords<T>(IStringDataSource dataSource, IStringDataReader dataReader);
	}
}