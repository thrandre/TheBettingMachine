using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TheBettingMachine.App.Data.Sources;

namespace TheBettingMachine.App.Data.Csv
{
	internal interface ICsvReader
	{
		IEnumerable<T> GetRecords<T>(IStringDataSource dataSource, IDictionary<int, Expression<Func<T, object>>> mappings);
	}
}