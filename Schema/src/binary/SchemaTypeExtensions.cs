using schema.binary.text;


namespace schema.binary;

public static class SchemaTypeExtensions {
  public static string GetTypeName(this SchemaNumberType type)
    => SchemaGeneratorUtil.GetTypeName(type);

  public static string GetTypeName(this SchemaIntegerType type)
    => SchemaGeneratorUtil.GetTypeName(type.AsNumberType());

  public static string GetIntLabel(this SchemaIntegerType type)
    => SchemaGeneratorUtil.GetIntLabel(type);
}