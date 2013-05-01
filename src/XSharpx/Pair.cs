using System;

namespace XSharpx {
  /// <summary>
  /// The conjunction of 2 values.
  /// </summary>
  /// <typeparam name="A">The element type of one of the two values.</typeparam>
  /// <typeparam name="B">The element type of one of the two values.</typeparam>
  /// <remarks>Also known as a pair.</remarks>
  public struct Pair<A, B> {
    private readonly A aa;
    private readonly B bb;

    private Pair(A a, B b) {
      aa = a;
      bb = b;
    }

    public A a {
      get {
        return aa;
      }
    }

    public B b {
      get {
        return bb;
      }
    }

    public Pair<A, X> Select<X>(Func<B, X> f) {
      return Pair<A, X>.pair(aa, f(bb));
    }

    // todo SelectMany etc, requires Semigroup

    public Pair<X, B> First<X>(Func<A, X> f) {
      return Pair<X, B>.pair(f(aa), bb);
    }

    public Pair<A, X> Second<X>(Func<B, X> f) {
      return Pair<A, X>.pair(aa, f(bb));
    }

    public Pair<X, Y> BinarySelect<X, Y>(Func<A, X> f, Func<B, Y> g) {
      return Pair<X, Y>.pair(f(aa), g(bb));
    }

    public Pair<B, A> Swap {
      get {
        return Pair<B, A>.pair(bb, aa);
      }
    }

    public Pair<X, Y> Swapped<X, Y>(Func<Pair<B, A>, Pair<Y, X>> f) {
      return f(Swap).Swap;
    }

    public X Fold<X>(Func<A, B, X> f) {
      return f(aa, bb);
    }

    public static Pair<A, B> pair(A a, B b) {
      return new Pair<A, B>(a, b);
    }

    public static Func<A, Func<B, Pair<A, B>>> pairF() {
      return a => b => new Pair<A, B>(a, b);
    }
  }
}
