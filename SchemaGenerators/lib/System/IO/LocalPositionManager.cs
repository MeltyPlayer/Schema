﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;

using schema.util;

namespace System.IO {
  public interface ILocalSpaceStack {
    void PushLocalSpace();
    void PopLocalSpace();
  }

  public interface IPositionManager : ILocalSpaceStack {
    long Position { get; set; }
    long Length { get; }
  }

  public class StreamPositionManager : IPositionManager {
    private readonly Stream impl_;
    private readonly Stack<long> positionStack_ = new();

    public StreamPositionManager(Stream impl) {
      this.impl_ = impl;
      this.positionStack_.Push(0);
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

    private long BaseOffset {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this.positionStack_.Peek();
    }
  }
}