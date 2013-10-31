using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CsvHelper;
using CsvHelper.Configuration;
using MoreLinq;
using SharpJags.Jags;
using SharpJags.Parsing.Coda;
using SharpJags.Sanitazion;
using TheBettingMachine.App.Data.Csv;
using TheBettingMachine.App.Data.Entities;
using TheBettingMachine.App.Data.Readers;
using TheBettingMachine.App.Data.Sources;
using TheBettingMachine.App.Dataproviders;
using TheBettingMachine.App.Models;

namespace TheBettingMachine.App
{
	class Program
	{
		static void Main()
		{
			JagsConfig.BinPath = @"C:\Program Files\JAGS\JAGS-3.4.0\x64\bin";

			var csvConfiguration = new CsvConfiguration();
				csvConfiguration.RegisterClassMap<FootballDataClassMap>();

			var csvParser = new CsvHelperAdapter(
				new CsvFactory(), csvConfiguration);

			var fixtureProvider = new FixtureProvider(
				csvParser,
				new StringDataReader(), 
				new[]
				{
					new WebDataSource("http://www.football-data.co.uk/fixtures.csv"),
					new WebDataSource("http://www.football-data.co.uk/mmz4281/1314/E0.csv")
				});

			var fixtures = fixtureProvider.GetFixtures(f => f.ContestId == "E0").ToList();

			var model = new TheBettingMachinePredictiveModel();

			var lastTen = fixtures.Where(f => f.Played)
				.OrderByDescending(f => f.Date).Take(10).ToList();

			foreach (var fixture in lastTen)
			{
				fixture.Predict = true;
			}

			var run = new JagsRun
			{
				ModelData = model.GetData(fixtures),
				ModelDefinition = model.Definition,
				Monitors = model.Monitors,
				OutputSanitizer = new OutputSanitizer(),
				Parameters = new MCMCParameters
				{
					BurnIn = 2000,
					SampleCount = 2000,
					Chains = 5
				},
				WorkingDirectory = Directory.GetCurrentDirectory()
			};

			var samples = JagsWrapper.Run(run, new CodaParser(new CodaDataReader()));
			var predictions = model.GetPredictions(samples).ToList();

			Debug.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", 
				"HomeTeam", "AwayTeam", "P(H)", "P(D)", "P(A)", "O(H)", "O(D)", "O(A)", "B_Max_Outcome", "B_Max", "ExpectedReturn", "Variance", "Stake", "R", "Profit");

			foreach (var fixture in lastTen)
			{
				var prediction = predictions.First(p => p.Fixture.Equals(fixture));
				
				var betas = new[]
				{
					CalculateBeta(prediction.HomeProbability, fixture.HomeOdds),
					CalculateBeta(prediction.DrawProbability, fixture.DrawOdds),
					CalculateBeta(prediction.AwayProbability, fixture.AwayOdds)
				};

				var betaMax = betas.Index().MaxBy(b => b.Value);

				string outcome;
				double expReturn;
				double variance;
				double odds;

				if (betaMax.Key == 0)
				{
					outcome = "H";
					odds = fixture.HomeOdds;
					expReturn = CalculateExpectedReturn(prediction.HomeProbability, fixture.HomeOdds);
					variance = CalculateVariance(prediction.HomeProbability, fixture.HomeOdds);
				}
				else if (betaMax.Key == 1)
				{
					outcome = "D";
					odds = fixture.DrawOdds;
					expReturn = CalculateExpectedReturn(prediction.DrawProbability, fixture.DrawOdds);
					variance = CalculateVariance(prediction.DrawProbability, fixture.DrawOdds);
				}
				else
				{
					outcome = "A";
					odds = fixture.AwayOdds;
					expReturn = CalculateExpectedReturn(prediction.AwayProbability, fixture.AwayOdds);
					variance = CalculateVariance(prediction.AwayProbability, fixture.AwayOdds);
				}

				string realOutcome = "";

				switch (fixture.Result)
				{
					case FixtureResult.HomeWin:
						realOutcome = "H";
						break;
					case FixtureResult.Draw:
						realOutcome = "D";
						break;
					case FixtureResult.AwayWin:
						realOutcome = "A";
						break;
				}

				var stake = 1 / variance;
				var profit = stake * ((outcome == realOutcome) ? odds : -1);

				Debug.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
					fixture.HomeTeam, fixture.AwayTeam, prediction.HomeProbability, prediction.DrawProbability, prediction.AwayProbability, 
					fixture.HomeOdds, fixture.DrawOdds, fixture.AwayOdds, outcome, betaMax.Value, expReturn, variance, stake, realOutcome, profit);
			}

			GetStandings(fixtures, predictions);
		
		}

		private static double CalculateExpectedReturn(double prob, double odds)
		{
			return (prob*odds) - 1;
		}

		private static double CalculateVariance(double prob, double odds)
		{
			return prob * Math.Pow(
				((odds - 1) - CalculateExpectedReturn(prob, odds)), 2) +
				(1 - prob) * Math.Pow(((-1) - CalculateExpectedReturn(prob, odds)), 2);
		}

		private static double CalculateBeta(double prob, double odds)
		{
			return Math.Max(0, CalculateExpectedReturn(prob, odds) / (Math.Pow(CalculateVariance(prob, odds), 2)));
		}

		private static void GetStandings(List<Fixture> fixtures, List<Prediction> predictions)
		{
			var teams = fixtures.Select(f => f.HomeTeam)
				.Union(fixtures.Select(f => f.AwayTeam)).ToList();

			var points = new List<int>(new int[teams.Count]);

			foreach (var fixture in fixtures)
			{
				var homeTeamIdx = teams.IndexOf(fixture.HomeTeam);
				var awayTeamIdx = teams.IndexOf(fixture.AwayTeam);

				if (fixture.Played)
				{
					if (fixture.Result == FixtureResult.HomeWin)
					{
						points[homeTeamIdx] += 3;
					}

					if (fixture.Result == FixtureResult.Draw)
					{
						points[homeTeamIdx] += 1;
						points[awayTeamIdx] += 1;
					}

					if (fixture.Result == FixtureResult.AwayWin)
					{
						points[awayTeamIdx] += 3;
					}
				}
				else
				{
					var probs = predictions.Where(
						p => p.Fixture.Equals(fixture)).Select(
							p => new[]
							{
								p.HomeProbability,
								p.DrawProbability,
								p.AwayProbability
							}).First();

					var maxProb = probs.Index().MaxBy(p => p.Value);

					if (maxProb.Key == 0)
					{
						points[homeTeamIdx] += 3;
						continue;
					}
					
					if (maxProb.Key == 1)
					{
						points[homeTeamIdx] += 1;
						points[awayTeamIdx] += 1;
						continue;
					}

					if (maxProb.Key == 2)
					{
						points[awayTeamIdx] += 3;
					}
				}
			}

			var standings = new List<TeamStanding>();

			foreach (var team in teams)
			{
				var teamIdx = teams.IndexOf(team);

				standings.Add(
					new TeamStanding
					{
						Team = team,
						Points = points[teamIdx]
					});
			}

			var orderedStandings = standings.OrderByDescending(s => s.Points);

			Debug.WriteLine("{0},{1}", "Team", "Points");

			foreach (var orderedStanding in orderedStandings)
			{
				Debug.WriteLine("{0},{1}", orderedStanding.Team, orderedStanding.Points);
			}
		}
	}

	internal class TeamStanding
	{
		public string Team { get; set; }
		public int Points { get; set; }}
}
