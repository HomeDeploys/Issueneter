using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Models;

public record WorkerConfiguration(
    WorkerId Id,
    ProviderInfo ProviderInfo,
    string Schedule,
    string Filter,
    ClientInfo ClientInfo,
    string Template);
