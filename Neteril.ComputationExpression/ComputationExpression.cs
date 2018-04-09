using System;

namespace Neteril.ComputationExpression
{
	public static class ComputationExpression
	{
		[ThreadStatic]
		internal static IMonadExpressionBuilder CurrentBuilder = null;

		public static TMonad Run<T, TMonad> (IMonadExpressionBuilder builder, Func<IMonad<T>> body)
			where TMonad : IMonad<T>
		{
			var prevBuilder = CurrentBuilder;
			try {
				CurrentBuilder = builder;
				return (TMonad)body ();
			} finally {
				CurrentBuilder = prevBuilder;
			}
		}

		public static CombineAwaitable<T> Yield<T> (T value) => new CombineAwaitable<T> (value);
	}
}
