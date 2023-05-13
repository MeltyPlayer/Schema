using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

using System.Linq;
using System.Text;

using schema.binary.util;


namespace schema.binary.io {
  public class DelayedContentOutputStreamTests {
    [Test]
    public async Task TestEmptyStream() {
      var impl = new DelayedContentOutputStream();
      var actualBytes = await ToBytes_(impl);
      Assert.AreEqual(0, actualBytes.Length);
    }

    [Test]
    public async Task TestEmptySynchronousStream() {
      var impl = new DelayedContentOutputStream();

      impl.Write(Array.Empty<byte>());

      var actualBytes = await ToBytes_(impl);
      Assert.AreEqual(0, actualBytes.Length);
    }

    [Test]
    public async Task TestEmptyDelayedStream() {
      var impl = new DelayedContentOutputStream();

      impl.WriteDelayed(Task.FromResult(Array.Empty<byte>()));

      var actualBytes = await ToBytes_(impl);
      Assert.AreEqual(0, actualBytes.Length);
    }


    [Test]
    public async Task TestWriteIndividual() {
      var impl = new DelayedContentOutputStream();

      impl.WriteByte(1);
      impl.WriteByte(2);
      impl.WriteByte(3);

      var actualBytes = await ToBytes_(impl);
      AssertSequence_(new byte[] {1, 2, 3}, actualBytes);
    }

    [Test]
    public async Task TestWriteArrays() {
      var impl = new DelayedContentOutputStream();

      impl.Write(new byte[] {1, 2});
      impl.Write(new byte[] {3, 4});
      impl.Write(new byte[] {5, 6});

      var actualBytes = await ToBytes_(impl);
      AssertSequence_(new byte[] {1, 2, 3, 4, 5, 6}, actualBytes);
    }

    [Test]
    public async Task TestWriteDelayed() {
      var impl = new DelayedContentOutputStream();

      impl.WriteDelayed(Task.FromResult(new byte[] {1, 2}));
      impl.WriteDelayed(Task.FromResult(new byte[] {3, 4}));
      impl.WriteDelayed(Task.FromResult(new byte[] {5, 6}));

      var actualBytes = await ToBytes_(impl);
      AssertSequence_(new byte[] {1, 2, 3, 4, 5, 6}, actualBytes);
    }

    [Test]
    public async Task TestWriteEverything() {
      var impl = new DelayedContentOutputStream();

      impl.WriteByte(1);
      impl.Write(new byte[] {2, 3});
      impl.WriteDelayed(Task.FromResult(new byte[] {4, 5}));
      impl.Write(new byte[] {6, 7});
      impl.WriteByte(8);
      impl.WriteDelayed(Task.FromResult(new byte[] {9, 10}));

      var actualBytes = await ToBytes_(impl);
      AssertSequence_(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, actualBytes);
    }


    [Test]
    public async Task TestPosition() {
      var impl = new DelayedContentOutputStream();

      var positionTask1 = impl.GetAbsolutePosition();
      impl.WriteDelayed(
          positionTask1.ContinueWith(pos => new[] {(byte)pos.Result}),
          Task.FromResult(1L));

      impl.WriteByte(1);
      impl.Write(new byte[] {2, 3});

      var positionTask2 = impl.GetAbsolutePosition();
      impl.WriteDelayed(
          positionTask2.ContinueWith(pos => new[] {(byte)pos.Result}),
          Task.FromResult(1L));

      impl.WriteDelayed(Task.FromResult(new byte[] {4, 5}));
      impl.Write(new byte[] {6, 7});

      var positionTask3 = impl.GetAbsolutePosition();
      impl.WriteDelayed(
          positionTask3.ContinueWith(pos => new[] {(byte)pos.Result}),
          Task.FromResult(1L));

      impl.WriteByte(8);
      impl.WriteDelayed(Task.FromResult(new byte[] {9, 10}));

      var positionTask4 = impl.GetAbsolutePosition();
      impl.WriteDelayed(
          positionTask4.ContinueWith(pos => new[] {(byte)pos.Result}),
          Task.FromResult(1L));


      var actualBytes = await ToBytes_(impl);
      AssertSequence_(new byte[] {0, 1, 2, 3, 4, 4, 5, 6, 7, 9, 8, 9, 10, 13},
                      actualBytes);
    }

    [Test]
    public async Task TestLength() {
      var impl = new DelayedContentOutputStream();

      var lengthTask = impl.GetAbsoluteLength();
      impl.WriteDelayed(
          lengthTask.ContinueWith(length => new[] {(byte)length.Result}),
          Task.FromResult(1L));

      impl.WriteByte(1);
      impl.Write(new byte[] {2, 3});
      impl.WriteDelayed(Task.FromResult(new byte[] {4, 5}));
      impl.Write(new byte[] {6, 7});
      impl.WriteByte(8);
      impl.WriteDelayed(Task.FromResult(new byte[] {9, 10}));

      var actualBytes = await ToBytes_(impl);
      AssertSequence_(new byte[] {11, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10},
                      actualBytes);
    }

