using System.Threading.Tasks;


namespace System.IO {
  public sealed partial class EndianBinaryWriter {
    public ISubEndianBinaryWriter EnterBlock(out Task<long> delayedLength)
      => new EndianBinaryWriter(this.Endianness,
                                this.impl_.EnterBlock(out delayedLength));

    public Task<long> GetAbsolutePosition() => this.impl_.GetAbsolutePosition();

    public Task<long> GetAbsoluteLength() => this.impl_.GetAbsoluteLength();

    public Task<long> GetStartPositionOfSubStream()
      => this.impl_.GetStartPositionOfSubStream();

    public Task<long> GetPositionInSubStream()
      => this.impl_.GetPositionInSubStream();

    public Task<long> GetLengthOfSubStream()
      => this.impl_.GetLengthOfSubStream();

    public Task CompleteAndCopyToDelayed(Stream stream)
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

    public void WriteByteDelayed(Task<byte> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => new[] {valueTask.Result}),
          Task.FromResult((long)sizeof(byte)));

    public void WriteSByteDelayed(Task<sbyte> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => new[] {(byte)valueTask.Result}),
          Task.FromResult((long)sizeof(sbyte)));

    public void WriteInt16Delayed(Task<short> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long)sizeof(short)));

    public void WriteUInt16Delayed(Task<ushort> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long)sizeof(ushort)));

    public void WriteInt32Delayed(Task<int> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long)sizeof(int)));

    public void WriteUInt32Delayed(Task<uint> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long)sizeof(uint)));

    public void WriteInt64Delayed(Task<long> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long)sizeof(long)));

    public void WriteUInt64Delayed(Task<ulong> delayedValue)
      => this.WriteBufferDelayed_(
          delayedValue.ContinueWith(
              valueTask => BitConverter.GetBytes(valueTask.Result)),
          Task.FromResult((long)sizeof(ulong)));
  }
}