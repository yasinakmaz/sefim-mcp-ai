using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using SefimMcp.Application.Policies;
using SefimMcp.Mcp.Dtos;

namespace SefimMcp.Mcp.Tools;

[McpServerToolType]
public sealed class OperationTools(OperationGuard operationGuard)
{
    [McpServerTool(Name = "prepare_destructive_operation", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false, UseStructuredContent = true)]
    [Description("Creates a short-lived server challenge for a specific destructive operation and target. A caller must obtain user confirmation outside the model before the challenge may be consumed by a guarded operation.")]
    public OperationChallengeDto PrepareDestructiveOperation(
        [Description("Exact server-defined operation name.")] string operation,
        [Description("Exact stable target identifier for that operation.")] string target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(target);
        var pending = operationGuard.Prepare(operation, target);
        return new(pending.Operation, pending.Target, pending.Challenge, pending.ExpiresAtUtc);
    }
}
