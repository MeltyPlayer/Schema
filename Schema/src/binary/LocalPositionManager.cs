using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using schema.util.asserts;
using schema.util.streams;


namespace schema.binary;

public interface ILocalSpaceStack {
  void PushLocalSpace();
  void PopLocalSpace();
}

public interface IPositionManager : ILocalSpaceStack {
  long Position { get; set; }
  long Length { get; }
}

public class StreamPositionManager(ISeekableStream impl) : IPositionManager {
  private readonly Stack<long> positionStack_ = new([0]);

  private readonly Stack<(int positionStackLength, long offset, long length)>
      subreadStack_ = new([(1, 0, impl.Length)]);

  public long Position {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => impl.Position - this.BaseOffset;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => impl.Position = this.BaseOffset + value;
  }

  public long Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var (_, offset, length) = this.subreadStack_.Peek();
      return offset + length - this.BaseOffset;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PushLocalSpace() {
    this.positionStack_.Push(impl.Position);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PopLocalSpace() {
    Asserts.True(this.positionStack_.Count >
                 this.subreadStack_.Peek().positionStackLength);
    this.positionStack_.Pop();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PushSubread(long offset, long length) {
    if (offset < 0) {
      throw new ArgumentOutOfRangeException(nameof(offset),
                                            "Offset cannot be less than zero.");
    }

    if (length < 0) {
      throw new ArgumentOutOfRangeException(nameof(length),
                                            "Length cannot be less than zero.");
    }

    this.subreadStack_.Push((this.positionStack_.Count, offset, length));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void PopSubread() {
    Asserts.True(this.subreadStack_.Count > 1);
    this.subreadStack_.Pop();
  }

  public long BaseOffset {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.positionStack_.Peek();
  }
}