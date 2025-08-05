using NUnit.Framework;


namespace schema.readOnly;

internal class VarianceTests {
  [Test]
  [TestCase("in")]
  [TestCase("out")]
  public void TestSupportsEachTypeOfVarianceInType(string variance) {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        $"""
          using schema.readOnly;

          namespace foo.bar;
          
          [GenerateReadOnly]
          public partial interface IWrapper<{variance} T>;
          """,
        $"""
          #nullable enable

          namespace foo.bar;
          
          public partial interface IWrapper<{variance} T> : IReadOnlyWrapper<T>;
          
          public partial interface IReadOnlyWrapper<{variance} T>;

          """);
  }

  [Test]
  public void TestAddsCovarianceWhenPossible() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        public interface IBase<out T>;
      
        [GenerateReadOnly]
        public partial interface IWrapper<T> : IBase<T> {
          T Field { get; set; }
          IEnumerable<T> Sequence { get; set; }
      
          IEnumerable<T> this[int foo] { get; set; }
      
          [Const]
          T GetField();
          [Const]
          IBase<T> GetSequence();
      
          // This will be ignored since it's not const
          void IgnoredFunction(T field, IEnumerable<T> sequence);
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          T IReadOnlyWrapper<T>.Field => Field;
          System.Collections.Generic.IEnumerable<T> IReadOnlyWrapper<T>.Sequence => Sequence;
          System.Collections.Generic.IEnumerable<T> IReadOnlyWrapper<T>.this[int foo] => this[foo];
          T IReadOnlyWrapper<T>.GetField() => GetField();
          IBase<T> IReadOnlyWrapper<T>.GetSequence() => GetSequence();
        }
        
        public partial interface IReadOnlyWrapper<out T> : IBase<T> {
          public T Field { get; }
          public System.Collections.Generic.IEnumerable<T> Sequence { get; }
          public System.Collections.Generic.IEnumerable<T> this[int foo] { get; }
          public T GetField();
          public IBase<T> GetSequence();
        }

        """);
  }

  [Test]
  public void TestAddsContravarianceWhenPossible() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        public interface IBase<in T>;
      
        [GenerateReadOnly]
        public partial interface IWrapper<T> : IBase<T> {
          T Field { set; }

          Enumerable<T> Sequence { set; }

          void this[T foo] { set; }
      
          [Const]
          void SetField(T foo);
      
          [Const]
          void SetSequence(IBase<T> foo);
      
          
          // This will be ignored since it's not const
          void IgnoredFunction();
        }
        
        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          void IReadOnlyWrapper<T>.SetField(T foo) => SetField(foo);
          void IReadOnlyWrapper<T>.SetSequence(IBase<T> foo) => SetSequence(foo);
        }
        
        public partial interface IReadOnlyWrapper<in T> : IBase<T> {
          public void SetField(T foo);
          public void SetSequence(IBase<T> foo);
        }

        """);
  }

  [Test]
  public void TestDoesNotAddVarianceForBothInAndOutTypes() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper<T> {
          [Const]
          public T Method(T value) => value;
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          T IReadOnlyWrapper<T>.Method(T value) => Method(value);
        }
        
        public partial interface IReadOnlyWrapper<T> {
          public T Method(T value);
        }

        """);
  }

  [Test]
  [TestCase("")]
  [TestCase("out ")]
  public void TestDoesNotAddContravarianceUnlessContravariantInOtherType(
      string variance) {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.readOnly;

          namespace foo.bar;
          
          public interface IValue<{{variance}}T>;
        
          [GenerateReadOnly]
          public partial interface IWrapper<T> {
            [Const]
            public void Method(IValue<T> foo);
          }
          """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          void IReadOnlyWrapper<T>.Method(IValue<T> foo) => Method(foo);
        }
        
        public partial interface IReadOnlyWrapper<T> {
          public void Method(IValue<T> foo);
        }

        """);
  }

  [Test]
  [TestCase("")]
  [TestCase("in ")]
  public void TestDoesNotAddCovarianceUnlessCovariantInOtherType(
      string variance) {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.readOnly;

          namespace foo.bar;
          
          public interface IValue<{{variance}}T>;
        
          [GenerateReadOnly]
          public partial interface IWrapper<T> {
            public IValue<T> Property { get; set; }
          }
          """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          IValue<T> IReadOnlyWrapper<T>.Property => Property;
        }
        
        public partial interface IReadOnlyWrapper<T> {
          public IValue<T> Property { get; }
        }

        """);
  }

  [Test]
  public void TestDoesNotAddContravarianceForSet() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper<T> {
          [Const]
          public void Method(ISet<T> foo);
        }

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          void IReadOnlyWrapper<T>.Method(System.Collections.Generic.ISet<T> foo) => Method(foo);
        }
        
        public partial interface IReadOnlyWrapper<T> {
          public void Method(System.Collections.Generic.ISet<T> foo);
        }

        """);
  }

  [Test]
  public void TestDoesNotAddVarianceWhenUsedAsTypeConstraint() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        public partial interface IFinCollection<in T>;
        
        [GenerateReadOnly]
        public partial interface ISubTypeDictionary<T1, T2>
             : IFinCollection<T2> where T2 : T1;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface ISubTypeDictionary<T1, T2> : IReadOnlySubTypeDictionary<T1, T2>;
        
        public partial interface IReadOnlySubTypeDictionary<T1, in T2> : IFinCollection<T2> where T2 : T1;

        """);
  }

  [Test]
  public void TestDoesNotAddVarianceWhenUsedAsMethodConstraint() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        public partial interface IFinCollection<out T>;
        
        [GenerateReadOnly]
        public partial interface ISubTypeDictionary<TKey, TValue>
            : IFinCollection<(TKey Key, TValue Value)> {
          [Const]
          TValueSub Get<TValueSub>(TKey key) where TValueSub : TValue;
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface ISubTypeDictionary<TKey, TValue> : IReadOnlySubTypeDictionary<TKey, TValue> {
          TValueSub IReadOnlySubTypeDictionary<TKey, TValue>.Get<TValueSub>(TKey key) => Get<TValueSub>(key);
        }
        
        public partial interface IReadOnlySubTypeDictionary<TKey, TValue> : IFinCollection<(TKey Key, TValue Value)> {
          public TValueSub Get<TValueSub>(TKey key) where TValueSub : TValue;
        }

        """);
  }
}