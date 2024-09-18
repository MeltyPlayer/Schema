using NUnit.Framework;

using schema.readOnly;


namespace readOnly;

[GenerateReadOnly]
public partial interface ISomeType<T> {
  public T Data { get; set; }
}

public partial class SomeType<T> : ISomeType<T> {
  public T Data { get; set; }
}

[GenerateReadOnly]
public partial interface IValueType {
  public string Value { get; set; }
}

public partial class ValueType : IValueType {
  public string Value { get; set; }
}

[GenerateReadOnly]
public partial class ParameterCastWrapper {
  [Const]
  public ISomeType<IValueType> Method(ISomeType<IValueType> value) {
    return value;
  }
}

[GenerateReadOnly]
public partial class GenericReturnValueCastWrapper<T> where T : IValueType {
  [Const]
  public ISomeType<T> Method() => this.Property;

  public required ISomeType<T> Property { get; init; }
}

[GenerateReadOnly]
public partial class GenericParameterCastWrapper<T> where T : IValueType {
  [Const]
  public ISomeType<T> Method(ISomeType<T> value) => value;
}

public class CastTests {
  [Test]
  public void TestParameterCasting() {
    var expectedValue = new SomeType<IValueType> { Data = new ValueType() };

    IReadOnlyParameterCastWrapper fooBar = new ParameterCastWrapper();
    var returnValue = fooBar.Method(expectedValue);

    Assert.AreSame(expectedValue, returnValue);
  }

  [Test]
  public void TestGenericReturnValueCasting() {
    var expectedValue = new SomeType<IValueType> { Data = new ValueType() };

    IReadOnlyGenericReturnValueCastWrapper<IValueType> fooBar
        = new GenericReturnValueCastWrapper<IValueType> {
            Property = expectedValue
        };
    var returnValue = fooBar.Method();

    Assert.AreSame(expectedValue, returnValue);
  }
}