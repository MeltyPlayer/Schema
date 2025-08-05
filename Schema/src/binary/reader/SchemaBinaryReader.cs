using System;
using System.IO;
using System.Runtime.CompilerServices;

using schema.util.asserts;
using schema.util.streams;


namespace schema.binary;

public sealed partial class SchemaBinaryReader : IBinaryReader {
  private bool disposed_;

  private readonly EndianBinaryBufferedStream bufferedStream_;
  private StreamPositionManager positionManagerImpl_;

  private ISeekableReadableStream BaseStream_ {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.bufferedStream_.BaseStream;
  }

  public SchemaBinaryReader(byte[] data)
      : this(new ReadableStream(data), null) { }

  public SchemaBinaryReader(byte[] data, Endianness endianness)
      : this(new ReadableStream(data), endianness) { }

  public SchemaBinaryReader(Stream baseStream)
      : this(new ReadableStream(baseStream), null) { }

  public SchemaBinaryReader(Stream baseStream, Endianness endianness)
      : this(new ReadableStream(baseStream), endianness) { }

  public SchemaBinaryReader(ISeekableReadableStream baseStream)
      : this(baseStream, null) { }

  public SchemaBinaryReader(ISeekableReadableStream baseStream,
                            Endianness endianness)
      : this(baseStream, (Endianness?) endianness) { }

  private SchemaBinaryReader(ISeekableReadableStream baseStream,
                             Endianness? endianness) {
    if (baseStream == null) {
      throw new ArgumentNullException(nameof(baseStream));
    }

    this.bufferedStream_ = new EndianBinaryBufferedStream(endianness) {
        BaseStream = baseStream,
    };
    this.positionManagerImpl_ = new StreamPositionManager(baseStream);
  }

  ~SchemaBinaryReader() => this.Dispose(false);

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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void FillBuffer(Span<byte> data, int stride)
    => this.bufferedStream_.FillBuffer(data, stride);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void FillBuffer_(long count, int? optStride = null)
    => this.bufferedStream_.FillBuffer(count, optStride);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ReadIntoBuffer(Span<byte> dst)
    => Asserts.Equal(dst.Length, this.TryToReadIntoBuffer(dst));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int TryToReadIntoBuffer(Span<byte> dst) {
    var position = this.Position;
    var length = this.Length;

    var remaining = length - position;
    var maxReadCount = Math.Min(dst.Length, remaining);

    var actualReadCount = this.BaseStream_.TryToReadIntoBuffer(dst.Slice(0, (int) maxReadCount));
    return actualReadCount;
  }
}