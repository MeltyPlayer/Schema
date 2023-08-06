using System.IO;

using NUnit.Framework;

using schema.binary;
using schema.binary.attributes;

namespace build {
  public partial class StringLengthSourceTests {
    [BinarySchema]
    public partial class MultipleStringWrapper : IBinaryConvertible {
      [WLengthOfString(nameof(String1))]
      [WLengthOfString(nameof(String2))]
      private uint length_;

      [RStringLengthSource(nameof(length_))]
      public string String1 { get; set; }

      [RStringLengthSource(nameof(length_))]
      public string String2 { get; set; }

      public override bool Equals(object other) {
        if (other is MultipleStringWrapper otherStringWrapper) {
          return this.String1.Equals(
                     otherStringWrapper.String1) &&
                 this.String2.Equals(
                     otherStringWrapper.String2);
        }

        return false;
      }

      public override string ToString()
        => $"{this.String1}, {this.String2}";
    }

    [Test]
    public void TestWriteAndReadMultiple() {
      var expectedSw = new MultipleStringWrapper {
          String1 = "holy",
          String2 = "moly",
      };

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new EndianBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      ms.Position = 0;
      var er = new EndianBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<MultipleStringWrapper>();

      Assert.AreEqual(expectedSw, actualSw);
    }
  }
}