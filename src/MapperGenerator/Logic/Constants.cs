using System.Collections.Generic;

namespace DtoGenerator.Logic
{
  public class Constants
  {
    public const string DtoSuffix = "Dto";

    public class Using
    {
      public const string NewtonsoftJson = "Newtonsoft.Json";

      public const string System = "System";

      public const string SystemCollectionsGeneric = "System.Collections.Generic";

      public const string SystemLinq = "System.Linq";

      public const string SystemRuntimeSerialization = "System.Runtime.Serialization";
    }

    public static List<string> SimpleTypeNames => new List<string>
    {
      "Boolean",
      "Byte",
      "SByte",
      "Char",
      "Decimal",
      "Double",
      "Single",
      "Int32",
      "UInt32",
      "Int64",
      "UInt64",
      "Object",
      "Int16",
      "UInt16",
      "String",

      "bool",
      "byte",
      "sbyte",
      "char",
      "decimal",
      "double",
      "float",
      "int",
      "uint",
      "long",
      "ulong",
      "object",
      "short",
      "ushort",
      "string",

      "DateTime"
    };
  }
}
