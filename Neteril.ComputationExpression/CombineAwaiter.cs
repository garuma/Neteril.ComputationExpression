using System;
using System.Runtime.CompilerServices;

namespace Neteril.ComputationExpression
{
	public struct CombineAwaitable<T>
	{
		T yieldedValue;
		public CombineAwaitable (T yieldedValue) => this.yieldedValue = yieldedValue;
		public CombineAwaiter<T> GetAwaiter () => new CombineAwaiter<T> (yieldedValue);
	}

	public class CombineAwaiter<T> : INotifyCompletion
	{
		T yieldedValue;
		public CombineAwaiter (T yieldedValue) => this.yieldedValue = yieldedValue;
		internal T YieldedValue => yieldedValue;

		public T GetResult () => YieldedValue;

		public bool IsCompleted => false;

		public void OnCompleted (Action continuation)
		{
		}
	}
}
