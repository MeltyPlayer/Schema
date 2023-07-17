﻿using NUnit.Framework;


namespace schema.binary.text {
  internal class SequenceDiagnosticsTests {
    [Test]
    public void TestFailsIfOutOfOrder() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;
using schema.binary.attributes.sequence;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [RSequenceLengthSource(""Count"")]
    public int[] Field { get; set; }

    public int Count { get; set; }
  }
}");
      BinarySchemaTestUtil.AssertDiagnostics(
          structure.Diagnostics,
          Rules.DependentMustComeAfterSource,
          Rules.SourceMustBePrivate);
    }

    [Test]
    public void TestAllowsIgnoredOutOfOrder() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;
using schema.binary.attributes.ignore;
using schema.binary.attributes.sequence;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [RSequenceLengthSource(""Count"")]
    public int[] Field { get; set; }

    [Ignore]
    public int Count { get; set; }
  }
}");
      BinarySchemaTestUtil.AssertDiagnostics(
          structure.Diagnostics);
    }

    [Test]
    public void TestAllowsSequenceAttributesOnISequence() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
using schema.binary;
using schema.binary.attributes.ignore;
using schema.binary.attributes.sequence;
using schema.util.sequences;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public SequenceImpl<int> Field1 { get; set; }

    [RSequenceLengthSource(""Count"")]
    public SequenceImpl<int> Field2 { get; set; }

    [Ignore]
    public int Count { get; set; }
  }

  public class SequenceImpl<T> : ISequence<SequenceImpl<T>, T> { 
  }
}");
      BinarySchemaTestUtil.AssertDiagnostics(
          structure.Diagnostics);
    }
  }
}