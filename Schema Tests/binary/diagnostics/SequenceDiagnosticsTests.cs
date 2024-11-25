using NUnit.Framework;


namespace schema.binary.text;

internal class SequenceDiagnosticsTests {
  [Test]
  public void TestFailsIfOutOfOrder() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;
                                                    using schema.binary.attributes;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public partial class ByteWrapper : IBinaryConvertible {
                                                        [RSequenceLengthSource("Count")]
                                                        public int[] Field { get; set; }
                                                    
                                                        public int Count { get; set; }
                                                      }
                                                    }
                                                    """);
    BinarySchemaTestUtil.AssertDiagnostics(
        structure.Diagnostics,
        Rules.DependentMustComeAfterSource,
        Rules.SourceMustBePrivate);
  }

  [Test]
  public void TestAllowsSkippedOutOfOrder() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;
                                                    using schema.binary.attributes;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public partial class ByteWrapper : IBinaryConvertible {
                                                        [RSequenceLengthSource("Count")]
                                                        public int[] Field { get; set; }
                                                    
                                                        [Skip]
                                                        public int Count { get; set; }
                                                      }
                                                    }
                                                    """);
    BinarySchemaTestUtil.AssertDiagnostics(
        structure.Diagnostics);
  }

  [Test]
  public void TestAllowsSequenceAttributesOnISequence() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;
                                                    using schema.binary.attributes;
                                                    using schema.util.sequences;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public partial class ByteWrapper : IBinaryConvertible {
                                                        [SequenceLengthSource(SchemaIntegerType.UINT32)]
                                                        public SequenceImpl<int> Field1 { get; set; }
                                                    
                                                        [RSequenceLengthSource("Count")]
                                                        public SequenceImpl<int> Field2 { get; set; }
                                                    
                                                        [Skip]
                                                        public int Count { get; set; }
                                                      }
                                                    
                                                      public class SequenceImpl<T> : ISequence<SequenceImpl<T>, T> { 
                                                      }
                                                    }
                                                    """);
    BinarySchemaTestUtil.AssertDiagnostics(
        structure.Diagnostics);
  }

  [Test]
  public void TestAllowsSequenceAttributesOnTupledISequence() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;
                                                    using schema.binary.attributes;
                                                    using schema.util.sequences;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public partial class ByteWrapper : IBinaryConvertible {
                                                        [SequenceLengthSource(SchemaIntegerType.UINT32)]
                                                        public SequenceImpl<int, int> Field1 { get; set; }
                                                    
                                                        [RSequenceLengthSource("Count")]
                                                        public SequenceImpl<int, int> Field2 { get; set; }
                                                    
                                                        [Skip]
                                                        public int Count { get; set; }
                                                      }
                                                    
                                                      public class SequenceImpl<T1, T2> : ISequence<SequenceImpl<(T1 First, T2 Second)>, (T1 First, T2 Second)> { 
                                                      }
                                                    }
                                                    """);
    BinarySchemaTestUtil.AssertDiagnostics(
        structure.Diagnostics);
  }
}