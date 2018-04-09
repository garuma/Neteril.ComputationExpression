using System;

namespace Neteril.ComputationExpression.Instances
{
	public class State<TState, TValue> : IMonad<TValue>
	{
		Func<TState, (TValue, TState)> stateProcessor;

		public State (Func<TState, (TValue, TState)> stateProcessor)
		{
			this.stateProcessor = stateProcessor;
		}

		public (TValue value, TState state) RunState (TState state) => stateProcessor (state);
	}

	public static class State
	{
		public static State<TState, TValue> Put<TState, TValue> (TState state)
		=> new State<TState, TValue> (_ => (default(TValue), state));

		public static State<TState, TState> Get<TState> ()
			=> new State<TState, TState> (s => (s, s));

		public static TValue EvalState<TState, TValue> (State<TState, TValue> stateMonad, TState state)
			=> stateMonad.RunState (state).value;

		public static TState ExecState<TState, TValue> (State<TState, TValue> stateMonad, TState state)
			=> stateMonad.RunState (state).state;
	}

	public class StateExpressionBuilder<TState> : IMonadExpressionBuilder
	{
		public static readonly StateExpressionBuilder<TState> Instance = new StateExpressionBuilder<TState> ();

		IMonad<T> IMonadExpressionBuilder.Bind<U, T> (IMonad<U> m, Func<U, IMonad<T>> f)
		{
			var previousStateMonad = ((State<TState, U>)m);
			return new State<TState, T> (s => {
				var (value, newState) = previousStateMonad.RunState (s);
				var nextMonad = (State<TState, T>)f (value);
				return nextMonad.RunState (newState);
			});
		}

		IMonad<T> IMonadExpressionBuilder.Return<T> (T v) => new State<TState, T> (s => (v, s));

		IMonad<T> IMonadExpressionBuilder.Zero<T> () => new State<TState, T> (s => (default (T), s));

		IMonad<T> IMonadExpressionBuilder.Combine<T> (IMonad<T> m, IMonad<T> n) => throw new NotSupportedException ();
	}
}
