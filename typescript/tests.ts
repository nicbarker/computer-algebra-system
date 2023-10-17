import { AlgebraFunction, AlgebraFunctionType, AlgebraSymbol, CloneAlgebraFunction, ExecuteFunction, FunctionArguments, FunctionPrimitive, PrintFunctions, TestResult } from "./algebra";

var debug = process.argv.length > 2 && process.argv[2] == "DEBUG";
function CheckFunctionOutput(name: string, assert: string, algebraFunction: AlgebraFunction) {
	switch (CheckFunctionOutputInternal(name, assert, algebraFunction, debug)) {
		case TestResult.SUCCESS: break;
		case TestResult.ASSERT_NOT_MATCH:
			{
				console.log("Step Summary:");
				CheckFunctionOutputInternal(name, assert, algebraFunction, true);
				break;
			}
		case TestResult.INFINITE_LOOP:
			{
				break;
			}
	}
}

function CheckFunctionOutputInternal(name: string, assert: string, algebraFunction: AlgebraFunction, debug = false): TestResult {
	var currentFunction = CloneAlgebraFunction(algebraFunction);
	if (debug) {
		console.log("------------------------------");
		console.log(PrintFunctions(currentFunction));
	}
	var resultsList: string[] = [];
	for (var i = 0; i < 1000; i++) {
		var resultString = PrintFunctions(currentFunction);
		var result = ExecuteFunction(currentFunction);
		if (debug) {
			console.log(PrintFunctions(result.algebraFunction));
		}
		currentFunction = result.algebraFunction;
		if (!result.collapsed) {
			if (resultString != assert) {
				console.log("\x1b[31m", `${name} - Failed`, '\x1b[0m');
				console.log(`Expected: \"${assert}\"`);
				console.log(`Output: \"${resultString}\"`);
				return TestResult.ASSERT_NOT_MATCH;
			}
			else {
				console.log("\x1b[32m", `${name} - Passed`, '\x1b[0m');
				return TestResult.SUCCESS;
			}
		}
		else if (result.collapsed && resultsList.findIndex(r => r == resultString) != -1) {
			console.log("\x1b[32m", `${name} - Passed with convergence`, '\x1b[0m');
			return TestResult.SUCCESS;
		}
		resultsList.push(resultString);
	}
	console.log("\x1b[31m", "Failed with infinite loop. Last function:", '\x1b[0m');
	console.log(PrintFunctions(currentFunction));
	return TestResult.INFINITE_LOOP;
}

var twoPlusThree = FunctionArguments(1, AlgebraFunctionType.ADD, FunctionPrimitive(2), FunctionPrimitive(3));
CheckFunctionOutput("Basic Add", "5", twoPlusThree);

var threeMinusFive = FunctionArguments(1, AlgebraFunctionType.ADD, FunctionPrimitive(3), FunctionPrimitive(-5));
CheckFunctionOutput("Basic Subtract", "-2", threeMinusFive);

CheckFunctionOutput("Add Two Functions", "3", FunctionArguments(1, AlgebraFunctionType.ADD, twoPlusThree, threeMinusFive));

CheckFunctionOutput("Add Pronumerals", "2X", FunctionArguments(1, AlgebraFunctionType.ADD, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(1, AlgebraSymbol.X)));

CheckFunctionOutput("Multiply Mul functions", "6(1X * 1Y * 1A * 1B)", FunctionArguments(1, AlgebraFunctionType.MUL,
	FunctionArguments(1, AlgebraFunctionType.MUL,
		FunctionPrimitive(1, AlgebraSymbol.X),
		FunctionPrimitive(3, AlgebraSymbol.Y)
	),
	FunctionArguments(1, AlgebraFunctionType.MUL,
		FunctionPrimitive(1, AlgebraSymbol.A),
		FunctionPrimitive(2, AlgebraSymbol.B)
	)
));

CheckFunctionOutput("Distribute Single Add function", "1(6(1X * 1Y) + 4(1A * 1Y) + 2(1B * 1Y))", FunctionArguments(1, AlgebraFunctionType.MUL,
	FunctionPrimitive(2, AlgebraSymbol.Y),
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(3, AlgebraSymbol.X),
		FunctionPrimitive(2, AlgebraSymbol.A),
		FunctionPrimitive(1, AlgebraSymbol.B)
	)
));

