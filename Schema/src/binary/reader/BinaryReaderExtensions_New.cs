namespace schema.binary;

public static partial class BinaryReaderExtensions {
  public static byte SubreadByteAt(this IBinaryReader br, long position) {
    var tmp = br.Position;
    br.Position = position;

    var value = br.ReadByte();

    br.Position = tmp;

    return value;
  }

  public static short SubreadInt16At(this IBinaryReader br, long position) {
    var tmp = br.Position;
    br.Position = position;

    var value = br.ReadInt16();

    br.Position = tmp;

    return value;
  }

  public static ushort SubreadUInt16At(this IBinaryReader br, long position) {
    var tmp = br.Position;
    br.Position = position;

    var value = br.ReadUInt16();

    br.Position = tmp;

    return value;
  }

  public static int SubreadInt32At(this IBinaryReader br, long position) {
    var tmp = br.Position;
    br.Position = position;

    var value = br.ReadInt32();

    br.Position = tmp;

    return value;
  }

  public static uint SubreadUInt32At(this IBinaryReader br, long position) {
    var tmp = br.Position;
    br.Position = position;

    var value = br.ReadUInt32();

    br.Position = tmp;

    return value;
  }

  public static string SubreadStringNTAt(this IBinaryReader br, long position) {
    var tmp = br.Position;
    br.Position = position;

    var value = br.ReadStringNT();

    br.Position = tmp;

    return value;
  }
}