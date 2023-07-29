using NUnit.Framework;


namespace schema.binary {
  public class BooleanDiagnosticTests {
    [Test]
    public void TestBooleanWithoutAltFormat() {
      var structure = BinarySchemaTestUtil.ParseFirst(@"
namespace foo.bar {
  [BinarySchema]
  public partial class BooleanWrapper {
    public bool field;
  }
}");
      BinarySchemaTestUtil.AssertDiagnostics(structure.Diagnostics,
                                       Rules.BooleanNeedsIntegerFormat);
    }
  }
}