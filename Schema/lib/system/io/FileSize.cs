// Decompiled with JetBrains decompiler
// Type: System.IO.FileSize
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

namespace System.IO {
  public static class FileSize {
    public static string[] BinaryExtensions = new string[5] {
        " B",
        " KiB",
        " MiB",
        " GiB",
        " TiB"
    };

    public static string[] SIExtensions = new string[5] {
        " B",
        " KB",
        " MB",
        " GB",
        " TB"
    };

    public const int BinaryStep = 1024;
    public const int SIStep = 1000;

    public static string FormatSize(int size) {
      double num = (double) size;
      int index = 0;
      while (num > (double) (FileSize.Step / 2)) {
        num /= (double) FileSize.Step;
        ++index;
      }
      return index > 0
                 ? num.ToString("#0.00") + FileSize.Extensions[index]
                 : num.ToString() + FileSize.Extensions[index];
    }

    private static string[] Extensions => FileSize.SIExtensions;
    private static int Step => 1000;
  }
}