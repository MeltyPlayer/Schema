# Schema

Library for converting classes to and from binary. Provides a C# Roslyn generator that automatically implements conversion logic for simple classes.

## Credits

- [@jefffhaynes](https://github.com/jefffhaynes), as their [BinarySerializer](https://github.com/jefffhaynes/BinarySerializer) attribute library inspired the schema attributes for configuring how binary data is read.
- [@Sergio0694](https://github.com/Sergio0694), as their [BinaryPack](https://github.com/Sergio0694/BinaryPack) generator inspired the schema source generator used to generate read/write methods.

## Usage

### Setting up a project

Copy this project into your solution, and then add the following into any other projects that you wish to automatically generate code for:

```
<ItemGroup>
  <ProjectReference Include="../Schema/Schema.csproj" PrivateAssets="all" OutputItemType="Analyzer" />
</ItemGroup>
```

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
