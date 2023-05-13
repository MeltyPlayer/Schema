using NUnit.Framework;


namespace schema.binary.attributes.memory {
  internal class MemoryGeneratorTests {
    /*[Test]
    public void TestOtherFieldBlock() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.memory;
using schema.memory;

namespace foo.bar {
  [BinarySchema]
  public partial class BlockWrapper {
    public long Size;

    [Block(nameof(Size))]
    public IMemoryBlock Block;

    public long Offset;

    [Pointer(nameof(Block), nameof(Offset))]
    public float Field;
  }
}",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class BlockWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Size = er.ReadInt64();
      this.Block = new MemoryBlock(MemoryBlockType.DATA, this.Size);
      this.Offset = er.ReadInt64();
      this.Pointer = this.Block.ClaimPointerWithin(
        this.Offset,
        er => {
          this.Field = ew.ReadSingle();
        },
        ew => {
          ew.WriteSingle(this.Field);
        });
      this.Pointer.Read(er);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class BlockWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt64(this.Size);
      er.WriteInt64(this.Offset);
      this.Pointer.Write(ew);
    }
  }
}
");
    }*/
  }
}