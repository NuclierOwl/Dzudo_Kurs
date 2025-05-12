using Avalonia.Threading;
using System;

public class TimerService
{
    private DispatcherTimer _mainTimer;
    private DispatcherTimer _holdTimer;
    private TimeSpan _mainTime = TimeSpan.FromMinutes(2);
    private TimeSpan _holdTime = TimeSpan.Zero;

    public TimeSpan MainTime => _mainTime;
    public TimeSpan HoldTime => _holdTime;
    public bool IsMainTimerRunning => _mainTimer?.IsEnabled ?? false;

    public event Action MainTimerChanged;
    public event Action HoldTimerChanged;

    public TimerService()
    {
        _mainTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _mainTimer.Tick += (s, e) => {
            _mainTime = _mainTime.Subtract(TimeSpan.FromSeconds(1));
            MainTimerChanged?.Invoke();
        };

        _holdTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _holdTimer.Tick += (s, e) => {
            _holdTime = _holdTime.Add(TimeSpan.FromSeconds(1));
            HoldTimerChanged?.Invoke();
        };
    }

    public void StartMainTimer() => _mainTimer.Start();
    public void StopMainTimer() => _mainTimer.Stop();
    public void ResetMainTimer() => _mainTime = TimeSpan.FromMinutes(2);

    public void StartHoldTimer() => _holdTimer.Start();
    public void StopHoldTimer() => _holdTimer.Stop();
    public void ResetHoldTimer() => _holdTime = TimeSpan.Zero;
}