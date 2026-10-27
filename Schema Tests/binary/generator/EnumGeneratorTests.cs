using NUnit.Framework;


namespace schema.binary.text;

internal class EnumGeneratorTests {
  [Test]
  public void TestReadEnum() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        enum A {}

        enum B : int {
        }

        [BinarySchema]
        public partial class EnumWrapper {
          [IntegerFormat(SchemaIntegerType.BYTE)]
          public A fieldA;
        
          public B fieldB;
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x5<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - fieldA: 0x0<br/>
        ///     - fieldB: 0x1<br/>
        /// </remarks>
        public partial class EnumWrapper {
          public const int SCHEMA_SIZE = 0x5;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class EnumWrapper {
          public void Read(IBinaryReader br) {
            this.fieldA = (A) br.ReadByte();
            this.fieldB = (B) br.ReadInt32();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class EnumWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.fieldA);
            bw.WriteInt32((int) this.fieldB);
          }
        }

        """);
  }

  [Test]
  public void TestAssertEnum() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        enum A {}
      
        enum B : int {
        }
       
        [BinarySchema]
        public partial class EnumWrapper {
          [IntegerFormat(SchemaIntegerType.BYTE)]
          public readonly A fieldA;
      
          public readonly B fieldB;
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x5<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - fieldA: 0x0<br/>
        ///     - fieldB: 0x1<br/>
        /// </remarks>
        public partial class EnumWrapper {
          public const int SCHEMA_SIZE = 0x5;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class EnumWrapper {
          public void Read(IBinaryReader br) {
            br.AssertByte((byte) this.fieldA);
            br.AssertInt32((int) this.fieldB);
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class EnumWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.fieldA);
            bw.WriteInt32((int) this.fieldB);
          }
        }

        """);
  }

  [Test]
  public void TestReadEnumArray() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        enum A {}

        enum B : int {
        }

        [BinarySchema]
        public partial class EnumWrapper {
          [IntegerFormat(SchemaIntegerType.BYTE)]
          public readonly A[] fieldA = new A[5];
        
          public readonly B[] fieldB = new B[5];
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: (indeterminate)<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - fieldA: 0x0<br/>
        ///     - fieldB: (indeterminate)<br/>
        /// </remarks>
        public partial class EnumWrapper;

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class EnumWrapper {
          public void Read(IBinaryReader br) {
            for (var i = 0; i < this.fieldA.Length; ++i) {
              this.fieldA[i] = (A) br.ReadByte();
            }
            for (var i = 0; i < this.fieldB.Length; ++i) {
              this.fieldB[i] = (B) br.ReadInt32();
            }
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class EnumWrapper {
          public void Write(IBinaryWriter bw) {
            for (var i = 0; i < this.fieldA.Length; ++i) {
              bw.WriteByte((byte) this.fieldA[i]);
            }
            for (var i = 0; i < this.fieldB.Length; ++i) {
              bw.WriteInt32((int) this.fieldB[i]);
            }
          }
        }

        """);
  }
}