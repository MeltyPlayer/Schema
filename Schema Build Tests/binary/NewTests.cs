using System;

using NUnit.Framework;

using schema.testing;


namespace schema.binary;

public partial class NewTests {
  [BinarySchema]
  private partial class BinarySchemaThatWillSucceed : IBinaryConvertible {
    public int Value { get; set; }
  }

  [Test]
  public void TestTryReadNewSucceeds() {
    using var br = SchemaMemoryStream.From([123456]).GetBinaryReader();
    Assert.True(
        br.TryReadNew<BinarySchemaThatWillSucceed>(out var successful));
    Assert.AreEqual(4, br.Position);
    Assert.AreEqual(123456, successful.Value);
  }

  [Test]
  public void TestTryReadNewFailsDueToEofError() {
    using var br = SchemaMemoryStream.From(Array.Empty<byte>())
                                     .GetBinaryReader();
    Assert.False(
        br.TryReadNew<BinarySchemaThatWillSucceed>(out var successful));
    Assert.AreEqual(0, br.Position);
    Assert.Null(successful);
  }

  private class BinarySchemaThatWillFailWithNonSchemaError
      : IBinaryDeserializable {
    public void Read(IBinaryReader br) {
      br.ReadByte();
      throw new Exception();
    }
  }

  [Test]
  public void TestTryReadNewFailsDueToNonSchemaError() {
    using var br = SchemaMemoryStream.From([123456]).GetBinaryReader();
    Assert.Throws<Exception>(
        () => {
          br.TryReadNew<BinarySchemaThatWillFailWithNonSchemaError>(out _);
        });
    Assert.AreEqual(0, br.Position);
  }
}