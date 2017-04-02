﻿using System;

namespace RefactoringEssentials.Demonstration
{
	class Program
	{
		static void Main(string[] args)
		{
			// CompareFloats() shows some detected comparison issues with floats
			CompareFloats();

			// Compare exactly the same constant value -> suggestion to convert to 'true'
			// Also other Refactorings: Convert to Equals() call, invert and negate condition etc.
			bool fiveIsFive = (5 == 5);

            // Refactoring: Conversion of operator assignment ("|=") to assignment with or operator and vice versa
            bool hasArgs = false;
            hasArgs |= (args.Length > 0);

			// Refactoring: Use explicit type instead of 'var'
			var commandLineSwitchParser = new CommandLineSwitchParser();
			commandLineSwitchParser.Parameters = args;
			commandLineSwitchParser.Parse();

			Console.WriteLine("Selected switches: {0}", commandLineSwitchParser.Switches);

            // Warning about unassigned created object
            new object();

            // 1. Analyzer + CodeFix to replace Nullable<T> by T?
            // 2. Refactoring: Replace type by 'var'
            Nullable<float> nullableFloat = 5;
		}

		static void CompareFloats()
		{
			// Doing some float comparison will be marked as potential issue
			float float1 = 1.0f;
			float float2 = 1.0f;

			if (float1 == float2)
			{
				// Default equality test might fail on floats
			}

			if (float1 != 0)
			{
				// Same for zero comparison
			}

			if (float1 == float.NaN)
			{
				// Not a good way to test for NaN
			}

			if (float1 != float.PositiveInfinity)
			{
				// Not a good way to test for infinity
			}
		}
	}
}