    [Test]
    public async Task TestBlockLength() {
      var impl = new DelayedContentOutputStream();

      impl.WriteDelayed(
          impl.GetAbsoluteLength()
              .ContinueWith(length => new[] {(byte)length.Result}),
          Task.FromResult(1L));

      {
        var s = impl.EnterBlock(out var lTask);
        s.WriteDelayed(
            lTask.ContinueWith(length => new[] {(byte)length.Result}),
            Task.FromResult(1L));
      }

      impl.WriteByte(29);
      impl.WriteDelayed(
          impl.GetAbsolutePosition()
              .ContinueWith(length => new[] {(byte)length.Result}),
          Task.FromResult(1L));

      {
        var s1 = impl.EnterBlock(out _);

        var s2 = s1.EnterBlock(out _);
        s1.WriteByte(1);

        s2.WriteByte(2);
      }

      impl.WriteByte(29);
      impl.WriteDelayed(
          impl.GetAbsolutePosition()
              .ContinueWith(length => new[] {(byte)length.Result}),
          Task.FromResult(1L));

      {
        var s = impl.EnterBlock(out var lTask);
        s.WriteDelayed(
            lTask.ContinueWith(length => new[] {(byte)length.Result}),
            Task.FromResult(1L));
        s.Align(6);
      }

      impl.WriteByte(29);
      impl.WriteDelayed(
          impl.GetAbsolutePosition()
              .ContinueWith(length => new[] {(byte)length.Result}),
          Task.FromResult(1L));

      {
        var s = impl.EnterBlock(out var lengthTask);
        s.WriteByte(0);
        s.WriteByte(1);
        s.WriteByte(2);

        impl.WriteDelayed(
            lengthTask.ContinueWith(length => new[] {(byte)length.Result}),
            Task.FromResult(1L));
      }

      var actualBytes = await ToBytes_(impl);
      AssertSequence_(
          new[] {
              new byte[] {18}, // Total length
              new byte[] {1}, // Length of block printing out its own length
              new byte[] {29, 3}, // First position
              new byte[] {2, 1}, // Nested blocks
              new byte[] {29, 7}, // Second position
              new byte[] {4, 0, 0, 0}, // Align, not looking right
              new byte[] {29, 13}, // Third position
              new byte[] {0, 1, 2, 3}, // Final block
          }.SelectMany(x => x),
          actualBytes);
    }

    [Test]
    public async Task TestAbsoluteDelayedLength() {
      var impl = new DelayedContentOutputStream();

      var lengthTask = impl.GetAbsoluteLength();
      impl.WriteDelayed(
          lengthTask.ContinueWith(length => new[] {(byte)length.Result}),
          Task.FromResult(1L));
      impl.WriteDelayed(
          Task.FromResult(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}),
          Task.FromResult(3L));

      var actualBytes = await ToBytes_(impl);
      AssertSequence_(new byte[] {4, 0, 1, 2,}, actualBytes);
    }


    private async Task<byte[]> ToBytes_(
        DelayedContentOutputStream subDelayedContentOutputStream) {
      using var memoryStream = new MemoryStream();
      await subDelayedContentOutputStream
          .CompleteAndCopyToDelayed(memoryStream);
      return memoryStream.ToArray();
    }

    private void AssertSequence_<TEnumerable>(
        TEnumerable enumerableA,
        TEnumerable enumerableB) where TEnumerable : IEnumerable {
      var enumeratorA = enumerableA.GetEnumerator();
      var enumeratorB = enumerableB.GetEnumerator();

      var hasA = enumeratorA.MoveNext();
      var hasB = enumeratorB.MoveNext();

      var index = 0;
      while (hasA && hasB) {
        var currentA = enumeratorA.Current;
        var currentB = enumeratorB.Current;

        if (!object.Equals(currentA, currentB)) {
          Asserts.Fail(
              $"Expected {currentA} to equal {currentB} at index ${index}.");
        }
        index++;

        hasA = enumeratorA.MoveNext();
        hasB = enumeratorB.MoveNext();
      }

      Asserts.True(!hasA && !hasB,
                   "Expected enumerables to be equal:\n" +
                   $"  A: {ConvertSequenceToString_(enumerableA)}\n" +
                   $"  B: {ConvertSequenceToString_(enumerableB)}");
    }

    private string ConvertSequenceToString_(IEnumerable enumerable) {
      var str = new StringBuilder();
      foreach (var value in enumerable) {
        if (str.Length > 0) {
          str.Append(", ");
        }
        str.Append(value);
      }
      return str.ToString();
    }
  }
}