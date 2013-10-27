using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheBettingMachine.App.Dataproviders;
using TheBettingMachine.App.Entities;

namespace TheBettingMachine.App.Repository
{
	internal class FixtureRepository
	{
		private readonly IFixtureProvider _fixtureProvider;
		private IList<Fixture> _fixtures;

		public FixtureRepository(IFixtureProvider fixtureProvider)
		{
			_fixtureProvider = fixtureProvider;
			_fixtures = new List<Fixture>();
		}


		public void Update()
		{
			var updatedFixtures = _fixtureProvider.GetFixtures();
			_fixtures = _fixtures.Union(updatedFixtures).ToList();
		}
	}
}