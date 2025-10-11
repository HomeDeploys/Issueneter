using Issueneter.Domain.Enums;

namespace Issueneter.Domain.Interfaces.Services;

public interface IClient
{
    static ClientType Type { get; }
    bool Validate(string target);
    Task Send(string target, string messages,  CancellationToken token);
}