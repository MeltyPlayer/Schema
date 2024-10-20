using System;

using NUnit.Framework;

using schema.binary.attributes;
using schema.testing;


namespace schema.binary;

public partial class NoEffectTests {
  [BinarySchema]
  private partial class NullArray : IBinaryConvertible {
    [Skip]
    private readonly int position_ = 0;

    [RAtPositionOrNull(nameof(position_))]
    [SequenceLengthSource(1)]
    public byte[]? Values { get; set; }
  }

  [Test]
  public void TestReadingNullArraySucceeds() {
    using var br = SchemaMemoryStream.From(Array.Empty<byte>())
                                     .GetBinaryReader();
    Assert.NotNull(br.ReadNew<NullArray>());
  }

  [BinarySchema]
  private partial class EmptyArray : IBinaryConvertible {
    [SequenceLengthSource((uint) 0)]
    public byte[] Values { get; set; }
  }

  [Test]
  public void TestReadingEmptyArraySucceeds() {
    using var br = SchemaMemoryStream.From(Array.Empty<byte>())
                                     .GetBinaryReader();
    Assert.NotNull(br.ReadNew<EmptyArray>());
  }
}