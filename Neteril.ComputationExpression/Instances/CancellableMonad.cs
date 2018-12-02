using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neteril.ComputationExpression.Instances
{
	public class CancellableMonad<T> : IMonad<T>
	{
		Func<CancellationToken, (T, bool)> innerFunction;

		internal CancellableMonad (Func<CancellationToken, (T, bool)> innerFunction)
		{
			this.innerFunction = innerFunction;
		}

		public static CancellableMonad<T> FromValue (T value) => new CancellableMonad<T> (t => (value, false));

		public (T value, bool wasCanceled) Run (CancellationToken? token = null)
		{
			return innerFunction (token ?? CancellationToken.None);
		}
	}

	public static class Cancellable
	{
		public static CancellableMonad<T> MaybeDo<T> (Func<T> innerFunc)
		{
			return new CancellableMonad<T> (token => (innerFunc (), false));
		}
	}

	public class CancellableExpressionBuilder : IMonadExpressionBuilder
	{
		public static readonly CancellableExpressionBuilder Instance = new CancellableExpressionBuilder ();

		public IMonad<T> Bind<U, T> (IMonad<U> m, Func<U, IMonad<T>> f)
		{
			var previous = (CancellableMonad<U>)m;
			return new CancellableMonad<T> (token => {
				if (token.IsCancellationRequested)
					return (default (T), true);
				var (result, wasCanceled) = previous.Run (token);
				if (wasCanceled)
					return result is T compatible ? (compatible, true) : (default (T), true);
				var next = (CancellableMonad<T>)f (result);
				if (token.IsCancellationRequested)
					return result is T compatible ? (compatible, true) : (default (T), true);
				return next.Run (token);
			});
		}

		public IMonad<T> Return<T> (T v) => CancellableMonad<T>.FromValue (v);

		public IMonad<T> Zero<T> () => CancellableMonad<T>.FromValue (default (T));

		public IMonad<T> Combine<T> (IMonad<T> m, IMonad<T> n)
		{
			throw new NotImplementedException ();
		}
	}
}
