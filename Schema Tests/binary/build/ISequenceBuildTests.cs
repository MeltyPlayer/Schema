using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using schema.binary.attributes.sequence;
using schema.util.enumerables;
using schema.util.sequences;

using IgnoreAttribute = schema.binary.attributes.ignore.IgnoreAttribute;

namespace schema.binary.build {
  public partial class ISequenceBuildTests {
    [BinarySchema]
    public partial class MutableSequenceWrapper : IBinaryConvertible {
      [SequenceLengthSource(SchemaIntegerType.UINT32)]
      public MutableSequenceImpl<IntWrapper> Sequence { get; } = new();

      public override bool Equals(object? otherObj) {
        if (otherObj is MutableSequenceWrapper other) {
          return this.Sequence.Equals(other.Sequence);
        }

        return false;
      }
    }

    [BinarySchema]
    public partial class MutableSequenceImpl<T> 
        : ISequence<MutableSequenceImpl<T>, T>
        where T : IBinaryConvertible, new() {
      [RSequenceLengthSource(nameof(Count))]
      private readonly List<T> impl_ = new();

      public T GetDefault() => new();

      [Ignore]
      public int Count {
        get => this.impl_.Count;
        set => SequencesUtil.ResizeSequenceInPlace(this.impl_, value);
      }

      [Ignore]
      public IReadOnlyList<T> Values => this.impl_;

      [Ignore]
      public T this[int index] {
        get => this.impl_[index];
        set => this.impl_[index] = value;
      }

      public void Clear() => this.impl_.Clear();

      public void ResizeInPlace(int newLength) {
        SequencesUtil.ResizeSequenceInPlace(this.impl_, newLength);
      }

      public void AddRange(IEnumerable<T> values)
        => this.impl_.AddRange(values);

      public MutableSequenceImpl<T> CloneWithNewLength(int newLength) {
        var sequence = new MutableSequenceImpl<T>();
        sequence.AddRange(this.impl_.Resized(newLength));
        return sequence;
      }

      IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
      public IEnumerator<T> GetEnumerator() => this.impl_.GetEnumerator();

      public override bool Equals(object? otherObj) {
        if (otherObj is MutableSequenceImpl<T> other) {
          return this.Values.SequenceEqual(other.Values);
        }

        return false;
      }
    }

    [BinarySchema]
    public partial class IntWrapper : IBinaryConvertible {
      public int Value { get; set; }

      public override bool Equals(object? otherObj) {
        if (otherObj is IntWrapper other) {
          return this.Value == other.Value;
        }

        return false;
      }
    }

    [Test]
    public void TestWriteAndRead() {
      var expectedSw = new MutableSequenceWrapper();
      expectedSw.Sequence.AddRange(
          new[] { 1, 2, 3, 4, 5, 9, 8, 7, 6 }
              .Select(value => new IntWrapper { Value = value }));

      var ms = new MemoryStream();

      var endianness = Endianness.BigEndian;
      var ew = new EndianBinaryWriter(endianness);

      expectedSw.Write(ew);
      ew.CompleteAndCopyToDelayed(ms).Wait();

      ms.Position = 0;
      var er = new EndianBinaryReader(ms, endianness);
      var actualSw = er.ReadNew<MutableSequenceWrapper>();

      Assert.AreEqual(expectedSw, actualSw);
    }
  }
}