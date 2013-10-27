using System;

namespace TheBettingMachine.App.Entities
{
	internal class Fixture : IEquatable<Fixture>
	{
		public string ContestId { get; set; }
		public DateTime Date { get; set; }
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		
		public bool Played { get; set; }
		public bool Predictable { get; set; }

		public override bool Equals(object obj)
		{
			var other = (obj as Fixture);
			return other != null && Equals(other);
		}

		public bool Equals(Fixture other)
		{
			return ContestId == other.ContestId
				   && HomeTeam == other.HomeTeam
			       && AwayTeam == other.AwayTeam
			       && Date.Equals(other.Date);
		}
	}
}