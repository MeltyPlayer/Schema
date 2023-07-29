using schema.binary.text;

namespace schema.binary {
  public static class SchemaTypeExtensions {
    public static SchemaNumberType AsNumberType(this SchemaIntegerType type)
      => SchemaPrimitiveTypesUtil.ConvertIntToNumber(type);

    public static string GetTypeName(this SchemaNumberType type)
      => SchemaGeneratorUtil.GetTypeName(type);

    public static string GetTypeName(this SchemaIntegerType type)
      => SchemaGeneratorUtil.GetTypeName(type.AsNumberType());

    public static string GetIntLabel(this SchemaIntegerType type)
      => SchemaGeneratorUtil.GetIntLabel(type);

    public static bool CanAcceptAnInt(this SchemaIntegerType type)
      => type is SchemaIntegerType.INT24
                 or SchemaIntegerType.UINT24
                 or SchemaIntegerType.INT32
                 or SchemaIntegerType.UINT32
                 or SchemaIntegerType.INT64
                 or SchemaIntegerType.UINT64;
  }
}