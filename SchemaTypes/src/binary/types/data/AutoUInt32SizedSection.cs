﻿using schema.binary.attributes;


namespace schema.binary.types.data;

/// <summary>
///   Schema class that implements a uint32-sized section without needing to
///   worry about passing in an instance of the contained data. This should
///   be adequate for most cases, except when the data class needs to access
///   parent data.
/// </summary>
[BinarySchema]
public partial class AutoUInt32SizedSection<T> : ISizedSection<T>
    where T : IBinaryConvertible, new() {
  private readonly PassThruUInt32SizedSection<T> impl_;

  [Skip]
  public T Data => this.impl_.Data;

  public AutoUInt32SizedSection() {
      this.impl_ = new(new T());
    }

  public AutoUInt32SizedSection(int tweakSize) {
      this.impl_ = new(new T(), tweakSize);
    }
}