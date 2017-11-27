using System;
using System.Runtime.CompilerServices;

namespace Neteril.ComputationExpression
{
	[AsyncMethodBuilder (typeof (MonadAsyncMethodBuilder<>))]
	public abstract class Monad<T>
	{
		public MonadAwaiter<T> GetAwaiter () => new MonadAwaiter<T> (this);
	}
}
