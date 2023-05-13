using NUnit.Framework;

using schema.binary;


namespace schema.defaultinterface {
  internal class IncludeDefaultInterfaceMethodsTests {
    [Test]
    public void TestHandlesNamespaces() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

namespace foo.bar {
  interface IInterface {
    void Something() => 1;
  }

  [IncludeDefaultInterfaceMethods]
  public partial class Class : IInterface {
  }
}
",
        @"using schema.defaultinterface;

namespace foo.bar {
  public partial class Class {
    public void Something() => 1;
  }
}
");
    }

    [Test]
    public void TestIgnoresMethodWithoutImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface {
  void Something();
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface {
}
",
        @"public partial class Class {
}
");
    }

    [Test]
    public void TestIncludesMethodWithImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface {
  void Something() {
    var a = 0;
  }
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public void Something() {
    var a = 0;
  }
}
");
    }

    [Test]
    public void TestIgnoresMethodIfAlreadyImplemented() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface {
  void Something() {
    var a = 0;
  }
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface {
  public void Something() {
    // Do nothing
  }
}
",
        @"public partial class Class {
}
");
    }

    [Test]
    public void TestIgnoresGenericMethodWithImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface<T> {
  T Something() {
    var a = 0;
  }
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface<int> {
  public int Something() {
    // Do nothing
  }
}
",
        @"public partial class Class {
}
");
    }

    [Test]
    public void TestIncludesGenericMethodWithoutImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface<T> {
  T Something() {
    var a = 0;
  }
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface<int> {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public int Something() {
    var a = 0;
  }
}
");
    }


    [Test]
    public void TestIgnoresGenericMethodOnlyWithImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface {
  void Something<T>(T value) {
    var a = 0;
  }
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface {
  public void Something<T>(T value) {
    // Do nothing
  }
}
",
        @"public partial class Class {
}
");
    }

    [Test]
    public void TestIncludesGenericMethodOnlyWithoutImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface {
  void Something<T>(T value) {
    var a = 0;
  }
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public void Something<T>(T value) {
    var a = 0;
  }
}
");
    }


    [Test]
    public void TestIgnoresPropertyWithoutImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface {
  byte Something { get; }
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface {
}
",
        @"public partial class Class {
}
");
    }

    [Test]
    public void TestIncludesPropertyWithImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface {
  byte Something => 1;
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public byte Something => 1;
}
");
    }

    [Test]
    public void TestIgnoresPropertyIfAlreadyImplemented() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface {
  byte Something => 1;
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface {
  public byte Something => 3;
}
",
        @"public partial class Class {
}
");
    }

    [Test]
    public void TestIgnoresGenericPropertyWithImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface<T> {
  T Something => default;
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface<int> {
  public int Something => 1;
}
",
        @"public partial class Class {
}
");
    }

    [Test]
    public void TestIncludesGenericPropertyWithoutImplementation() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface<T> {
  T Something => default;
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface<int> {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public int Something => default;
}
");
    }

    [Test]
    public void TestHandlesPropertiesWithAttributes() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface<T> {
  [Something]
  T Something => default;
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface<int> {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  [Something]
  public int Something => default;
}
");
    }

    [Test]
    public void TestHandlesRepeatedProperty() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface1 {
  int Something { get; }
}

interface IInterface2 : IInterface1 {
  int IInterface1.Something => 1;
}


[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface2 {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public int Something => 1;
}
");
    }

    [Test]
    public void TestHandlesGenericRepeatedProperty1() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface1<T> {
  T Something { get; }
}

interface IInterface2 : IInterface1<int> {
  int IInterface1<int>.Something => 1;
}


[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface2 {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public int Something => 1;
}
");
    }

    [Test]
    public void TestHandlesGenericRepeatedProperty2() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface1<T1, T2> {
  T Something { get; }
}

interface IInterface2 : IInterface1<int, int> {
  int IInterface1<int, int>.Something => 1;
}


[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface2 {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public int Something => 1;
}
");
    }

    [Test]
    public void TestHandlesGenericRepeatedProperty3() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

namespace foo.bar {
  public class A {}
}

interface IInterface1<T> {
  T Something { get; }
}

interface IInterface2 : IInterface1<foo.bar.A> {
  foo.bar.A IInterface1<foo.bar.A>.Something => default;
}


[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface2 {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public foo.bar.A Something => default;
}
");
    }

    [Test]
    public void TestCanIgnoreGenericRepeatedProperty() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface1<T> {
  T Something { get; }
}

interface IInterface2 : IInterface1<int> {
  int IInterface1<int>.Something => 1;
}


[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface2 {
  public int Something => 3;
}
",
        @"public partial class Class {
}
");
    }

    [Test]
    public void TestIncludesStaticMethods() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

interface IInterface<TSelf> where TSelf : IInterface<TSelf> {
  static TSelf Something() => default;
}

[IncludeDefaultInterfaceMethods]
public partial class Class : IInterface<Class> {
}
",
        @"using schema.defaultinterface;

public partial class Class {
  public static Class Something() => default;
}
");
    }

    [Test]
    public void TestIncludesStaticMethods2() {
      DefaultInterfaceMethodsTestUtil.AssertGenerated(@"
using schema.defaultinterface;

namespace foo.bar {
  public class A {}

  interface IInterface1<TSelf, T1, T2> {
    static abstract TSelf Something(T1 value1, T2 value2);
  }

  interface IInterface2 : IInterface1<Class, A, int> {
    static Class IInterface1<Class, A, int>.Something(A value1, int value2) => default;
  }

  [IncludeDefaultInterfaceMethods]
  public partial class Class : IInterface2 {
  }
}
",
        @"using schema.defaultinterface;

namespace foo.bar {
  public partial class Class {
    public static Class Something(A value1, int value2) => default;
  }
}
");
    }
  }
}