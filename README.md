# Neteril.ComputationExpression

![logo image](http://neteril.org/~jeremie/icon_neteril_computation_expression_github.png)


<a href="https://www.nuget.org/packages/Neteril.ComputationExpression"><img src="https://img.shields.io/nuget/v/Neteril.ComputationExpression.svg" alt="NuGet" /></a>
[![Build Status](https://dev.azure.com/jelaval/Neteril.ComputationExpression/_apis/build/status/garuma.Neteril.ComputationExpression)](https://dev.azure.com/jelaval/Neteril.ComputationExpression/_build/latest?definitionId=1)

A generic re-implementation of [F# computation expressions](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions) in C# by (ab)using async/await.

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
  => down == 0 ? None<int>.Value : Some.Of (up / down);

class MaybeBuilder : IMonadExpressionBuilder
{
  IMonad<T> IMonadExpressionBuilder.Bind<U, T> (IMonad<U> m, Func<U, IMonad<T>> f)
  {
    switch ((Option<U>)m) {
      case Some<U> some: return f (some.Item);
      case None<U> none:
      default: return None<T>.Value;
    }
  }
  public IMonad<T> Return<T> (T v) => Some.Of (v);
  public IMonad<T> Zero<T> () => None<T>.Value;
  // We don't have optional interface methods in C# quite yet
  public IMonad<T> Combine<T> (IMonad<T> m, IMonad<T> n) => throw new NotSupportedException ();
}

ComputationExpression.Run<int, Option<int>> (new OptionExpressionBuilder (), async () => {
  var val1 = await TryDivide (120, 2);
  var val2 = await TryDivide (val1, 2);
  var val3 = await TryDivide (val2, 2);
  return val3;
})
```

In this example the code is very similar with C#'s `await` becoming the equivalent of F#'s `let!`/`do!`. The library also supports the `yield` keyword via an extra API call.

In addition to the plumbing, the library provides some monads and their expression builder already. They can be found in the *Instances* folder. See the included [Workbook](https://github.com/Microsoft/Workbooks) `Examples` for API usage and full running samples.