CheckFunctionOutput("Distribute Two Add functions", "1(8(1A * 1X) + 10(1B * 1X) + 12(1A * 1Y) + 15(1B * 1Y))", FunctionArguments(1, AlgebraFunctionType.MUL,
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(2, AlgebraSymbol.X),
		FunctionPrimitive(3, AlgebraSymbol.Y)
	),
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(4, AlgebraSymbol.A),
		FunctionPrimitive(5, AlgebraSymbol.B)
	)
));

CheckFunctionOutput("Exponential", "2(1X ^ 2)", FunctionArguments(1, AlgebraFunctionType.MUL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(2, AlgebraSymbol.X)));

CheckFunctionOutput("Exponential Zero and One", "1(1 + 1X)", FunctionArguments(1, AlgebraFunctionType.ADD,
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(0)),
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(1))
));

CheckFunctionOutput("Add Exponential", "1(3(1X ^ 2) + 1(2X ^ 2) + 2)", FunctionArguments(1, AlgebraFunctionType.ADD,
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(2)),
	FunctionArguments(2, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(2)),
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(2, AlgebraSymbol.X), FunctionPrimitive(2)),
	FunctionPrimitive(2)
));

CheckFunctionOutput("Multiply Exponential by Pronumeral", "2(1X ^ 3)", FunctionArguments(1, AlgebraFunctionType.MUL, FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(2)), FunctionPrimitive(2, AlgebraSymbol.X)));

CheckFunctionOutput("Simplify and Add Exponentials", "3(1X ^ 4)", FunctionArguments(1, AlgebraFunctionType.ADD,
	FunctionArguments(1, AlgebraFunctionType.MUL,
		FunctionPrimitive(1, AlgebraSymbol.X),
		FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(3))
	),
	FunctionArguments(1, AlgebraFunctionType.MUL,
		FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(2)),
		FunctionArguments(2, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(3)),
		FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(-1))
	)
));

CheckFunctionOutput("Exponential Primitive", "1(4 + 1(1X ^ 2))", FunctionArguments(1, AlgebraFunctionType.ADD,
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(2), FunctionPrimitive(2)),
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(1, AlgebraSymbol.X), FunctionPrimitive(2))
));

CheckFunctionOutput("Exponential Add", "1(1(1X ^ 3) + 3(1X ^ 2) + 3X + 1)", FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(1, AlgebraSymbol.X),
		FunctionPrimitive(1)
	),
	FunctionPrimitive(3)
));

CheckFunctionOutput("Nested Exponential", "1(1X ^ 6(1Y * 1A))", FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
	FunctionPrimitive(1, AlgebraSymbol.X),
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(2, AlgebraSymbol.Y), FunctionPrimitive(3, AlgebraSymbol.A))
));

CheckFunctionOutput("Basic Divide", "3", FunctionArguments(1, AlgebraFunctionType.DIV, FunctionPrimitive(6), FunctionPrimitive(2)));

CheckFunctionOutput("Stable Rational", "1(3 / 2)", FunctionArguments(1, AlgebraFunctionType.DIV, FunctionPrimitive(3), FunctionPrimitive(2)));

CheckFunctionOutput("Divide Out Pronumerals", "1(3 / 2)", FunctionArguments(1, AlgebraFunctionType.DIV, FunctionPrimitive(3, AlgebraSymbol.X), FunctionPrimitive(2, AlgebraSymbol.X)));

CheckFunctionOutput("Divide Out Multiple Terms", "2", FunctionArguments(1, AlgebraFunctionType.DIV, FunctionPrimitive(4, AlgebraSymbol.X), FunctionPrimitive(2, AlgebraSymbol.X)));

CheckFunctionOutput("Divide Add Function", "5", FunctionArguments(1, AlgebraFunctionType.DIV,
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(3, AlgebraSymbol.X),
		FunctionPrimitive(2, AlgebraSymbol.X)
	),
	FunctionPrimitive(1, AlgebraSymbol.X)
));

CheckFunctionOutput("Divide Exponential", "1(1(1X ^ 6) / 4)", FunctionArguments(1, AlgebraFunctionType.DIV,
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
		FunctionPrimitive(1, AlgebraSymbol.X),
		FunctionPrimitive(7)
	),
	FunctionPrimitive(4, AlgebraSymbol.X)
));

