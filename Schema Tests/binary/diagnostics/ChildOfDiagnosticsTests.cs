using System;

using NUnit.Framework;


namespace schema.binary {
  internal class ChildOfDiagnosticsTests {
    [Test]
    [TestCase(typeof(IBinaryDeserializable), typeof(IBinaryDeserializable))]
    [TestCase(typeof(IBinarySerializable), typeof(IBinarySerializable))]
    [TestCase(typeof(IBinaryDeserializable), typeof(IBinaryConvertible))]
    [TestCase(typeof(IBinarySerializable), typeof(IBinaryConvertible))]
    public void TestSatisfyingBinaryConvertibility(
        Type childInterface,
        Type parentInterface) {
      var structure = BinarySchemaTestUtil.ParseFirst(@$"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {{
  [BinarySchema]
  public partial class Child : IChildOf<Parent>, {childInterface.Name} {{
    public Parent Parent {{ get; set; }}
  }}

  [BinarySchema]
  public partial class Parent : {parentInterface.Name} {{
    public Child Child {{ get; set; }}
  }}
}}");

      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics);
    }

    [Test]
    [TestCase(typeof(IBinarySerializable), typeof(IBinaryDeserializable))]
    [TestCase(typeof(IBinaryDeserializable), typeof(IBinarySerializable))]
    [TestCase(typeof(IBinaryConvertible), typeof(IBinarySerializable))]
    [TestCase(typeof(IBinaryConvertible), typeof(IBinaryDeserializable))]
    public void TestNonSatisfyingBinaryConvertibility(
        Type childInterface,
        Type parentInterface) {
      var structure = BinarySchemaTestUtil.ParseFirst(@$"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {{
  [BinarySchema]
  public partial class Child : IChildOf<Parent>, {childInterface.Name} {{
    public Parent Parent {{ get; set; }}
  }}

  [BinarySchema]
  public partial class Parent : {parentInterface.Name} {{
    public Child Child {{ get; set; }}
  }}
}}");

      BinarySchemaTestUtil.AssertDiagnostics(
          structure.Diagnostics,
          Rules.ParentBinaryConvertabilityMustSatisfyChild);
    }
  }
}