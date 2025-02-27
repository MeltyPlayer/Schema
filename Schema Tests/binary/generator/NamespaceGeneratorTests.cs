﻿using NUnit.Framework;


namespace schema.binary.text;

internal class NamespaceGeneratorTests {
  [Test]
  public void TestFromSameNamespace() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;

        namespace foo.bar;

        public enum A : byte {
        }

        [BinarySchema]
        public partial class Wrapper : IBinaryConvertible {
          public A Field { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = (A) br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestFromHigherNamespace() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;

        namespace foo {
          public enum A : byte {
          }
        
          namespace bar {
            [BinarySchema]
            public partial class Wrapper : IBinaryConvertible {
              public foo.A Field { get; set; }
            }
          }
        }
        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = (A) br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestFromLowerNamespace() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;

        namespace foo.bar {
          namespace goo {
            public enum A : byte {
            }
          }
        
          [BinarySchema]
          public partial class Wrapper : IBinaryConvertible {
            public goo.A Field { get; set; }
          }
        }
        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = (goo.A) br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;
        
        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestFromSimilarNamespace() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;

        namespace foo.bar {
          namespace goo {
            public enum A : byte {
            }
          }
        
          namespace gar {
            [BinarySchema]
            public partial class Wrapper : IBinaryConvertible {
              public foo.bar.goo.A Field { get; set; }
            }
          }
        }
        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar.gar;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = (foo.bar.goo.A) br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar.gar;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestBothInGlobalNamespace() {
    BinarySchemaTestUtil.AssertGenerated(
        """
        using schema.binary;

        public enum A : byte {
        }

        [BinarySchema]
        public partial class Wrapper : IBinaryConvertible {
          public A Field { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = (A) br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestFromGlobalNamespace() {
    BinarySchemaTestUtil.AssertGenerated(
        """
        using schema.binary;

        namespace foo {
          public enum A : byte {
          }
        }

        [BinarySchema]
        public partial class Wrapper : IBinaryConvertible {
          public foo.A Field { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = (foo.A) br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.Field);
          }
        }

        """);
  }

  [Test]
  public void TestInGlobalNamespace() {
    BinarySchemaTestUtil.AssertGenerated(
        """
        using schema.binary;
          
        public enum A : byte {
        }

        namespace foo {
          [BinarySchema]
          public partial class Wrapper : IBinaryConvertible {
            public A Field { get; set; }
          }
        }
        """,
        """
        using System;
        using schema.binary;
        
        namespace foo;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.Field = (global::A) br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo;
        
        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteByte((byte) this.Field);
          }
        }

        """);
  }
}