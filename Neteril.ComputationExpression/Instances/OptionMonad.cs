using System;

namespace Neteril.ComputationExpression.Instances
{
	public abstract class Option<T> : IMonad<T> { }

	public sealed class None<T> : Option<T> { public static readonly None<T> Value = new None<T> (); }
	public sealed class Some<T> : Option<T>
	{
		public readonly T Item;
		public Some (T item) => Item = item;
		public static explicit operator T (Some<T> option) => option.Item;
	}

	public static class Some
	{
		public static Some<T> Of<T> (T value) => new Some<T> (value);
	}

	public class OptionExpressionBuilder : IMonadExpressionBuilder
	{
		IMonad<T> IMonadExpressionBuilder.Bind<U, T> (IMonad<U> m, Func<U, IMonad<T>> f)
		{
			switch ((Option<U>)m) {
				case Some<U> some:
					return f (some.Item);
				case None<U> none:
				default:
					return None<T>.Value;
			}
		}

		IMonad<T> IMonadExpressionBuilder.Return<T> (T v) => Some.Of (v);

		IMonad<T> IMonadExpressionBuilder.Zero<T> () => None<T>.Value;

		IMonad<T> IMonadExpressionBuilder.Combine<T> (IMonad<T> m, IMonad<T> n) => throw new NotSupportedException ();
	}
}
