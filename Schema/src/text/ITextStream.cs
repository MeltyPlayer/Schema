namespace schema.text {
  public interface ITextStream : IDataStream {
    int TabWidth { get; set; }

    int LineNumber { get; }
    int IndexInLine { get; }
  }
}