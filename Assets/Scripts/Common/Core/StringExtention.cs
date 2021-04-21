
public static class StringExtention
{
    public static string FirstLetterCapital(this string str)
    {
        return char.ToUpper(str[0]) + str.Remove(0, 1);            
    }
}
