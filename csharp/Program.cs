using Algebra;
using static Algebra.Utility;
using static Algebra.Functions;

var debug = args.Length > 0 && args[0] == "DEBUG";

void CheckFunctionOutput(string name, string assert, Function function)
{
	switch (CheckFunctionOutputInternal(name, assert, function, debug))
	{
		case TestResult.SUCCESS: break;
		case TestResult.ASSERT_NOT_MATCH:
			{
				Console.WriteLine($"Step Summary:");
				CheckFunctionOutputInternal(name, assert, function, true);
				break;
			}
		case TestResult.INFINITE_LOOP:
			{
				break;
			}
	}
}

static TestResult CheckFunctionOutputInternal(string name, string assert, Function function, bool debug = false)
{
	var currentFunction = function.Clone();
	if (debug)
	{
		Console.WriteLine("------------------------------");
		Console.WriteLine(PrintFunctions(currentFunction));
	}
	var resultsList = new List<string>() { };
	for (var i = 0; i < 1000; i++)
	{
		var resultString = PrintFunctions(currentFunction);
		var result = ExecuteFunction(currentFunction);
		if (debug)
		{
			Console.WriteLine(PrintFunctions(result.function));
		}
		currentFunction = result.function;
		if (!result.collapsed)
		{
			if (resultString != assert)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{name} - Failed");
				Console.ResetColor();
				Console.WriteLine($"Expected: \"{assert}\"");
				Console.WriteLine($"Output: \"{resultString}\"");
				return TestResult.ASSERT_NOT_MATCH;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"{name} - Passed");
				Console.ResetColor();
				return TestResult.SUCCESS;
			}
		}
		else if (result.collapsed && resultsList.Exists(r => r == resultString))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{name} - Passed with convergence");
			Console.ResetColor();
			return TestResult.SUCCESS;
		}
		resultsList.Add(resultString);
	}
	Console.WriteLine("Error, hit infinite loop. Last function:");
	Console.WriteLine(PrintFunctions(currentFunction));
	return TestResult.INFINITE_LOOP;
}

var twoPlusThree = FunctionArguments(1, FunctionType.ADD, FunctionPrimitive(2), FunctionPrimitive(3));
CheckFunctionOutput("Basic Add", "5", twoPlusThree);

var threeMinusFive = FunctionArguments(1, FunctionType.ADD, FunctionPrimitive(3), FunctionPrimitive(-5));
CheckFunctionOutput("Basic Subtract", "-2", threeMinusFive);

CheckFunctionOutput("Add Two Functions", "3", FunctionArguments(1, FunctionType.ADD, twoPlusThree, threeMinusFive));

CheckFunctionOutput("Add Pronumerals", "2X", FunctionArguments(1, FunctionType.ADD, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(1, Symbol.X)));

CheckFunctionOutput("Multiply Mul functions", "6(1X * 1Y * 1A * 1B)", FunctionArguments(1, FunctionType.MUL,
	FunctionArguments(1, FunctionType.MUL,
		FunctionPrimitive(1, Symbol.X),
		FunctionPrimitive(3, Symbol.Y)
	),
	FunctionArguments(1, FunctionType.MUL,
		FunctionPrimitive(1, Symbol.A),
		FunctionPrimitive(2, Symbol.B)
	)
));

CheckFunctionOutput("Distribute Single Add function", "1(6(1X * 1Y) + 4(1A * 1Y) + 2(1B * 1Y))", FunctionArguments(1, FunctionType.MUL,
	FunctionPrimitive(2, Symbol.Y),
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(3, Symbol.X),
		FunctionPrimitive(2, Symbol.A),
		FunctionPrimitive(1, Symbol.B)
	)
));

CheckFunctionOutput("Distribute Two Add functions", "1(8(1A * 1X) + 10(1B * 1X) + 12(1A * 1Y) + 15(1B * 1Y))", FunctionArguments(1, FunctionType.MUL,
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(2, Symbol.X),
		FunctionPrimitive(3, Symbol.Y)
	),
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(4, Symbol.A),
		FunctionPrimitive(5, Symbol.B)
	)
));

