using System.Collections.Generic;
using TheBettingMachine.App.Data;
using TheBettingMachine.App.Entities;

namespace TheBettingMachine.App.Dataproviders
{
	internal interface IFixtureProvider
	{
		IEnumerable<Fixture> GetFixtures();
	}
}