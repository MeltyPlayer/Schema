using NUnit.Framework;


namespace schema.binary.attributes;

internal class IIndexedGeneratorTests {
  [Test]
  public void TestSkipsIndexAutomatically() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class IndexedImpl : IBinaryConvertible, IIndexed {
          public int Index { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class IndexedImpl {
          public void Read(IBinaryReader br) {
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class IndexedImpl {
          public void Write(IBinaryWriter bw) {
          }
        }

        """);
  }

  [Test]
  public void TestReadsIndexedClassInArray() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class Wrapper : IBinaryConvertible {
          [SequenceLengthSource(10)]
          public IValue[] Field { get; set; }
        }

        public partial class IValue : IBinaryConvertible, IIndexed;
        """,
        """
        using System;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 10);
            for (var i = 0; i < this.Field.Length; ++i) {
              var e = this.Field[i];
              e.Index = i;
              e.Read(br);
            }
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            foreach (var e in this.Field) {
              e.Write(bw);
            }
          }
        }

        """);
  }

  [Test]
  public void TestReadsIndexedClassInList() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;
        using System.Collections.Generic;

        namespace foo.bar;
        
        [BinarySchema]
        public partial class Wrapper : IBinaryConvertible {
          [SequenceLengthSource(10)]
          public List<IValue> Field { get; set; }
        }
      
        public partial class IValue : IBinaryConvertible, IIndexed;
        """,
        """
        using System;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;
        
        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            SequencesUtil.ResizeSequenceInPlace(this.Field, 10);
            for (var i = 0; i < this.Field.Count; ++i) {
              var e = this.Field[i];
              e.Index = i;
              e.Read(br);
            }
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            foreach (var e in this.Field) {
              e.Write(bw);
            }
          }
        }

        """);
  }

  [Test]
  public void TestReadsIndexedStructInArray() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class Wrapper : IBinaryConvertible {
          [SequenceLengthSource(10)]
          public IValue[] Field { get; set; }
        }

        public partial struct IValue : IBinaryConvertible, IIndexed;
        """,
        """
        using System;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 10);
            for (var i = 0; i < this.Field.Length; ++i) {
              var e = this.Field[i];
              e.Index = i;
              e.Read(br);
              this.Field[i] = e;
            }
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            foreach (var e in this.Field) {
              e.Write(bw);
            }
          }
        }

        """);
  }

  [Test]
  public void TestReadsIndexedStructInList() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;
        using System.Collections.Generic;

        namespace foo.bar;

        [BinarySchema]
        public partial class Wrapper : IBinaryConvertible {
          [SequenceLengthSource(10)]
          public List<IValue> Field { get; set; }
        }

        public partial struct IValue : IBinaryConvertible, IIndexed;
        """,
        """
        using System;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            SequencesUtil.ResizeSequenceInPlace(this.Field, 10);
            for (var i = 0; i < this.Field.Count; ++i) {
              var e = this.Field[i];
              e.Index = i;
              e.Read(br);
              this.Field[i] = e;
            }
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            foreach (var e in this.Field) {
              e.Write(bw);
            }
          }
        }

        """);
  }
}