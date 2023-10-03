namespace schema.text {
  public interface ITextStream : IDataStream {
    int TabWidth { get; }

    int LineNumber { get; }
    int IndexInLine { get; }
  }
}