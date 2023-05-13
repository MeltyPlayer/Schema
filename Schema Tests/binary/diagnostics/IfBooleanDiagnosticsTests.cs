using NUnit.Framework;


namespace schema.binary.text {
  internal class IfBooleanDiagnosticsTests {
    [Test]
    public void TestIfBooleanNonReference() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;
namespace foo.bar {
  [BinarySchema]
  public partial class BooleanWrapper : IBinaryConvertible {
    [IfBoolean(SchemaIntegerType.BYTE)]
    public int field;
  }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.IfBooleanNeedsNullable);
    }

    [Test]
    public void TestIfBooleanNonNullable() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;
namespace foo.bar {
  [BinarySchema]
  public partial class BooleanWrapper : IBinaryConvertible {
    [IfBoolean(SchemaIntegerType.BYTE)]
    public A field;
  }

  [BinarySchema]
  public partial class A : IBinaryConvertible {
  }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.IfBooleanNeedsNullable);
    }

    [Test]
    public void TestOutOfOrder() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }

    [IntegerFormat(SchemaIntegerType.BYTE)]
    private bool Field { get; set; }
  }

  public class A : IBinaryConvertible { }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.DependentMustComeAfterSource);
    }

    [Test]
    public void TestPublicPropertySource() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool Field { get; set; }

    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }
  }

  public class A : IBinaryConvertible { }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.SourceMustBePrivate);
    }

    [Test]
    public void TestProtectedPropertySource() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    protected bool Field { get; set; }

    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }
  }

  public class A : IBinaryConvertible { }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.SourceMustBePrivate);
    }

    [Test]
    public void TestInternalPropertySource() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    internal bool Field { get; set; }

    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }
  }

  public class A : IBinaryConvertible { }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.SourceMustBePrivate);
    }

    [Test]
    public void TestPublicFieldSource() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool Field;

    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }
  }

  public class A : IBinaryConvertible { }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.SourceMustBePrivate);
    }

    [Test]
    public void TestProtectedFieldSource() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    protected bool Field;

    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }
  }

  public class A : IBinaryConvertible { }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.SourceMustBePrivate);
    }

    [Test]
    public void TestInternalFieldSource() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    internal bool Field;

    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }
  }

  public class A : IBinaryConvertible { }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.SourceMustBePrivate);
    }
  }
}