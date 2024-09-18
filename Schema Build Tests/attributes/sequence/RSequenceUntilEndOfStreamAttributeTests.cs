﻿using System.IO;
using System.Linq;

using NUnit.Framework;

using schema.binary;
using schema.binary.attributes;


namespace build;

public partial class RSequenceUntilEndOfStreamAttributeTests {
  [BinarySchema]
  public partial class ByteSequenceWrapper : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public byte[] Values { get; set; }

    public override bool Equals(object other) {
      if (other is ByteSequenceWrapper otherSequenceWrapper) {
        return this.Values.SequenceEqual(otherSequenceWrapper.Values);
      }

      return false;
    }

    public override string ToString() => string.Join(", ", Values);
  }

  [Test]
  public void TestWriteAndReadBytes() {
    var expectedSw = new ByteSequenceWrapper {
        Values = new byte[] { 1, 2, 3, 4, 5, 9, 8, 7, 6 }
    };

    var ms = new MemoryStream();

    var endianness = Endianness.BigEndian;

    var ew = new SchemaBinaryWriter(endianness);
    expectedSw.Write(ew);
    ew.CompleteAndCopyTo(ms);

    ms.Position = 0;
    var er = new SchemaBinaryReader(ms, endianness);
    var actualSw = er.ReadNew<ByteSequenceWrapper>();
    Assert.AreEqual(expectedSw, actualSw);
  }

  [Test]
  public void TestWriteAndReadBytesInSubstream() {
    var bytes = new byte[] { 0, 0, 0, 1, 2, 3, 4, 5, 6, 0, 0, 0, };
    var expectedSw = new ByteSequenceWrapper {
        Values = new byte[] { 1, 2, 3, 4, 5, 6 }
    };

    var ms = new MemoryStream(bytes);
    var er = new SchemaBinaryReader(ms);

    ByteSequenceWrapper actualSw = default;
    er.SubreadAt(3,
                 expectedSw.Values.Length,
                 ser => {
                   Assert.AreEqual(3, ser.Position);
                   Assert.AreEqual(9, ser.Length);

                   actualSw = ser.ReadNew<ByteSequenceWrapper>();

                   Assert.AreEqual(9, ser.Position);
                 });

    Assert.AreEqual(expectedSw, actualSw);
  }

  [Test]
  public void TestWriteAndReadBytesInLocalSubstream() {
    var bytes = new byte[] { 0, 0, 0, 1, 2, 3, 4, 5, 6, 0, 0, 0, };
    var expectedSw = new ByteSequenceWrapper {
        Values = new byte[] { 1, 2, 3, 4, 5, 6 }
    };

    var ms = new MemoryStream(bytes);
    var er = new SchemaBinaryReader(ms);

    er.Position = 1;
    er.PushLocalSpace();
    Assert.AreEqual(0, er.Position);

    ByteSequenceWrapper actualSw = default;
    er.SubreadAt(2,
                 expectedSw.Values.Length,
                 ser => {
                   Assert.AreEqual(2, ser.Position);
                   Assert.AreEqual(8, ser.Length);

                   actualSw = ser.ReadNew<ByteSequenceWrapper>();

                   Assert.AreEqual(8, ser.Position);
                 });

    Assert.AreEqual(expectedSw, actualSw);
  }

  [BinarySchema]
  public partial class IntSequenceWrapper : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public int[] Values { get; set; }

    public override bool Equals(object other) {
      if (other is IntSequenceWrapper otherSequenceWrapper) {
        return this.Values.SequenceEqual(otherSequenceWrapper.Values);
      }

      return false;
    }

    public override string ToString() => string.Join(", ", Values);
  }

  [Test]
  public void TestWriteAndReadInts() {
    var expectedSw = new IntSequenceWrapper {
        Values = new[] { 1, 2, 3, 4, 5, 9, 8, 7, 6 }
    };

    var ms = new MemoryStream();

    var endianness = Endianness.BigEndian;

    var ew = new SchemaBinaryWriter(endianness);
    expectedSw.Write(ew);
    ew.CompleteAndCopyTo(ms);

    ms.Position = 0;
    var er = new SchemaBinaryReader(ms, endianness);
    var actualSw = er.ReadNew<IntSequenceWrapper>();
    Assert.AreEqual(expectedSw, actualSw);
  }


  [BinarySchema]
  public partial class Vector3f : IBinaryConvertible {
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public override bool Equals(object other) {
      if (other is Vector3f otherVector) {
        return X == otherVector.X &&
               Y == otherVector.Y &&
               Z == otherVector.Z;
      }

      return false;
    }

    public override string ToString() => $"({X}, {Y}, {Z})";
  }

  [BinarySchema]
  public partial class FloatClassSequenceWrapper : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public Vector3f[] Values { get; set; }

    public override bool Equals(object other) {
      if (other is FloatClassSequenceWrapper otherSequenceWrapper) {
        return this.Values.SequenceEqual(otherSequenceWrapper.Values);
      }

      return false;
    }

    public override string ToString()
      => string.Join(", ", Values.Select(value => value.ToString()));
  }

  [Test]
  public void TestWriteAndReadClasses() {
    var expectedSw = new FloatClassSequenceWrapper {
        Values = new Vector3f[] {
            new() { X = 1, Y = 2, Z = 3 },
            new() { X = 2, Y = 3, Z = 4 },
            new() { X = 3, Y = 4, Z = 5 },
        },
    };

    var ms = new MemoryStream();

    var endianness = Endianness.BigEndian;

    var ew = new SchemaBinaryWriter(endianness);
    expectedSw.Write(ew);
    ew.CompleteAndCopyTo(ms);

    ms.Position = 0;
    var er = new SchemaBinaryReader(ms, endianness);
    var actualSw = er.ReadNew<FloatClassSequenceWrapper>();
    Assert.AreEqual(expectedSw, actualSw);
  }


  [BinarySchema]
  public partial class Vector3b : IBinaryConvertible {
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte Z { get; set; }

    public override bool Equals(object other) {
      if (other is Vector3b otherVector) {
        return X == otherVector.X &&
               Y == otherVector.Y &&
               Z == otherVector.Z;
      }

      return false;
    }

    public override string ToString() => $"({X}, {Y}, {Z})";
  }

  [BinarySchema]
  public partial class ByteClassSequenceWrapper : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public Vector3b[] Values { get; set; }

    public override bool Equals(object other) {
      if (other is ByteClassSequenceWrapper otherSequenceWrapper) {
        return this.Values.SequenceEqual(otherSequenceWrapper.Values);
      }

      return false;
    }

    public override string ToString()
      => string.Join(", ", Values.Select(value => value.ToString()));
  }

  [Test]
  public void TestWriteAndReadClassesInLocalSubstream() {
    var bytes = new byte[] { 0, 0, 0, 1, 2, 3, 2, 3, 4, 3, 4, 5, 0, 0, 0, };
    var expectedSw = new ByteClassSequenceWrapper {
        Values = new Vector3b[] {
            new() { X = 1, Y = 2, Z = 3 },
            new() { X = 2, Y = 3, Z = 4 },
            new() { X = 3, Y = 4, Z = 5 },
        },
    };

    var ms = new MemoryStream(bytes);
    var er = new SchemaBinaryReader(ms);

    er.Position = 1;
    er.PushLocalSpace();
    Assert.AreEqual(0, er.Position);

    ByteClassSequenceWrapper actualSw = default;
    er.SubreadAt(2,
                 3 * expectedSw.Values.Length,
                 ser => {
                   Assert.AreEqual(2, ser.Position);
                   Assert.AreEqual(11, ser.Length);

                   actualSw = ser.ReadNew<ByteClassSequenceWrapper>();

                   Assert.AreEqual(11, ser.Position);
                 });

    Assert.AreEqual(expectedSw, actualSw);
  }
}