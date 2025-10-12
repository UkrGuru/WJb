using UkrGuru.WJb.Data;

namespace UkrGuru.WJb.Actions;

public interface IAction
{
    Task<bool> ExecAsync(CancellationToken cancellationToken);
    Task<bool> NextAsync(bool execResult, CancellationToken cancellationToken);
    void Init(Job job);
}