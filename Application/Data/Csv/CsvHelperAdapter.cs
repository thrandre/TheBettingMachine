using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using TheBettingMachine.App.Data.Readers;
using TheBettingMachine.App.Data.Sources;

namespace TheBettingMachine.App.Data.Csv
{
	internal class CsvHelperAdapter : IDataParser
	{
		private readonly CsvFactory _factory;
		private readonly CsvConfiguration _configuration;

		public CsvHelperAdapter(CsvFactory factory, CsvConfiguration configuration)
		{
			_factory = factory;
			_configuration = configuration;
		}

		public IEnumerable<T> GetRecords<T>(IStringDataSource dataSource, IStringDataReader dataReader)
		{
			return _factory.CreateReader(GetData(dataSource, dataReader), _configuration).GetRecords<T>();
		}

		private StringReader GetData(IStringDataSource dataSource, IStringDataReader dataReader)
		{
			return new StringReader(dataReader.Read(dataSource));
		}
	}
}