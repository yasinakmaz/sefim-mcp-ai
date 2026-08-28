using System.Security.Cryptography;

namespace SefimMcp.Application.Policies;

public sealed class OperationGuard
{
    private readonly Dictionary<string, PendingOperation> pending = new(StringComparer.Ordinal);
    private readonly object gate = new();

    public PendingOperation Prepare(string operation, string target)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        var pendingOperation = new PendingOperation(token, operation, target, DateTimeOffset.UtcNow.AddMinutes(2));
        lock (gate) pending[token] = pendingOperation;
        return pendingOperation;
    }

    public bool Consume(string challenge, string operation, string target)
    {
        lock (gate)
        {
            if (!pending.Remove(challenge, out var pendingOperation))
                return false;
            return pendingOperation.ExpiresAtUtc >= DateTimeOffset.UtcNow && pendingOperation.Operation == operation && pendingOperation.Target == target;
        }
    }
}

public sealed record PendingOperation(string Challenge, string Operation, string Target, DateTimeOffset ExpiresAtUtc);
