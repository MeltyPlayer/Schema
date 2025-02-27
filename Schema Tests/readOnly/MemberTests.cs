﻿using NUnit.Framework;


namespace schema.readOnly;

internal class MemberTests {
  [Test]
  [TestCase("bool")]
  [TestCase("byte")]
  [TestCase("sbyte")]
  [TestCase("short")]
  [TestCase("ushort")]
  [TestCase("int")]
  [TestCase("uint")]
  [TestCase("long")]
  [TestCase("ulong")]
  [TestCase("float")]
  [TestCase("double")]
  [TestCase("char")]
  public void TestPrimitive(string primitiveType) {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.readOnly;

          namespace foo.bar;
          
          [GenerateReadOnly]
          public partial interface IWrapper {
            public {{primitiveType}} Value { get; set; }
          }
          """,
        $$"""
          #nullable enable

          namespace foo.bar;
          
          public partial interface IWrapper : IReadOnlyWrapper {
            {{primitiveType}} IReadOnlyWrapper.Value => Value;
          }
          
          public partial interface IReadOnlyWrapper {
            public {{primitiveType}} Value { get; }
          }

          """);
  }

  [Test]
  public void TestGeneric() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          public IEnumerable<bool> Value { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          System.Collections.Generic.IEnumerable<bool> IReadOnlyWrapper.Value => Value;
        }
        
        public partial interface IReadOnlyWrapper {
          public System.Collections.Generic.IEnumerable<bool> Value { get; }
        }

        """);
  }

  [Test]
  public void TestNestedGeneric() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          public IEnumerable<IEnumerable<bool>> Value { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<bool>> IReadOnlyWrapper.Value => Value;
        }
        
        public partial interface IReadOnlyWrapper {
          public System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<bool>> Value { get; }
        }

        """);
  }

  [Test]
  public void TestIndexer() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          public bool this[int index] { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          bool IReadOnlyWrapper.this[int index] => this[index];
        }
        
        public partial interface IReadOnlyWrapper {
          public bool this[int index] { get; }
        }

        """);
  }

  [Test]
  public void TestMultiIndexer() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          public bool this[int x, int y] { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          bool IReadOnlyWrapper.this[int x, int y] => this[x, y];
        }
        
        public partial interface IReadOnlyWrapper {
          public bool this[int x, int y] { get; }
        }

        """);
  }

  [Test]
  [TestCase("ref")]
  [TestCase("out")]
  [TestCase("in")]
  public void TestSpecialParameterTypes(string paramType) {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar;
          
          [GenerateReadOnly]
          public partial interface IWrapper {
            [Const]
            public void Foo({{paramType}} int bar) {}
          }
          """,
        $$"""
          #nullable enable

          namespace foo.bar;
          
          public partial interface IWrapper : IReadOnlyWrapper {
            void IReadOnlyWrapper.Foo({{paramType}} int bar) => Foo({{paramType}} bar);
          }
          
          public partial interface IReadOnlyWrapper {
            public void Foo({{paramType}} int bar);
          }

          """);
  }

  [Test]
  public void TestParams() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;

        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          public void Foo(params int[] bar) {}
        }
        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          void IReadOnlyWrapper.Foo(params int[] bar) => Foo(bar);
        }
        
        public partial interface IReadOnlyWrapper {
          public void Foo(params int[] bar);
        }

        """);
  }

  [Test]
  public void TestNullableValues() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          public int? Foo(int? bar) {}
      
          public int? Bar { get; }
        }
        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          int? IReadOnlyWrapper.Foo(int? bar) => Foo(bar);
          int? IReadOnlyWrapper.Bar => Bar;
        }
        
        public partial interface IReadOnlyWrapper {
          public int? Foo(int? bar);
          public int? Bar { get; }
        }

        """);
  }

  [Test]
  public void TestOptionalValues() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          [Const]
          public int Foo(int a = 123, char b = '0', string c = "hello", int? d = null) {}
        }
        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          int IReadOnlyWrapper.Foo(int a, char b, string c, int? d) => Foo(a, b, c, d);
        }
        
        public partial interface IReadOnlyWrapper {
          public int Foo(int a = 123, char b = '0', string c = "hello", int? d = null);
        }

        """);
  }

  [Test]
  public void TestNamelessTuple() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          public (bool, int) Tuple { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          (bool, int) IReadOnlyWrapper.Tuple => Tuple;
        }
        
        public partial interface IReadOnlyWrapper {
          public (bool, int) Tuple { get; }
        }

        """);
  }

  [Test]
  public void TestNamedTuple() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper {
          public (bool a, int b) Tuple { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          (bool a, int b) IReadOnlyWrapper.Tuple => Tuple;
        }
        
        public partial interface IReadOnlyWrapper {
          public (bool a, int b) Tuple { get; }
        }

        """);
  }
}