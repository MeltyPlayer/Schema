﻿using System;
using System.IO;

using CommunityToolkit.HighPerformance;

using schema.binary;
using schema.util.streams;

namespace schema.testing {
  public class SchemaMemoryStream(MemoryStream impl)
      : ISeekableReadableStream, ISeekableWritableStream {
    public static unsafe SchemaMemoryStream From<T>(T[] src)
        where T : unmanaged {
      var size = sizeof(T);
      var data = new byte[size * src.Length];
      src.AsSpan().AsBytes().CopyTo(data);

      var ms = new MemoryStream(data);
      return new SchemaMemoryStream(ms);
    }

    public IBinaryReader GetBinaryReader()
      => new SchemaBinaryReader(this, this.Endianness);

    public void Dispose() => impl.Dispose();

    public byte ReadByte() => (byte) impl.ReadByte();
    public int Read(Span<byte> dst) => impl.Read(dst);

    public long Position {
      get => impl.Position;
      set => impl.Position = value;
    }

    public long Length => impl.Length;

    public void WriteByte(byte b) => impl.WriteByte(b);
    public void Write(ReadOnlySpan<byte> src) => impl.Write(src);

    public void Write(IReadableStream readableStream)
      => impl.WriteImpl(readableStream);

    public Endianness Endianness => EndiannessUtil.SystemEndianness;
  }
}