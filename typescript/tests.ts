/// <reference types="node" />
import { AlgebraConfig, AlgebraFunction, AlgebraFunctionType, AlgebraSymbol, CloneAlgebraFunction, ExecuteFunction, FunctionArguments, FunctionPrimitive, PrintFunctions, PrintFunctionsLatex, PrintFunctionsWithoutColors, TestResult, collapseTypeDocumentation } from "./algebra";

var debug = process.argv.length > 2 && process.argv[2] == "DEBUG";
function CheckFunctionOutput(name: string, assert: string, algebraFunction: AlgebraFunction) {
	AlgebraConfig.DEBUG = debug;
	switch (CheckFunctionOutputInternal(name, assert, algebraFunction, debug)) {
		case TestResult.SUCCESS: break;
		case TestResult.ASSERT_NOT_MATCH:
			{
				console.log("Step Summary:");
				AlgebraConfig.DEBUG = true;
				console.log("------------------------------");
				CheckFunctionOutputInternal(name, assert, algebraFunction, true);
				AlgebraConfig.DEBUG = false;
				break;
			}
		case TestResult.INFINITE_LOOP:
			{
				break;
			}
	}
	AlgebraConfig.DEBUG = true;
}

function PrintFunctionsWithColors(functionString: string, consoleColor: string = "\x1b[31m") {
	let split = functionString.split("__MODIFIED__");
	let toOutput = "";
	if (split.length > 1) {
		for (let i = 0; i < split.length; i++) {
			toOutput += ((i % 2 == 1) ? consoleColor : "") + split[i] + ((i % 2 == 1) ? "\x1b[0m" : "");
		}
		console.log(toOutput);
	}
	else {
		console.log(split[0]);
	}
}

function CheckFunctionOutputInternal(name: string, assert: string, algebraFunction: AlgebraFunction, debug = false): TestResult {
	var currentFunction = CloneAlgebraFunction(algebraFunction);
	if (debug) {
		console.log("\x1b[33m" + name + "\x1b[0m");
		console.log("\x1b[36m" + `Starting value: ${PrintFunctionsWithoutColors(currentFunction)}` + "\x1b[0m");
		console.log("\x1b[36m" + `Expecting result: ${assert}` + "\x1b[0m");
	}
	var resultsList: string[] = [];
	for (var i = 0; i < 1000; i++) {
		var resultString = PrintFunctionsWithoutColors(currentFunction);
		var result = ExecuteFunction(currentFunction);
		if (debug) {
			if (result.collapsed && result.functionCollapseInfo) {
				var collapseType = result.functionCollapseInfo.functionCollapseType;
				var docs = collapseTypeDocumentation[collapseType].devMessage;
				if (result.functionCollapseInfo.additionalInfo) {
					docs = docs.replace("?", PrintFunctionsWithoutColors(result.functionCollapseInfo.additionalInfo));
				}
				console.log("- " + docs + " ↓");
			}
			else if (!result.collapsed) {
				console.log("\x1b[30m" + "- Result" + "\x1b[0m");
			}
			if (result.functionCollapseInfo) {
				PrintFunctionsWithColors(PrintFunctions(currentFunction, result.functionCollapseInfo.beforeFunctionIds, result.functionCollapseInfo.functionCollapseType));
				if (result.functionCollapseInfo.afterFunctionIds != null) {
					PrintFunctionsWithColors(PrintFunctions(result.algebraFunction, result.functionCollapseInfo.afterFunctionIds, result.functionCollapseInfo.functionCollapseType), "\x1b[34m");
				}
			}
		}
		currentFunction = result.algebraFunction;
		if (!result.collapsed) {
			if (resultString != assert) {
				console.log("\x1b[31m" + `${name} - Failed` + '\x1b[0m');
				console.log(`Expected: \"${assert}\"`);
				console.log(`Output: \"${resultString}\"\n`);
				return TestResult.ASSERT_NOT_MATCH;
			}
			else {
				console.log("\x1b[32m" + `${name} - Passed\n` + '\x1b[0m');
				return TestResult.SUCCESS;
			}
		}
		else if (result.collapsed && resultsList.findIndex(r => r == resultString) != -1 && resultString == assert) {
			console.log("\x1b[32m" + `${name} - Passed with convergence\n` + '\x1b[0m');
			return TestResult.SUCCESS;
		}
		resultsList.push(resultString);
	}
	console.log("\x1b[31m" + "Failed with infinite loop. Last function:" + '\x1b[0m');
	console.log(PrintFunctionsWithoutColors(currentFunction));
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

CheckFunctionOutput("Nested Exponential as Exponent", "1(1X ^ 6(1Y * 1A))", FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
	FunctionPrimitive(1, AlgebraSymbol.X),
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(2, AlgebraSymbol.Y), FunctionPrimitive(3, AlgebraSymbol.A))
));

