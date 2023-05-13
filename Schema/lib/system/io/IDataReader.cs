namespace System.IO {
  public interface IDataReader : IDisposable {
    long Position { get; set; }
    long Length { get; }
    bool Eof { get; }

    void AssertByte(byte expectedValue);
    byte ReadByte();

    void AssertSByte(sbyte expectedValue);
    sbyte ReadSByte();

    void AssertInt16(short expectedValue);
    short ReadInt16();

    void AssertUInt16(ushort expectedValue);
    ushort ReadUInt16();

    void AssertInt32(int expectedValue);
    int ReadInt32();

    void AssertUInt32(uint expectedValue);
    uint ReadUInt32();
    
    void AssertInt64(long expectedValue);
    long ReadInt64();

    void AssertUInt64(ulong expectedValue);
    ulong ReadUInt64();

    void AssertSingle(float expectedValue);
    float ReadSingle();

    void AssertDouble(double expectedValue);
    double ReadDouble();

    void AssertChar(char expectedValue);
    char ReadChar();
    char[] ReadChars(long count);
    char[] ReadChars(char[] dst);

    void AssertString(string expectedValue);
    string ReadString(long count);
    string ReadLine();
  }
}