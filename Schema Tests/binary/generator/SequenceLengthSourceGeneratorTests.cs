using NUnit.Framework;


namespace schema.binary.text;

internal class SequenceLengthSourceGeneratorTests {
  [Test]
  public void TestConstLength() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using System.Collections.Generic;
        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        [BinarySchema]
        public partial class ConstLengthWrapper : IBinaryConvertible {
          [SequenceLengthSource(3)]
          public int[] Field { get; set; }
      
          [SequenceLengthSource(3)]
          public int[]? NullableField { get; set; }
      
          [Skip]
          private bool Toggle { get; set; }
      
          [RIfBoolean(nameof(Toggle))]
          [SequenceLengthSource(3)]
          public int[]? IfBooleanArray { get; set; }
      
          [RIfBoolean(nameof(Toggle))]
          [SequenceLengthSource(3)]
          public List<int>? IfBooleanList { get; set; }
        }
        """,
        """
        using System;
        using System.Collections.Generic;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;
        
        public partial class ConstLengthWrapper {
          public void Read(IBinaryReader br) {
            this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 3);
            br.ReadInt32s(this.Field);
            this.NullableField = SequencesUtil.CloneAndResizeSequence(this.NullableField, 3);
            br.ReadInt32s(this.NullableField);
            if (this.Toggle) {
              this.IfBooleanArray = SequencesUtil.CloneAndResizeSequence(this.IfBooleanArray, 3);
              br.ReadInt32s(this.IfBooleanArray);
            }
            else {
              this.IfBooleanArray = null;
            }
            if (this.Toggle) {
              this.IfBooleanList = new System.Collections.Generic.List<int>();
              SequencesUtil.ResizeSequenceInPlace(this.IfBooleanList, 3);
              for (var i = 0; i < this.IfBooleanList.Count; ++i) {
                this.IfBooleanList[i] = br.ReadInt32();
              }
            }
            else {
              this.IfBooleanList = null;
            }
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class ConstLengthWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteInt32s(this.Field);
            if (this.NullableField != null) {
              bw.WriteInt32s(this.NullableField);
            }
            if (this.IfBooleanArray != null) {
              bw.WriteInt32s(this.IfBooleanArray);
            }
            if (this.IfBooleanList != null) {
              for (var i = 0; i < this.IfBooleanList.Count; ++i) {
                bw.WriteInt32(this.IfBooleanList[i]);
              }
            }
          }
        }

        """);
  }

  [Test]
  public void TestConstLength0() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using System.Collections.Generic;
        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class ConstLengthWrapper : IBinaryConvertible {
          [SequenceLengthSource((uint) 0)]
          public int[] Field { get; set; }
        
          [SequenceLengthSource((uint) 0)]
          public int[]? NullableField { get; set; }
        
          [Skip]
          private bool Toggle { get; set; }
        
          [RIfBoolean(nameof(Toggle))]
          [SequenceLengthSource((uint) 0)]
          public int[]? IfBooleanArray { get; set; }
        
          [RIfBoolean(nameof(Toggle))]
          [SequenceLengthSource((uint) 0)]
          public List<int>? IfBooleanList { get; set; }
        }
        """,
        """
        using System;
        using System.Collections.Generic;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;

        public partial class ConstLengthWrapper {
          public void Read(IBinaryReader br) {
            this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 0);
            this.NullableField = SequencesUtil.CloneAndResizeSequence(this.NullableField, 0);
            if (this.Toggle) {
              this.IfBooleanArray = SequencesUtil.CloneAndResizeSequence(this.IfBooleanArray, 0);
            }
            else {
              this.IfBooleanArray = null;
            }
            if (this.Toggle) {
              this.IfBooleanList = new System.Collections.Generic.List<int>();
              SequencesUtil.ResizeSequenceInPlace(this.IfBooleanList, 0);
            }
            else {
              this.IfBooleanList = null;
            }
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class ConstLengthWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteInt32s(this.Field);
            if (this.NullableField != null) {
              bw.WriteInt32s(this.NullableField);
            }
            if (this.IfBooleanArray != null) {
              bw.WriteInt32s(this.IfBooleanArray);
            }
            if (this.IfBooleanList != null) {
              for (var i = 0; i < this.IfBooleanList.Count; ++i) {
                bw.WriteInt32(this.IfBooleanList[i]);
              }
            }
          }
        }

        """);
  }

  [Test]
  public void TestISequence() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;
        using schema.util.sequences;

        namespace foo.bar;

        [BinarySchema]
        public partial class ConstLengthWrapper : IBinaryConvertible {
          [SequenceLengthSource(3)]
          public SequenceImpl<int, int> Field { get; } = new();
        }

        public class SequenceImpl<T1, T2> : ISequence<SequenceImpl<(T1 First, T2 Second)>, (T1 First, T2 Second)> { 
        }
        """,
        """
        using System;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;

        public partial class ConstLengthWrapper {
          public void Read(IBinaryReader br) {
            SequencesUtil.ResizeSequenceInPlace(this.Field, 3);
            this.Field.Read(br);
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class ConstLengthWrapper {
          public void Write(IBinaryWriter bw) {
            this.Field.Write(bw);
          }
        }

        """);
  }
}