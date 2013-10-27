using CsvHelper;
using SharpJags.Jags;
using TheBettingMachine.App.Data.Csv;
using TheBettingMachine.App.Data.Readers;
using TheBettingMachine.App.Dataproviders;
using TheBettingMachine.App.Repository;

namespace TheBettingMachine.App
{
	class Program
	{
		static void Main()
		{
			JagsConfig.BinPath = @"C:\Program Files\JAGS\JAGS-3.4.0\x64\bin";
			
			var csvReader = new CsvHelperAdapter(
				new CsvFactory(), new StringDataReader());

			var provider = new FootballDataFixtureProvider(
				new FootballDataFixtureProviderSettings
				{
					ContestId = "E0",
					SeasonId = "1314"
				}, 
				csvReader);

			var repository = new FixtureRepository(provider);
			repository.Update();


		}
	}
}
