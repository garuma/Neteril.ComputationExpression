using System;
using NUnit.Framework;

using static Neteril.ComputationExpression.Operators;
using Neteril.ComputationExpression.Instances;

namespace Neteril.ComputationExpression.Tests
{
	[TestFixture]
	public class EnumerableTest
	{
		[Test]
		public void NestedYield ()
		{
			var result = CxRun<int, EnumerableMonad<int>> (EnumerableExpressionBuilder.Instance, async () => {
				var item = await (EnumerableMonad<int>)new[] { 1, 2, 3 };
				var item2 = await (EnumerableMonad<int>)new[] { 100, 200 };
				await CxYield (item);
				await CxYield (item2);
				return item * item2;
			});

			var expected = new[] {
				1, 100, 100,
				1, 200, 200,
				2, 100, 200,
				2, 200, 400,
				3, 100, 300,
				3, 200, 600
			};

			CollectionAssert.AreEqual (expected, result);
		}
	}
}
