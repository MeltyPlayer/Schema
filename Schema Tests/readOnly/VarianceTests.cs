﻿using NUnit.Framework;

using schema.binary;

namespace schema.readOnly {
  internal class VarianceTests {
    [Test]
    [TestCase("in")]
    [TestCase("out")]
    public void TestSupportsEachTypeOfVarianceInType(string variance) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper<{{variance}} T>;
            }
            """,
          $$"""
            namespace foo.bar {
              public partial interface IWrapper<{{variance}} T> : IReadOnlyWrapper<T>;
              
              public interface IReadOnlyWrapper<{{variance}} T>;
            }

            """);
    }

    [Test]
    public void TestAddsCovarianceWhenPossible() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
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
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
              T IReadOnlyWrapper<T>.Field => Field;
              System.Collections.Generic.IEnumerable<T> IReadOnlyWrapper<T>.Sequence => Sequence;
              System.Collections.Generic.IEnumerable<T> IReadOnlyWrapper<T>.this[int foo] => this[foo];
              T IReadOnlyWrapper<T>.GetField() => GetField();
              IBase<T> IReadOnlyWrapper<T>.GetSequence() => GetSequence();
            }
            
            public interface IReadOnlyWrapper<out T> : IBase<T> {
              public T Field { get; }
              public System.Collections.Generic.IEnumerable<T> Sequence { get; }
              public System.Collections.Generic.IEnumerable<T> this[int foo] { get; }
              public T GetField();
              public IBase<T> GetSequence();
            }
          }

          """);
    }

    [Test]
    public void TestAddsContravarianceWhenPossible() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
            public interface IBase<in T>;
          
            [GenerateReadOnly]
            public partial interface IWrapper<T> : IBase<T> {
              // These will be ignored
              T Field { set; }
              IEnumerable<T> Sequence { set; }
              void this[T foo] { set; }
          
              [Const]
              void SetField(T foo);
              [Const]
              void SetSequence(IBase<T> foo);
              
              // This will be ignored since it's not const
              T field IgnoredFunction();
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
              void IReadOnlyWrapper<T>.SetField(T foo) => SetField(foo);
              void IReadOnlyWrapper<T>.SetSequence(IBase<T> foo) => SetSequence(foo);
            }
            
            public interface IReadOnlyWrapper<in T> : IBase<T> {
              public void SetField(T foo);
              public void SetSequence(IBase<T> foo);
            }
          }

          """);
    }

    [Test]
    public void TestDoesNotAddVarianceForBothInAndOutTypes() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper<T> {
              [Const]
              public T Method(T value) => value;
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
              T IReadOnlyWrapper<T>.Method(T value) => Method(value);
            }
            
            public interface IReadOnlyWrapper<T> {
              public T Method(T value);
            }
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

            namespace foo.bar {
              public interface IValue<{{variance}}T>;
            
              [GenerateReadOnly]
              public partial interface IWrapper<T> {
                [Const]
                public void Method(IValue<T> foo);
              }
            }
            """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
              void IReadOnlyWrapper<T>.Method(IValue<T> foo) => Method(foo);
            }
            
            public interface IReadOnlyWrapper<T> {
              public void Method(IValue<T> foo);
            }
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

            namespace foo.bar {
              public interface IValue<{{variance}}T>;
            
              [GenerateReadOnly]
              public partial interface IWrapper<T> {
                public IValue<T> Property { get; set; }
              }
            }
            """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
              IValue<T> IReadOnlyWrapper<T>.Property => Property;
            }
            
            public interface IReadOnlyWrapper<T> {
              public IValue<T> Property { get; }
            }
          }

          """);
    }

    [Test]
    public void TestDoesNotAddContravarianceForSet() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
            using schema.readOnly;
            using System.Collections.Generic;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper<T> {
                [Const]
                public void Method(ISet<T> foo);
              }
            }
            """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
              void IReadOnlyWrapper<T>.Method(System.Collections.Generic.ISet<T> foo) => Method(foo);
            }
            
            public interface IReadOnlyWrapper<T> {
              public void Method(System.Collections.Generic.ISet<T> foo);
            }
          }

          """);
    }
  }
}