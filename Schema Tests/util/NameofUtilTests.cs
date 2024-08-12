using System.Runtime.CompilerServices;

using NUnit.Framework;


namespace schema.util {
  public class NameofUtilTests {
    public static uint Value { get; set; }

    [Test]
    public void TestNameof() {
      Assert.AreEqual("Value",
                      NameofUtil.GetChainedAccessFromCallerArgumentExpression(
                          ReturnArgumentText_(nameof(Value))));
    }

    [Test]
    public void TestThisAccessorNameof() {
      Assert.AreEqual("Value",
                      NameofUtil.GetChainedAccessFromCallerArgumentExpression(
                          ReturnArgumentText_(nameof(this.Value))));
    }

    [Test]
    public void TestStaticAccessorNameof() {
      Assert.AreEqual("Value",
                      NameofUtil.GetChainedAccessFromCallerArgumentExpression(
                          typeof(NameofUtilTests),
                          ReturnArgumentText_(nameof(NameofUtilTests.Value))));
    }

    [Test]
    public void TestQualifiedStaticAccessorNameof() {
      Assert.AreEqual("Value",
                      NameofUtil.GetChainedAccessFromCallerArgumentExpression(
                          typeof(NameofUtilTests),
                          ReturnArgumentText_(
                              nameof(schema.util.NameofUtilTests.Value))));
    }

    [Test]
    public void TestPartiallyQualifiedStaticAccessorNameof() {
      Assert.AreEqual("Value",
                      NameofUtil.GetChainedAccessFromCallerArgumentExpression(
                          typeof(NameofUtilTests),
                          ReturnArgumentText_(
                              nameof(util.NameofUtilTests.Value))));
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