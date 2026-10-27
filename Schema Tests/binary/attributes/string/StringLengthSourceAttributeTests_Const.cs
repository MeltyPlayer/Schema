using NUnit.Framework;


namespace schema.binary.attributes;

internal partial class StringLengthSourceAttributeTests {
  [Test]
  public void TestConstString() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;

        namespace foo.bar;

        [BinarySchema]
        public partial class StringWrapper {
          public readonly string Field = "foo";
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: (indeterminate)<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Field: 0x0<br/>
        /// </remarks>
        public partial class StringWrapper;

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class StringWrapper {
          public void Read(IBinaryReader br) {
            br.AssertString(this.Field);
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class StringWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteString(this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestConstLengthString() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class StringWrapper {
          [StringLengthSource(3)]
          public string Field;
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x3<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Field: 0x0<br/>
        /// </remarks>
        public partial class StringWrapper {
          public const int SCHEMA_SIZE = 0x3;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class StringWrapper {
          public void Read(IBinaryReader br) {
            this.Field = br.ReadString(3);
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class StringWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteStringWithExactLength(this.Field, 3);
          }
        }

        """);
  }

  [Test]
  public void TestConstLengthString0() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        [BinarySchema]
        public partial class StringWrapper {
          [StringLengthSource((int) 0)]
          public string Field;
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x0<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - Field: 0x0<br/>
        /// </remarks>
        public partial class StringWrapper {
          public const int SCHEMA_SIZE = 0x0;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class StringWrapper {
          public void Read(IBinaryReader br) {
            this.Field = "";
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class StringWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteStringWithExactLength(this.Field, 0);
          }
        }

        """);
  }
}