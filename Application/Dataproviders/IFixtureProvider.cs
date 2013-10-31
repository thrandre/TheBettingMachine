using System;
using System.Collections.Generic;
using TheBettingMachine.App.Data.Entities;

namespace TheBettingMachine.App.Dataproviders
{
	internal interface IFixtureProvider
	{
		IEnumerable<Fixture> GetFixtures(Func<Fixture, bool> selector);
	}
}