using System;
using System.Runtime.CompilerServices;

namespace Neteril.ComputationExpression
{
	public class MonadAsyncMethodBuilder<T>
	{
		IMonadExpressionBuilder builder;
		Monad<T> finalResult;
		System.Reflection.MethodInfo processBind;

		public static MonadAsyncMethodBuilder<T> Create ()
		{
			var builder = ComputationExpression.CurrentBuilder;
			if (builder == null)
				throw new NotSupportedException ($"Computation expression can only be run from {nameof (ComputationExpression)}.{nameof (ComputationExpression.Run)}");
			return new MonadAsyncMethodBuilder<T> (builder);
		}

		public MonadAsyncMethodBuilder (IMonadExpressionBuilder builder)
		{
			this.builder = builder;
			finalResult = builder.Zero<T> ();
			processBind = GetType ().GetMethod (
				"ProcessBind",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
			);
		}

		public void Start<TStateMachine> (ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
		{
			stateMachine.MoveNext ();
		}

		public Monad<T> Task => finalResult;

		public void SetStateMachine (IAsyncStateMachine stateMachine) { }
		public void SetResult (T result)
		{
			finalResult = builder.Return<T> (result);
		}

		public void SetException (Exception ex) { throw ex; }

		public void AwaitOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
			where TAwaiter : INotifyCompletion
			where TStateMachine : IAsyncStateMachine
		{
			if (!typeof (TAwaiter).IsGenericType)
				throw new InvalidOperationException ("Invalid awaiter given");

			if (typeof (TAwaiter).GetGenericTypeDefinition () == typeof (MonadAwaiter<>)) {
				/* Unfortunately we can't infer the U of MonadAwaiter<U>
				* from the constructed TAwaiter type that's given to us
				* so we have to resort to good old reflection.
				*/
				var monadUType = typeof (TAwaiter).GetGenericArguments ()[0];
				var monadAwaiter = awaiter;
				var stateMachineCopy = stateMachine;
				processBind.MakeGenericMethod (monadUType, typeof (TStateMachine))
				           .Invoke (this, new object[] { monadAwaiter, stateMachineCopy });
				return;
			}
			if (typeof (TAwaiter).GetGenericTypeDefinition () == typeof (CombineAwaiter<>)) {
				var yieldAwaiter = (CombineAwaiter<T>)(object)awaiter;
				var m = builder.Return (yieldAwaiter.YieldedValue);
				stateMachine.MoveNext ();
				this.finalResult = builder.Combine (m, finalResult);
				return;
			}

			throw new InvalidOperationException ("Invalid awaiter given");
		}

		void ProcessBind<U, TStateMachine> (MonadAwaiter<U> monadAwaiter, TStateMachine stateMachine)
			where TStateMachine : IAsyncStateMachine
		{
			var monad = monadAwaiter.CurrentMonad;
			var machineState = Machinist<TStateMachine>.GetState (stateMachine);
			var userMonad = builder.Bind<U, T> (monad, value => {
				/* If we are called that means we keep the
				 * control of the execution flow, no need
				 * to produce a monad instance of our own
				 * at that stage since it will be fed in
				 * later.
				 */
				monadAwaiter.SetNextStep (value);
				if (Machinist<TStateMachine>.GetState (stateMachine) != machineState)
					Machinist<TStateMachine>.ResetMachine (stateMachine, machineState, monadAwaiter);
				stateMachine.MoveNext ();
				return finalResult;
			});
			if (userMonad != null)
				this.finalResult = userMonad;
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
			where TAwaiter : ICriticalNotifyCompletion
			where TStateMachine : IAsyncStateMachine
		{
			throw new NotSupportedException ();
		}
	}
}
