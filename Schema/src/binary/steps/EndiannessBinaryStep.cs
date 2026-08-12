namespace schema.binary.steps;

public sealed class EndiannessBinaryStep : IBinaryStep {
  public Endianness Endianness { get; set; }
}