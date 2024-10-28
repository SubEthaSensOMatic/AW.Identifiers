using System;
using System.Diagnostics;
using System.Threading;

namespace AW.Identifiers;

public class FlakeFactory
{
    public static FlakeFactory Instance { get; }

    private readonly object _lock = new();
    private readonly long _machine;
    private readonly long _startTime;
    private readonly Stopwatch _timer;
    private long _sequence = 0L;
    private long _lastTimestamp = 0L;

    static FlakeFactory()
        => Instance = new FlakeFactory(0L);

    public FlakeFactory(long machine = 0L)
    {
        if (machine < 0)
            throw new InvalidOperationException($"Machine has to be greater than or equal to 0!");

        if (machine > Flake.MAX_MACHINE_ID)
            throw new InvalidOperationException($"Machine has to be smaller than {Flake.MAX_MACHINE_ID + 1}!");

        _machine = machine;

        _startTime = (long)(DateTime.UtcNow - Flake.BEGINNING_OF_TIME).TotalMilliseconds;

        // Beginn der Zählung mit Stopwatch, da DateTime nur eine Genauigkeit von 10-15ms hat.
        _timer = new Stopwatch();
        _timer.Start();

        _lastTimestamp = GetCurrentTimestamp();
    }

    public Flake NewFlake()
    {
        lock (_lock)
        {
            var now = GetCurrentTimestamp();

            if (now < _lastTimestamp)
            {
                throw new InvalidOperationException("System clock error!");
            }
            else if (now > _lastTimestamp)
            {
                _sequence = 0L;
            }
            else if (now == _lastTimestamp)
            {
                _sequence++;
                if (_sequence > Flake.MAX_SEQUENCE_NUMBER)
                {
                    while ((now = GetCurrentTimestamp()) <= _lastTimestamp)
                        Thread.Sleep(1);
                    _sequence = 0L;
                }
            }
            _lastTimestamp = now;

            return new Flake(now, _machine, _sequence);
        }
    }
    private long GetCurrentTimestamp()
        => _startTime + _timer.ElapsedMilliseconds;
}