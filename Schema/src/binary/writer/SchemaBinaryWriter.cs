// Decompiled with JetBrains decompiler
// Type: System.IO.SchemaBinaryWriter
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using schema.binary.io;


namespace schema.binary {
  public sealed partial class SchemaBinaryWriter : ITopLevelBinaryWriter {
    private readonly IDelayedContentOutputStream impl_;

    private bool disposed_;

    public SchemaBinaryWriter() {
      this.impl_ = new DelayedContentOutputStream(null);
      this.localPositionStack_.Push(Task.FromResult(0L));
    }

    public SchemaBinaryWriter(Endianness endianness) {
      this.impl_ = new DelayedContentOutputStream(endianness);
      this.localPositionStack_.Push(Task.FromResult(0L));
    }

    private SchemaBinaryWriter(Endianness? endianness,
                               ISubDelayedContentOutputStream impl) {
      this.impl_ = impl as IDelayedContentOutputStream;
      this.localPositionStack_.Push(Task.FromResult(0L));
    }

    ~SchemaBinaryWriter() {
      this.Dispose(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Align(uint amt) => this.impl_.Align(amt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Close() => this.Dispose();

    public void Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing) {
      if (this.disposed_)
        return;
      this.disposed_ = true;
    }
  }
}