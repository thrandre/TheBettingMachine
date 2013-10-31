using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheBettingMachine.App.Data;
using TheBettingMachine.App.Data.Csv;
using TheBettingMachine.App.Data.Entities;
using TheBettingMachine.App.Data.Readers;
using TheBettingMachine.App.Data.Sources;

namespace TheBettingMachine.App.Dataproviders
{
	internal class FixtureProvider : IFixtureProvider
	{
		private readonly IDataParser _dataParser;
		private readonly IStringDataReader _dataReader;
		private readonly IEnumerable<IStringDataSource> _dataSources;
		
		private List<Fixture> _fixtures;

		public FixtureProvider(IDataParser dataParser, IStringDataReader dataReader, IEnumerable<IStringDataSource> dataSources)
		{
			_dataParser = dataParser;
			_dataReader = dataReader;
			_dataSources = dataSources;
		}

		private void FetchFixtures(Func<Fixture, bool> selector)
		{
			_fixtures = _dataSources
				.SelectMany(s => _dataParser.GetRecords<Fixture>(s, _dataReader))
				.Where(selector).ToList();

			CreateAndAddDummyFixtures(_fixtures);
		}

		private void CreateAndAddDummyFixtures(IList<Fixture> fixtures)
		{
			var teams = fixtures.Select(f => f.HomeTeam)
				.Union(fixtures.Select(f => f.AwayTeam)).ToList();

			var contestId = fixtures.First().ContestId;

			foreach (var t in teams)
			{
				var team = t;

				var teamsExcl = teams.Except(new[] { team }).ToList();

				var notPlayedAtHome = teamsExcl.Except(
					fixtures.Where(f => f.HomeTeam == team).Select(f => f.AwayTeam));

				var notPlayedAway = teamsExcl.Except(
					fixtures.Where(f => f.AwayTeam == team).Select(f => f.HomeTeam));

				foreach (var awayTeam in notPlayedAtHome)
				{
					fixtures.Add(
						new Fixture
						{
							ContestId = contestId,
							HomeTeam = team,
							AwayTeam = awayTeam,
							Date = DateTime.Now,
							Dummy = true
						});
				}

				foreach (var homeTeam in notPlayedAway)
				{
					fixtures.Add(
						new Fixture
						{
							ContestId = contestId,
							HomeTeam = homeTeam,
							AwayTeam = team,
							Date = DateTime.Now,
							Dummy = true
						});
				}
			}
		}

		public IEnumerable<Fixture> GetFixtures(Func<Fixture, bool> selector)
		{
			if (_fixtures == null)
			{
				FetchFixtures(selector);
			}

			return _fixtures;
		}
	}
}
