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

		delegate int StateFieldGetterDelegate (ref TStateMachine stateMachine);
		delegate void StateFieldSetterDelegate (ref TStateMachine stateMachine, int state);
		delegate void AwaiterFieldSetterDelegate (ref TStateMachine stateMachine, object awaiter);

		static StateFieldGetterDelegate stateGetter;
		static StateFieldSetterDelegate stateSetter;
		static AwaiterFieldSetterDelegate awaiterSetter;

		/* In Debug mode TStateMachine will be a class and in Release mode
		 * it will be a struct. This means we have to slightly vary the IL
		 * we generate to add an load indirection when the machine is a class
		 */
		static bool IsMachineStruct => typeof (TStateMachine).IsValueType;

		public static int GetState (ref TStateMachine stateMachine)
		{
			if (stateGetter == null) {
				var stateField = stateMachine
					.GetType ()
					.GetField (StateFieldName, BindingFlags.Instance | BindingFlags.Public);
				var dynamicGetter = new DynamicMethod ("__MagicSpecialGetState" + typeof (TStateMachine).Name,
				                                       typeof (int),
				                                       new[] { typeof (TStateMachine).MakeByRefType () },
				                                       restrictedSkipVisibility: true);
				var generator = dynamicGetter.GetILGenerator ();
				EmitGetField (generator, stateField, IsMachineStruct);
				stateGetter = (StateFieldGetterDelegate)dynamicGetter.CreateDelegate (typeof (StateFieldGetterDelegate));
			}
			return stateGetter (ref stateMachine);
		}

		public static void Reset (ref TStateMachine stateMachine, int state, object awaiter)
		{
			if (stateSetter == null) {
				var stateField = stateMachine
					.GetType ()
					.GetField (StateFieldName, BindingFlags.Instance | BindingFlags.Public);
				var dynamicSetter = new DynamicMethod ("__MagicSpecialSetState" + typeof (TStateMachine).Name,
				                                       null,
				                                       new[] { typeof (TStateMachine).MakeByRefType (), typeof (int) },
				                                       restrictedSkipVisibility: true);
				EmitSetField (dynamicSetter.GetILGenerator (), stateField, IsMachineStruct);
				stateSetter = (StateFieldSetterDelegate)dynamicSetter.CreateDelegate (typeof (StateFieldSetterDelegate));

				var awaiterField = stateMachine
					.GetType ()
					.GetField (AwaiterFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				dynamicSetter = new DynamicMethod ("__MagicSpecialSetAwaiter" + typeof (TStateMachine).Name,
				                                   null,
				                                   new[] { typeof (TStateMachine).MakeByRefType (), typeof (object) },
				                                   restrictedSkipVisibility: true);
				EmitSetField (dynamicSetter.GetILGenerator (), awaiterField, IsMachineStruct);
				awaiterSetter = (AwaiterFieldSetterDelegate)dynamicSetter.CreateDelegate (typeof (AwaiterFieldSetterDelegate));
			}
			stateSetter (ref stateMachine, state);
			awaiterSetter (ref stateMachine, awaiter);
		}

		static void EmitGetField (ILGenerator generator, FieldInfo field, bool isMachineStruct)
		{
			generator.Emit (OpCodes.Ldarg_0);
			if (!isMachineStruct)
				generator.Emit (OpCodes.Ldind_Ref);
			generator.Emit (OpCodes.Ldfld, field);
			generator.Emit (OpCodes.Ret);
		}

		static void EmitSetField (ILGenerator generator, FieldInfo field, bool isMachineStruct)
		{
			generator.Emit (OpCodes.Ldarg_0);
			if (!isMachineStruct)
				generator.Emit (OpCodes.Ldind_Ref);
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Stfld, field);
			generator.Emit (OpCodes.Ret);
		}
	}
}
