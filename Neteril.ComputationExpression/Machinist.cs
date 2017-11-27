using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Neteril.ComputationExpression
{
	static class Machinist<TStateMachine> where TStateMachine : IAsyncStateMachine
	{
		const string StateFieldName = "<>1__state";
		const string AwaiterFieldName = "<>u__1";

		delegate int StateFieldGetterDelegate (TStateMachine stateMachine);
		delegate void StateFieldSetterDelegate (TStateMachine stateMachine, int state);
		delegate void AwaiterFieldSetterDelegate (TStateMachine stateMachine, object awaiter);

		static StateFieldGetterDelegate stateGetter;
		static StateFieldSetterDelegate stateSetter;
		static AwaiterFieldSetterDelegate awaiterSetter;

		public static int GetState (TStateMachine stateMachine)
		{
			if (stateGetter == null) {
				var stateField = stateMachine
					.GetType ()
					.GetField (StateFieldName, BindingFlags.Instance | BindingFlags.Public);
				var dynamicGetter = new DynamicMethod ("__MagicSpecialGetState" + typeof (TStateMachine).Name,
				                                       typeof (int),
				                                       new[] { typeof (TStateMachine) },
				                                       restrictedSkipVisibility: true);
				var generator = dynamicGetter.GetILGenerator ();
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldfld, stateField);
				generator.Emit (OpCodes.Ret);
				stateGetter = (StateFieldGetterDelegate)dynamicGetter.CreateDelegate (typeof (StateFieldGetterDelegate));
			}
			return stateGetter (stateMachine);
		}

		public static void ResetMachine (TStateMachine stateMachine, int state, object awaiter)
		{
			if (stateSetter == null) {
				var stateField = stateMachine
					.GetType ()
					.GetField (StateFieldName, BindingFlags.Instance | BindingFlags.Public);
				var dynamicSetter = new DynamicMethod ("__MagicSpecialSetState" + typeof (TStateMachine).Name,
				                                       null,
				                                       new[] { typeof (TStateMachine), typeof (int) },
													   restrictedSkipVisibility: true);
				EmitSetField (dynamicSetter.GetILGenerator (), stateField);
				stateSetter = (StateFieldSetterDelegate)dynamicSetter.CreateDelegate (typeof (StateFieldSetterDelegate));

				var awaiterField = stateMachine
					.GetType ()
					.GetField (AwaiterFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				dynamicSetter = new DynamicMethod ("__MagicSpecialSetAwaiter" + typeof (TStateMachine).Name,
				                                   null,
				                                   new[] { typeof (TStateMachine), typeof (object) },
				                                   restrictedSkipVisibility: true);
				EmitSetField (dynamicSetter.GetILGenerator (), awaiterField);
				awaiterSetter = (AwaiterFieldSetterDelegate)dynamicSetter.CreateDelegate (typeof (AwaiterFieldSetterDelegate));
			}
			stateSetter (stateMachine, state);
			awaiterSetter (stateMachine, awaiter);
		}

		static void EmitSetField (ILGenerator generator, FieldInfo field)
		{
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Stfld, field);
			generator.Emit (OpCodes.Ret);
		}
	}
}
