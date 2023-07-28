// Decompiled with JetBrains decompiler
// Type: System.IO.EndianBinaryWriter
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using schema.binary.io;


namespace System.IO {
  public sealed partial class EndianBinaryWriter : IEndianBinaryWriter {
    private readonly IDelayedContentOutputStream impl_;

    private bool disposed_;

    public EndianBinaryWriter() {
      this.impl_ = new DelayedContentOutputStream(null);
    }

    public EndianBinaryWriter(Endianness endianness) {
      this.impl_ = new DelayedContentOutputStream(endianness);
    }

    private EndianBinaryWriter(Endianness? endianness, 
                               ISubDelayedContentOutputStream impl) {
      this.impl_ = impl as IDelayedContentOutputStream;
    }

    ~EndianBinaryWriter() {
      this.Dispose(false);
    }

    public void Align(uint amt) => this.impl_.Align(amt);

    public void Close() {
      this.Dispose();
    }

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