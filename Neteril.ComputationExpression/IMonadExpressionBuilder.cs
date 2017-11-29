using System;

namespace Neteril.ComputationExpression
{
	// Follows the type signature of
	// https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions
	public interface IMonadExpressionBuilder
	{
		IMonad<T> Bind<U, T> (IMonad<U> m, Func<U, IMonad<T>> f);
		IMonad<T> Return<T> (T v);
		IMonad<T> Zero<T> ();
		IMonad<T> Combine<T> (IMonad<T> m, IMonad<T> n);
	}
}
