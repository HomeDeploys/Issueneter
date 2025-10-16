using Issueneter.Domain.Models;

namespace Issueneter.Domain.Interfaces.Services;

public interface IMessageFormatter
{
    string Format(string template, Entity entity);
}