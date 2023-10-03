using System;
using System.IO;
using System.Runtime.CompilerServices;

using schema.util.streams;

namespace schema.binary {
  public sealed partial class EndianBinaryReader : IEndianBinaryReader,
                                                   IDisposable {
    private bool disposed_;

    private readonly EndianBinaryBufferedStream bufferedStream_;
    private StreamPositionManager positionManagerImpl_;

    private ISeekableReadableStream BaseStream_ {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.bufferedStream_.BaseStream;
    }

    public EndianBinaryReader(byte[] data)
        : this(new ReadableStream(data), null) { }

    public EndianBinaryReader(byte[] data, Endianness endianness)
        : this(new ReadableStream(data), endianness) { }

    public EndianBinaryReader(Stream baseStream)
        : this(new ReadableStream(baseStream), null) { }

    public EndianBinaryReader(Stream baseStream, Endianness endianness)
        : this(new ReadableStream(baseStream), endianness) { }

    public EndianBinaryReader(ISeekableReadableStream baseStream)
        : this(baseStream, null) { }

    public EndianBinaryReader(ISeekableReadableStream baseStream,
                              Endianness endianness)
        : this(baseStream, (Endianness?) endianness) { }

    private EndianBinaryReader(ISeekableReadableStream baseStream,
                               Endianness? endianness) {
      if (baseStream == null) {
        throw new ArgumentNullException(nameof(baseStream));
      }

      this.bufferedStream_ = new EndianBinaryBufferedStream(endianness) {
          BaseStream = baseStream,
      };
      this.positionManagerImpl_ = new StreamPositionManager(baseStream);
    }

    ~EndianBinaryReader() {
      this.Dispose(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Close() => Dispose();

    public void Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing) {
      if (this.disposed_) {
        return;
      }

      if (disposing && this.BaseStream_ != null) {
        this.BaseStream_.Dispose();
      }

      this.disposed_ = true;
    }

    public void Subread(long position,
                        int len,
                        Action<IEndianBinaryReader> subread) {
      var tempPos = this.Position;
      {
        this.Position = position;

        var baseOffset = this.positionManagerImpl_.BaseOffset;
        var substream =
            new RangedReadableSubstream(this.BaseStream_,
                                        position,
                                        baseOffset + len);
        using var ser = new EndianBinaryReader(substream, this.Endianness);
        ser.positionManagerImpl_ =
            new StreamPositionManager(substream, baseOffset);
        subread(ser);
      }
      this.Position = tempPos;
    }

    public void Subread(long position, Action<IEndianBinaryReader> subread) {
      var tempPos = this.Position;
      {
        this.Position = position;
        subread(this);
      }
      this.Position = tempPos;
    }


    public T Subread<T>(long position,
                        int len,
                        Func<IEndianBinaryReader, T> subread) {
      T value = default;

      this.Subread(
          position,
          len,
          ser => { value = subread(ser); });

      return value!;
    }

    public T Subread<T>(long position, Func<IEndianBinaryReader, T> subread) {
      T value = default;

      this.Subread(
          position,
          ser => { value = subread(ser); });

      return value!;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Assert_<T>(T expectedValue, T actualValue) {
      if (!expectedValue.Equals(actualValue)) {
        throw new Exception(
            "Expected " + actualValue + " to be " + expectedValue);
      }
    }

    private static void AssertAlmost_(double expectedValue,
                                      double actualValue,
                                      double delta = .01) {
      if (Math.Abs(expectedValue - actualValue) > delta) {
        throw new Exception(
            "Expected " + actualValue + " to be " + expectedValue);
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillBuffer_(long count, int? optStride = null)
      => this.bufferedStream_.FillBuffer(count, optStride);
  }
}