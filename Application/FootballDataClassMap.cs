using CsvHelper.Configuration;
using TheBettingMachine.App.Data.Entities;

namespace TheBettingMachine.App
{
	internal class FootballDataClassMap : CsvClassMap<Fixture>
	{
		public override void CreateMap()
		{
			Map(m => m.ContestId).Name("Div");
			Map(m => m.Date).Name("Date");
			Map(m => m.HomeTeam).Name("HomeTeam");
			Map(m => m.AwayTeam).Name("AwayTeam");
			Map(m => m.HomeGoals).Name("FTHG").Default(0);
			Map(m => m.AwayGoals).Name("FTAG").Default(0);
			Map(m => m.Result).Name("FTR").TypeConverter<ResultTypeConverter>();
			Map(m => m.HomeOdds).Name("WHH").Default(0);
			Map(m => m.DrawOdds).Name("WHD").Default(0);
			Map(m => m.AwayOdds).Name("WHA").Default(0);
		}
	}
}