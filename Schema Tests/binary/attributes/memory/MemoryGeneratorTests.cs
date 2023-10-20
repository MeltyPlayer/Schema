namespace schema.binary.attributes {
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

namespace foo.bar {
  public partial class BlockWrapper {
    public void Read(IBinaryReader br) {
      this.Size = br.ReadInt64();
      this.Block = new MemoryBlock(MemoryBlockType.DATA, this.Size);
      this.Offset = br.ReadInt64();
      this.Pointer = this.Block.ClaimPointerWithin(
        this.Offset,
        br => {
          this.Field = bw.ReadSingle();
        },
        bw => {
          bw.WriteSingle(this.Field);
        });
      this.Pointer.Read(br);
    }
  }
}
",
                                     @"using System;

namespace foo.bar {
  public partial class BlockWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteInt64(this.Size);
      br.WriteInt64(this.Offset);
      this.Pointer.Write(bw);
    }
  }
}
");
    }*/
  }
}