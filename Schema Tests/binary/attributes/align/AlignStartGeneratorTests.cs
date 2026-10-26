using NUnit.Framework;


namespace schema.binary.attributes;

internal class AlignStartGeneratorTests {
  [Test]
  public void TestConstAlign() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class AlignWrapper : IBinaryConvertible {
          [AlignStart(0x2)]
          public byte Field { get; set; }
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: (indeterminate)<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Align: 0x0<br/>
        ///     - Field: (indeterminate)<br/>
        /// </remarks>
        public partial class AlignWrapper;

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class AlignWrapper {
          public void Read(IBinaryReader br) {
            br.Align(2);
            this.Field = br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class AlignWrapper {
          public void Write(IBinaryWriter bw) {
            bw.Align(2);
            bw.WriteByte(this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestOtherAlign() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class AlignWrapper : IBinaryConvertible {
          public uint Value { get; set; }
        
          [AlignStart(nameof(Value))]
          public byte Field { get; set; }
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: (indeterminate)<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Value: 0x0<br/>
        ///     - Align: 0x4<br/>
        ///     - Field: (indeterminate)<br/>
        /// </remarks>
        public partial class AlignWrapper;

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class AlignWrapper {
          public void Read(IBinaryReader br) {
            this.Value = br.ReadUInt32();
            br.Align(Value);
            this.Field = br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class AlignWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteUInt32(this.Value);
            bw.Align(Value);
            bw.WriteByte(this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestNestedOtherAlign() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class AlignWrapper : IBinaryConvertible {
          public Wrapper Wrapper { get; set; }
        
          [AlignStart(nameof(Wrapper.Value))]
          public byte Field { get; set; }
        }

        public partial class Wrapper : IBinaryConvertible {
          public uint Value { get; set; }
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: (indeterminate)<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Wrapper: 0x0<br/>
        ///     - Align: 0x4<br/>
        ///     - Field: (indeterminate)<br/>
        /// </remarks>
        public partial class AlignWrapper;

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class AlignWrapper {
          public void Read(IBinaryReader br) {
            this.Wrapper.Read(br);
            br.Align(Wrapper.Value);
            this.Field = br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class AlignWrapper {
          public void Write(IBinaryWriter bw) {
            this.Wrapper.Write(bw);
            bw.Align(Wrapper.Value);
            bw.WriteByte(this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestAlignWithImmediate() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        [BinarySchema]
        public partial class AlignWrapper : IBinaryConvertible {
          [AlignStart(0x2)]
          [SequenceLengthSource(SchemaIntegerType.UINT32)]
          public int[] Field { get; set; }
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: (indeterminate)<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Align: 0x0<br/>
        ///     - Field.Length: (indeterminate)<br/>
        ///     - Field: (indeterminate)<br/>
        /// </remarks>
        public partial class AlignWrapper;

        """,
        """
        using System;
        using schema.binary;
        using schema.util.sequences;

        namespace foo.bar;
        
        public partial class AlignWrapper {
          public void Read(IBinaryReader br) {
            br.Align(2);
            {
              var c = br.ReadUInt32();
              this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, (int) c);
            }
            br.ReadInt32s(this.Field);
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class AlignWrapper {
          public void Write(IBinaryWriter bw) {
            bw.Align(2);
            bw.WriteUInt32((uint) this.Field.Length);
            bw.WriteInt32s(this.Field);
          }
        }

        """);
  }
}