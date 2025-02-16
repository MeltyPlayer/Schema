using NUnit.Framework;


namespace schema.binary;

public partial class SchemaStructureParserTests {
  [Test]
  public void TestByte() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class ByteWrapper {
                                                        public byte field;
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    Assert.AreEqual("bar", structure.TypeSymbol.ContainingNamespace.Name);
    Assert.AreEqual("ByteWrapper", structure.TypeSymbol.Name);

    Assert.AreEqual(1, structure.Members.Count);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("field", field.Name);

    var memberType = field.MemberType;
    var primitiveType = (memberType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.BYTE, primitiveType.PrimitiveType);
    Assert.AreEqual(false, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }

  [Test]
  public void TestSByte() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class SByteWrapper {
                                                        public sbyte field;
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    Assert.AreEqual("bar", structure.TypeSymbol.ContainingNamespace.Name);
    Assert.AreEqual("SByteWrapper", structure.TypeSymbol.Name);

    Assert.AreEqual(1, structure.Members.Count);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("field", field.Name);

    var memberType = field.MemberType;
    var primitiveType = (memberType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.SBYTE, primitiveType.PrimitiveType);
    Assert.AreEqual(false, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }

  [Test]
  public void TestInt16() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class Int16Wrapper {
                                                        public short field;
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    Assert.AreEqual("bar", structure.TypeSymbol.ContainingNamespace.Name);
    Assert.AreEqual("Int16Wrapper", structure.TypeSymbol.Name);

    Assert.AreEqual(1, structure.Members.Count);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("field", field.Name);

    var memberType = field.MemberType;
    var primitiveType = (memberType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.INT16, primitiveType.PrimitiveType);
    Assert.AreEqual(false, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }

  [Test]
  public void TestEnum() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;
                                                    using schema.binary.attributes;

                                                    namespace foo.bar {
                                                      public enum ValueType {
                                                        A,
                                                        B,
                                                        C
                                                      }
                                                    
                                                      [BinarySchema]
                                                      public class EnumWrapper {
                                                        [IntegerFormat(SchemaIntegerType.UINT16)]
                                                        public ValueType field;
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    Assert.AreEqual("bar", structure.TypeSymbol.ContainingNamespace.Name);
    Assert.AreEqual("EnumWrapper", structure.TypeSymbol.Name);

    Assert.AreEqual(1, structure.Members.Count);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("field", field.Name);

    var memberType = field.MemberType;
    var primitiveType = (memberType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.ENUM, primitiveType.PrimitiveType);
    Assert.AreEqual(false, primitiveType.IsReadOnly);
    Assert.AreEqual(true, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UINT16, primitiveType.AltFormat);
  }

  [Test]
  public void TestConstArray() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class ArrayWrapper {
                                                        public readonly int[] field;
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("field", field.Name);

    var memberType = field.MemberType;
    var arrayType = (memberType as ISequenceMemberType)!;
    Assert.AreEqual(SequenceType.MUTABLE_ARRAY,
                    arrayType.SequenceTypeInfo.SequenceType);
    Assert.AreEqual(SequenceLengthSourceType.READ_ONLY,
                    arrayType.LengthSourceType);

    var primitiveType = (arrayType.ElementType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.INT32, primitiveType.PrimitiveType);
    Assert.AreEqual(false, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }

  [Test]
  public void TestConstCharArray() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class CharWrapper {
                                                        public readonly char[] field;
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("field", field.Name);

    var memberType = field.MemberType;
    var arrayType = (memberType as ISequenceMemberType)!;
    Assert.AreEqual(SequenceType.MUTABLE_ARRAY,
                    arrayType.SequenceTypeInfo.SequenceType);
    Assert.AreEqual(SequenceLengthSourceType.READ_ONLY,
                    arrayType.LengthSourceType);

    var primitiveType = (arrayType.ElementType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.CHAR, primitiveType.PrimitiveType);
    Assert.AreEqual(false, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }

  [Test]
  public void TestField() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class ByteWrapper {
                                                        public byte field;
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    Assert.AreEqual("bar", structure.TypeSymbol.ContainingNamespace.Name);
    Assert.AreEqual("ByteWrapper", structure.TypeSymbol.Name);

    Assert.AreEqual(1, structure.Members.Count);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("field", field.Name);

    var memberType = field.MemberType;
    var primitiveType = (memberType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.BYTE, primitiveType.PrimitiveType);
    Assert.AreEqual(false, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }

  [Test]
  public void TestProperty() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class ByteWrapper {
                                                        public byte Field { get; set; }
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    Assert.AreEqual("bar", structure.TypeSymbol.ContainingNamespace.Name);
    Assert.AreEqual("ByteWrapper", structure.TypeSymbol.Name);

    Assert.AreEqual(1, structure.Members.Count);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("Field", field.Name);

    var memberType = field.MemberType;
    var primitiveType = (memberType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.BYTE, primitiveType.PrimitiveType);
    Assert.AreEqual(false, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }

  [Test]
  public void TestReadonlyPrimitiveField() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class ByteWrapper {
                                                        public readonly byte field;
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    Assert.AreEqual("bar", structure.TypeSymbol.ContainingNamespace.Name);
    Assert.AreEqual("ByteWrapper", structure.TypeSymbol.Name);

    Assert.AreEqual(1, structure.Members.Count);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("field", field.Name);

    var memberType = field.MemberType;
    var primitiveType = (memberType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.BYTE, primitiveType.PrimitiveType);
    Assert.AreEqual(true, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }

  [Test]
  public void TestReadonlyPrimitiveProperty() {
    var structure = BinarySchemaTestUtil.ParseFirst("""

                                                    using schema.binary;

                                                    namespace foo.bar {
                                                      [BinarySchema]
                                                      public class ByteWrapper {
                                                        public byte Field { get; }
                                                      }
                                                    }
                                                    """);

    Assert.IsEmpty(structure.Diagnostics);

    Assert.AreEqual("bar", structure.TypeSymbol.ContainingNamespace.Name);
    Assert.AreEqual("ByteWrapper", structure.TypeSymbol.Name);

    Assert.AreEqual(1, structure.Members.Count);

    var field = structure.Members[0] as ISchemaValueMember;
    Assert.AreEqual("Field", field.Name);

    var memberType = field.MemberType;
    var primitiveType = (memberType as IPrimitiveMemberType)!;
    Assert.AreEqual(SchemaPrimitiveType.BYTE, primitiveType.PrimitiveType);
    Assert.AreEqual(true, primitiveType.IsReadOnly);
    Assert.AreEqual(false, primitiveType.UseAltFormat);
    Assert.AreEqual(SchemaNumberType.UNDEFINED, primitiveType.AltFormat);
  }
}