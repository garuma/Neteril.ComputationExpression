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
			var good = CxRun<int, Option<int>> (OptionExpressionBuilder.Instance, async () => {
				var val1 = await TryDivide (120, 2);
				var val2 = await TryDivide (val1, 3);
				var val3 = await TryDivide (val2, 2);

				return val3;
			});

			Assert.IsInstanceOf<Some<int>> (good);
			Assert.AreEqual (10, ((Some<int>)good).Item);
		}

		[Test]
		public void Bad ()
		{
			var bad = CxRun<int, Option<int>> (OptionExpressionBuilder.Instance, async () => {
				var val1 = await TryDivide (120, 2);
				var val2 = await TryDivide (val1, 0);
				var val3 = await TryDivide (val2, 2);

				return val3;
			});

			Assert.IsInstanceOf<None<int>> (bad);
		}

		static Option<int> TryDivide (int up, int down)
		{
			if (down == 0)
				return None<int>.Value;
			return Some.Of (up / down);
		}
	}
}
