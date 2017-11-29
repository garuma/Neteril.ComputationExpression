using System;
using System.Runtime.CompilerServices;

namespace Neteril.ComputationExpression
{
	[AsyncMethodBuilder (typeof (MonadAsyncMethodBuilder<>))]
	public interface IMonad<T>
	{
	}

	public static class MonadExtensions
	{
		public static MonadAwaiter<T> GetAwaiter<T> (this IMonad<T> monad) => new MonadAwaiter<T> (monad);
	}
}
