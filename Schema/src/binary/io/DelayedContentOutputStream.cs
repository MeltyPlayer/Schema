using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using CommunityToolkit.HighPerformance;

using schema.util;

namespace schema.binary.io {
  public interface IDelayedContentOutputStream :
      ISubDelayedContentOutputStream {
    Task CompleteAndCopyToDelayed(Stream stream);
  }

  public interface ISubDelayedContentOutputStream : IEndiannessStack {
    Task<long> GetAbsolutePosition();
    Task<long> GetAbsoluteLength();

    Task<long> GetStartPositionOfSubStream();
    Task<long> GetPositionInSubStream();
    Task<long> GetLengthOfSubStream();

    ISubDelayedContentOutputStream EnterBlock(out Task<long> delayedLength);

    void Align(uint amount);

    void WriteByte(byte value);
    void WriteBytes(ReadOnlySpan<byte> bytes);
    void WriteBytes(byte[] bytes, int offset, int count);

    void WriteBytesAndFlip(ReadOnlySpan<byte> bytes, int size);
    void WriteBytesAndFlip(byte[] bytes, int offset, int count, int size);

    void WriteAndFlip<T>(T value) where T : unmanaged;
    void WriteAndFlip<T>(ReadOnlySpan<T> values) where T : unmanaged;
    void WriteAndFlip<T>(T[] values, int offset, int count) where T : unmanaged;


    void WriteDelayed(Task<byte[]> delayedBytes, Task<long> delayedBytesLength);
    void WriteDelayed(Task<byte[]> delayedBytes);
  }

  public class DelayedContentOutputStream : IDelayedContentOutputStream {
    private readonly DelayedContentOutputStream? parent_;
    private readonly INestedList<Task<IDataChunk>> dataChunks_;
    private readonly INestedList<Task<ISizeChunk>> sizeChunks_;

    private List<DelayedContentOutputStream> children_ = new();

    private LinkedList<byte>? currentBytes_ = null;
    private readonly Task<long> startPositionTask_;
    private readonly TaskCompletionSource<long>? lengthTaskCompletionSource_;
    private readonly Task<long> lengthTask_;
    private bool isCompleted_ = false;

    public DelayedContentOutputStream(Endianness? endianness) {
      this.endiannessImpl_ = new EndiannessStackImpl(endianness);
      this.dataChunks_ = new NestedList<Task<IDataChunk>>();
      this.sizeChunks_ = new NestedList<Task<ISizeChunk>>();
      this.startPositionTask_ = Task.FromResult(0L);
      this.lengthTaskCompletionSource_ = new();
      this.lengthTask_ = this.lengthTaskCompletionSource_.Task;
    }

    private DelayedContentOutputStream(
        DelayedContentOutputStream parent,
        Task<long> startPositionTask,
        Task<long> lengthTask
    ) {
      this.endiannessImpl_ = new EndiannessStackImpl(parent.Endianness);
      this.parent_ = parent;
      this.dataChunks_ = parent.dataChunks_.Enter();
      this.sizeChunks_ = parent.sizeChunks_.Enter();
      this.startPositionTask_ = startPositionTask;
      this.lengthTask_ = lengthTask;
    }

    private readonly IEndiannessStack endiannessImpl_;

    public Endianness Endianness => this.endiannessImpl_.Endianness;

    public bool IsOppositeEndiannessOfSystem
      => this.endiannessImpl_.IsOppositeEndiannessOfSystem;

    public void PushStructureEndianness(Endianness endianness)
      => this.endiannessImpl_.PushStructureEndianness(endianness);

    public void PushMemberEndianness(Endianness endianness)
      => this.endiannessImpl_.PushMemberEndianness(endianness);

    public void PopEndianness() => this.endiannessImpl_.PopEndianness();


    public Task<long> GetAbsolutePosition() {
      this.AssertNotCompleted_();

      this.PushCurrentBytes_();
      var task = new TaskCompletionSource<long>();
      this.sizeChunks_.Add(
          Task.FromResult<ISizeChunk>(new PositionSizeChunk(task)));
      return task.Task;
    }

    public Task<long> GetAbsoluteLength() {
      this.AssertNotCompleted_();
      return this.parent_?.GetAbsoluteLength() ?? this.lengthTask_;
    }


    public Task<long> GetStartPositionOfSubStream()
      => this.startPositionTask_;

    public Task<long> GetPositionInSubStream()
      => Task.WhenAll(this.GetAbsolutePosition(),
                      this.GetStartPositionOfSubStream())
             .ContinueWith(positions =>
                               positions.Result[0] - positions.Result[1]);

