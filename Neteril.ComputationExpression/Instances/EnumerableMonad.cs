using System;
using System.Linq;
using System.Collections.Generic;

namespace Neteril.ComputationExpression.Instances
{
	public class EnumerableMonad<T> : IMonad<T>, IEnumerable<T>
	{
		IEnumerable<T> seed;

		public EnumerableMonad (IEnumerable<T> seed) => this.seed = seed;

		public static implicit operator EnumerableMonad<T> (List<T> seed) => new EnumerableMonad<T> (seed);
		public static implicit operator EnumerableMonad<T> (T[] seed) => new EnumerableMonad<T> (seed);

		public IEnumerator<T> GetEnumerator () => seed.GetEnumerator ();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () => seed.GetEnumerator ();
	}

	public class EnumerableExpressionBuilder : IMonadExpressionBuilder
	{
		IMonad<T> IMonadExpressionBuilder.Bind<U, T> (IMonad<U> m, Func<U, IMonad<T>> f)
		{
			var previousEnumerableMonad = (EnumerableMonad<U>)m;
			return new EnumerableMonad<T> (previousEnumerableMonad.SelectMany (u => (EnumerableMonad<T>)f (u)));
		}

		IMonad<T> IMonadExpressionBuilder.Return<T> (T v) => new EnumerableMonad<T> (Enumerable.Repeat (v, 1));

		IMonad<T> IMonadExpressionBuilder.Zero<T> () => new EnumerableMonad<T> (Enumerable.Empty<T> ());

		IMonad<T> IMonadExpressionBuilder.Combine<T> (IMonad<T> m, IMonad<T> n)
		{
			var enumerableMonad1 = (EnumerableMonad<T>)m;
			var enumerableMonad2 = (EnumerableMonad<T>)n;
			return new EnumerableMonad<T> (enumerableMonad1.Concat (enumerableMonad2));
		}
	}
}
