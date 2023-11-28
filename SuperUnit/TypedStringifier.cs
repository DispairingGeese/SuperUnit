namespace SuperUnit;

public static class TypedStringifier
{
    public static string ToTypedString(this object on)
    {
        return $"<{on.GetType().Name}>({on})";
    }
}