    public Task<long> GetLengthOfSubStream() => this.lengthTask_;


    public ISubDelayedContentOutputStream EnterBlock(
        out Task<long> delayedLength) {
      this.AssertNotCompleted_();

      var startPosition = this.GetAbsolutePosition();
      var task = new TaskCompletionSource<long>();
      delayedLength = task.Task;

      this.PushCurrentBytes_();
      this.sizeChunks_.Add(
          Task.FromResult<ISizeChunk>(new SizeChunkBlockStart(task)));

      var child =
          new DelayedContentOutputStream(this, startPosition, delayedLength);
      this.children_.Add(child);

      this.sizeChunks_.Add(
          Task.FromResult<ISizeChunk>(new SizeChunkBlockEnd(task)));

      return child;
    }


    public void Align(uint amount) {
      this.AssertNotCompleted_();

      this.PushCurrentBytes_();
      this.dataChunks_.Add(
          Task.FromResult<IDataChunk>(new AlignDataChunk(amount)));
      this.sizeChunks_.Add(
          Task.FromResult<ISizeChunk>(new AlignSizeChunk(amount)));
    }


    public void WriteByte(byte value) {
      this.AssertNotCompleted_();

      this.CreateCurrentBytesIfNull_();
      this.currentBytes_!.AddLast(value);
    }

    public void WriteBytes(byte[] bytes, int offset, int count)
      => this.WriteBytes(bytes.AsSpan(offset, count));

    public void WriteBytes(ReadOnlySpan<byte> bytes) {
      this.AssertNotCompleted_();

      this.CreateCurrentBytesIfNull_();
      foreach (var b in bytes) {
        this.currentBytes_!.AddLast(b);
      }
    }


    public void WriteBytesAndFlip(byte[] values,
                                  int offset,
                                  int count,
                                  int size)
      => this.WriteBytesAndFlip(values.AsSpan(offset, count), size);

    public void WriteBytesAndFlip(ReadOnlySpan<byte> values, int size) {
      if (size == 1 || !this.IsOppositeEndiannessOfSystem) {
        this.WriteBytes(MemoryMarshal.AsBytes(values));
        return;
      }

      Span<byte> bytes = stackalloc byte[size];
      for (var i = 0; i < values.Length / size; ++i) {
        values.Slice(i * size, size).CopyTo(bytes);
        bytes.Reverse();
        this.WriteBytes(bytes);
      }
    }


    public unsafe void WriteAndFlip<T>(T value) where T : unmanaged {
      T* ptr = &value;
      var span = new Span<T>(ptr, 1);
      this.WriteAndFlip<T>(span);
    }

    public void WriteAndFlip<T>(T[] values, int offset, int count)
        where T : unmanaged
      => this.WriteAndFlip<T>(values.AsSpan(offset, count));

    public unsafe void WriteAndFlip<T>(ReadOnlySpan<T> values)
        where T : unmanaged
      => this.WriteBytesAndFlip(values.AsBytes(), sizeof(T));


    public void WriteDelayed(Task<byte[]> delayedBytes)
      => WriteDelayed(
          delayedBytes,
          delayedBytes.ContinueWith(bytesTask => bytesTask.Result.LongLength));

    public void WriteDelayed(
        Task<byte[]> delayedBytes,
        Task<long> delayedBytesLength) {
      this.AssertNotCompleted_();

      this.PushCurrentBytes_();
      this.dataChunks_.Add(
          Task.WhenAll(delayedBytes, delayedBytesLength)
              .ContinueWith(
                  _ => {
                    var bytes = delayedBytes.Result;
                    var length = delayedBytesLength.Result;
                    return (IDataChunk) new BytesDataChunk(
                        bytes,
                        0,
                        (int) length);
                  }));
      this.sizeChunks_.Add(
          delayedBytesLength.ContinueWith(
              lengthTask =>
                  (ISizeChunk) new BytesSizeChunk(lengthTask.Result)));
    }


