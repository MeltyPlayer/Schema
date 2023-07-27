using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace schema.util {
  public class NameofUtilTests {
    public static uint Value { get; set; }

    [Test]
    public void TestNameof() {
      Assert.AreEqual("NameofUtilTests.Value",
                      NameofUtil.GetChainedAccessFromCallerArgumentExpression(
                          ReturnArgumentText_(nameof(NameofUtilTests.Value))));
    }

    [Test]
    public void TestString() {
      Assert.AreEqual("Some.Field.Name",
                      NameofUtil.GetChainedAccessFromCallerArgumentExpression(
                          ReturnArgumentText_("Some.Field.Name")));
    }

    [Test]
    public void TestOther() {
      Assert.AreEqual("$\"Something${true}\"",
                      NameofUtil.GetChainedAccessFromCallerArgumentExpression(
                          ReturnArgumentText_($"Something${true}")));
    }

    private string ReturnArgumentText_(
        string arg,
        [CallerArgumentExpression(nameof(arg))] string argText = "")
      => argText;
  }
}