using System;

using NUnit.Framework;

using schema.testing;


namespace schema.binary {
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

    private class BinarySchemaThatWillFail : IBinaryDeserializable {
      public void Read(IBinaryReader br) {
        br.ReadByte();
        throw new Exception();
      }
    }

    [Test]
    public void TestTryReadNewFails() {
      using var br = SchemaMemoryStream.From([123456]).GetBinaryReader();
      Assert.False(
          br.TryReadNew<BinarySchemaThatWillFail>(out var successful));
      Assert.AreEqual(0, br.Position);
      Assert.Null(successful);
    }
  }
}