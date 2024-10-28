namespace AW.Identifiers.Internal;

internal static class CharacterExtensions
{
    public static bool TryGetUTF8Bytes(this char input, ref byte b0, ref byte b1, ref byte b2)
    {
        // Nur BMP (Basic Multilingual Plane) Zeichen zulassen
        if ((input >= 0xD800 && input <= 0xDFFF) || input > 0xFFFF)
            return false;

        if (input <= 0x7f)
        {
            // 1 Byte
            b0 = (byte)input;
            b1 = 0;
            b2 = 0;
        }
        else if (input <= 0x7ff)
        {
            // 2 Bytes
            b0 = (byte)((input >> 6) | 0b11000000);
            b1 = (byte)((input & 0b00111111) | 0b10000000);
            b2 = 0;
        }
        else
        {
            // 3 Bytes
            b0 = (byte)((input >> 12) | 0b11100000);
            b1 = (byte)((input >> 6 & 0b00111111) | 0b10000000);
            b2 = (byte)((input & 0b00111111) | 0b10000000);
        }

        return true;
    }

    public static bool TryGetHexDigit(this char input, out char hexDigit)
    {
        hexDigit = '\0';

        if (input >= 'A' && input <= 'F')
        {
            hexDigit = (char)(input + 32);
            return true;
        }

        if ((input >= 'a' && input <= 'z')
            || (input >= '0' && input <= '9'))
        {
            hexDigit = input;
            return true;
        }

        return false;
    }

    public static bool IsValidNSSChar(this char input)
        => (input >= 'a' && input <= 'z')
            || (input >= 'A' && input <= 'Z')
            || (input >= '0' && input <= '9')
            || input == '-'
            || input == '_'
            || input == '.'
            || input == '~';

    public static bool IsValidNIDChar(this char input)
        => (input >= 'a' && input <= 'z')
            || (input >= 'A' && input <= 'Z')
            || (input >= '0' && input <= '9')
            || input == '-';
}
