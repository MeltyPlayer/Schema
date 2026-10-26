using NUnit.Framework;


namespace schema.binary.text;

internal class FixedPointTests {
  [Test]
  public void TestMutableFixedPoint_1_19_12()
    => BinarySchemaTestUtil.AssertGenerated(
        """
        using System.Numerics;
        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class Wrapper {
          [FixedPoint(1, 19, 12)]
          public float SingleField { get; set; }
        
          [FixedPoint(1, 19, 12)]
          public double DoubleField { get; set; }
        }
        """,
        """
        using System;
        using schema.binary;
        using schema.util;

        namespace foo.bar;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            this.SingleField = BitLogic.ConvertFixedPointToSingle(br.ReadUInt32(), 1, 19, 12);
            this.DoubleField = BitLogic.ConvertFixedPointToDouble(br.ReadUInt32(), 1, 19, 12);
          }
        }

        """,
        """
        using System;
        using schema.binary;
        using schema.util;

        namespace foo.bar;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteUInt32(BitLogic.ConvertSingleToFixedPoint(this.SingleField, 1, 19, 12));
            bw.WriteUInt32(BitLogic.ConvertDoubleToFixedPoint(this.DoubleField, 1, 19, 12));
          }
        }

        """);

  [Test]
  public void TestReadonlyFixedPoint_1_19_12()
    => BinarySchemaTestUtil.AssertGenerated(
        """
        using System.Numerics;
        using schema.binary;
        using schema.binary.attributes;

        namespace foo.bar;

        [BinarySchema]
        public partial class Wrapper {
          [FixedPoint(1, 19, 12)]
          public float SingleField { get; }
        
          [FixedPoint(1, 19, 12)]
          public double DoubleField { get; }
        }
        """,
        """
        using System;
        using schema.binary;
        using schema.util;

        namespace foo.bar;

        public partial class Wrapper {
          public void Read(IBinaryReader br) {
            br.AssertUInt32(BitLogic.ConvertSingleToFixedPoint(this.SingleField, 1, 19, 12));
            br.AssertUInt32(BitLogic.ConvertDoubleToFixedPoint(this.DoubleField, 1, 19, 12));
          }
        }

        """,
        """
        using System;
        using schema.binary;
        using schema.util;

        namespace foo.bar;

        public partial class Wrapper {
          public void Write(IBinaryWriter bw) {
            bw.WriteUInt32(BitLogic.ConvertSingleToFixedPoint(this.SingleField, 1, 19, 12));
            bw.WriteUInt32(BitLogic.ConvertDoubleToFixedPoint(this.DoubleField, 1, 19, 12));
          }
        }

        """);
}