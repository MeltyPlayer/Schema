using Microsoft.CodeAnalysis;

using NUnit.Framework;


namespace schema.binary {
  public partial class SchemaStructureParserTests {
    public class Enum {
      [Test]
      public void TestEnumWithFormat() {
        var structure = BinarySchemaTestUtil.ParseFirst(@"
namespace foo.bar {
  public enum ValueType : byte {
    A,
    B,
    C
  }

  [BinarySchema]
  public partial class EnumWrapper {
    public ValueType field;
  }
}");
        BinarySchemaTestUtil.AssertDiagnostics(
            structure.Diagnostics,
            System.Array.Empty<DiagnosticDescriptor>());
      }

      [Test]
      public void TestEnumWithoutFormat() {
        var structure = BinarySchemaTestUtil.ParseFirst(@"
namespace foo.bar {
  public enum ValueType {
    A,
    B,
    C
  }

  [BinarySchema]
  public partial class EnumWrapper {
    public ValueType field;
  }
}");
        BinarySchemaTestUtil.AssertDiagnostics(
            structure.Diagnostics,
            System.Array.Empty<DiagnosticDescriptor>());
      }

      [Test]
      public void TestEnumArrayWithoutFormat() {
        var structure = BinarySchemaTestUtil.ParseFirst(@"
namespace foo.bar {
  public enum ValueType {
    A,
    B,
    C
  }

  [BinarySchema]
  public partial class EnumWrapper {
    public readonly ValueType[] field = new ValueType[1];
  }
}");
        BinarySchemaTestUtil.AssertDiagnostics(
            structure.Diagnostics,
            System.Array.Empty<DiagnosticDescriptor>());
      }
    }
  }
}