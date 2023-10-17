using static Algebra.Utility;
using static Primes;

namespace Algebra
{
	static class Functions
	{
		public static FunctionResult ExecuteFunction(Function function)
		{
			if (function.functionType == FunctionType.PRIMITIVE)
			{
				return new FunctionResult { collapsed = false, function = function.Clone() };
			}
			else
			{
				var results = new List<FunctionResult>();
				var newFunction = function.Clone();

				// Multiply quantity into group
				if (newFunction.quantity != 1 && new FunctionType[] { FunctionType.ADD, FunctionType.DIV }.Contains(newFunction.functionType))
				{
					for (var i = 0; i < newFunction.arguments.Count; i++)
					{
						var argument = newFunction.arguments[i];
						argument.quantity *= newFunction.quantity;
						newFunction.arguments[i] = argument;
						if (newFunction.functionType != FunctionType.ADD) break;
					}
					newFunction.quantity = 1;
					return new FunctionResult { collapsed = true, function = newFunction };
				}

				// Execute sub functions, break on collapse
				for (var i = 0; i < newFunction.arguments.Count; i++)
				{
					var result = ExecuteFunction(newFunction.arguments[i]);
					if (result.collapsed)
					{
						newFunction.arguments[i] = result.function;
						return new FunctionResult { collapsed = true, function = newFunction };
					}
					// Hoist out Matryoshka doll add and mul functions
					if ((newFunction.functionType == FunctionType.ADD || newFunction.functionType == FunctionType.MUL) && newFunction.functionType == result.function.functionType)
					{
						newFunction.arguments.RemoveAt(i);
						for (var j = result.function.arguments.Count - 1; j >= 0; j--)
						{
							var toInsert = result.function.arguments[j].Clone();
							if (result.function.functionType == FunctionType.MUL && j == 0 && result.function.quantity != 1)
							{
								toInsert.quantity *= result.function.quantity;
							}
							newFunction.arguments.Insert(i, toInsert);
						}
						return new FunctionResult { collapsed = true, function = newFunction };
					}
					// Multiply quantity into mul
					if (newFunction.functionType == FunctionType.MUL && result.function.quantity != 1)
					{
						newFunction.quantity *= result.function.quantity;
						result.function.quantity = 1;
						newFunction.arguments[i] = result.function;
						return new FunctionResult { collapsed = true, function = newFunction };
					}
					results.Add(result);
				}

				// If the function itself only has one child, hoist it
				if (newFunction.arguments.Count == 1)
				{
					var result = results[0].function.Clone();
					result.quantity *= newFunction.quantity;
					return new FunctionResult { collapsed = true, function = result };
				}
				switch (function.functionType)
				{
					case FunctionType.ADD: return Add(function);
					case FunctionType.MUL: return Mul(function);
					case FunctionType.DIV: return Div(function);
					case FunctionType.EXPONENTIAL: return Exp(function);
				}
			}
			throw new Exception("ExecuteFunction fell through");
		}

		public static FunctionResult Add(Function function)
		{
			for (var i = 0; i < function.arguments.Count; i++)
			{
				var argument1 = function.arguments[i];
				var argument1Hash = CalculateResultHashes(argument1);
				// Try to add function function.arguments
				for (var j = i + 1; j < function.arguments.Count; j++)
				{
					var argument2 = function.arguments[j];
					if (argument1Hash.addHash == null)
					{
						throw new Exception("Error: Add hash was null");
					}
					if (argument1.functionType == FunctionType.DIV && argument2.functionType == FunctionType.DIV && CalculateResultHashes(argument1.arguments[1]).exactHash == CalculateResultHashes(argument2.arguments[1]).exactHash)
					{
						function.arguments[i].arguments[0] = FunctionArguments(1, FunctionType.ADD, argument1.arguments[0].Clone(), argument2.arguments[0].Clone());
						function.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = function };
					}
					if (argument1Hash.addHash == CalculateResultHashes(argument2).addHash)
					{
						argument1.quantity += argument2.quantity;
						function.arguments[i] = argument1;
						function.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = function };
					}
				}
			}

