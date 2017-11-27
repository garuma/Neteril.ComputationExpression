using System;
using System.Runtime.CompilerServices;

namespace Neteril.ComputationExpression
{
	// Our awaiter simply acts as a mutable holder for the bind state
	public class MonadAwaiter<T> : INotifyCompletion
	{
		Monad<T> monad;
		T result;

		public MonadAwaiter (Monad<T> m)
		{
			this.monad = m;
		}

		// Helpers to get/set the intermediate results of Bind
		internal Monad<T> CurrentMonad => monad;
		internal void SetNextStep (T value) => this.result = value;

		public T GetResult () => result;

		/* We never want to turn on the async machinery optimization
		 * and instead continuously create continuations
		 */
		public bool IsCompleted => false;

		public void OnCompleted (Action continuation)
		{
			/* We never need to execute the continuation cause
			 * the async method builder drives everything.
			 */
		}
	}
}
