using NUnit.Framework;


namespace schema.binary.text;

internal class PositionGeneratorTests {
  [Test]
  public void TestPosition() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        [BinarySchema]
        public partial class PositionWrapper : IBinaryConvertible {
          [RPositionRelativeToStream]
          public long Position { get; set; }
      
          public byte Value { get; set; }
      
          [RPositionRelativeToStream]
          public long ExpectedPosition { get; }
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x11<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Position: 0x0<br/>
        ///     - Value: 0x8<br/>
        ///     - ExpectedPosition: 0x9<br/>
        /// </remarks>
        public partial class PositionWrapper {
          public const int SCHEMA_SIZE = 0x11;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class PositionWrapper {
          public void Read(IBinaryReader br) {
            this.Position = br.Position;
            this.Value = br.ReadByte();
            br.AssertPosition(this.ExpectedPosition);
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class PositionWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte(this.Value);
          }
        }

        """);
  }
}