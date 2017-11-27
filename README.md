# Neteril.ComputationExpression

<a href="https://www.nuget.org/packages/Neteril.ComputationExpression"><img src="https://img.shields.io/nuget/v/Neteril.ComputationExpression.svg" alt="NuGet" /></a>

A generic re-implementation of F# computation expressions in C# by (ab)using async/await.

TL;DR lifting the tricks of [my previous attempt](https://blog.neteril.org/blog/2017/04/26/maybe-computation-expression-csharp/) at borrowing F# computation expression concepts in C# into a generic form that can be reused for other builder types.

This code utilizes the F# definition of computation expression builders (or more precisely the subset of Bind/Return/Zero/Combine members) for a given monad type and plug it into a customized async/await method builder and awaiter system.

Basically the idea is to go from this kind of F#:

```fsharp
let divideBy bottom top =
	if bottom = 0
	then None
	else Some(top/bottom)

type MaybeBuilder() =
	member this.Bind(m, f) = Option.bind f m
	member this.Return(x) = Some x

let maybe = new MaybeBuilder()

let divideByWorkflow =
	maybe {
		let! a = 120 |> divideBy 2
		let! b = a |> divideBy 2
		let! c = b |> divideBy 2
		return c
	}
```

To this kind of C#:

```csharp
Option<int> TryDivide (int up, int down)
	=> return down == 0 ? None<int>.Value : Some.Of (up / down);

class MaybeBuilder : IMonadExpressionBuilder
{
	public Monad<T> Bind<T> (Monad<T> m, Func<T, Monad<T>> f)
	{
		switch ((Option<T>)m) {
			case Some<T> some: return f (some.Item);
			case None<T> none: return none;
			default: return None<T>.Value;
		}
	}
	public Monad<T> Return<T> (T v) => Some.Of (v);
	public Monad<T> Zero<T> () => None<T>.Value;
}

ComputationExpression.Run<int, Option<int>> (new MaybeBuilder (), async () => {
	var val1 = await TryDivide (120, 2);
	var val2 = await TryDivide (val1, 2);
	var val3 = await TryDivide (val2, 2);
	return val3;
})
```

In this example the code is very similar with C#'s `await` becoming the equivalent of F#'s `let!`/`do!`. The library also supports the `yield` keyword via an extra API call.

See the included [Workbook](https://github.com/Microsoft/Workbooks) `Examples` for API usage and full running samples.