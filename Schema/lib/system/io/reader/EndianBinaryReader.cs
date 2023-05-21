using System.Runtime.CompilerServices;

using SubstreamSharp;


namespace System.IO {
  public sealed partial class EndianBinaryReader : IEndianBinaryReader {
    private bool disposed_;

    // TODO: This should be private.
    // TODO: Does caching the buffer actually help, or can this logic be pulled into the extensions?
    private EndianBinaryBufferedStream BufferedStream_ { get; set; }

    private Stream BaseStream_ {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.BufferedStream_.BaseStream;
    }

    public EndianBinaryReader(byte[] data)
      => this.Init_(new MemoryStream(data), null);

    public EndianBinaryReader(byte[] data, Endianness endianness)
      => this.Init_(new MemoryStream(data), endianness);

    public EndianBinaryReader(Stream baseStream)
      => this.Init_(baseStream, null);

    public EndianBinaryReader(Stream baseStream, Endianness endianness)
      => this.Init_(baseStream, endianness);

    private void Init_(Stream baseStream, Endianness? endianness) {
      if (baseStream == null) {
        throw new ArgumentNullException(nameof(baseStream));
      }

      if (!baseStream.CanRead) {
        throw new ArgumentException(nameof(baseStream));
      }

      this.BufferedStream_ = new EndianBinaryBufferedStream(endianness) {
          BaseStream = baseStream,
      };
    }

    ~EndianBinaryReader() {
      this.Dispose(false);
    }

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
        this.BaseStream_.Close();
      }

      this.disposed_ = true;
    }

    public long Position {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.BaseStream_.Position;
      set => this.BaseStream_.Position = value;
    }

    public void AssertPosition(long expectedPosition) {
      EndianBinaryReader.Assert_(expectedPosition, this.Position);
    }

    public long Length {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.BaseStream_.Length;
    }

    public bool Eof {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.Position >= this.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertNotEof() {
      if (this.Eof) {
        throw new Exception(
            $"Attempted to read past the end of the stream: position '{this.Position}' of stream length '{this.Length}'");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Align(uint amt) {
      var offs = amt - (this.Position % amt);
      if (offs != amt) {
        this.Position += offs;
      }
    }

    public byte[] ReadBytesAtOffset(long position, int len) {
      var startingOffset = this.Position;
      this.Position = position;

      var bytes = this.ReadBytes(len);

      this.Position = startingOffset;

      return bytes;
    }

    public string ReadStringAtOffset(long position, int len) {
      var startingOffset = this.Position;
      this.Position = position;

      var str = this.ReadString(len);

      this.Position = startingOffset;

      return str;
    }

    public string ReadStringNTAtOffset(long position) {
      var startingOffset = this.Position;
      this.Position = position;

      var str = this.ReadStringNT();

      this.Position = startingOffset;

      return str;
    }


    public void Subread(long position,
                        int len,
                        Action<IEndianBinaryReader> subread) {
      var tempPos = this.Position;
      {
        this.Position = position;

        using var ser =
            new EndianBinaryReader(
                new Substream(this.BaseStream_, position, len),
                this.Endianness);
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
      => this.BufferedStream_.FillBuffer(count, optStride);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillBuffer_(Span<byte> buffer)
      => this.BufferedStream_.FillBuffer(buffer);
  }
}