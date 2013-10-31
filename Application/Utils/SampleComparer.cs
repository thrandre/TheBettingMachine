using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheBettingMachine.App.Data.Entities;

namespace TheBettingMachine.App.Utils
{
	internal class SampleComparer
	{
		public static Prediction GetPrediction(IList<Double> homeGoalSamples, IList<Double> awayGoalSamples)
		{
			var noSamples = (double) homeGoalSamples.Count() * awayGoalSamples.Count();

			var gt = 0;
			var eq = 0;
			var lt = 0;
			
			foreach (var hSample in homeGoalSamples)
			{
				foreach (var aSample in awayGoalSamples)
				{
					if (hSample > aSample) gt++;
					if (hSample == aSample) eq++;
					if (hSample < aSample) lt++;
				}
			}

			var homeProb = gt/noSamples;
			var drawProb = eq/noSamples;
			var awayProb = lt/noSamples;

			return new Prediction
			{
				HomeProbability = homeProb,
				DrawProbability = drawProb,
				AwayProbability = awayProb
			};
		}
	}
}
