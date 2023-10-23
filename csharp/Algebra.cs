using static Algebra.Utility;
using static Primes;

namespace Algebra
{
	static class Functions
	{
		public static bool DEBUG = false;
		public static int nextFunctionId = 1;

		public static readonly FunctionCollapseTypeDocumentation[] collapseTypeDocumentation = new FunctionCollapseTypeDocumentation[] {
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.DISTRIBUTE_QUANTITY_INTO_FUNCTION, affectsQuantity = true, devMessage = "Distribute quantity into function arguments", humanReadableMessage = "Multiply inner terms by outer quantity (distribute)" },
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.HOIST_NESTED_FUNCTION_WITH_SAME_TYPE, devMessage = "Hoist nested function with same type", humanReadableMessage = "Hoist nested function with same type", internalOnly = true},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.EXTRACT_ARGUMENT_QUANTITY_INTO_FUNCTION, affectsQuantity = true, devMessage = "Extract argument quantities into outer MUL function", humanReadableMessage = "Extract argument quantities into outer MUL function", internalOnly = true},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.HOIST_SINGLE_ARGUMENT_INTO_PARENT, affectsQuantity = true, devMessage = "Hoist single argument into parent", humanReadableMessage = "Hoist single argument into parent", internalOnly = true},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.ELIMINATE_ZERO_TERMS, devMessage = "Eliminate ADD arguments with quantity 0", humanReadableMessage = "Remove terms in addition with a quantity of zero"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.COMBINE_DIV_WITH_MATCHING_DENOMINATOR, devMessage = "Combine DIV numerators with exactly matching denominator", humanReadableMessage = "Add together two fractions with the same denominator"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.COMBINE_ADDABLE_TERMS, devMessage = "Combine add-able function arguments", humanReadableMessage = "Combine addable terms together"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.MULTIPLY_PRIMITIVE_NUMBERS, affectsQuantity = true, devMessage = "Multiply primitive number function", humanReadableMessage = "Multiply numerical quantities together"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.MULTIPLY_DIV_FUNCTIONS, devMessage = "Straight multiply DIV numerator & denominator", humanReadableMessage = "Multiply numerator by numerator, and denominator by denominator"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.MULTIPLY_INTO_DIV_NUMERATOR, devMessage = "Multiply into DIV numerator argument", humanReadableMessage = "Multiply by fraction numerator"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.DISTRIBUTE_FUNCTION_INTO_ADD_ARGUMENTS, devMessage = "Distribute function into ADD arguments", humanReadableMessage = "Multiply by all addition terms (distribute)"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.MULTIPLY_EXPONENTS, devMessage = "Multiply and promote or increase exponent", humanReadableMessage = "Multiply two like terms together by adding their exponents"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.MULTIPLY_OUT_NESTED_EXPONENTS, devMessage = "Multiply out nested exponents", humanReadableMessage = "Combine nested exponents by multiplying powers"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.EXPONENT_IS_ZERO, devMessage = "Convert argument with exponent of 0 into 1", humanReadableMessage = "Convert term with exponent of 0 into 1"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.CONVERT_NEGATIVE_EXPONENT_TO_RECIPROCAL, devMessage = "Convert negative exponent into 1 / positive exponent", humanReadableMessage = "Convert negative exponent into 1 / positive exponent"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.CONVERT_NUMERIC_EXPONENT_TO_REPEATED_MUL, devMessage = "Convert numeric exponent to repeated MUL", humanReadableMessage = "Convert numeric exponent into repeated multiply"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.SQUARE_ROOT_OF_PRIMITIVE_NUMBER, devMessage = "Root of primitive number", humanReadableMessage = "Compute the root of a primitive value"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.COLLAPSE_DIV_WITH_DENOMINATOR_1, devMessage = "Collapse DIV with denominator = 1", humanReadableMessage = "Simplify fraction with denominator of 1"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.COLLAPSE_DIV_WITH_NUMERATOR_0, devMessage = "Collapse DIV with numerator = 0", humanReadableMessage = "Eliminate fraction with numerator of 0"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.CONVERT_NESTED_DIV_TO_RECIPROCAL_MUL, devMessage = "Convert nested DIV to reciprocal MUL", humanReadableMessage = "Simplified nested fractions by multiplying by the reciprocal of the denominator"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.COLLAPSE_VALUE_DIVIDED_BY_ITSELF, devMessage = "Collapse DIV with identical numerator and denominator to 1", humanReadableMessage = "Simplify fraction with identical numerator and denominator to 1"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.SPLIT_AND_DIVIDE_DIV, devMessage = "Split DIV with ADD numerator to allow partial division", humanReadableMessage = "Split the fraction numerator to allow division"},
			new FunctionCollapseTypeDocumentation { functionCollapseType = FunctionCollapseType.DIV_NUMERATOR_DENOMINATOR_COMMON_FACTOR, devMessage = "Numerator and Denominator can be divided by common factor ?", humanReadableMessage = "Fraction numerator and denominator can be divided by common factor ?"}
		};

		public static void PrintDebug(string msg)
		{
			if (DEBUG)
			{
				Console.ForegroundColor = ConsoleColor.Black;
				Console.WriteLine("- " + msg + " ↓");
				Console.ResetColor();
			}
		}
		public static FunctionResult ExecuteFunction(Function function)
		{
			if (function.functionType == FunctionType.PRIMITIVE)
			{
				return new FunctionResult { collapsed = false, function = function };
			}
			else
			{
				var results = new List<FunctionResult>();
				var newFunction = function.Clone();

				// Multiply quantity into group
				if (newFunction.quantity != 1 && new FunctionType[] { FunctionType.ADD, FunctionType.DIV }.Contains(newFunction.functionType))
				{
					var affectedFunctionIds = new int[newFunction.arguments.Count + 1];
					affectedFunctionIds[0] = newFunction.id;
					for (var i = 0; i < newFunction.arguments.Count; i++)
					{
						var argument = newFunction.arguments[i];
						argument.quantity *= newFunction.quantity;
						newFunction.arguments[i] = argument;
						affectedFunctionIds[i + 1] = argument.id;
						if (newFunction.functionType != FunctionType.ADD) break;
					}
					newFunction.quantity = 1;
					return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.DISTRIBUTE_QUANTITY_INTO_FUNCTION, beforeFunctionIds = affectedFunctionIds, afterFunctionIds = affectedFunctionIds } };
				}

				// Execute sub functions, break on collapse
				for (var i = 0; i < newFunction.arguments.Count; i++)
				{
					var result = ExecuteFunction(newFunction.arguments[i]);
					if (result.collapsed)
					{
						newFunction.arguments[i] = result.function;
						return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = result.functionCollapseInfo };
					}
					// Hoist out Matryoshka doll add and mul functions
					if ((newFunction.functionType == FunctionType.ADD || newFunction.functionType == FunctionType.MUL) && newFunction.functionType == result.function.functionType)
					{
						var oldId = newFunction.arguments[i].id;
						var argumentIds = new List<int>();
						newFunction.arguments.RemoveAt(i);
						for (var j = result.function.arguments.Count - 1; j >= 0; j--)
						{
							var toInsert = result.function.arguments[j].Clone();
							if (result.function.functionType == FunctionType.MUL && j == 0 && result.function.quantity != 1)
							{
								toInsert.quantity *= result.function.quantity;
							}
							newFunction.arguments.Insert(i, toInsert);
							argumentIds.Add(toInsert.id);
						}
						return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.HOIST_NESTED_FUNCTION_WITH_SAME_TYPE, beforeFunctionIds = new int[] { oldId }, afterFunctionIds = argumentIds.ToArray() } };
					}
					// Hoist quantity out into MUL function
					if (newFunction.functionType == FunctionType.MUL && result.function.quantity != 1)
					{
						newFunction.quantity *= result.function.quantity;
						result.function.quantity = 1;
						newFunction.arguments[i] = result.function;
						return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.EXTRACT_ARGUMENT_QUANTITY_INTO_FUNCTION, beforeFunctionIds = new int[] { newFunction.id, result.function.id }, afterFunctionIds = new int[] { newFunction.id } } };
					}
					results.Add(result);
				}

				// If the function itself only has one child, hoist it
				if (newFunction.arguments.Count == 1)
				{
					var result = results[0].function.Clone();
					result.quantity *= newFunction.quantity;
					return new FunctionResult { collapsed = true, function = result, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.HOIST_SINGLE_ARGUMENT_INTO_PARENT, beforeFunctionIds = new int[] { result.id }, afterFunctionIds = new int[] { result.id } } };
				}
				switch (function.functionType)
				{
					case FunctionType.ADD: return Add(newFunction);
					case FunctionType.MUL: return Mul(newFunction);
					case FunctionType.DIV: return Div(newFunction);
					case FunctionType.EXPONENTIAL: return Exp(newFunction);
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
					if (argument1.quantity == 0)
					{
						function.arguments.RemoveAt(i);
						return new FunctionResult { collapsed = true, function = function, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.ELIMINATE_ZERO_TERMS, beforeFunctionIds = new int[] { argument1.id } } };
					}
					if (argument1.functionType == FunctionType.DIV && argument2.functionType == FunctionType.DIV && CalculateResultHashes(argument1.arguments[1]).exactHash == CalculateResultHashes(argument2.arguments[1]).exactHash)
					{
						var numeratorIds = new int[] { argument1.arguments[0].id, argument2.arguments[0].id };
						function.arguments[i].arguments[0] = FunctionArguments(1, FunctionType.ADD, argument1.arguments[0].Clone(), argument2.arguments[0].Clone());
						function.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = function, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.COMBINE_DIV_WITH_MATCHING_DENOMINATOR, beforeFunctionIds = numeratorIds, afterFunctionIds = new int[] { function.arguments[i].arguments[0].id } } };
					}
					if (argument1Hash.addHash == CalculateResultHashes(argument2).addHash)
					{
						argument1.quantity += argument2.quantity;
						function.arguments[i] = argument1;
						function.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = function, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.COMBINE_ADDABLE_TERMS, beforeFunctionIds = new int[] { argument1.id, argument2.id }, afterFunctionIds = new int[] { argument1.id } } };
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
					if (IsPrimitiveNumber(argument2)) // Always multiply and combine primitive numbers
					{
						newFunction.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.MULTIPLY_PRIMITIVE_NUMBERS, beforeFunctionIds = new int[] { argument1.id, argument2.id }, afterFunctionIds = new int[] { argument1.id } } };
					}
					if (argument2.functionType == FunctionType.DIV) // Distribute into DIV
					{
						var functionCollapseInfo = new FunctionCollapseInfo { };
						if (argument1.functionType == FunctionType.DIV) // Div / Div - Straight multiply to avoid convergence loop
						{
							newFunction.arguments[i] = FunctionArguments(argument1.quantity, FunctionType.DIV,
								FunctionArguments(1, FunctionType.MUL, argument1.arguments[0].Clone(), argument2.arguments[0].Clone()),
								FunctionArguments(1, FunctionType.MUL, argument1.arguments[1].Clone(), argument2.arguments[1].Clone())
							);
							functionCollapseInfo.functionCollapseType = FunctionCollapseType.MULTIPLY_DIV_FUNCTIONS;
							functionCollapseInfo.beforeFunctionIds = new int[] { argument1.id, argument2.id };
							functionCollapseInfo.afterFunctionIds = new int[] { newFunction.arguments[i].id };
						}
						else
						{
							var newNumerator = FunctionArguments(1, FunctionType.MUL, argument1.Clone(), argument2.arguments[0]);
							newFunction.arguments[i] = FunctionArguments(argument1.quantity, FunctionType.DIV,
								newNumerator,
								argument2.arguments[1]
							);
							functionCollapseInfo.functionCollapseType = FunctionCollapseType.MULTIPLY_INTO_DIV_NUMERATOR;
							functionCollapseInfo.beforeFunctionIds = new int[] { argument1.id, argument2.arguments[0].id };
							functionCollapseInfo.afterFunctionIds = new int[] { newNumerator.id };
						}
						newFunction.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = functionCollapseInfo };
					}
					if (argument1.functionType == FunctionType.ADD) // Distribute across add function
					{
						var beforeFunctionIds = new int[argument1.arguments.Count + 1];
						var afterFunctionIds = new int[argument1.arguments.Count];
						beforeFunctionIds[0] = argument2.id;
						for (var argIndex = 0; argIndex < argument1.arguments.Count; argIndex++)
						{
							beforeFunctionIds[argIndex + 1] = argument1.arguments[argIndex].id;
							argument1.arguments[argIndex] = FunctionArguments(argument1.quantity, FunctionType.MUL,
								argument1.arguments[argIndex].Clone(true),
								argument2.Clone(true)
							);
							afterFunctionIds[argIndex] = argument1.arguments[argIndex].id;
						}
						newFunction.arguments[i] = argument1;
						newFunction.arguments.RemoveAt(j);
						return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.DISTRIBUTE_FUNCTION_INTO_ADD_ARGUMENTS, beforeFunctionIds = beforeFunctionIds, afterFunctionIds = afterFunctionIds } };
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
						return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.MULTIPLY_EXPONENTS, beforeFunctionIds = new int[] { argument1.id, argument2.id }, afterFunctionIds = new int[] { newFunction.arguments[i].id } } };
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
				return new FunctionResult { collapsed = true, function = function, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.MULTIPLY_OUT_NESTED_EXPONENTS, beforeFunctionIds = new int[] { exponent.arguments[0].id, exponent.arguments[1].id }, afterFunctionIds = new int[] { newFunction.id } } };
			}
			else if (expBase.functionType == FunctionType.EXPONENTIAL)
			{ // Fold down nested exponents
				var newExponent = FunctionArguments(1, FunctionType.MUL, expBase.arguments[1].Clone(), exponent.Clone());
				function.arguments[1] = newExponent;
				function.arguments[0] = expBase.arguments[0].Clone();
				return new FunctionResult { collapsed = true, function = function, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.MULTIPLY_OUT_NESTED_EXPONENTS, beforeFunctionIds = new int[] { expBase.arguments[1].id, exponent.id }, afterFunctionIds = new int[] { newExponent.id } } };
			}
			if (exponent.functionType == FunctionType.PRIMITIVE && exponent.symbol == Symbol.NUMBER)
			{
				if (exponent.quantity == 0)
				{
					var newPrimitive = FunctionPrimitive(function.quantity);
					return new FunctionResult { collapsed = true, function = newPrimitive, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.EXPONENT_IS_ZERO, beforeFunctionIds = new int[] { function.id }, afterFunctionIds = new int[] { newPrimitive.id } } };
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
					return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.CONVERT_NEGATIVE_EXPONENT_TO_RECIPROCAL, beforeFunctionIds = new int[] { exponent.id }, afterFunctionIds = new int[] { newFunction.id } } };
				}
				else if (!(expBase.functionType == FunctionType.PRIMITIVE && expBase.symbol != Symbol.NUMBER) || exponent.quantity == 1)
				{
					if (exponent.quantity > 0)
					{
						var newFunctionIds = new List<int>();
						var newFunction = FunctionArguments(function.quantity, FunctionType.MUL);
						for (var i = 0; i < exponent.quantity; i++)
						{
							var functionBase = function.arguments[0].Clone(true);
							newFunction.arguments.Add(functionBase);
							newFunctionIds.Add(functionBase.id);
						}
						return new FunctionResult { collapsed = true, function = newFunction, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.CONVERT_NUMERIC_EXPONENT_TO_REPEATED_MUL, beforeFunctionIds = new int[] { exponent.id }, afterFunctionIds = newFunctionIds.ToArray() } };
					}
				}
			}
			// Root of primitive number
			else if (exponent.functionType == FunctionType.DIV)
			{
				if (exponent.arguments[1].functionType == FunctionType.PRIMITIVE && exponent.arguments[1].symbol == Symbol.NUMBER
					&& expBase.functionType == FunctionType.PRIMITIVE && expBase.symbol == Symbol.NUMBER)
				{
					var result = Math.Pow(expBase.quantity, 1d / exponent.arguments[1].quantity);
					if (Math.Floor(result) == result)
					{
						var newPrimitive = FunctionPrimitive((int)result);
						return new FunctionResult { collapsed = true, function = newPrimitive, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.SQUARE_ROOT_OF_PRIMITIVE_NUMBER, beforeFunctionIds = new int[] { expBase.id }, afterFunctionIds = new int[] { newPrimitive.id } } };
					}
					return new FunctionResult { collapsed = false, function = function };
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
				return new FunctionResult { collapsed = true, function = numerator, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.COLLAPSE_DIV_WITH_DENOMINATOR_1, beforeFunctionIds = new int[] { denominator.id }, afterFunctionIds = new int[] { } } };
			}
			if (numerator.functionType == FunctionType.PRIMITIVE && numerator.symbol == Symbol.NUMBER && numerator.quantity == 0)
			{
				return new FunctionResult { collapsed = true, function = numerator, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.COLLAPSE_DIV_WITH_NUMERATOR_0, beforeFunctionIds = new int[] { numerator.id }, afterFunctionIds = new int[] { numerator.id } } };
			}
			// function / Div = Multiply by reciprocal
			var numeratorIsDiv = numerator.functionType == FunctionType.DIV;
			var denominatorIsDiv = denominator.functionType == FunctionType.DIV;
			if (numeratorIsDiv || denominatorIsDiv)
			{
				var newFunction = FunctionArguments(function.quantity, FunctionType.MUL,
					FunctionArguments(1, FunctionType.DIV,
						numeratorIsDiv ? numerator.arguments[0] : numerator,
						numeratorIsDiv ? numerator.arguments[1] : FunctionPrimitive(1)
					),
					FunctionArguments(1, FunctionType.DIV,
						denominatorIsDiv ? denominator.arguments[1] : FunctionPrimitive(1),
						denominatorIsDiv ? denominator.arguments[0] : denominator
					)
				);
				return new FunctionResult
				{
					collapsed = true,
					function = newFunction,
					functionCollapseInfo = new FunctionCollapseInfo
					{
						functionCollapseType = FunctionCollapseType.CONVERT_NESTED_DIV_TO_RECIPROCAL_MUL,
						beforeFunctionIds = new int[] { denominator.id },
						afterFunctionIds = new int[] { newFunction.arguments[1].id }
					}
				};
			}
			if (CalculateResultHashes(numerator).exactHash == CalculateResultHashes(denominator).exactHash)
			{
				var newPrimitive = FunctionPrimitive(1);
				return new FunctionResult { collapsed = true, function = newPrimitive, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.COLLAPSE_VALUE_DIVIDED_BY_ITSELF, beforeFunctionIds = new int[] { numerator.id, denominator.id }, afterFunctionIds = new int[] { newPrimitive.id } } };
			}

			foreach (var prime in primesGenerated)
			{
				var primePrimitive = FunctionPrimitive(prime);
				var tryNumerator = DivInternal(numerator, primePrimitive);
				var tryDenominator = DivInternal(denominator, primePrimitive);
				if (tryNumerator.collapsed && tryDenominator.collapsed)
				{
					function.arguments[0] = tryNumerator.remainder;
					function.arguments[1] = tryDenominator.remainder;
					return new FunctionResult { collapsed = true, function = function, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.DIV_NUMERATOR_DENOMINATOR_COMMON_FACTOR, beforeFunctionIds = new int[] { numerator.id, denominator.id }, afterFunctionIds = new int[] { function.arguments[0].id, function.arguments[1].id }, additionalInfo = primePrimitive } };
				}
			}
			foreach (Symbol symbol in Enum.GetValues(typeof(Symbol)))
			{
				var symbolPrimitive = FunctionPrimitive(1, symbol);
				var tryNumerator = DivInternal(numerator, symbolPrimitive);
				var tryDenominator = DivInternal(denominator, symbolPrimitive);
				if (tryNumerator.collapsed && tryDenominator.collapsed && tryNumerator.divisor.symbol == tryDenominator.divisor.symbol)
				{
					function.arguments[0] = tryNumerator.remainder;
					function.arguments[1] = tryDenominator.remainder;
					return new FunctionResult { collapsed = true, function = function, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.DIV_NUMERATOR_DENOMINATOR_COMMON_FACTOR, beforeFunctionIds = new int[] { numerator.id, denominator.id }, afterFunctionIds = new int[] { function.arguments[0].id, function.arguments[1].id }, additionalInfo = symbolPrimitive } };
				}
			}

			if (numerator.functionType == FunctionType.ADD) // If even one term in the numerator add function is divisible, split into ADD(DIV + DIV)
			{
				for (var i = 0; i < numerator.arguments.Count; i++)
				{
					var result = Div(FunctionArguments(1, FunctionType.DIV, numerator.arguments[i], denominator));
					if (result.collapsed)
					{
						var numeratorClone = numerator.Clone();
						numeratorClone.arguments.RemoveAt(i);
						var lhs = FunctionArguments(1, FunctionType.DIV, numerator.arguments[i].Clone(true), denominator.Clone());
						var rhs = FunctionArguments(1, FunctionType.DIV, numeratorClone, denominator.Clone(true));
						var func = new FunctionResult
						{
							collapsed = true,
							function = FunctionArguments(1, FunctionType.ADD, lhs, rhs),
							functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.SPLIT_AND_DIVIDE_DIV, beforeFunctionIds = new int[] { numerator.arguments[i].id, denominator.id }, afterFunctionIds = new int[] { lhs.id, rhs.id } }
						};
						return func;
					}
				}
			}

			var dividedNumerator = DivInternal(numerator, denominator);

			if (dividedNumerator.collapsed)
			{
				function.arguments[0] = dividedNumerator.remainder;
				function.arguments[1] = DivInternal(denominator, dividedNumerator.divisor).remainder;
				return new FunctionResult { collapsed = true, function = function, functionCollapseInfo = dividedNumerator.functionCollapseInfo };
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
				return new DivisionResult { collapsed = true, remainder = clonedNumerator, divisor = FunctionPrimitive(clonedDenominator.quantity), functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.DIV_NUMERATOR_DENOMINATOR_COMMON_FACTOR, beforeFunctionIds = new int[] { numerator.id, denominator.id }, additionalInfo = clonedDenominator } };
			}
			if (clonedNumerator.functionType == FunctionType.PRIMITIVE && clonedDenominator.functionType == FunctionType.PRIMITIVE)
			{
				if (clonedNumerator.symbol != Symbol.NUMBER && clonedNumerator.symbol == clonedDenominator.symbol)
				{
					return new DivisionResult { collapsed = true, remainder = FunctionPrimitive(clonedNumerator.quantity), divisor = FunctionPrimitive(1, clonedNumerator.symbol), functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.DIV_NUMERATOR_DENOMINATOR_COMMON_FACTOR, beforeFunctionIds = new int[] { numerator.id, denominator.id }, additionalInfo = clonedNumerator } };
				}
				return new DivisionResult { collapsed = false };
			}
			else if (clonedNumerator.functionType == FunctionType.ADD)
			{
				var divisor = new Function();
				var functionCollapseInfo = new FunctionCollapseInfo();
				for (var i = 0; i < clonedNumerator.arguments.Count; i++)
				{
					var result = DivInternal(clonedNumerator.arguments[i], clonedDenominator);
					if (!result.collapsed)
					{
						return result;
					}
					else
					{
						functionCollapseInfo = result.functionCollapseInfo;
						clonedNumerator.arguments[i] = result.remainder;
						divisor = result.divisor;
					}
				}
				return new DivisionResult { collapsed = true, remainder = clonedNumerator, divisor = divisor, functionCollapseInfo = functionCollapseInfo };
			}
			else if (clonedNumerator.functionType == FunctionType.MUL)
			{
				for (var i = 0; i < clonedNumerator.arguments.Count; i++)
				{
					var result = DivInternal(clonedNumerator.arguments[i], clonedDenominator);
					if (result.collapsed)
					{
						clonedNumerator.arguments[i] = result.remainder;
						return new DivisionResult { collapsed = true, remainder = clonedNumerator, divisor = result.divisor, functionCollapseInfo = result.functionCollapseInfo };
					}
				}
				return new DivisionResult { collapsed = false, remainder = clonedNumerator };
			}
			else if (clonedNumerator.functionType == FunctionType.PRIMITIVE || clonedNumerator.functionType == FunctionType.EXPONENTIAL)
			{
				if (CalculateResultHashes(clonedNumerator).mulHash == CalculateResultHashes(clonedDenominator).mulHash)
				{
					var numeratorExponentContents = numerator.functionType == FunctionType.EXPONENTIAL ? numerator.arguments[1].Clone(true) : FunctionPrimitive(1);
					var denominatorExponentContents = denominator.functionType == FunctionType.EXPONENTIAL ? denominator.arguments[1].Clone(true) : FunctionPrimitive(1);
					denominatorExponentContents.quantity *= -1;
					var newExponent = FunctionArguments(1, FunctionType.ADD, numeratorExponentContents, denominatorExponentContents);
					var newBase = numerator.functionType == FunctionType.EXPONENTIAL ? numerator.arguments[0] : numerator;
					newBase.quantity = 1;
					var newExponential = FunctionArguments(numerator.quantity, FunctionType.EXPONENTIAL, newBase, newExponent);
					clonedDenominator.quantity = 1;
					return new DivisionResult { collapsed = true, remainder = newExponential, divisor = clonedDenominator, functionCollapseInfo = new FunctionCollapseInfo { functionCollapseType = FunctionCollapseType.DIV_NUMERATOR_DENOMINATOR_COMMON_FACTOR, beforeFunctionIds = new int[] { numerator.id, denominator.id }, additionalInfo = clonedDenominator } };
				}
				return new DivisionResult { collapsed = false, remainder = clonedNumerator };
			}

			return new DivisionResult { collapsed = false, remainder = clonedNumerator };
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
				functionType = FunctionType.PRIMITIVE,
				id = Functions.nextFunctionId++
			};
		}

		public static Function FunctionArguments(int quantity, FunctionType functionType, params Function[] arguments)
		{
			return new Function
			{
				quantity = quantity,
				functionType = functionType,
				arguments = new List<Function>(arguments),
				id = Functions.nextFunctionId++
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
				if (i == 1 && function.functionType == FunctionType.DIV)
				{
					resultHashes.addHash += subResults.exactHash;
				}
			}
			if (function.functionType == FunctionType.EXPONENTIAL)
			{
				resultHashes.addHash = resultHashes.exactHash;
			}
			return resultHashes;
		}

		public static string PrintFunctionsWithoutColors(Function function)
		{
			return PrintFunctions(function, new int[0], FunctionCollapseType.DISTRIBUTE_QUANTITY_INTO_FUNCTION);
		}

		public static string PrintFunctions(Function function, int[] affectedFunctionIds, FunctionCollapseType functionCollapseType)
		{
			var output = "";
			var modifiedMatched = false;
			var toInspect = new List<Printable> { new Printable { function = function } };
			while (toInspect.Count > 0)
			{
				var current = toInspect[toInspect.Count - 1];
				toInspect.RemoveAt(toInspect.Count - 1);
				if (current.isString)
				{
					output += current.stringValue;
				}
				else
				{
					var affectedFunction = affectedFunctionIds != null && affectedFunctionIds.Contains(current.function.id);
					var collapseDocumentation = Functions.collapseTypeDocumentation[(int)functionCollapseType];
					if (current.function.functionType == FunctionType.PRIMITIVE)
					{
						if (affectedFunction) output += "__MODIFIED__";
						output += current.function.quantity.ToString();
						if (affectedFunction && collapseDocumentation.affectsQuantity) output += "__MODIFIED__";
						if (current.function.symbol != Symbol.NUMBER) output += current.function.symbol;
						if (affectedFunction && !collapseDocumentation.affectsQuantity) output += "__MODIFIED__";
					}
					else
					{
						if (affectedFunction && !collapseDocumentation.affectsQuantity)
						{
							output += "__MODIFIED__";
							toInspect.Add(new Printable { isString = true, stringValue = "__MODIFIED__" });
						}
						toInspect.Add(new Printable { isString = true, stringValue = ")" });
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
						toInspect.Add(new Printable { isString = true, stringValue = $"(" });
						if (affectedFunction && collapseDocumentation.affectsQuantity)
						{
							toInspect.Add(new Printable { isString = true, stringValue = "__MODIFIED__" });
						}
						toInspect.Add(new Printable { isString = true, stringValue = $"{current.function.quantity}" });
						if (affectedFunction && collapseDocumentation.affectsQuantity)
						{
							toInspect.Add(new Printable { isString = true, stringValue = "__MODIFIED__" });
						}
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
		NONE,
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
		public int id;

		public Function Clone(bool newId = false)
		{
			var newArguments = new List<Function>();
			if (arguments != null)
			{
				foreach (var argument in arguments)
				{
					newArguments.Add(argument.Clone(newId));
				}
			}
			return new Function
			{
				arguments = newArguments,
				quantity = this.quantity,
				functionType = this.functionType,
				symbol = this.symbol,
				id = newId ? Functions.nextFunctionId++ : this.id
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
		public FunctionCollapseInfo functionCollapseInfo;
	}

	public enum FunctionCollapseType : byte
	{
		DISTRIBUTE_QUANTITY_INTO_FUNCTION,
		HOIST_NESTED_FUNCTION_WITH_SAME_TYPE,
		EXTRACT_ARGUMENT_QUANTITY_INTO_FUNCTION,
		HOIST_SINGLE_ARGUMENT_INTO_PARENT,
		ELIMINATE_ZERO_TERMS,
		COMBINE_DIV_WITH_MATCHING_DENOMINATOR,
		COMBINE_ADDABLE_TERMS,
		MULTIPLY_PRIMITIVE_NUMBERS,
		MULTIPLY_DIV_FUNCTIONS,
		MULTIPLY_INTO_DIV_NUMERATOR,
		DISTRIBUTE_FUNCTION_INTO_ADD_ARGUMENTS,
		MULTIPLY_EXPONENTS,
		MULTIPLY_OUT_NESTED_EXPONENTS,
		EXPONENT_IS_ZERO,
		CONVERT_NEGATIVE_EXPONENT_TO_RECIPROCAL,
		CONVERT_NUMERIC_EXPONENT_TO_REPEATED_MUL,
		SQUARE_ROOT_OF_PRIMITIVE_NUMBER,
		COLLAPSE_DIV_WITH_DENOMINATOR_1,
		COLLAPSE_DIV_WITH_NUMERATOR_0,
		CONVERT_NESTED_DIV_TO_RECIPROCAL_MUL,
		COLLAPSE_VALUE_DIVIDED_BY_ITSELF,
		SPLIT_AND_DIVIDE_DIV,
		DIV_NUMERATOR_DENOMINATOR_COMMON_FACTOR,
	}

	public struct FunctionCollapseTypeDocumentation
	{
		public FunctionCollapseType functionCollapseType;
		public string devMessage;
		public string humanReadableMessage;
		public bool affectsQuantity;
		public bool internalOnly;
		public bool hasAdditionalInfo;
	}

	struct FunctionCollapseInfo
	{
		public int[] beforeFunctionIds;
		public int[] afterFunctionIds;
		public FunctionCollapseType functionCollapseType;
		public Function additionalInfo;
	}

	struct FunctionResult
	{
		public bool collapsed;
		public Function function;
		public FunctionCollapseInfo functionCollapseInfo;
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