			return new FunctionResult { collapsed = false, function = function };
		}

		public static FunctionResult Mul(Function function)
		{
			var newFunction = function.Clone();
			for (var i = 0; i < newFunction.arguments.Count; i++)
			{
				var argument1 = newFunction.arguments[i];
				// Try to multiply function function.arguments
				for (var j = 0; j < newFunction.arguments.Count; j++)
				{
					if (i == j) continue;
					var argument2 = newFunction.arguments[j];
					if (argument1.quantity != 1 && argument2.quantity != 1)
					{
						argument1.quantity *= argument2.quantity;
						newFunction.arguments[i] = argument1;
						argument2.quantity = 1;
						newFunction.arguments[j] = argument2;
						return new FunctionResult { collapsed = true, function = newFunction };
					}
					if (IsPrimitiveNumber(argument2)) // Always multiply and combine primitive numbers
					{
						newFunction.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = newFunction };
					}
					if (argument2.functionType == FunctionType.DIV) // Distribute into DIV
					{
						if (argument1.functionType == FunctionType.DIV) // Div / Div - Straight multiply to avoid convergence loop
						{
							newFunction.arguments[i] = FunctionArguments(argument1.quantity, FunctionType.DIV,
								FunctionArguments(1, FunctionType.MUL, argument1.arguments[0].Clone(), argument2.arguments[0].Clone()),
								FunctionArguments(1, FunctionType.MUL, argument1.arguments[1].Clone(), argument2.arguments[1].Clone())
							);
						}
						else
						{
							newFunction.arguments[i] = FunctionArguments(argument1.quantity, FunctionType.DIV,
								FunctionArguments(1, FunctionType.MUL, argument1.Clone(), argument2.arguments[0]),
								argument2.arguments[1]
							);
						}
						newFunction.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = newFunction };
					}
					if (argument1.functionType == FunctionType.ADD) // Distribute across add function
					{
						for (var argIndex = 0; argIndex < argument1.arguments.Count; argIndex++)
						{
							argument1.arguments[argIndex] = FunctionArguments(argument1.quantity, FunctionType.MUL,
								argument1.arguments[argIndex].Clone(),
								argument2.Clone()
							);
						}
						newFunction.arguments[i] = argument1;
						newFunction.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = newFunction };
					}
					if (CalculateResultHashes(argument1).mulHash == CalculateResultHashes(argument2).mulHash)
					{
						var function1Exponent = argument1.functionType == FunctionType.EXPONENTIAL ? argument1.arguments[1] : FunctionPrimitive(1);
						var function2Exponent = argument2.functionType == FunctionType.EXPONENTIAL ? argument2.arguments[1] : FunctionPrimitive(1);
						var newBase = argument1.functionType == FunctionType.EXPONENTIAL ? argument1.arguments[0] : argument1;
						var exponential = FunctionArguments(1, FunctionType.EXPONENTIAL,
							newBase,
							FunctionArguments(1, FunctionType.ADD, function1Exponent, function2Exponent)
						);
						newFunction.arguments[i] = exponential;
						newFunction.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = newFunction };
					}
				}
			}

			return new FunctionResult { collapsed = false, function = newFunction };
		}

		public static FunctionResult Exp(Function function)
		{
			var expBase = function.arguments[0];
			var exponent = function.arguments[1];
			if (exponent.functionType == FunctionType.EXPONENTIAL)
			{ // Fold down nested exponents
				var newFunction = FunctionArguments(exponent.quantity, FunctionType.MUL, exponent.arguments[0].Clone(), exponent.arguments[1].Clone());
				function.arguments[1] = newFunction;
				return new FunctionResult { collapsed = true, function = function };
			}
			if (exponent.functionType == FunctionType.PRIMITIVE && exponent.symbol == Symbol.NUMBER)
			{
				if (exponent.quantity == 0)
				{
					return new FunctionResult { collapsed = true, function = FunctionPrimitive(function.quantity) };
				}
				if (exponent.quantity < 0)
				{
					var newFunction = FunctionArguments(function.quantity, FunctionType.DIV,
						FunctionPrimitive(1),
						FunctionArguments(1, FunctionType.EXPONENTIAL,
							expBase,
							FunctionPrimitive(exponent.quantity * -1)
						)
					);
					return new FunctionResult { collapsed = true, function = newFunction };
				}
				else if (!(expBase.functionType == FunctionType.PRIMITIVE && expBase.symbol != Symbol.NUMBER) || exponent.quantity == 1)
				{
					if (exponent.quantity > 0)
					{
						var newFunction = FunctionArguments(function.quantity, FunctionType.MUL);
						for (var i = 0; i < exponent.quantity; i++)
						{
							newFunction.arguments.Add(function.arguments[0].Clone());
						}
						return new FunctionResult { collapsed = true, function = newFunction };
					}
				}
			}
			return new FunctionResult { collapsed = false, function = function };
		}

		public static FunctionResult Div(Function function)
		{
			var numerator = function.arguments[0];
			var denominator = function.arguments[1];
			if (denominator.functionType == FunctionType.PRIMITIVE && denominator.symbol == Symbol.NUMBER && denominator.quantity == 1)
			{
				return new FunctionResult { collapsed = true, function = numerator };
			}
			// function / Div = Multiply by reciprocal
			var numeratorIsDiv = numerator.functionType == FunctionType.DIV;
			var denominatorIsDiv = denominator.functionType == FunctionType.DIV;
			if (numeratorIsDiv || denominatorIsDiv)
			{
				var newFunction = FunctionArguments(function.quantity, FunctionType.MUL,
					FunctionArguments(numerator.quantity, FunctionType.DIV,
						numeratorIsDiv ? numerator.arguments[0] : numerator,
						numeratorIsDiv ? numerator.arguments[1] : FunctionPrimitive(1)
					),
					FunctionArguments(denominator.quantity, FunctionType.DIV,
						denominatorIsDiv ? denominator.arguments[1] : FunctionPrimitive(1),
						denominatorIsDiv ? denominator.arguments[0] : denominator
					)
				);
				return new FunctionResult { collapsed = true, function = newFunction };
			}
			if (CalculateResultHashes(numerator).exactHash == CalculateResultHashes(denominator).exactHash)
			{
				return new FunctionResult { collapsed = true, function = FunctionPrimitive(1) };
			}
			var dividedNumerator = DivInternal(numerator, denominator);

			if (dividedNumerator.collapsed)
			{
				function.arguments[0] = dividedNumerator.remainder;
				function.arguments[1] = DivInternal(denominator, dividedNumerator.divisor).remainder;
				return new FunctionResult { collapsed = true, function = function };
			}
			else
			{
				foreach (var prime in primesGenerated)
				{
					var tryNumerator = DivInternal(numerator, FunctionPrimitive(prime));
					var tryDenominator = DivInternal(denominator, FunctionPrimitive(prime));
					if (tryNumerator.collapsed && tryDenominator.collapsed)
					{
						function.arguments[0] = tryNumerator.remainder;
						function.arguments[1] = tryDenominator.remainder;
						return new FunctionResult { collapsed = true, function = function };
					}
				}
				foreach (Symbol symbol in Enum.GetValues(typeof(Symbol)))
				{
					var tryNumerator = DivInternal(numerator, FunctionPrimitive(1, symbol));
					var tryDenominator = DivInternal(denominator, FunctionPrimitive(1, symbol));
					if (tryNumerator.collapsed && tryDenominator.collapsed && tryNumerator.divisor.symbol == tryDenominator.divisor.symbol)
					{
						function.arguments[0] = tryNumerator.remainder;
						function.arguments[1] = tryDenominator.remainder;
						return new FunctionResult { collapsed = true, function = function };
					}
				}
			}
			return new FunctionResult { collapsed = false, function = function };
		}

		public static DivisionResult DivInternal(Function numerator, Function denominator)
		{
			var clonedNumerator = numerator.Clone();
			var clonedDenominator = denominator.Clone();
			// If we can divide out the quantity of the whole function, early return
			if (clonedDenominator.quantity != 1 && (float)clonedNumerator.quantity / clonedDenominator.quantity == Math.Floor((float)clonedNumerator.quantity / clonedDenominator.quantity))
			{
				clonedNumerator.quantity /= clonedDenominator.quantity;
				return new DivisionResult { collapsed = true, remainder = clonedNumerator, divisor = FunctionPrimitive(clonedDenominator.quantity) };
			}
			if (clonedNumerator.functionType == FunctionType.PRIMITIVE && clonedDenominator.functionType == FunctionType.PRIMITIVE)
			{
				if (clonedNumerator.symbol != Symbol.NUMBER && clonedNumerator.symbol == clonedDenominator.symbol)
				{
					return new DivisionResult { collapsed = true, remainder = FunctionPrimitive(clonedNumerator.quantity), divisor = FunctionPrimitive(1, clonedNumerator.symbol) };
				}
				return new DivisionResult { collapsed = false };
			}
			else if (clonedNumerator.functionType == FunctionType.ADD)
			{
				var divisor = new Function();
				for (var i = 0; i < clonedNumerator.arguments.Count; i++)
				{
					var result = DivInternal(clonedNumerator.arguments[i], clonedDenominator);
					if (!result.collapsed)
					{
						return result;
					}
					else
					{
						clonedNumerator.arguments[i] = result.remainder;
						divisor = result.divisor;
					}
				}
				return new DivisionResult { collapsed = true, remainder = clonedNumerator, divisor = divisor };
			}
			else if (clonedNumerator.functionType == FunctionType.MUL)
			{
				for (var i = 0; i < clonedNumerator.arguments.Count; i++)
				{
					var result = DivInternal(clonedNumerator.arguments[i], clonedDenominator);
					if (result.collapsed)
					{
						clonedNumerator.arguments[i] = result.remainder;
						return new DivisionResult { collapsed = true, remainder = clonedNumerator, divisor = result.divisor };
					}
				}
				return new DivisionResult { collapsed = false, remainder = clonedNumerator };
			}
			else if (clonedNumerator.functionType == FunctionType.PRIMITIVE || clonedNumerator.functionType == FunctionType.EXPONENTIAL)
			{
				if (CalculateResultHashes(clonedNumerator).mulHash == CalculateResultHashes(clonedDenominator).mulHash)
				{
					var numeratorExponentContents = numerator.functionType == FunctionType.EXPONENTIAL ? numerator.arguments[1] : FunctionPrimitive(1);
					var denominatorExponentContents = denominator.functionType == FunctionType.EXPONENTIAL ? denominator.arguments[1] : FunctionPrimitive(1);
					var newExponent = FunctionArguments(1, FunctionType.ADD, numeratorExponentContents, FunctionArguments(1, FunctionType.MUL, FunctionPrimitive(-1), denominatorExponentContents));
					var newBase = numerator.functionType == FunctionType.EXPONENTIAL ? numerator.arguments[0] : numerator;
					newBase.quantity = 1;
					var newExponential = FunctionArguments(numerator.quantity, FunctionType.EXPONENTIAL, newBase, newExponent);
					clonedDenominator.quantity = 1;
					return new DivisionResult { collapsed = true, remainder = newExponential, divisor = clonedDenominator };
				}
				return new DivisionResult { collapsed = false, remainder = clonedNumerator };
			}

			throw new Exception("DivInternal fell through");
		}
	}

	static class Utility
	{
		public static Function FunctionPrimitive(int quantity, Symbol symbol = Symbol.NUMBER)
		{
			return new Function
			{
				quantity = quantity,
				symbol = symbol,
				functionType = FunctionType.PRIMITIVE
			};
		}

		public static Function FunctionArguments(int quantity, FunctionType functionType, params Function[] arguments)
		{
			return new Function
			{
				quantity = quantity,
				functionType = functionType,
				arguments = new List<Function>(arguments),
			};
		}

		public static bool IsPrimitiveNumber(Function function)
		{
			return function.functionType == FunctionType.PRIMITIVE && function.symbol == Symbol.NUMBER;
		}

		public static ResultHashes CalculateResultHashes(Function function)
		{
			if (function.functionType == FunctionType.PRIMITIVE)
			{
				return new ResultHashes
				{
					addHash = function.symbol.ToString(),
					mulHash = function.symbol.ToString(),
					exactHash = function.quantity.ToString() + "_" + function.symbol.ToString()
				};
			}
			var resultHashes = new ResultHashes
			{
				addHash = function.functionType.ToString() + "_",
				exactHash = function.functionType.ToString() + "_",
				mulHash = ""
			};
			for (var i = 0; i < function.arguments.Count; i++)
			{
				var subResults = CalculateResultHashes(function.arguments[i]);
				resultHashes.addHash += subResults.addHash;
				resultHashes.exactHash += subResults.exactHash;
				if (i == 0 || function.functionType != FunctionType.EXPONENTIAL)
				{
					resultHashes.mulHash += subResults.mulHash;
				}
			}
			if (function.functionType == FunctionType.EXPONENTIAL)
			{
				resultHashes.addHash = resultHashes.exactHash;
			}
			return resultHashes;
		}

		public static string PrintFunctions(Function function)
		{
			var output = "";
			var toInspect = new List<Printable> { new Printable { function = function } };
			while (toInspect.Count > 0)
			{
				var current = toInspect[toInspect.Count - 1];
				toInspect.RemoveAt(toInspect.Count - 1);
				if (current.isString)
				{
					output += current.stringValue;
				}
				else if (current.function.functionType == FunctionType.PRIMITIVE)
				{
					output += current.function.quantity.ToString() + (current.function.symbol != Symbol.NUMBER ? current.function.symbol : "");
				}
				else
				{
					if (current.function.functionType != FunctionType.PRIMITIVE)
					{
						toInspect.Add(new Printable { isString = true, stringValue = ")" });
					}
					for (var i = current.function.arguments.Count - 1; i >= 0; i--)
					{
						if (i < current.function.arguments.Count - 1)
						{
							var separator = "+";
							switch (current.function.functionType)
							{
								case FunctionType.MUL: separator = "*"; break;
								case FunctionType.EXPONENTIAL: separator = "^"; break;
								case FunctionType.DIV: separator = "/"; break;
							}
							toInspect.Add(new Printable { isString = true, stringValue = $" {separator} " });
						}
						toInspect.Add(new Printable { function = current.function.arguments[i] });
					}
					if (current.function.functionType != FunctionType.PRIMITIVE)
					{
						toInspect.Add(new Printable { isString = true, stringValue = $"{current.function.quantity}(" });
					}
				}
			}
			return output;
		}
	}

	enum Symbol
	{
		NUMBER,
		A,
		B,
		X,
		Y,
	}

	enum FunctionType
	{
		PRIMITIVE,
		ADD,
		MUL,
		DIV,
		EXPONENTIAL
	}

	struct Function
	{
		public List<Function> arguments;
		public int quantity;
		public FunctionType functionType;
		public Symbol symbol; // Only used for primitives

		public Function Clone()
		{
			var newArguments = new List<Function>();
			if (arguments != null)
			{
				foreach (var argument in arguments)
				{
					newArguments.Add(argument.Clone());
				}
			}
			return new Function
			{
				arguments = newArguments,
				quantity = this.quantity,
				functionType = this.functionType,
				symbol = this.symbol
			};
		}
	}

	struct ResultHashes
	{
		public string addHash;
		public string mulHash;
		public string exactHash;
	}

	struct DivisionResult
	{
		public bool collapsed;
		public Function remainder;
		public Function divisor;
	}

	struct FunctionResult
	{
		public bool collapsed;
		public Function function;
	}

	struct Printable
	{
		public bool isString;
		public string stringValue;
		public Function function;
	}

	enum TestResult
	{
		SUCCESS,
		ASSERT_NOT_MATCH,
		INFINITE_LOOP
	}
}