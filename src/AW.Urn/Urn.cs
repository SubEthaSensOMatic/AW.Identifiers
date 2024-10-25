using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace AW.Identifiers;

/// <summary>
/// Urn Implementierung nach RFC-2141
/// Siehe https://datatracker.ietf.org/doc/html/rfc2141
/// </summary>
[TypeConverter(typeof(UrnTypeConverter))]
[JsonConverter(typeof(UrnJsonConverter))]
public readonly struct Urn : IEquatable<Urn>, IComparable, IComparable<Urn>
{
    public static readonly Urn Empty = new(string.Empty, -1, []);

    public readonly ReadOnlySpan<char> NSS
        => _nssStart > 0
            ? _canonicalUrn.AsSpan(_nssStart)
            : [];

    public readonly int NIDCount => _nidPositions.Count;

    public readonly bool IsEmpty
        => _nssStart < 0;
    public readonly ReadOnlySpan<char> this[int index]
    {
        get => index >= 0 && index < _nidPositions.Count
            ? _canonicalUrn.AsSpan(_nidPositions[index].NIDStart, _nidPositions[index].NIDLength)
            : throw new IndexOutOfRangeException("Namespace identifier index is out of range.");
    }

    private readonly string _canonicalUrn;
    private readonly IReadOnlyList<(int NIDStart, int NIDLength)> _nidPositions;
    private readonly int _nssStart;

    /// <summary>
    /// Create URN
    /// </summary>
    /// <param name="nss">Unencoded namespace specific string</param>
    /// <param name="nids">Namespace identifiers</param>
    public Urn(string nss, IReadOnlyList<string> nids)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nss, nameof(nss));
        ArgumentNullException.ThrowIfNull(nids, nameof(nids));
        
        if (nids.Count == 0)
            throw new InvalidOperationException($"Namespace identitfer collection is empty.");

        var nidPositions = new (int NIDStart, int NIDLength)[nids.Count];

        var canonicalUrn = new StringBuilder("urn");
        int nidStart;
        InvalidOperationException? exc;

        for (var i = 0; i < nids.Count; i++)
        {
            canonicalUrn.Append(':');
            nidStart = canonicalUrn.Length;
            
            exc = AppendNID(nids[i], canonicalUrn);
            if (exc != null)
                throw exc;

            nidPositions[i] = (nidStart, canonicalUrn.Length - nidStart);
        }

        canonicalUrn.Append(':');
        _nssStart = canonicalUrn.Length;
        
        exc = AppendNSS(nss, canonicalUrn);
        if (exc != null)
            throw exc;

        _canonicalUrn = canonicalUrn.ToString();
        _nidPositions = nidPositions;
    }

    /// <summary>
    /// Empty constructor
    /// </summary>
    public Urn()
        => this = Empty;

    /// <summary>
    /// Create URN from string. Urn string should be encoded.
    /// </summary>
    /// <param name="urn"></param>
    public Urn(string urn): this(urn.AsSpan()) {}

    /// <summary>
    /// Create URN from string. Urn string should be encoded.
    /// </summary>
    /// <param name="urn"></param>
    public Urn(ReadOnlySpan<char> urn)
    {
        var (parsedUrn, exc) = ParserUrnInternal(urn);
        if (exc != null) throw exc;

        this = parsedUrn;
    }

    /// <summary>
    /// Interner Konstruktor für geparste Urns
    /// </summary>
    /// <param name="canonicalUrn"></param>
    /// <param name="nssStart"></param>
    /// <param name="nids"></param>
    private Urn(string canonicalUrn, int nssStart, IReadOnlyList<(int NIDStart, int NIDLength)> nids)
    {
        _canonicalUrn = canonicalUrn;
        _nssStart = nssStart;
        _nidPositions = nids;
    }

    public static bool TryParse(string input, out Urn urn)
        => TryParse(input.AsSpan(), out urn);

    public static bool TryParse(ReadOnlySpan<char> input, out Urn urn)
    {
        urn = Empty;

        var (parsedUrn, exc) = ParserUrnInternal(input);
        if (exc != null) return false;

        urn = parsedUrn;
        return true;
    }

    public static Urn Parse(string input)
        => Parse(input.AsSpan());

    public static Urn Parse(ReadOnlySpan<char> input)
    {
        var (parsedUrn, exc) = ParserUrnInternal(input);
        if (exc != null) throw exc;

        return parsedUrn;
    }

    public readonly override string ToString()
        => _canonicalUrn;

    public readonly override int GetHashCode() =>
        _canonicalUrn.GetHashCode();

    public static bool operator ==(Urn left, Urn right)
        => left.Equals(right);

    public static bool operator !=(Urn left, Urn right)
        => false == (left == right);

    public readonly bool Equals(Urn urn)
    {
        if (_canonicalUrn == null && urn._canonicalUrn == null)
            return true;
        else if (_canonicalUrn != null && urn._canonicalUrn == null)
            return false;
        else if (_canonicalUrn == null && urn._canonicalUrn != null)
            return false;
        else
            return _canonicalUrn!.Equals(urn._canonicalUrn);
    }

    public readonly override bool Equals(object? obj)
    {
        var urn = obj as Urn?;
        return urn != null && Equals(urn.Value);
    }

    public readonly int CompareTo(Urn other)
    {
        if (_canonicalUrn == null && other._canonicalUrn == null)
            return 0;
        if (_canonicalUrn == null && other._canonicalUrn != null)
            return -1;
        if (_canonicalUrn != null && other._canonicalUrn == null)
            return 1;
        return string.Compare(_canonicalUrn, other._canonicalUrn, StringComparison.Ordinal);
    }

    public readonly int CompareTo(object? obj)
    {
        var urn = obj as Urn?;

        return urn == null
            ? 1
            : CompareTo(urn.Value);
    }

    public static explicit operator string(Urn urn)
        => urn.ToString();

    public static explicit operator Urn(string s)
        => new (s);

    private static InvalidOperationException? AppendEscapedNSSChar(char input, StringBuilder output)
    {
        if (input.IsValidNSSChar())
        {
            output.Append(input);
        }
        else
        {
            byte b0 = 0;
            byte b1 = 0;
            byte b2 = 0;

            if (input.TryGetUTF8Bytes(ref b0, ref b1, ref b2) == false)
            {
                return new InvalidOperationException(
                    "Invalid UTF-8 character. Only 1 to 3 byte characters are allowed.");
            }

            if (b0 > 0)
            {
                output
                    .Append('%')
                    .Append(b0.GetHighNibbleAsHex())
                    .Append(b0.GetLowNibbleAsHex());
            }

            if (b1 > 0)
            {
                output
                    .Append('%')
                    .Append(b1.GetHighNibbleAsHex())
                    .Append(b1.GetLowNibbleAsHex());
            }

            if (b2 > 0)
            {
                output
                    .Append('%')
                    .Append(b2.GetHighNibbleAsHex())
                    .Append(b2.GetLowNibbleAsHex());
            }
        }

        return null;
    }

    private static InvalidOperationException? AppendNSS(ReadOnlySpan<char> input, StringBuilder output)
    {
        for (int i = 0; i < input.Length; i++)
        {
            var exc = AppendEscapedNSSChar(input[i], output);
            if (exc != null)
                return exc;
        }

        return null;
    }

    private static InvalidOperationException? AppendNID(ReadOnlySpan<char> input, StringBuilder canonicalUrn)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i].IsValidNIDChar() == false)
                return new InvalidOperationException($"Invalid namespace identifier character '{input[i]}'.");

            if ((i == 0 || i == input.Length - 1) && input[i] == '-')
                return new InvalidOperationException($"Invalid namespace identifier. Character '-' is not allowed at first or last position.");

            // Lowercase verwenden
            if (input[i] >= 65 && input[i] <= 90)
                canonicalUrn.Append((char)(input[i] + 32));
            else
                canonicalUrn.Append(input[i]);
        }

        return null;
    }

    private static bool IsValidUrnScheme(ReadOnlySpan<char> input)
        => input.Length >= 4
            && (input[0] == 'u' || input[0] == 'U')
            && (input[1] == 'r' || input[1] == 'R')
            && (input[2] == 'n' || input[2] == 'N')
            && input[3] == ':';

    private static (Urn Urn, Exception? Error) ParserUrnInternal(ReadOnlySpan<char> input)
    {
        // Muss mit 'urn:' beginnen
        if (IsValidUrnScheme(input) == false)
            return (Empty, new InvalidOperationException("Urn has to start with 'urn:'."));

        // Mindestlänge urn:a:b -> 7
        if (input.Length < 7)
            return (Empty, new InvalidOperationException("Urn must have at least one namespace identifier and one namespace specific string."));

        // NIDs ermitteln
        var nidPositions = new LinkedList<(int Start, int Length)>();
        var last = 4;

        for (var i = 4; i < input.Length; i++)
        {
            if (input[i] == ':')
            {
                nidPositions.AddLast((last, i - last));
                last = i + 1;
            }
        }

        if (nidPositions.Count == 0)
            return (Empty, new InvalidOperationException("Urn must have at least one namespace identifier."));

        // NSS ist der Rest ab Position 'last'
        if (last >= input.Length)
            return (Empty, new InvalidOperationException("Unexpected end of urn. Missing namespace specific string."));

        var canonicalUrn = new StringBuilder("urn");
        InvalidOperationException? exc = null;

        // Namespaces checken und anfügen
        foreach (var (nsStart, nsLength) in nidPositions)
        {
            canonicalUrn.Append(':');
            exc = AppendNID(input.Slice(nsStart, nsLength), canonicalUrn);

            if (exc != null)
                return (Empty, exc);
        }

        // NSS checken und anfügen
        canonicalUrn.Append(':');

        for (var i = last; i < input.Length; i++)
        {
            if (input[i].IsValidNSSChar())
            {
                canonicalUrn.Append(input[i]);
            }
            else if (input[i] == '%')
            {
                if (i + 2 >= input.Length)
                    return (Empty, new InvalidOperationException($"Invalid escaped character in namespace specific string."));

                if (input[i + 1].TryGetHexDigit(out var digit1) == false)

                    return (Empty, new InvalidOperationException($"Invalid escaped character in namespace specific string."));
                
                if (input[i + 2].TryGetHexDigit(out var digit2) == false)
                    return (Empty, new InvalidOperationException($"Invalid escaped character in namespace specific string."));

                canonicalUrn.Append('%').Append(digit1).Append(digit2);

                i += 2;
            }
            else
            {
                // Die länge kann sich im Vergleich zur Eingabe ändern. Die 
                // Positionen der NIDs, die vorher ermittelt wurden,
                // ändern sich aber nicht. Daher ist die Längenänderung
                // nicht problematisch.
                AppendEscapedNSSChar(input[i], canonicalUrn);
            }
        }

        return (new Urn(canonicalUrn.ToString(), last, [.. nidPositions]), null);
    }
}
