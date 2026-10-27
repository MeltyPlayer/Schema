using NUnit.Framework;


namespace schema.binary.attributes;

internal class SizeOfMemberInBytesGeneratorTests {
  [Test]
  public void TestSizeOfInStructure() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class SizeWrapper : IBinaryConvertible {
          [WSizeOfMemberInBytes(nameof(Foo)]
          public uint FooSize { get; set; }
        
          public byte Foo;
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x5<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - FooSize: 0x0<br/>
        ///     - Foo: 0x4<br/>
        /// </remarks>
        public partial class SizeWrapper {
          public const int SCHEMA_SIZE = 0x5;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class SizeWrapper {
          public void Read(IBinaryReader br) {
            this.FooSize = br.ReadUInt32();
            this.Foo = br.ReadByte();
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class SizeWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteUInt32Delayed(bw.GetSizeOfMemberRelativeToScope("Foo").ContinueWith(task => (uint) task.Result));
            bw.MarkStartOfMember("Foo");
            bw.WriteByte(this.Foo);
            bw.MarkEndOfMember();
          }
        }

        """);
  }

  [Test]
  public void TestSizeOfThroughChild() {
    BinarySchemaTestUtil.AssertGenerated(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class SizeWrapper : IBinaryConvertible {
          [WSizeOfMemberInBytes(nameof(Foo.Bar)]
          public uint FooBarSize { get; set; }
        
          public Child Foo;
        }

        [BinarySchema]
        public partial class Child : IBinaryConvertible {
          public byte Bar;
        }
        """,
        """
        namespace foo.bar;

        /// <remarks>
        ///   Size: 0x5<br/>
        ///<br/>
        ///   Offsets:<br/>
        ///     - FooBarSize: 0x0<br/>
        ///     - Foo: 0x4<br/>
        /// </remarks>
        public partial class SizeWrapper {
          public const int SCHEMA_SIZE = 0x5;
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class SizeWrapper {
          public void Read(IBinaryReader br) {
            this.FooBarSize = br.ReadUInt32();
            this.Foo.Read(br);
          }
        }

        """,
        """
        using System;
        using schema.binary;

        namespace foo.bar;

        public partial class SizeWrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteUInt32Delayed(bw.GetSizeOfMemberRelativeToScope("Foo.Bar").ContinueWith(task => (uint) task.Result));
            bw.MarkStartOfMember("Foo");
            this.Foo.Write(bw);
            bw.MarkEndOfMember();
          }
        }

        """);
  }

  [Test]
  public void TestSizeOfThroughParent() {
    BinarySchemaTestUtil.AssertGeneratedForAll(
        """

        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;
        
        [BinarySchema]
        public partial class SizeWrapper : IChildOf<ParentImpl>, IBinaryConvertible {
          public ParentImpl Parent;
      
          [WSizeOfMemberInBytes(nameof(Parent.Foo)]
          public uint FooSize { get; set; }
        }
      
        [BinarySchema]
        public partial class ParentImpl : IBinaryConvertible {
          public SizeWrapper Child;
      
          public byte Foo;
        }
        """,
        // Size Wrapper                                           
        ("""
         using System;
         using schema.binary;

         namespace foo.bar;
         
         public partial class SizeWrapper {
           public void Read(IBinaryReader br) {
             this.FooSize = br.ReadUInt32();
           }
         }

         """,
         """
         using System;
         using schema.binary;

         namespace foo.bar;
         
         public partial class SizeWrapper {
           public void Write(IBinaryWriter bw) {
             bw.WriteUInt32Delayed(bw.GetSizeOfMemberRelativeToScope("Foo").ContinueWith(task => (uint) task.Result));
           }
         }

         """),
        // Parent Impl
        ("""
         using System;
         using schema.binary;

         namespace foo.bar;
         
         public partial class ParentImpl {
           public void Read(IBinaryReader br) {
             this.Child.Parent = this;
             this.Child.Read(br);
             this.Foo = br.ReadByte();
           }
         }

         """,
         """
         using System;
         using schema.binary;

         namespace foo.bar;
         
         public partial class ParentImpl {
           public void Write(IBinaryWriter bw) {
             this.Child.Parent = this;
             this.Child.Write(bw);
             bw.MarkStartOfMember("Foo");
             bw.WriteByte(this.Foo);
             bw.MarkEndOfMember();
           }
         }

         """));
  }
}