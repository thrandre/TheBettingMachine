using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using CsvHelper;
using CsvHelper.Configuration;
using TheBettingMachine.App.Data.Readers;
using TheBettingMachine.App.Data.Sources;

namespace TheBettingMachine.App.Data.Csv
{
	internal class CsvHelperAdapter : ICsvReader
	{
		private readonly CsvFactory _factory;
		private readonly IStringDataReader _dataReader;

		public CsvHelperAdapter(CsvFactory factory, IStringDataReader dataReader)
		{
			_factory = factory;
			_dataReader = dataReader;
		}

		public IEnumerable<T> GetRecords<T>(IStringDataSource dataSource, IDictionary<int, Expression<Func<T, object>>> mappings)
		{
			var data = GetData(dataSource);
			var config = BuildConfiguration(mappings);

			return _factory.CreateReader(data, config).GetRecords<T>();
		}

		private CsvConfiguration BuildConfiguration<T>(IDictionary<int, Expression<Func<T, object>>> mappings)
		{
			var configuration = new CsvConfiguration();
			configuration.RegisterClassMap(
				new GenericClassMap<T>(mappings));

			return configuration;
		}

		private StringReader GetData(IStringDataSource dataSource)
		{
			return new StringReader(_dataReader.Read(dataSource));
		}

		internal class GenericClassMap<T> : CsvClassMap<T>
		{
			private readonly IDictionary<int, Expression<Func<T, object>>> _mappings;

			public GenericClassMap(IDictionary<int, Expression<Func<T, object>>> mappings)
			{
				_mappings = mappings;
			}

			public override void CreateMap()
			{
				foreach (var mapping in _mappings)
				{
					Map(mapping.Value).Index(mapping.Key);
				}
			}
		}
	}
}