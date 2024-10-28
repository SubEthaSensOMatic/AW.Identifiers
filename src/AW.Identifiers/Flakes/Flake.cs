using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AW.Identifiers;

[Serializable]
public readonly struct Flake : IEquatable<Flake>, IComparable<Flake>, ISerializable
{
    public static readonly Flake Empty = new (0L);

    // 63bit
    // 48time + 4machine + 11seq
    // Layout
    // | Byte 7 | Byte 6 | Byte 5 | Byte 4 | Byte 3 | Byte 2 | Byte 1 | Byte 0 |
    // |--------|--------|--------|--------|--------|--------|--------|--------|
    // |0TTTTTTT|TTTTTTTT|TTTTTTTT|TTTTTTTT|TTTTTTTT|TTTTTTTT|TMMMMSSS|SSSSSSSS|
    //
    // 0 - Nicht verwendet
    // T - Timestamp
    // M - Maschinen Id
    // S - Sequenznummer

    public static readonly DateTime BEGINNING_OF_TIME = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly long MAX_SEQUENCE_NUMBER = 2047L;

    public static readonly long MAX_MACHINE_ID = 15L;

    private static readonly string BASE_62_CHARACTER_POOL = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public readonly long SequenceNumber
        => _id & 0b00000000_00000000_00000000_00000000_00000000_00000000_00000111_11111111L;

    public readonly long MachineId
        => (_id & 0b00000000_00000000_00000000_00000000_00000000_00000000_01111000_00000000L) >> 11;

    public DateTime Time => BEGINNING_OF_TIME
        .AddMilliseconds((_id & 0b01111111_11111111_11111111_11111111_11111111_11111111_10000000_00000000L) >> 15);

    private readonly long _id;

    public Flake(long time, long machineId, long sequence)
    {
        if (sequence < 0)
            throw new InvalidOperationException($"Sequence number has to be greater than or equal to 0!");

        if (sequence > MAX_SEQUENCE_NUMBER)
            throw new InvalidOperationException($"Sequence number has to be smaller than {MAX_SEQUENCE_NUMBER + 1}!");

        if (machineId < 0)
            throw new InvalidOperationException($"Machine id has to be greater than or equal to 0!");

        if (machineId > MAX_MACHINE_ID)
            throw new InvalidOperationException($"Machine id has to be smaller than {MAX_MACHINE_ID + 1}!");

        _id = (time << 15) | (machineId << 11) | sequence;
    }

    public Flake(long id)
    {
        if (id < 0)
            throw new InvalidOperationException($"Id has to be greater than or equal to 0!");
        _id = id;
    }

    public Flake(Flake flake)
        => _id = flake._id;

    public Flake(string s)
    {
        var id = long.Parse(s);
        if (id < 0)
            throw new InvalidOperationException($"Id has to be greater than or equal to 0!");
        _id = id;
    }

    public Flake(byte[] raw) =>
        _id = raw == null || raw.Length != 8
            ? throw new InvalidCastException("The byte array must have a length of 8 bytes!")
            : ((long)raw[0] << 56) | ((long)raw[1] << 48) | ((long)raw[2] << 40) | ((long)raw[3] << 32) |
                ((long)raw[4] << 24) | ((long)raw[5] << 16) | ((long)raw[6] << 8) | raw[7];

    public Flake(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info);

        var id = long.Parse(info.GetString("#") ?? string.Empty);
        if (id < 0)
            throw new InvalidOperationException($"Id has to be greater than or equal to 0!");
        _id = id;
    }

    public static implicit operator long(Flake id) => id._id;

    public static implicit operator Flake(long id) => new (id);

    public static implicit operator byte[](Flake id) =>
    [
        (byte) (id._id >> 56), (byte) (id._id >> 48), (byte) (id._id >> 40), (byte) (id._id >> 32),
        (byte) (id._id >> 24), (byte) (id._id >> 16), (byte) (id._id >> 8), (byte) id._id
    ];

    public static implicit operator Flake(byte[] raw) => new (raw);

    public static bool operator ==(Flake left, Flake right)
        => left._id == right._id;

    public static bool operator !=(Flake left, Flake right)
        => left._id != right._id;

    public override string ToString()
        => _id.ToString();

    public string ToString(string format)
        => _id.ToString(format);

    public override bool Equals(object? obj)
        => obj != null && obj is Flake flake && _id.Equals(flake);

    public override int GetHashCode()
        => _id.GetHashCode();

    public bool Equals(Flake other)
        => _id == other._id;

    public int CompareTo(Flake other)
        => _id.CompareTo(other._id);

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info);
        info.AddValue("#", _id.ToString());
    }

    public string ToBase62(bool padded = false)
        => ConvertBase(62, padded);

    public string ToBase36(bool padded = false)
        => ConvertBase(36, padded);

    public string ConvertBase(byte @base, bool padded = false)
    {
        if (@base < 2 || @base > 62)
            throw new InvalidOperationException("Base has to be between 2 and 62 including.");

        var maxLength = MAX_LENGTHS[@base];
        var data = new char[maxLength];
        var p = 0;
        var v = _id;
        var r = 0L;

        do
        {
            r = v % @base;
            v = v / @base;
            data[p++] = BASE_62_CHARACTER_POOL[(int)r];
        }
        while (v > 0);

        if (padded)
            for (; p < maxLength; p++)
                data[p] = '0';

        Array.Reverse(data);

        var result = new string(data, maxLength - p, p);

        return result;
    }

    private static readonly Dictionary<int, int> MAX_LENGTHS = new()
    {
        {2, 70}, {3, 45}, {4, 35}, {5, 31}, {6, 27}, {7, 25}, {8, 24}, {9, 23},
        {10, 21}, {11, 21}, {12, 20}, {13, 19}, {14, 19}, {15, 18}, {16, 18},
        {17, 18}, {18, 17}, {19, 17}, {20, 17}, {21, 16}, {22, 16}, {23, 16},
        {24, 16}, {25, 16}, {26, 15}, {27, 15}, {28, 15}, {29, 15}, {30, 15},
        {31, 15}, {32, 14}, {33, 14}, {34, 14}, {35, 14}, {36, 14}, {37, 14},
        {38, 14}, {39, 14}, {40, 14}, {41, 14}, {42, 13}, {43, 13}, {44, 13},
        {45, 13}, {46, 13}, {47, 13}, {48, 13}, {49, 13}, {50, 13}, {51, 13},
        {52, 13}, {53, 13}, {54, 13}, {55, 13}, {56, 13}, {57, 12}, {58, 12},
        {59, 12}, {60, 12}, {61, 12}, {62, 12}
    };
}