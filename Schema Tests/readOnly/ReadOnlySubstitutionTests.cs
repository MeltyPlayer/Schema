using NUnit.Framework;

using schema.binary;

namespace schema.readOnly {
  internal class ReadOnlySubstitutionTests {
    [Test]
    public void TestSubstitutesTypeConstraints() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper<T> where T : IValue;
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T>;
            
            public interface IReadOnlyWrapper<T> where T : schema.readOnly.IReadOnlyValue;
          }

          """);
    }

    [Test]
    public void TestDoesNotSubstituteMutableTypeConstraints() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper<[KeepMutableType] T> where T : IValue;
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T>;
            
            public interface IReadOnlyWrapper<T> where T : schema.readOnly.IValue;
          }

          """);
    }

    [Test]
    public void TestDoesNotSubstituteMutableTypeConstraintsDownstream() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IBase<[KeepMutableType] T1, T2> where T1 : IValue where T2 : IValue;
          
            [GenerateReadOnly]
            public partial interface IChild : IBase<IValue, IValue>;
          }
          """,
          """
          namespace foo.bar {
            public partial interface IBase<T1, T2> : IReadOnlyBase<T1, T2>;
            
            public interface IReadOnlyBase<T1, T2> where T1 : schema.readOnly.IValue where T2 : schema.readOnly.IReadOnlyValue;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IChild : IReadOnlyChild;
            
            public interface IReadOnlyChild : IReadOnlyBase<schema.readOnly.IValue, schema.readOnly.IReadOnlyValue>;
          }

          """);
    }

    [Test]
    public void TestSubstitutesMethodConstraints() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              void Foo<T>() where T : IValue;
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              void IReadOnlyWrapper.Foo<T>() => Foo<T>();
            }
            
            public interface IReadOnlyWrapper {
              public void Foo<T>() where T : schema.readOnly.IReadOnlyValue;
            }
          }

          """);
    }

    [Test]
    public void TestDoesNotSubstituteMutableMethodConstraints() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              void Foo<[KeepMutableType] T>() where T : IValue;
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              void IReadOnlyWrapper.Foo<T>() => Foo<T>();
            }
            
            public interface IReadOnlyWrapper {
              public void Foo<T>() where T : schema.readOnly.IValue;
            }
          }

          """);
    }

    [Test]
    public void TestSubstitutesReturnValues() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              IValue Foo();
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              schema.readOnly.IReadOnlyValue IReadOnlyWrapper.Foo() => Foo();
            }
            
            public interface IReadOnlyWrapper {
              public schema.readOnly.IReadOnlyValue Foo();
            }
          }

          """);
    }

    [Test]
    public void TestDoesNotSubstituteMutableReturnValues() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              [KeepMutableType] IValue Foo();
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              schema.readOnly.IReadOnlyValue IReadOnlyWrapper.Foo() => Foo();
            }
            
            public interface IReadOnlyWrapper {
              public schema.readOnly.IReadOnlyValue Foo();
            }
          }

          """);
    }

    [Test]
    public void TestSubstitutesParameters() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              void Foo(IValue value);
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              void IReadOnlyWrapper.Foo(schema.readOnly.IReadOnlyValue value) => Foo(value);
            }
            
            public interface IReadOnlyWrapper {
              public void Foo(schema.readOnly.IReadOnlyValue value);
            }
          }

          """);
    }

    [Test]
    public void TestDoesNotSubstituteMutableParameters() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              void Foo([KeepMutableType] IValue value);
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              void IReadOnlyWrapper.Foo(schema.readOnly.IValue value) => Foo(value);
            }
            
            public interface IReadOnlyWrapper {
              public void Foo(schema.readOnly.IValue value);
            }
          }

          """);
    }

    [Test]
    public void TestSubstitutesIndexerParameters() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              int this[IValue value] { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              int IReadOnlyWrapper.this[schema.readOnly.IReadOnlyValue value] => this[value];
            }
            
            public interface IReadOnlyWrapper {
              public int this[schema.readOnly.IReadOnlyValue value] { get; }
            }
          }

          """);
    }

    [Test]
    public void TestDoesNotSubstituteMutableIndexerParameters() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              int this[[KeepMutableType] IValue value] { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              int IReadOnlyWrapper.this[schema.readOnly.IValue value] => this[value];
            }
            
            public interface IReadOnlyWrapper {
              public int this[schema.readOnly.IValue value] { get; }
            }
          }

          """);
    }

    [Test]
    public void TestSubstitutesProperties() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              IValue Foo { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              schema.readOnly.IReadOnlyValue IReadOnlyWrapper.Foo => Foo;
            }
            
            public interface IReadOnlyWrapper {
              public schema.readOnly.IReadOnlyValue Foo { get; }
            }
          }

          """);
    }

    [Test]
    public void TestDoesNotSubstituteMutableProperties() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [KeepMutableType] IValue Foo { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              schema.readOnly.IValue IReadOnlyWrapper.Foo => Foo;
            }
            
            public interface IReadOnlyWrapper {
              public schema.readOnly.IValue Foo { get; }
            }
          }

          """);
    }

    [Test]
    public void TestSubstitutesInGenericInterface() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper : IEnumerable<IValue>;
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper;
            
            public interface IReadOnlyWrapper : System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue>;
          }

          """);
    }

    [Test]
    public void TestSubstitutesInGenericReturnValue() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              IEnumerable<IValue> Foo();
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue> IReadOnlyWrapper.Foo() => Foo();
            }
            
            public interface IReadOnlyWrapper {
              public System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue> Foo();
            }
          }

          """);
    }
  }
}