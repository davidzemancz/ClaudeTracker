namespace ClaudeTracker.Services;

public interface IAutoStartService
{
    bool IsEnabled();
    void SetEnabled(bool enable);
}
