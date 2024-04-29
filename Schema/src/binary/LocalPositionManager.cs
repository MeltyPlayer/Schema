using System.Collections.Generic;
using System.Runtime.CompilerServices;

using schema.util.asserts;
using schema.util.streams;

namespace schema.binary {
  public interface ILocalSpaceStack {
    void PushLocalSpace();
    void PopLocalSpace();
  }

  public interface IPositionManager : ILocalSpaceStack {
    long Position { get; set; }
    long Length { get; }
  }

  public class StreamPositionManager : IPositionManager {
    private readonly ISeekableStream impl_;
    private readonly Stack<long> positionStack_ = new();

    public StreamPositionManager(ISeekableStream impl,
                                 long startingPosition = 0) {
      this.impl_ = impl;
      this.positionStack_.Push(startingPosition);
    }


    public long Position {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.impl_.Position - this.BaseOffset;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => this.impl_.Position = this.BaseOffset + value;
    }

    public long Length {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.impl_.Length - this.BaseOffset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushLocalSpace() {
      this.positionStack_.Push(this.impl_.Position);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopLocalSpace() {
      Asserts.True(this.positionStack_.Count >= 2);
      this.positionStack_.Pop();
    }

    public long BaseOffset {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.positionStack_.Peek();
    }
  }
}