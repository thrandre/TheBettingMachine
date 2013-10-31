using System;
using CsvHelper.TypeConversion;
using TheBettingMachine.App.Data.Entities;

namespace TheBettingMachine.App
{
	internal class ResultTypeConverter : ITypeConverter
	{
		public string ConvertToString(TypeConverterOptions options, object value)
		{
			return String.Empty;
		}

		public object ConvertFromString(TypeConverterOptions options, string text)
		{
			if (text == "H")
			{
				return FixtureResult.HomeWin;
			}

			if (text == "D")
			{
				return FixtureResult.Draw;
			}

			if (text == "A")
			{
				return FixtureResult.AwayWin;
			}

			return FixtureResult.Undetermined;
		}

		public bool CanConvertFrom(Type type)
		{
			return typeof(string).IsAssignableFrom(type);
		}

		public bool CanConvertTo(Type type)
		{
			return typeof(FixtureResult).IsAssignableFrom(type);
		}
	}
}