CheckFunctionOutput("Exponential", "2(1X ^ 2)", FunctionArguments(1, FunctionType.MUL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(2, Symbol.X)));

CheckFunctionOutput("Exponential Zero and One", "1(1 + 1X)", FunctionArguments(1, FunctionType.ADD,
	FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(0)),
	FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(1))
));

CheckFunctionOutput("Add Exponential", "1(3(1X ^ 2) + 1(2X ^ 2) + 2)", FunctionArguments(1, FunctionType.ADD,
	FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(2)),
	FunctionArguments(2, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(2)),
	FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(2, Symbol.X), FunctionPrimitive(2)),
	FunctionPrimitive(2)
));

CheckFunctionOutput("Multiply Exponential by Pronumeral", "2(1X ^ 3)", FunctionArguments(1, FunctionType.MUL, FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(2)), FunctionPrimitive(2, Symbol.X)));

CheckFunctionOutput("Simplify and Add Exponentials", "3(1X ^ 4)", FunctionArguments(1, FunctionType.ADD,
	FunctionArguments(1, FunctionType.MUL,
		FunctionPrimitive(1, Symbol.X),
		FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(3))
	),
	FunctionArguments(1, FunctionType.MUL,
		FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(2)),
		FunctionArguments(2, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(3)),
		FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(-1))
	)
));

CheckFunctionOutput("Exponential Primitive", "1(4 + 1(1X ^ 2))", FunctionArguments(1, FunctionType.ADD,
	FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(2), FunctionPrimitive(2)),
	FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(1, Symbol.X), FunctionPrimitive(2))
));

CheckFunctionOutput("Exponential Add", "1(1(1X ^ 3) + 3(1X ^ 2) + 3X + 1)", FunctionArguments(1, FunctionType.EXPONENTIAL,
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(1, Symbol.X),
		FunctionPrimitive(1)
	),
	FunctionPrimitive(3)
));

CheckFunctionOutput("Nested Exponential", "1(1X ^ 6(1Y * 1A))", FunctionArguments(1, FunctionType.EXPONENTIAL,
		FunctionPrimitive(1, Symbol.X),
		FunctionArguments(1, FunctionType.EXPONENTIAL, FunctionPrimitive(2, Symbol.Y), FunctionPrimitive(3, Symbol.A))
));

CheckFunctionOutput("Basic Divide", "3", FunctionArguments(1, FunctionType.DIV, FunctionPrimitive(6), FunctionPrimitive(2)));

CheckFunctionOutput("Stable Rational", "1(3 / 2)", FunctionArguments(1, FunctionType.DIV, FunctionPrimitive(3), FunctionPrimitive(2)));

CheckFunctionOutput("Divide Out Pronumerals", "1(3 / 2)", FunctionArguments(1, FunctionType.DIV, FunctionPrimitive(3, Symbol.X), FunctionPrimitive(2, Symbol.X)));

CheckFunctionOutput("Divide Out Multiple Terms", "2", FunctionArguments(1, FunctionType.DIV, FunctionPrimitive(4, Symbol.X), FunctionPrimitive(2, Symbol.X)));

CheckFunctionOutput("Divide Add Function", "5", FunctionArguments(1, FunctionType.DIV,
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(3, Symbol.X),
		FunctionPrimitive(2, Symbol.X)
	),
	FunctionPrimitive(1, Symbol.X)
));

CheckFunctionOutput("Divide Exponential", "1(1(1X ^ 6) / 4)", FunctionArguments(1, FunctionType.DIV,
	FunctionArguments(1, FunctionType.EXPONENTIAL,
		FunctionPrimitive(1, Symbol.X),
		FunctionPrimitive(7)
	),
	FunctionPrimitive(4, Symbol.X)
));

CheckFunctionOutput("Divide By Exponential", "1(2 / 1X)", FunctionArguments(1, FunctionType.DIV,
	FunctionPrimitive(2, Symbol.X),
	FunctionArguments(1, FunctionType.EXPONENTIAL,
		FunctionPrimitive(1, Symbol.X),
		FunctionPrimitive(2)
	)
));

