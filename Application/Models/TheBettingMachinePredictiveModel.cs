using System;
using System.Collections.Generic;
using System.Linq;
using SharpJags.Collections;
using SharpJags.Jags;
using SharpJags.Math;
using SharpJags.Models;
using SharpJags.Parsing;
using TheBettingMachine.App.Data.Entities;
using TheBettingMachine.App.Utils;

namespace TheBettingMachine.App.Models
{
	internal class TheBettingMachinePredictiveModel
	{
		private IList<Fixture> _fixtures;
		private IList<string> _teams;
		
		private Matrix<double> _teamMx;
		private Matrix<double> _timesliceMx;

		public ModelDefinition Definition
		{
			get
			{
				return new ModelDefinition
				{
					Name = "TheBettingMachine",
					Definition =
						@"
							data {
								homeGoalAvg <- 0.395
								awayGoalAvg <- 0.098
							}

							model {
								precision ~ dgamma(10, 0.2)
	
								for(t in 1:noTeams) {
									attack[t, 1] 	~ dnorm(0, precision)
									defense[t, 1] 	~ dnorm(0, precision)

									for(s in 2:noTimeslices) {
			
										attack[t, s] 	~ dnorm(attack[t, (s-1)], precision)
			
										defense[t, s] 	~ dnorm(defense[t, (s-1)], precision)
									}
								}
								
								gamma ~ dunif(0, 0.1)

								for(i in 1:noGames) {					
									
									delta[i]			<- (attack[team[i, 1], timeslice[i, 1]] + defense[team[i, 1], timeslice[i, 1]] -
															attack[team[i, 2], timeslice[i, 2]] - defense[team[i, 2], timeslice[i, 2]]) / 2

									log(homeLambda[i]) 	<- (homeGoalAvg + (
																attack[team[i, 1], timeslice[i, 1]] - 
																defense[team[i, 2], timeslice[i, 2]] -
																gamma * delta[i]))
		
									log(awayLambda[i]) 	<- (awayGoalAvg + (
																attack[team[i, 2], timeslice[i, 2]] - 
																defense[team[i, 1], timeslice[i, 1]] +
																gamma * delta[i]))
		
									goalsScored[i, 1] ~ dpois(homeLambda[i])
									goalsScored[i, 2] ~ dpois(awayLambda[i])
		
								}
	
							}
						"
				};
			}
		}

		public IEnumerable<JagsMonitor> Monitors
		{
			get
			{
				return new[]
				{
					new JagsMonitor { ParameterName = "goalsScored" },
					new JagsMonitor { ParameterName = "attack" },
					new JagsMonitor { ParameterName = "defense" }
				};
			}
		}

		public JagsData GetData(IList<Fixture> fixtures)
		{
			var teams = fixtures.Select(f => f.HomeTeam)
				.Union(fixtures.Select(f => f.AwayTeam)).ToList();
			
			var totalGames = (teams.Count - 1) * teams.Count;
			var noTimeslices = (teams.Count - 1) * 2;

			var teamMx = new Matrix<double>(2, totalGames);
			var timesliceMx = new Matrix<double>(2, totalGames);
			var goalsScoredMx = new Matrix<double>(2, totalGames);

			var teamGamesPlayed = new Vector<int>(teams.Count);

			for (var i = 0; i < fixtures.Count; i++)
			{
				var fixture = fixtures[i];
				
				var homeTeamIdx = teams.IndexOf(fixture.HomeTeam);
				var awayTeamIdx = teams.IndexOf(fixture.AwayTeam);

				teamGamesPlayed[homeTeamIdx]++;
				teamGamesPlayed[awayTeamIdx]++;

				teamMx[0, i] = homeTeamIdx + 1;
				teamMx[1, i] = awayTeamIdx + 1;
				
				timesliceMx[0, i] = teamGamesPlayed[homeTeamIdx];
				timesliceMx[1, i] = teamGamesPlayed[awayTeamIdx];

				if (fixture.Result == FixtureResult.Undetermined || fixture.Dummy || fixture.Predict)
				{
					goalsScoredMx[0, i] = Double.NaN;
					goalsScoredMx[1, i] = Double.NaN;
				}
				else
				{
					goalsScoredMx[0, i] = fixture.HomeGoals;
					goalsScoredMx[1, i] = fixture.AwayGoals;
				}

				if (fixture.Result == FixtureResult.Undetermined)
				{
					fixture.Predict = true;
				}
			}

			_fixtures = fixtures;
			_teams = teams;

			_teamMx = teamMx;
			_timesliceMx = timesliceMx;

			return new JagsData
			{
				{ "team", teamMx },
				{ "timeslice", timesliceMx },
				{ "goalsScored", goalsScoredMx },
				{ "noGames", totalGames },
				{ "noTeams", teams.Count },
				{ "noTimeslices", noTimeslices }
			};
		}

		public IEnumerable<Prediction> GetPredictions(SampleCollection samples)
		{
			var fixturesPredicted = _fixtures.Where(f => f.Predict);

			var predictions = new List<Prediction>();

			var goalsScoredMx = samples.Get<ModelParameterMatrix>("goalsScored");
			var attackMx = samples.Get<ModelParameterMatrix>("attack");
			var defenseMx = samples.Get<ModelParameterMatrix>("defense");

			foreach (var fixture in fixturesPredicted)
			{
				var fixtureIdx = _fixtures.IndexOf(fixture);

				var homeGoalSamples = goalsScoredMx[fixtureIdx][0].Samples;
				var awayGoalSamples = goalsScoredMx[fixtureIdx][1].Samples;

				var prediction = SampleComparer.GetPrediction(homeGoalSamples, awayGoalSamples);
				prediction.Fixture = fixture;

				predictions.Add(prediction);
			}

			return predictions;
		}
	}
}