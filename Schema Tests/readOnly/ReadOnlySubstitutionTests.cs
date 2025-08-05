using NUnit.Framework;


namespace schema.readOnly;

internal class ReadOnlySubstitutionTests {
  [Test]
  public void TestSubstitutesTypeConstraints() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper<T> where T : IValue;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper<T> : IReadOnlyWrapper<T>;
        
        public partial interface IReadOnlyWrapper<out T> where T : schema.readOnly.IReadOnlyValue;

        """);
  }

  [Test]
  public void TestDoesNotSubstituteMutableTypeConstraints() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper<[KeepMutableType] T> where T : IValue;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper<T> : IReadOnlyWrapper<T>;
        
        public partial interface IReadOnlyWrapper<out T> where T : schema.readOnly.IValue;

        """);
  }

  [Test]
  public void TestDoesNotSubstituteMutableTypeConstraintsDownstream() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IBase<[KeepMutableType] T1, T2> where T1 : IValue where T2 : IValue;
      
        [GenerateReadOnly]
        public partial interface IChild : IBase<IValue, IValue>;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IBase<T1, T2> : IReadOnlyBase<T1, T2>;
        
        public partial interface IReadOnlyBase<out T1, out T2> where T1 : schema.readOnly.IValue where T2 : schema.readOnly.IReadOnlyValue;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IChild : IReadOnlyChild;
        
        public partial interface IReadOnlyChild : IReadOnlyBase<schema.readOnly.IValue, schema.readOnly.IReadOnlyValue>;

        """);
  }

  [Test]
  public void TestSubstitutesMethodConstraints() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          void Foo<T>() where T : IValue;
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          void IReadOnlyWrapper.Foo<T>() => Foo<T>();
        }
        
        public partial interface IReadOnlyWrapper {
          public void Foo<T>() where T : schema.readOnly.IReadOnlyValue;
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteMutableMethodConstraints() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          void Foo<[KeepMutableType] T>() where T : IValue;
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          void IReadOnlyWrapper.Foo<T>() => Foo<T>();
        }
        
        public partial interface IReadOnlyWrapper {
          public void Foo<T>() where T : schema.readOnly.IValue;
        }

        """);
  }

  [Test]
  public void TestSubstitutesReturnValues() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          IValue Foo();
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          schema.readOnly.IReadOnlyValue IReadOnlyWrapper.Foo() => Foo();
        }
        
        public partial interface IReadOnlyWrapper {
          public schema.readOnly.IReadOnlyValue Foo();
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteMutableReturnValues() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          [KeepMutableType] IValue Foo();
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          schema.readOnly.IValue IReadOnlyWrapper.Foo() => Foo();
        }
        
        public partial interface IReadOnlyWrapper {
          public schema.readOnly.IValue Foo();
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteParameters() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          void Foo(IValue value);
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          void IReadOnlyWrapper.Foo(schema.readOnly.IValue value) => Foo(value);
        }
        
        public partial interface IReadOnlyWrapper {
          public void Foo(schema.readOnly.IValue value);
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteMutableParameters() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          void Foo([KeepMutableType] IValue value);
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          void IReadOnlyWrapper.Foo(schema.readOnly.IValue value) => Foo(value);
        }
        
        public partial interface IReadOnlyWrapper {
          public void Foo(schema.readOnly.IValue value);
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteIndexerParameters() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          int this[IValue value] { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          int IReadOnlyWrapper.this[schema.readOnly.IValue value] => this[value];
        }
        
        public partial interface IReadOnlyWrapper {
          public int this[schema.readOnly.IValue value] { get; }
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteMutableIndexerParameters() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          int this[[KeepMutableType] IValue value] { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          int IReadOnlyWrapper.this[schema.readOnly.IValue value] => this[value];
        }
        
        public partial interface IReadOnlyWrapper {
          public int this[schema.readOnly.IValue value] { get; }
        }

        """);
  }

  [Test]
  public void TestSubstitutesProperties() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          IValue Foo { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          schema.readOnly.IReadOnlyValue IReadOnlyWrapper.Foo => Foo;
        }
        
        public partial interface IReadOnlyWrapper {
          public schema.readOnly.IReadOnlyValue Foo { get; }
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteMutableProperties() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [KeepMutableType] IValue Foo { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          schema.readOnly.IValue IReadOnlyWrapper.Foo => Foo;
        }
        
        public partial interface IReadOnlyWrapper {
          public schema.readOnly.IValue Foo { get; }
        }

        """);
  }

  [Test]
  public void TestSubstitutesGenericInterface() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper : IEnumerable<IValue>;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper;
        
        public partial interface IReadOnlyWrapper : System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue>;

        """);
  }

  [Test]
  public void TestSubstitutesGenericReturnValue() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          IEnumerable<IValue> Foo();
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue> IReadOnlyWrapper.Foo() => (System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue>)(object) Foo();
        }
        
        public partial interface IReadOnlyWrapper {
          public System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue> Foo();
        }

        """);
  }

  [Test]
  public void TestSubstitutesGenericProperty() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          IEnumerable<IValue> Foo { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue> IReadOnlyWrapper.Foo => (System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue>)(object) Foo;
        }
        
        public partial interface IReadOnlyWrapper {
          public System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue> Foo { get; }
        }

        """);
  }

  [Test]
  public void TestSubstitutesGenericIndexer() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          IEnumerable<IValue> this[int foo] { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue> IReadOnlyWrapper.this[int foo] => (System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue>)(object) this[foo];
        }
        
        public partial interface IReadOnlyWrapper {
          public System.Collections.Generic.IEnumerable<schema.readOnly.IReadOnlyValue> this[int foo] { get; }
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteGenericIndexerParameter() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;

        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          void this[IEnumerable<IValue> foo] { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
      
        public partial interface IWrapper : IReadOnlyWrapper {
          void IReadOnlyWrapper.this[System.Collections.Generic.IEnumerable<schema.readOnly.IValue> foo] => this[foo];
        }
        
        public partial interface IReadOnlyWrapper {
          public void this[System.Collections.Generic.IEnumerable<schema.readOnly.IValue> foo] { get; }
        }

        """);
  }

  [Test]
  public void TestDoesNotSubstituteGenericMethodParameter() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;

        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          void Foo(IEnumerable<IValue> foo);
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          void IReadOnlyWrapper.Foo(System.Collections.Generic.IEnumerable<schema.readOnly.IValue> foo) => Foo(foo);
        }
        
        public partial interface IReadOnlyWrapper {
          public void Foo(System.Collections.Generic.IEnumerable<schema.readOnly.IValue> foo);
        }

        """);
  }
}