    public async Task CompleteAndCopyToDelayed(Stream stream) {
      this.AssertNotCompleted_();
      this.isCompleted_ = true;

      this.PushCurrentBytesInSelfAndChildren_();

      // Updates position and length Tasks first.
      var blockStack = new Stack<(SizeChunkBlockStart, long)>();
      var position = 0L;
      var sizeChunks = await Task.WhenAll(this.sizeChunks_);
      foreach (var sizeChunk in sizeChunks) {
        switch (sizeChunk) {
          case PositionSizeChunk positionSizeChunk: {
            positionSizeChunk.Task.SetResult(position);
            break;
          }
          case AlignSizeChunk alignSizeChunk: {
            var pos = position;
            var amt = alignSizeChunk.Amount;
            var align = this.GetAlign_(pos, amt);
            position += align;
            break;
          }
          case BytesSizeChunk bytesSizeChunk: {
            position += bytesSizeChunk.Length;
            break;
          }
          case SizeChunkBlockStart blockStart: {
            blockStack.Push((blockStart, position));
            break;
          }
          case SizeChunkBlockEnd blockEnd: {
            var (blockStart, startPosition) = blockStack.Pop();
            Asserts.Same(blockStart.Task, blockEnd.Task);
            blockStart.Task.SetResult(position - startPosition);
            break;
          }
        }
      }

      this.lengthTaskCompletionSource_.SetResult(position);

      // Writes data.
      var dataChunks = await Task.WhenAll(this.dataChunks_);
      foreach (var dataChunk in dataChunks) {
        switch (dataChunk) {
          case AlignDataChunk alignDataChunk: {
            var pos = stream.Position;
            var amt = alignDataChunk.Amount;
            var align = this.GetAlign_(pos, amt);
            for (var i = 0; i < align; ++i) {
              stream.WriteByte(0);
            }

            break;
          }
          case BytesDataChunk bytesDataChunk: {
            var bytes = bytesDataChunk.Bytes;
            await stream.WriteAsync(bytes,
                                    bytesDataChunk.Offset,
                                    bytesDataChunk.Count);
            break;
          }
        }
      }
    }

    private void AssertNotCompleted_()
      => Asserts.False(this.parent_?.isCompleted_ ?? this.isCompleted_);

    private void CreateCurrentBytesIfNull_() {
      if (this.currentBytes_ != null) {
        return;
      }

      this.currentBytes_ = new LinkedList<byte>();
    }

    private void PushCurrentBytes_() {
      if (this.currentBytes_ == null) {
        return;
      }

      this.dataChunks_.Add(
          Task.FromResult<IDataChunk>(
              new BytesDataChunk(this.currentBytes_.ToArray())));
      this.sizeChunks_.Add(
          Task.FromResult<ISizeChunk>(
              new BytesSizeChunk(this.currentBytes_.Count)));
      this.currentBytes_ = null;
    }

    private void PushCurrentBytesInSelfAndChildren_() {
      this.PushCurrentBytes_();
      foreach (var child in this.children_) {
        child.PushCurrentBytesInSelfAndChildren_();
      }
    }


    private long GetAlign_(long pos, long amt) {
      var delta = amt - (pos % amt);
      if (delta == amt) {
        return 0;
      }

      return delta;
    }


    private interface IDataChunk { }

    private class AlignDataChunk : IDataChunk {
      public AlignDataChunk(uint amount) {
        this.Amount = amount;
      }

      public uint Amount { get; }
    }

    private class BytesDataChunk : IDataChunk {
      public BytesDataChunk(byte[] bytes) : this(bytes, 0, bytes.Length) { }

      public BytesDataChunk(
          byte[] bytes,
          int offset,
          int count) {
        this.Bytes = bytes;
        this.Offset = offset;
        this.Count = count;
      }

      public byte[] Bytes { get; }
      public int Offset { get; }
      public int Count { get; }
    }


    private interface ISizeChunk { }

    private class SizeChunkBlockStart : ISizeChunk {
      public SizeChunkBlockStart(TaskCompletionSource<long> task) {
        this.Task = task;
      }

      public TaskCompletionSource<long> Task { get; }
    }

    private class SizeChunkBlockEnd : ISizeChunk {
      public SizeChunkBlockEnd(TaskCompletionSource<long> task) {
        this.Task = task;
      }

      public TaskCompletionSource<long> Task { get; }
    }

    private class PositionSizeChunk : ISizeChunk {
      public PositionSizeChunk(TaskCompletionSource<long> task) {
        this.Task = task;
      }

      public TaskCompletionSource<long> Task { get; }
    }

    private class BytesSizeChunk : ISizeChunk {
      public BytesSizeChunk(long length) {
        this.Length = length;
      }

      public long Length { get; }
    }

    private class AlignSizeChunk : ISizeChunk {
      public AlignSizeChunk(uint amount) {
        this.Amount = amount;
      }

      public uint Amount { get; }
    }
  }
}