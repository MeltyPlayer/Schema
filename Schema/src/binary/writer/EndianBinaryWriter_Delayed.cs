using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using schema.util.streams;


namespace schema.binary {
  public sealed partial class EndianBinaryWriter {
    public ISubEndianBinaryWriter EnterBlock(out Task<long> delayedLength)
      => new EndianBinaryWriter(this.Endianness,
                                this.impl_.EnterBlock(out delayedLength));

    public Task<long> GetLocalPosition()
      => Task.WhenAll(this.impl_.GetAbsolutePosition(),
                      this.localPositionStack_.Peek())
             .ContinueWith(task => {
               var result = task.Result;
               return result[0] - result[1];
             });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<long> GetAbsolutePosition() => this.impl_.GetAbsolutePosition();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<long> GetAbsoluteLength() => this.impl_.GetAbsoluteLength();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<long> GetStartPositionOfSubStream()
      => this.impl_.GetStartPositionOfSubStream();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<long> GetPositionInSubStream()
      => this.impl_.GetPositionInSubStream();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<long> GetLengthOfSubStream()
      => this.impl_.GetLengthOfSubStream();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task CompleteAndCopyToDelayed(Stream stream)
      => this.CompleteAndCopyToDelayed(new WritableStream(stream));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task CompleteAndCopyToDelayed(ISizedWritableStream stream)
      => this.impl_.CompleteAndCopyToDelayed(stream);

    private void WriteBufferDelayed_(Task<byte[]> delayedBytes) {
      var isReversed = this.IsOppositeEndiannessOfSystem;
      this.impl_.WriteDelayed(
          delayedBytes.ContinueWith(bytesTask => {
            var bytes = bytesTask.Result;
            if (isReversed) {
              Array.Reverse(bytes, 0, bytes.Length);
            }

            return bytes;
          }));
    }

    private void WriteBufferDelayed_(Task<byte[]> delayedBytes,
                                     Task<long> delayedBytesLength) {
      var isReversed = this.IsOppositeEndiannessOfSystem;
      this.impl_.WriteDelayed(
          delayedBytes.ContinueWith(bytesTask => {
            var bytes = bytesTask.Result;
            if (isReversed) {
              Array.Reverse(bytes, 0, bytes.Length);
            }

            return bytes;
          }),
          delayedBytesLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByteDelayed(Task<byte> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => new[] { valueTask.Result }),
          Task.FromResult((long) sizeof(byte)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSByteDelayed(Task<sbyte> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => new[] { (byte) valueTask.Result }),
          Task.FromResult((long) sizeof(sbyte)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16Delayed(Task<short> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long) sizeof(short)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16Delayed(Task<ushort> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long) sizeof(ushort)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32Delayed(Task<int> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long) sizeof(int)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32Delayed(Task<uint> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long) sizeof(uint)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64Delayed(Task<long> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long) sizeof(long)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64Delayed(Task<ulong> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long) sizeof(ulong)));
  }
}