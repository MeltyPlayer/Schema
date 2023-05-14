# Schema

![GitHub](https://img.shields.io/github/license/MeltyPlayer/Schema)
[![Nuget](https://img.shields.io/nuget/v/schema)](https://www.nuget.org/packages/schema)
![Nuget](https://img.shields.io/nuget/dt/schema)
![Unit tests](https://github.com/MeltyPlayer/Schema/actions/workflows/dotnet.yml/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/MeltyPlayer/Schema/badge.svg?service=github)](https://coveralls.io/github/MeltyPlayer/Schema)

## Overview

WIP library for converting classes to and from binary. Provides a C# Roslyn generator that automatically implements conversion logic for simple classes.

**Warning: The design of this library is still in flux, so anticipate making changes when upgrading to future versions.**

## Credits

- [@connorhaigh](https://github.com/connorhaigh), whose [SubstreamSharp](https://github.com/connorhaigh/SubstreamSharp) library was pulled in for reading substreams.
- [@jefffhaynes](https://github.com/jefffhaynes), whose [BinarySerializer](https://github.com/jefffhaynes/BinarySerializer) attribute library inspired the schema attributes for configuring how binary data is read.
- [@Kermalis](https://github.com/Kermalis), whose [EndianBinaryIO](https://github.com/Kermalis/EndianBinaryIO) library inspired [Span](https://learn.microsoft.com/en-us/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay)-based performance improvements.
- [@Sergio0694](https://github.com/Sergio0694), whose [BinaryPack](https://github.com/Sergio0694/BinaryPack) generator inspired the schema source generator used to generate read/write methods.

## Usage

### Implementing binary schema classes

To write a binary schema class, you must first do the following steps:
1) Mark it as partial.
1) Have it implement the `IBinaryConvertible` interface (a combination of the `IBinarySerializable` and `IBinaryDeserializable` interfaces).

Then, based on how complicated your schema class is, you can either choose to automatically or manually implement `Read()`/`Write()` methods.

#### Automatically

For most schema classes, you should be able to use the automatic code generator.

All you have to do is annotate the schema class with the `[BinarySchema]` attribute; this will flag to the generator that it should implement read/write methods for this class.
It will then look into all fields/properties in the schema class, and attempt to implement read/write logic in the same order that the fields/properties appear.

Any nested schema classes will be automatically read/written as expected.

Some types require additional attributes in order to clarify any ambiguity.
For example, booleans require a `[IntegerFormat(SchemaIntegerType.###)]` attribute to know what type of integer to read, which it will then compare to 0.

Any readonly primitives will treated as assertions, which is useful for validating things like magic text or padding.

#### Manually

For complicated schema classes, such as ones that use decompression logic or pointers, you'll need to implement the read/write logic manually.

Specifically, you'll need to implement both a `Read(IEndianBinaryReader er)` and `Write(ISubEndianBinaryWriter ew)` method.
The `EndianBinaryReader` and `EndianBinaryWriter` classes provide many helpful methods for reading/writing a number of different primitive formats, including basic ones such as `byte`/`int`/`float`, but also more complex/unique ones such as `Half` (two-byte float) and `un16` (unsigned normalized 16-bit float).

Similar to the automatic process, you can nest schema classes and manually read/write them by calling their `Read()`/`Write()` methods. 
This can allow you to automatically generate subsections, so only the most complex logic needs to be manually written.

### How to use a binary schema class

To convert a given schema class to or from binary, simply instantiate an `EndianBinaryReader` or `EndianBinaryWriter` and pass it into the corresponding `Read()` or `Write()` methods in the schema class. 
You must use the `EndianBinaryReader`/`EndianBinaryWriter` defined within this project, as this adds more functionality.

### Supported Attributes

The following attributes are currently supported in this library **when automatically generating code**. Some attributes are only used at read or write time—these are prefixed with an R or W respectively.

**Warning: These names are not final, so they may change in future versions.**

#### Align

TODO

#### Endianness

Forces a type, field, or property to be read with a given [endianness](https://en.wikipedia.org/wiki/Endianness) (big-endian or little-endian). Tracked via a stack within the `EndianBinaryReader`/`EndianBinaryWriter`. If unspecified, will use whatever endianness was last specified in the stack (or the system endianness by default).
```cs
[BinarySchema]
[Endianness(Endianness.BigEndian)]
public partial class BigEndianType {
  ...
  
  [Endianness(Endianness.LittleEndian)]
  public int LittleEndianProperty { get; set; }
  
  ...
}
```

#### IChildOf&lt;TParent&gt;

This pseudo-attribute marks a type as a "child" of some "parent" type—that it is contained as one of the members of the "parent type"—and passes the parent down to the child so it can be referenced in Schema logic.

Used by having the child type implement the `IChildOf<TParent>` interface, where `TParent` stores the child type in a field/property or as a member of a sequence (array/list):
```cs
[BinarySchema]
public partial class ChildType : IBinaryConvertible, IChildOf<ParentType> {
  public ParentType Parent { get; set; }
  
  ...
}
```

Below is a simple example where a boolean from the parent is used to decide when to read a value in the child:
```cs
[BinarySchema]
public partial class ParentType : IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool ChildHasSomeField { get; set; }

  public ChildType Child { get; } = new();
}

[BinarySchema]
public partial class ChildType : IBinaryConvertible, IChildOf<ParentType> {
  // This is automatically ignored while reading/writing.
  public ParentType Parent { get; set; }

  [Ignore]
  private bool HasSomeField => Parent.ChildHasSomeField;

  [IfBoolean(nameof(HasSomeField))]
  public int? SomeField { get; set; }
}
```

#### Ignore

Designates that a field or property should be ignored while reading/writing.

*Note: `IChildOf<TParent>.Parent` is automatically ignored.*
```cs
[Ignore]
public int ignoredField;

[Ignore]
public int IgnoredProperty { get; set; }
```

This can be used to encapsulate boolean logic within properties, such as in the following examples:

1) **Value conversion**
```cs
[StringLengthSource(4)]
public string Magic { get; set; }

[Ignore]
public MagicType Type => this.Magic switch {
  "IMGE" => MagicType.IMAGE,
  "SOND" => MagicType.SOUND,
  "TEXT" => MagicType.TEXT,
};
```

2) **"Switch" cases**
```cs
[NullTerminatedString]
public string Magic { get; set; }

[Ignore]
public ISection? Section => this.imageSection_ ?? this.soundSection_ ?? this.textSection_;

[Ignore]
private bool IsImage_ => this.Magic == "IMAGE";
[Ignore]
private bool IsSound_ => this.Magic == "SOUND";
[Ignore]
private bool IsText_ => this.Magic == "TEXT";

[IfBoolean(nameof(this.IsImage))]
private ImageSection? imageSection_ { get; set; }

[IfBoolean(nameof(this.IsSound_))]
private SoundSection? soundSection_ { get; set; }

[IfBoolean(nameof(this.IsText_))]
private TextSection? textSection_ { get; set; }
```

#### Numbers/Enums

##### NumberFormat

TODO

##### IntegerFormat

TODO


#### Strings

TODO

##### StringLengthSource/RStringLengthSource

TODO

##### NullTerminatedString

TODO


#### Sequences

"Sequences" are the term used within Schema to refer to arrays/lists of elements. Multiple attributes are supported for specifying how many elements to read in the list.

##### SequenceLengthSource/RSequenceLengthSource

TODO

##### RSequenceUntilEndOfStreamAttribute

TODO


#### Pointers/Memory

TODO

##### WPointerTo

TODO

##### WSizeOfMemberInBytes

TODO
