using System;

namespace TheBettingMachine.App.Data.Entities
{
	internal class Fixture : IEquatable<Fixture>
	{
		public string ContestId { get; set; }
		
		public DateTime Date { get; set; }
		
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }

		public int HomeGoals { get; set; }
		public int AwayGoals { get; set; }
		
		public double HomeOdds { get; set; }
		public double DrawOdds { get; set; }
		public double AwayOdds { get; set; }
		
		public FixtureResult Result { get; set; }

		public bool Played
		{
			get
			{
				return Result != FixtureResult.Undetermined;
			}
		}

		public bool Predict { get; set; }
		public bool Dummy { get; set; }
		
		public bool Equals(Fixture other)
		{
			return ContestId == other.ContestId
			       && HomeTeam == other.HomeTeam
			       && AwayTeam == other.AwayTeam
			       && Date == other.Date;
		}
	}
}