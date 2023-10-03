namespace schema.text {
  public interface ITextStream {
    int TabWidth { get; }

    int LineNumber { get; }
    int IndexInLine { get; }
  }
}