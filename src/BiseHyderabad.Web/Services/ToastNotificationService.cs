using System;
using System.Collections.Generic;
using System.Timers;

namespace BiseHyderabad.Web.Services;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ToastType Type { get; set; } = ToastType.Success;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int DurationMs { get; set; } = 7000;
}

public interface IToastNotificationService
{
    IReadOnlyList<ToastItem> Toasts { get; }
    event Action? OnChange;
    void Show(string message, ToastType type = ToastType.Success, string? title = null, int durationMs = 7000);
    void ShowSuccess(string message, string title = "Operation Successful", int durationMs = 7000);
    void ShowError(string message, string title = "Operation Failed", int durationMs = 7000);
    void ShowWarning(string message, string title = "Attention", int durationMs = 7000);
    void ShowInfo(string message, string title = "Information", int durationMs = 7000);
    void Dismiss(Guid id);
}

public class ToastNotificationService : IToastNotificationService
{
    private readonly List<ToastItem> _toasts = new();
    public IReadOnlyList<ToastItem> Toasts => _toasts.AsReadOnly();
    public event Action? OnChange;

    public void Show(string message, ToastType type = ToastType.Success, string? title = null, int durationMs = 7000)
    {
        title ??= type switch
        {
            ToastType.Success => "Success",
            ToastType.Error => "Error",
            ToastType.Warning => "Warning",
            _ => "Notification"
        };

        var toast = new ToastItem
        {
            Type = type,
            Title = title,
            Message = message,
            DurationMs = durationMs
        };

        lock (_toasts)
        {
            _toasts.Add(toast);
        }

        NotifyStateChanged();

        if (durationMs > 0)
        {
            var timer = new System.Timers.Timer(durationMs);
            timer.AutoReset = false;
            timer.Elapsed += (sender, e) =>
            {
                Dismiss(toast.Id);
                timer.Dispose();
            };
            timer.Start();
        }
    }

    public void ShowSuccess(string message, string title = "Operation Successful", int durationMs = 7000)
        => Show(message, ToastType.Success, title, durationMs);

    public void ShowError(string message, string title = "Operation Failed", int durationMs = 7000)
        => Show(message, ToastType.Error, title, durationMs);

    public void ShowWarning(string message, string title = "Attention", int durationMs = 7000)
        => Show(message, ToastType.Warning, title, durationMs);

    public void ShowInfo(string message, string title = "Information", int durationMs = 7000)
        => Show(message, ToastType.Info, title, durationMs);

    public void Dismiss(Guid id)
    {
        lock (_toasts)
        {
            _toasts.RemoveAll(t => t.Id == id);
        }
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
