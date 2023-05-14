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
- [@Kermalis](https://github.com/Kermalis), whose [EndianBinaryIO](https://github.com/Kermalis/EndianBinaryIO) library inspired Span-based performance improvements.
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

The following attributes are currently supported in this library. Some attributes are only used at read or write time—these are prefixed with an R or W respectively.

**Warning: These names are not final, so they may change in future versions.**

#### Align

TODO

#### Endianness

TODO

#### IChildOf&lt;TParent&gt;

This pseudo-attribute marks a type as a "child" of some "parent" type—that it is contained as one of the members of the "parent type"—and passes the parent down to the child so it can be referenced in Schema logic. A child can be stored directly in the parent type, or as a member of a sequence (array/list).

Below is a simple example where a boolean from the parent is used to decide when to read a value in the child:

```
[BinarySchema]
public partial class ParentType : IBinaryConvertible {
  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool ChildHasSomeField { get; set; }

  public ChildType Child { get; } = new();
}

[BinarySchema]
public partial class ChildType : IBinaryConvertible, IChildOf<ParentType> {
  public ParentType Parent { get; set; }

  [Ignore]
  private bool HasSomeField => Parent.ChildHasSomeField;

  [IfBoolean(nameof(HasSomeField))]
  public int? SomeField { get; set; }
}
```

#### Ignore

TODO


#### Numbers/Enums

##### NumberFormat

TODO

##### IntegerFormat

TODO


#### Strings

TODO

##### StringLengthSource/RStringLengthSource

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
