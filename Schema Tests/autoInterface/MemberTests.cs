using NUnit.Framework;

using schema.binary;


namespace schema.autoInterface;

internal class MemberTests {
  [Test]
  [TestCase("get;")]
  [TestCase("get; set;")]
  [TestCase("set;")]
  public void TestGetterAndSetter(string getterAndOrSetter) {
    InterfaceGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.autoInterface;

          namespace foo.bar {
            [GenerateInterface]
            public partial class Wrapper {
              public bool Value { {{getterAndOrSetter}} }
            }
          }
          """,
        $$"""
          namespace foo.bar {
            public partial class Wrapper : IWrapper;
            
            #nullable enable
            public interface IWrapper {
              public bool Value { {{getterAndOrSetter}} }
            }
          }

          """);
  }

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
    InterfaceGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.autoInterface;

          namespace foo.bar {
            [GenerateInterface]
            public partial class Wrapper {
              public {{primitiveType}} Value { get; set; }
            }
          }
          """,
        $$"""
          namespace foo.bar {
            public partial class Wrapper : IWrapper;
            
            #nullable enable
            public interface IWrapper {
              public {{primitiveType}} Value { get; set; }
            }
          }

          """);
  }

  [Test]
  public void TestGeneric() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;
        using System.Collections.Generic;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            public IEnumerable<bool> Value { get; set; }
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public System.Collections.Generic.IEnumerable<bool> Value { get; set; }
          }
        }

        """);
  }

  [Test]
  public void TestNestedGeneric() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;
        using System.Collections.Generic;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            public IEnumerable<IEnumerable<bool>> Value { get; set; }
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<bool>> Value { get; set; }
          }
        }

        """);
  }

  [Test]
  public void TestIndexer() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            public bool this[int index] { get; set; }
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public bool this[int index] { get; set; }
          }
        }

        """);
  }

  [Test]
  public void TestMultiIndexer() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;
        using System.Collections.Generic;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            public bool this[int x, int y] { get; set; }
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public bool this[int x, int y] { get; set; }
          }
        }

        """);
  }

  [Test]
  [TestCase("ref")]
  [TestCase("out")]
  [TestCase("in")]
  public void TestSpecialParameterTypes(string paramType) {
    InterfaceGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.autoInterface;
          using System.Collections.Generic;

          namespace foo.bar {
            [GenerateInterface]
            public partial class Wrapper {
              public void Foo({{paramType}} int bar) {}
            }
          }
          """,
        $$"""
          namespace foo.bar {
            public partial class Wrapper : IWrapper;
            
            #nullable enable
            public interface IWrapper {
              public void Foo({{paramType}} int bar);
            }
          }

          """);
  }

  [Test]
  public void TestParams() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;
        using System.Collections.Generic;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            [Const]
            public void Foo(params int[] bar) {}
          }
        }
        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public void Foo(params int[] bar);
          }
        }

        """);
  }

  [Test]
  public void TestNullableValues() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;
        using System.Collections.Generic;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            [Const]
            public int? Foo(int? bar) {}
        
            public int? Bar { get; }
          }
        }
        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public int? Foo(int? bar);
            public int? Bar { get; }
          }
        }

        """);
  }

  [Test]
  public void TestOptionalValues() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;
        using System.Collections.Generic;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            [Const]
            public int Foo(int a = 123, char b = '0', string c = "hello", int? d = null) {}
          }
        }
        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public int Foo(int a = 123, char b = '0', string c = "hello", int? d = null);
          }
        }

        """);
  }

  [Test]
  public void TestNamelessTuple() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;
        using System.Collections.Generic;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            public (bool, int) Tuple { get; set; }
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public (bool, int) Tuple { get; set; }
          }
        }

        """);
  }

  [Test]
  public void TestNamedTuple() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;
        using System.Collections.Generic;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            public (bool a, int b) Tuple { get; set; }
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public (bool a, int b) Tuple { get; set; }
          }
        }

        """);
  }
}