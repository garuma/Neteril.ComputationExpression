using System;

namespace Neteril.ComputationExpression
{
	/// <summary>
	/// Use this class with a `using static` statement to have a shorter
	/// set of "operator" to work with instead of using the full length
	/// methods in <see cref="ComputationExpression"/>
	/// </summary>
	public static class Operators
	{
		public static TMonad CxRun<T, TMonad> (IMonadExpressionBuilder builder, Func<IMonad<T>> body)
			where TMonad : IMonad<T>
		{
			return ComputationExpression.Run<T, TMonad> (builder, body);
		}

		public static CombineAwaitable<T> CxYield<T> (T value) => ComputationExpression.Yield<T> (value);
	}
}
