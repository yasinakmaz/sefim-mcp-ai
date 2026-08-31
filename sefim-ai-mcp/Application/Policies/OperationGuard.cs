using System.Security.Cryptography;

namespace SefimMcp.Application.Policies;

/// <summary>
/// Two-step confirmation for state-changing tools: a challenge is issued, then consumed exactly once.
/// Challenges are short-lived and swept on every use, so an abandoned session cannot grow the process memory.
/// </summary>
public sealed class OperationGuard
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);
    private const int MaximumPending = 256;

    private readonly Dictionary<string, PendingOperation> pending = new(StringComparer.Ordinal);
    private readonly object gate = new();

    public PendingOperation Prepare(string operation, string target)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        var pendingOperation = new PendingOperation(token, operation, target, DateTimeOffset.UtcNow.Add(Lifetime));
        lock (gate)
        {
            Sweep();
            if (pending.Count >= MaximumPending)
                throw new InvalidOperationException("Too many unconfirmed operations are pending. Confirm or abandon them before preparing another.");

            pending[token] = pendingOperation;
        }

        return pendingOperation;
    }

    public bool Consume(string challenge, string operation, string target)
    {
        lock (gate)
        {
            Sweep();
            if (!pending.Remove(challenge, out var pendingOperation))
                return false;

            return pendingOperation.ExpiresAtUtc >= DateTimeOffset.UtcNow
                && pendingOperation.Operation == operation
                && pendingOperation.Target == target;
        }
    }

    private void Sweep()
    {
        if (pending.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        var expired = pending.Where(entry => entry.Value.ExpiresAtUtc < now).Select(entry => entry.Key).ToArray();
        foreach (var token in expired)
            pending.Remove(token);
    }
}

public sealed record PendingOperation(string Challenge, string Operation, string Target, DateTimeOffset ExpiresAtUtc);
