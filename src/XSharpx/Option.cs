using System;
using System.Collections;
using System.Collections.Generic;

// todo Sequence (requires List), Traverse (requires List), Boolean
namespace XSharpx {
  /// <summary>
  /// An immutable list with a maximum length of 1.
  /// </summary>
  /// <typeparam name="A">The element type held by this homogenous structure.</typeparam>
  /// <remarks>This data type is also used in place of a nullable type.</remarks>
  public struct Option<A> : IEnumerable<A> {
    private readonly bool e;
    private readonly A a;

    private Option(bool e, A a) {
      this.e = e;
      this.a = a;
    }

    public bool IsEmpty {
      get {
        return e;
      }
    }

    public bool IsNotEmpty{
      get {
        return !e;
      }
    }

    public X Fold<X>(Func<A, X> some, Func<X> empty) {
      return IsEmpty ? empty() : some(a);
    }

    public Option<B> Select<B>(Func<A, B> f) {
      return Fold<Option<B>>(a => Option<B>.Some(f(a)), () => Option<B>.Empty);
    }

    public Option<B> SelectMany<B>(Func<A, Option<B>> f) {
      return Fold(f, () => Option<B>.Empty);
    }

    public Option<C> SelectMany<B, C>(Func<A, Option<B>> p, Func<A, B, C> f) {
      return SelectMany<C>(a => p(a).Select<C>(b => f(a, b)));
    }

    public Option<C> ZipWith<B, C>(Option<B> o, Func<A, Func<B, C>> f) {
      return SelectMany<C>(a => o.Select<C>(b => f(a)(b)));
    }

    public Option<Pair<A, B>> Zip<B>(Option<B> o) {
      return ZipWith<B, Pair<A, B>>(o, Pair<A, B>.pairF());
    }

    public void ForEach(Action<A> a) {
      foreach(A x in this) {
        a(x);
      }
    }

    public Option<A> Where(Func<A, bool> p) {
      var t = this;
      return Fold(a => p(a) ? t : Empty, () => Empty);
    }

    public A ValueOr(Func<A> or) {
      return IsEmpty ? or() : a;
    }

    public Option<A> OrElse(Func<Option<A>> o) {
      return IsEmpty ? o() : this;
    }

    public bool All(Func<A, bool> f) {
      return IsEmpty || f(a);
    }

    public bool Any(Func<A, bool> f) {
      return !IsEmpty && f(a);
    }

    private class OptionEnumerator : IEnumerator<A> {
      private bool z = true;
      private readonly Option<A> o;
      private Option<A> a;

      internal OptionEnumerator(Option<A> o) {
        this.o = o;
      }

      public void Dispose() {}

      public void Reset() {
        z = true;
      }

      public bool MoveNext() {
        if(z) {
          a = o;
          z = false;
        } else
          a = Option<A>.Empty;

        return !a.IsEmpty;
      }

      A IEnumerator<A>.Current {
        get {
          return o.Value;
        }
      }

      public object Current {
        get {
          return o.Value;
        }
      }
    }

    private OptionEnumerator Enumerate() {
      return new OptionEnumerator(this);
    }

    IEnumerator<A> IEnumerable<A>.GetEnumerator() {
        return Enumerate();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return Enumerate();
    }

    public static Option<A> Empty {
      get {
        return new Option<A>(true, default(A));
      }
    }

    public static Option<A> Some(A t) {
      return new Option<A>(false, t);
    }

    private A Value {
      get {
        if(e)
          throw new Exception("Value on empty Option");
        else
          return a;
      }
    }

  }

  public static class OptionExtension {
    public static Option<B> Apply<A, B>(this Option<Func<A, B>> f, Option<A> o) {
      return f.SelectMany(g => o.Select(p => g(p)));
    }

    public static Option<A> Flatten<A>(this Option<Option<A>> o) {
      return o.SelectMany(z => z);
    }

    public static Pair<Option<A>, Option<B>> Unzip<A, B>(Option<Pair<A, B>> p) {
      return p.IsEmpty ?
        Pair<Option<A>, Option<B>>.pair(Option<A>.Empty, Option<B>.Empty) :
        Pair<Option<A>, Option<B>>.pair(Option<A>.Empty, Option<B>.Empty);
    }
  }
}

