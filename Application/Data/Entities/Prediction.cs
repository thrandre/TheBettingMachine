namespace TheBettingMachine.App.Data.Entities
{
	internal class Prediction
	{
		public Fixture Fixture { get; set; }
		
		public double HomeProbability { get; set; }
		public double DrawProbability { get; set; }
		public double AwayProbability { get; set; }
	}
}