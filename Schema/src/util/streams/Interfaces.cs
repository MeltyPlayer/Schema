using System;

namespace schema.util.streams {
  // Common interfaces for C# streams.
  //
  // The built-in C# Stream class is kind of garbage, tbh. Everything is
  // designed around the one class, which can theoretically be used for both
  // reading and writing! This type is too much of a one-size-fits-all
  // approach. Furthermore, this makes it an absolute nightmare to implement
  // custom streams because they have to implement ALL of this logic as well.
  //
  // The interfaces in this namespace are meant to narrow down to *just* the
  // expected functionality for a needed stream, and no more.

  public interface ISizedStream {
    long Position { get; }
    long Length { get; }
  }

  public interface ISeekableStream : ISizedStream {
    new long Position { get; set; }
  }

  public interface IReadableStream {
    int Read(Span<byte> dst);
  }

  public interface IWritableStream {
    void Write(ReadOnlySpan<byte> src);
    void Write(IReadableStream readableStream);
  }

  public interface ISizedReadableStream : IReadableStream,
                                          ISizedStream,
                                          IDisposable { }

  public interface ISeekableReadableStream : ISizedReadableStream,
                                             ISeekableStream { }

  public interface ISizedWritableStream : IWritableStream,
                                          ISizedStream,
                                          IDisposable { }

  public interface ISeekableWritableStream : ISizedWritableStream,
                                             ISeekableStream { }
}