CheckFunctionOutput("Nested Exponential as Base", "1(2Y ^ 3(1A * 1X))", FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL, FunctionPrimitive(2, AlgebraSymbol.Y), FunctionPrimitive(3, AlgebraSymbol.A)),
	FunctionPrimitive(1, AlgebraSymbol.X)
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

CheckFunctionOutput("Divide DIV", "1(25 / 1X)", FunctionArguments(1, AlgebraFunctionType.DIV,
	FunctionArguments(1, AlgebraFunctionType.DIV,
		FunctionPrimitive(125),
		FunctionPrimitive(1, AlgebraSymbol.X)
	),
	FunctionPrimitive(5),
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

CheckFunctionOutput("Add primitive fractions", "1", FunctionArguments(1, AlgebraFunctionType.ADD,
	FunctionArguments(1, AlgebraFunctionType.DIV,
		FunctionPrimitive(1),
		FunctionPrimitive(3)
	),
	FunctionArguments(1, AlgebraFunctionType.DIV,
		FunctionPrimitive(2),
		FunctionPrimitive(3)
	)
));

CheckFunctionOutput("Add Fractions", "1(1(1 / 3) + 1(1(10 + 1X) / 3Y) + 2)", FunctionArguments(1, AlgebraFunctionType.ADD,
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

CheckFunctionOutput("Partial Division", "1(1(1(1X ^ 2) / 1(1Y ^ 2)) + 1(10X / 1Y) + 25)",
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
		FunctionArguments(1, AlgebraFunctionType.ADD,
			FunctionArguments(1, AlgebraFunctionType.DIV,
				FunctionPrimitive(1, AlgebraSymbol.X),
				FunctionPrimitive(1, AlgebraSymbol.Y)
			),
			FunctionPrimitive(5)
		),
		FunctionPrimitive(2)
	)
);

CheckFunctionOutput("Primitive square root", "2",
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
		FunctionPrimitive(8),
		FunctionArguments(1, AlgebraFunctionType.DIV,
			FunctionPrimitive(1),
			FunctionPrimitive(3)
		)
	)
);

CheckFunctionOutput("Nested square root", "1(1X ^ 1(1 / 4))",
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
		FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
			FunctionPrimitive(1, AlgebraSymbol.X),
			FunctionArguments(1, AlgebraFunctionType.DIV,
				FunctionPrimitive(1),
				FunctionPrimitive(2)
			)
		),
		FunctionArguments(1, AlgebraFunctionType.DIV,
			FunctionPrimitive(1),
			FunctionPrimitive(2)
		)
	)
);

CheckFunctionOutput("Exponential square root", "1(1X ^ 1(1 / 2))",
	FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
		FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
			FunctionPrimitive(1, AlgebraSymbol.X),
			FunctionArguments(1, AlgebraFunctionType.DIV,
				FunctionPrimitive(1),
				FunctionPrimitive(4)
			)
		),
		FunctionPrimitive(2)
	)
);

CheckFunctionOutput("Complex Numerator", "1(12 + 6X + 1(1X ^ 2))",
	FunctionArguments(1, AlgebraFunctionType.DIV,
		FunctionArguments(1, AlgebraFunctionType.ADD,
			FunctionArguments(1, AlgebraFunctionType.EXPONENTIAL,
				FunctionArguments(1, AlgebraFunctionType.ADD,
					FunctionPrimitive(2),
					FunctionPrimitive(1, AlgebraSymbol.X)
				),
				FunctionPrimitive(3)
			),
			FunctionArguments(-1, AlgebraFunctionType.EXPONENTIAL,
				FunctionPrimitive(2),
				FunctionPrimitive(3)
			)
		),
		FunctionPrimitive(1, AlgebraSymbol.X)
	)
);

CheckFunctionOutput("Zero Numerator", "1",
	FunctionArguments(1, AlgebraFunctionType.ADD,
		FunctionPrimitive(1),
		FunctionArguments(1, AlgebraFunctionType.DIV, FunctionPrimitive(0), FunctionPrimitive(1, AlgebraSymbol.X))
	)
);

