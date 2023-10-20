using schema.binary.io;
using schema.util;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using schema.util.asserts;


namespace schema.binary {
  public sealed partial class EndianBinaryWriter {
    private readonly Stack<Task<long>> localPositionStack_ = new();
    private readonly Stack<string> scopes_ = new();
    private readonly OutOfOrderDictionary<string, long> startPositions_ = new();
    private readonly OutOfOrderDictionary<string, long> endPositions_ = new();

    public Task<long> GetPointerToMemberRelativeToScope(
        string memberPath) {
      var fullPath = this.GetCurrentScope_();
      if (fullPath.Length > 0) {
        fullPath += ".";
      }

      fullPath += memberPath;
      return this.startPositions_.Get(fullPath);
    }

    public Task<long> GetSizeOfMemberRelativeToScope(
        string memberPath) {
      var fullPath = this.GetCurrentScope_();
      if (fullPath.Length > 0) {
        fullPath += ".";
      }

      fullPath += memberPath;
      var startTask = this.startPositions_.Get(fullPath);
      var endTask = this.endPositions_.Get(fullPath);
      return Task.WhenAll(startTask, endTask)
                 .ContinueWith(_ => endTask.Result - startTask.Result);
    }

    public void MarkStartOfMember(string memberName) {
      this.scopes_.Push(memberName);
      var currentScope = this.GetCurrentScope_();
      this.startPositions_.Set(currentScope, this.GetLocalPosition());
    }

    public void MarkEndOfMember() {
      var currentScope = this.GetCurrentScope_();
      this.endPositions_.Set(currentScope, this.GetLocalPosition());
      this.scopes_.Pop();

      /*if (this.scopes_.Count == 0) {
        this.startPositions_.AssertAllPopulated();
        this.startPositions_.Clear();
        this.endPositions_.AssertAllPopulated();
        this.endPositions_.Clear();
      }*/
    }

    private string GetCurrentScope_() {
      var totalString = new StringBuilder();
      foreach (var scope in scopes_.Reverse()) {
        if (totalString.Length > 0) {
          totalString.Append(".");
        }

        totalString.Append(scope);
      }

      return totalString.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushLocalSpace()
      => this.localPositionStack_.Push(this.impl_.GetAbsolutePosition());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopLocalSpace() {
      Asserts.True(this.localPositionStack_.Count >= 2);
      this.localPositionStack_.Pop();
    }
  }
}