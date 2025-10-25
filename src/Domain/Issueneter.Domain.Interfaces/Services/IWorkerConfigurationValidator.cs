using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Domain.Interfaces.Services;

public interface IWorkerConfigurationValidator
{
    ValidationResult Validate(WorkerConfiguration configuration);
}