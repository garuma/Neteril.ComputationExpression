using System;

namespace Neteril.ComputationExpression
{
	// Follows the type signature of
	// https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions
	public interface IMonadExpressionBuilder
	{
		Monad<T> Bind<U, T> (Monad<U> m, Func<U, Monad<T>> f);
		Monad<T> Return<T> (T v);
		Monad<T> Zero<T> ();
		Monad<T> Combine<T> (Monad<T> m, Monad<T> n);
	}
}
