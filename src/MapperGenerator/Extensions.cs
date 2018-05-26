namespace DtoGenerator
{
  public static class Extensions
  {
    public static string ToLowerCamelCase(this string str)
    {
      return $"{str[0].ToString().ToLower()}{str.Substring(1)}";
    }
  }
}
