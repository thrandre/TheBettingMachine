using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using TheBettingMachine.App.Data;
using TheBettingMachine.App.Data.Sources;
using TheBettingMachine.App.Entities;
using ICsvReader = TheBettingMachine.App.Data.Csv.ICsvReader;

namespace TheBettingMachine.App.Dataproviders
{
	internal class FootballDataFixtureProvider : IFixtureProvider
	{
		private readonly FootballDataFixtureProviderSettings _providerSettings;
		private readonly ICsvReader _csvReader;
		
		private readonly IDictionary<int, Expression<Func<Fixture, object>>> _mappings;

		public FootballDataFixtureProvider(FootballDataFixtureProviderSettings providerSettings, ICsvReader csvReader)
		{
			_providerSettings = providerSettings;
			_csvReader = csvReader;

			_mappings = new Dictionary<int, Expression<Func<Fixture, object>>>
			{
				{0, m => m.ContestId},
				{1, m => m.Date},
				{2, m => m.HomeTeam},
				{3, m => m.AwayTeam}
			};
		}

		private IEnumerable<Fixture> FetchFixtures(string url)
		{
			return _csvReader.GetRecords(
				new WebDataSource(new Uri(url)), _mappings);
		}

		public IEnumerable<Fixture> GetFixtures()
		{
			var results = FetchFixtures(GetResultsUrl()).Select(
				f =>
				{
					f.Played = true;
					return f;
				}).ToList();

			var fixtures = FetchFixtures(GetFixturesUrl())
				.Where(f => f.ContestId == _providerSettings.ContestId).ToList();

			return results.Union(fixtures).OrderBy(f => f.Date);
		}

		private string GetFixturesUrl()
		{
			return "http://www.football-data.co.uk/fixtures.csv";
		}

		private string GetResultsUrl()
		{
			return String.Format("http://www.football-data.co.uk/mmz4281/{0}/{1}.csv",
				_providerSettings.SeasonId, _providerSettings.ContestId);
		}
	}
}