CheckFunctionOutput("Divide By Exponential", "1(2 / 1X)", FunctionArguments(1, AlgebraFunctionType.DIV,
	FunctionPrimitive(2, AlgebraSymbol.X),
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
		FunctionPrimitive(1, AlgebraSymbol.X),
		FunctionPrimitive(2)
	)
));

CheckFunctionOutput("Divide By Exact Match", "1", FunctionArguments(1, AlgebraFunctionType.DIV,
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(1, AlgebraSymbol.X),
		FunctionPrimitive(2)
	),
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(1, AlgebraSymbol.X),
		FunctionPrimitive(2)
	)
));

CheckFunctionOutput("Divide By GCD", "1(3X / 2Y)", FunctionArguments(1, AlgebraFunctionType.DIV, FunctionPrimitive(6, AlgebraSymbol.X), FunctionPrimitive(4, AlgebraSymbol.Y)));

CheckFunctionOutput("Divide Add By GCD", "1(1(3X + 4) / 1(1X + 2))", FunctionArguments(1, AlgebraFunctionType.DIV,
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(9, AlgebraSymbol.X),
		FunctionPrimitive(12)
	),
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(3, AlgebraSymbol.X),
		FunctionPrimitive(6)
	)
));

CheckFunctionOutput("Divide Add By Common Pronumeral", "1(1(3X + 4) / 1(1A + 2Y))", FunctionArguments(1, AlgebraFunctionType.DIV,
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionArguments(1, AlgebraFunctionType.MUL, FunctionPrimitive(1, AlgebraSymbol.Y), FunctionPrimitive(9, AlgebraSymbol.X)),
		FunctionArguments(1, AlgebraFunctionType.MUL, FunctionPrimitive(1, AlgebraSymbol.Y), FunctionPrimitive(12))
	),
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionArguments(1, AlgebraFunctionType.MUL, FunctionPrimitive(1, AlgebraSymbol.Y), FunctionPrimitive(3, AlgebraSymbol.A)),
		FunctionArguments(1, AlgebraFunctionType.MUL, FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
			FunctionPrimitive(1, AlgebraSymbol.Y),
			FunctionPrimitive(2)
		), FunctionPrimitive(6))
	)
));

CheckFunctionOutput("Add Fractions", "1(1(1(1Y + 10 + 1X) / 3Y) + 2)", FunctionArguments(1, AlgebraFunctionType.ADD,
	FunctionArguments(1, AlgebraFunctionType.DIV,
		FunctionArguments(1, AlgebraFunctionType.ADD,
			FunctionPrimitive(1, AlgebraSymbol.Y),
			FunctionPrimitive(5)
		),
		FunctionPrimitive(3, AlgebraSymbol.Y)
	),
	FunctionArguments(1, AlgebraFunctionType.DIV,
		FunctionArguments(1, AlgebraFunctionType.ADD,
			FunctionPrimitive(1, AlgebraSymbol.X),
			FunctionPrimitive(5)
		),
		FunctionPrimitive(3, AlgebraSymbol.Y)
	),
	FunctionArguments(1, AlgebraFunctionType.DIV, FunctionPrimitive(3), FunctionPrimitive(4)),
	FunctionArguments(1, AlgebraFunctionType.DIV, FunctionPrimitive(5), FunctionPrimitive(4))
));

CheckFunctionOutput("Divide Fractions", "1(1 / 1(1(1X ^ 3) * 1Y))", FunctionArguments(1, AlgebraFunctionType.DIV,
	FunctionArguments(1, AlgebraFunctionType.DIV,
		FunctionArguments(1, AlgebraFunctionType.DIV,
			FunctionPrimitive(1),
			FunctionPrimitive(1, AlgebraSymbol.X)
		),
		FunctionArguments(1, AlgebraFunctionType.MUL,
			FunctionPrimitive(1, AlgebraSymbol.X),
			FunctionPrimitive(1, AlgebraSymbol.Y)
		)
	),
	FunctionPrimitive(1, AlgebraSymbol.X)
));

CheckFunctionOutput("Divide Exponentials", "1(1(1X ^ 2) / 1(1Y ^ 2))",
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
		FunctionArguments(1, AlgebraFunctionType.DIV,
			FunctionPrimitive(1, AlgebraSymbol.X),
			FunctionPrimitive(1, AlgebraSymbol.Y)
		),
		FunctionPrimitive(2)
	)
);
