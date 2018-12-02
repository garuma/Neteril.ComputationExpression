using System;
using NUnit.Framework;

using static Neteril.ComputationExpression.Operators;
using Neteril.ComputationExpression.Instances;

namespace Neteril.ComputationExpression.Tests
{
	[TestFixture]
	public class MaybeTest
	{
		[Test]
		public void Good ()
		{
			var called = false;
			var good = CxRun<int, Option<int>> (OptionExpressionBuilder.Instance, async () => {
				var val1 = await TryDivide (120, 2);
				var val2 = await TryDivide (val1, 3);
				called = true;
				var val3 = await TryDivide (val2, 2);

				return val3;
			});

			Assert.IsInstanceOf<Some<int>> (good);
			Assert.AreEqual (10, ((Some<int>)good).Item);
			Assert.IsTrue (called);
		}

		[Test]
		public void Bad ()
		{
			var called = false;
			var bad = CxRun<int, Option<int>> (OptionExpressionBuilder.Instance, async () => {
				var val1 = await TryDivide (120, 2);
				var val2 = await TryDivide (val1, 0);
				called = true;
				var val3 = await TryDivide (val2, 2);

				return val3;
			});

			Assert.IsInstanceOf<None<int>> (bad);
			Assert.IsFalse (called);
		}

		static Option<int> TryDivide (int up, int down)
		{
			if (down == 0)
				return None<int>.Value;
			return Some.Of (up / down);
		}

		[Test]
		public void Nested ()
		{
			var outer = CxRun<int, Option<int>> (OptionExpressionBuilder.Instance, async () => {
				var inner = await CxRun<int, Option<int>> (OptionExpressionBuilder.Instance, async () => {
					var i = await new Some<int> (42);
					return i;
				});
				return inner;
			});
			Assert.IsInstanceOf<Some<int>> (outer);
			Assert.AreEqual (42, ((Some<int>)outer).Item);
		}
	}
}