CheckFunctionOutput("Divide By Exact Match", "1", FunctionArguments(1, FunctionType.DIV,
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(1, Symbol.X),
		FunctionPrimitive(2)
	),
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(1, Symbol.X),
		FunctionPrimitive(2)
	)
));

CheckFunctionOutput("Divide By GCD", "1(3X / 2Y)", FunctionArguments(1, FunctionType.DIV, FunctionPrimitive(6, Symbol.X), FunctionPrimitive(4, Symbol.Y)));

CheckFunctionOutput("Divide Add By GCD", "1(1(3X + 4) / 1(1X + 2))", FunctionArguments(1, FunctionType.DIV,
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(9, Symbol.X),
		FunctionPrimitive(12)
	),
	FunctionArguments(1, FunctionType.ADD,
		FunctionPrimitive(3, Symbol.X),
		FunctionPrimitive(6)
	)
));

CheckFunctionOutput("Divide Add By Common Pronumeral", "1(1(3X + 4) / 1(1A + 2Y))", FunctionArguments(1, FunctionType.DIV,
	FunctionArguments(1, FunctionType.ADD,
		FunctionArguments(1, FunctionType.MUL, FunctionPrimitive(1, Symbol.Y), FunctionPrimitive(9, Symbol.X)),
		FunctionArguments(1, FunctionType.MUL, FunctionPrimitive(1, Symbol.Y), FunctionPrimitive(12))
	),
	FunctionArguments(1, FunctionType.ADD,
		FunctionArguments(1, FunctionType.MUL, FunctionPrimitive(1, Symbol.Y), FunctionPrimitive(3, Symbol.A)),
		FunctionArguments(1, FunctionType.MUL, FunctionArguments(1, FunctionType.EXPONENTIAL,
			FunctionPrimitive(1, Symbol.Y),
			FunctionPrimitive(2)
		), FunctionPrimitive(6))
	)
));

CheckFunctionOutput("Add Fractions", "1(1(1(1Y + 10 + 1X) / 3Y) + 2)", FunctionArguments(1, FunctionType.ADD,
	FunctionArguments(1, FunctionType.DIV,
		FunctionArguments(1, FunctionType.ADD,
			FunctionPrimitive(1, Symbol.Y),
			FunctionPrimitive(5)
		),
		FunctionPrimitive(3, Symbol.Y)
	),
	FunctionArguments(1, FunctionType.DIV,
		FunctionArguments(1, FunctionType.ADD,
			FunctionPrimitive(1, Symbol.X),
			FunctionPrimitive(5)
		),
		FunctionPrimitive(3, Symbol.Y)
	),
	FunctionArguments(1, FunctionType.DIV, FunctionPrimitive(3), FunctionPrimitive(4)),
	FunctionArguments(1, FunctionType.DIV, FunctionPrimitive(5), FunctionPrimitive(4))
));

CheckFunctionOutput("Divide Fractions", "1(1 / 1(1(1X ^ 3) * 1Y))", FunctionArguments(1, FunctionType.DIV,
	FunctionArguments(1, FunctionType.DIV,
		FunctionArguments(1, FunctionType.DIV,
			FunctionPrimitive(1),
			FunctionPrimitive(1, Symbol.X)
		),
		FunctionArguments(1, FunctionType.MUL,
			FunctionPrimitive(1, Symbol.X),
			FunctionPrimitive(1, Symbol.Y)
		)
	),
	FunctionPrimitive(1, Symbol.X)
));

CheckFunctionOutput("Divide Exponentials", "1(1(1X ^ 2) / 1(1Y ^ 2))",
	FunctionArguments(1, FunctionType.EXPONENTIAL,
		FunctionArguments(1, FunctionType.DIV,
			FunctionPrimitive(1, Symbol.X),
			FunctionPrimitive(1, Symbol.Y)
		),
		FunctionPrimitive(2)
	)
);
