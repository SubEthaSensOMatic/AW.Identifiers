namespace AW.Identifiers;

internal static class ByteExtensions
{
    private static readonly char[] HEX_CHARACTERS
        = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'];

    public static char GetHighNibbleAsHex(this byte b)
        => HEX_CHARACTERS[(b >> 4) & 0x0f];

    public static char GetLowNibbleAsHex(this byte b)
        => HEX_CHARACTERS[b & 0x0f];
}
