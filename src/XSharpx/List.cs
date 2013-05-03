using System;
using System.Collections;
using System.Collections.Generic;

namespace XSharpx
{
  /// <summary>
  /// An immutable in-memory single-linked list.
  /// </summary>
  /// <typeparam name="A">The element type held by this homogenous list.</typeparam>
  /// <remarks>Also known as a cons-list.</remarks>
  public sealed class List<A> : IEnumerable<A> {
    private readonly bool e;
    private readonly A h;
    private List<A> t;

    private List(bool e, A h, List<A> t) {
      this.e = e;
      this.h = h;
      this.t = t;
    }

    // To be used by ListBuffer only
    internal void UnsafeTailUpdate(List<A> t) {
      this.t = t;
    }

    // To be used by ListBuffer only
    internal A UnsafeHead {
      get {
        if(e)
          throw new Exception("Head on empty List");
        else
          return h;
      }
    }

    // To be used by ListBuffer only
    internal List<A> UnsafeTail {
      get {
        if(e)
          throw new Exception("Tail on empty List");
        else
          return t;
      }
    }

    public bool IsEmpty {
      get {
        return e;
      }
    }

    public bool IsNotEmpty {
      get {
        return !e;
      }
    }

    public List<B> Select<B>(Func<A, B> f) {
      var b = ListBuffer<B>.Empty();

      foreach(var a in this)
        b.Snoc(f(a));

      return b.ToList;
    }

    public List<B> SelectMany<B>(Func<A, List<B>> f) {
      var b = ListBuffer<B>.Empty();

      foreach(var a in this)
        b.Append(f(a));

      return b.ToList;
    }

    public List<C> SelectMany<B, C>(Func<A, List<B>> p, Func<A, B, C> f) {
      return SelectMany<C>(a => p(a).Select<C>(b => f(a, b)));
    }

    public List<C> ProductWith<B, C>(List<B> o, Func<A, Func<B, C>> f) {
      return SelectMany<C>(a => o.Select<C>(b => f(a)(b)));
    }

    public List<Pair<A, B>> Product<B>(List<B> o) {
      return ZipWith<B, Pair<A, B>>(o, Pair<A, B>.pairF());
    }

    public static List<A> operator +(A h, List<A> t) {
      return Cons(h, t);
    }

    public static List<A> Empty {
      get {
        return new List<A>(true, default(A), default(List<A>));
      }
    }

    public static List<A> Cons(A h, List<A> t) {
      return new List<A>(false, h, t);
    }

    public static List<A> list(params A[] a) {
      var k = List<A>.Empty;

      for(int i = a.Length - 1; i >= 0; i--) {
        k = a[i] + k;
      }

      return k;
    }

    private class ListEnumerator : IEnumerator<A> {
      private bool z = true;
      private readonly List<A> o;
      private List<A> a;

      public ListEnumerator(List<A> o) {
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
          a = a.UnsafeTail;

        return !a.IsEmpty;
      }

      A IEnumerator<A>.Current {
        get {
          return a.UnsafeHead;
        }
      }

      public object Current {
        get {
          return a.UnsafeHead;
        }
      }
    }

    private ListEnumerator Enumerate() {
      return new ListEnumerator(this);
    }

    IEnumerator<A> IEnumerable<A>.GetEnumerator() {
      return Enumerate();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return Enumerate();
    }

    public ListBuffer<A> Buffer {
      get {
        var b = ListBuffer<A>.Empty();

        foreach(var a in this)
          b.Snoc(a);

        return b;
      }
    }

    public Option<A> Head {
      get {
        return IsEmpty ? Option.Empty : Option.Some(UnsafeHead);
      }
    }

    public Option<List<A>> Tail {
      get {
        return IsEmpty ? Option.Empty : Option.Some(UnsafeTail);
      }
    }

    public A HeadOr(Func<A> a) {
      return IsEmpty ? a() : UnsafeHead;
    }

    public List<A> TailOr(Func<List<A>> a) {
      return IsEmpty ? a() : UnsafeTail;
    }

    public X Uncons<X>(Func<X> nil, Func<A, List<A>, X> headTail) {
      return IsEmpty ? nil() : headTail(UnsafeHead, UnsafeTail);
    }

    public B FoldRight<B>(Func<A, B, B> f, B b) {
      return e ? b : f(UnsafeHead, UnsafeTail.FoldRight(f, b));
    }

    public B FoldLeft<B>(Func<B, A, B> f, B b) {
      var x = b;

      foreach(A z in this) {
        x = f(x, z);
      }

      return x;
    }

    public void ForEach(Action<A> a) {
      foreach(A x in this) {
        a(x);
      }
    }

    public List<A> Where(Func<A, bool> f) {
      var b = ListBuffer<A>.Empty();

      foreach(var a in this)
        if(f(a))
          b.Snoc(a);

      return b.ToList;
    }

    public List<A> Take(int n) {
      return n <= 0 || e
        ? List<A>.Empty
        : UnsafeHead + UnsafeTail.Take(n - 1);
    }

    public List<A> Drop(int n) {
      return n <= 0
        ? this
        : e
          ? List<A>.Empty
          : UnsafeTail.Drop(n - 1);
    }

    public List<A> TakeWhile(Func<A, bool> p) {
      return e
        ? this
        : p(UnsafeHead)
          ? UnsafeHead + UnsafeTail.TakeWhile(p)
          : List<A>.Empty;
    }

    public List<A> DropWhile(Func<A, bool> p) {
      var a = this;

      while(!a.IsEmpty && p(a.UnsafeHead)) {
        a = a.UnsafeTail;
      }

      return a;
    }

    public int Length {
      get {
        return FoldLeft((b, _) => b + 1, 0);
      }
    }

    public List<A> Reverse() {
      return FoldLeft<List<A>>((b, a) => a + b, List<A>.Empty);
    }

    public Option<A> this [int n] {
      get {
        return n < 0 || IsEmpty
          ? Option.Empty
          : n == 0
            ? Option.Some(UnsafeHead)
            : UnsafeTail[n - 1];
      }
    }

    public List<C> ZipWith<B, C>(List<B> bs, Func<A, Func<B, C>> f) {
      return IsEmpty && bs.IsEmpty
        ? List<C>.Empty
        : f(UnsafeHead)(bs.UnsafeHead) + UnsafeTail.ZipWith(bs.UnsafeTail, f);
    }

    public List<Pair<A, B>> Zip<B>(List<B> bs) {
      return ZipWith<B, Pair<A, B>>(bs, a => b => Pair<A, B>.pair(a, b));
    }

    public bool All(Func<A, bool> f) {
      var x = true;

      foreach(var t in this) {
        if(!f(t))
          return false;
      }

      return x;
    }

    public bool Any(Func<A, bool> f) {
      var x = false;

      foreach(var t in this) {
        if(f(t))
          return true;
      }

      return x;
    }

    public Option<List<B>> TraverseOption<B>(Func<A, Option<B>> f) {
      return FoldRight<Option<List<B>>>(
        (a, b) => f(a).ZipWith<List<B>, List<B>>(b, aa => bb => aa + bb)
      , List<B>.Empty.Some()
      );
    }
  }

  public static class ListExtension {
    public static List<A> ListValue<A>(this A a) {
      return List<A>.Cons(a, List<A>.Empty);
    }
  }
}

