using System;
using System.Threading;
using NUnit.Framework;

using Neteril.ComputationExpression.Instances;
using static Neteril.ComputationExpression.Operators;
using static Neteril.ComputationExpression.Instances.Cancellable;

namespace Neteril.ComputationExpression.Tests
{
	[TestFixture]
	public class CancellableTests
	{
		[Test]
		public void ReturnSimpleValue ()
		{
			var (result, wasCanceled) = CxRun<int, CancellableMonad<int>> (CancellableExpressionBuilder.Instance, async () => {
				return await CancellableMonad<int>.FromValue (3);
			}).Run (CancellationToken.None);
			Assert.IsFalse (wasCanceled);
			Assert.AreEqual (3, result);
		}

		[Test]
		public void ReturnAfterOperations ()
		{
			var (result, wasCanceled) = CxRun<int, CancellableMonad<int>> (CancellableExpressionBuilder.Instance, async () => {
				var op = await CancellableMonad<int>.FromValue (1);
				op = await MaybeDo (() => op + 1);
				op = await MaybeDo (() => op + 10);
				return op;
			}).Run (CancellationToken.None);
			Assert.IsFalse (wasCanceled);
			Assert.AreEqual (12, result);
		}

		[Test]
		public void Cancelled_BeforeUse ()
		{
			var source = new CancellationTokenSource ();
			source.Cancel ();
			var (result, wasCanceled) = CxRun<int, CancellableMonad<int>> (CancellableExpressionBuilder.Instance, async () => {
				var r = await CancellableMonad<int>.FromValue (42);
				return r;
			}).Run (source.Token);
			Assert.IsTrue (wasCanceled);
		}

		[Test]
		public void Cancelled_Between ()
		{
			var source = new CancellationTokenSource ();
			var (result, wasCanceled) = CxRun<int, CancellableMonad<int>> (CancellableExpressionBuilder.Instance, async () => {
				var op = await CancellableMonad<int>.FromValue (1);
				op = await MaybeDo (() => op + 1);
				source.Cancel ();
				op = await MaybeDo (() => op + 10);
				return op;
			}).Run (source.Token);
			Assert.IsTrue (wasCanceled);
			Assert.AreEqual (2, result);
		}
	}
}
