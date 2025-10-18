using Issueneter.Domain.Enums;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Services;

public interface IClient
{
    ClientType Type { get; }
    ValidationResult Validate(string target);
    Task Send(string target, string message,  CancellationToken token);
}