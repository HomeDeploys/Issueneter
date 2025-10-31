using Issueneter.Domain.Models;

namespace Issueneter.Tests.Helpers;

/// <summary>
/// Test entity for unit tests with various property types for testing Entity base class functionality.
/// </summary>
internal class TestEntity : Entity
{
    public string StringProperty { get; set; } = string.Empty;
    public string? NullStringProperty { get; set; }
    public int IntProperty { get; set; }
    public int[] IntArrayProperty { get; set; } = [];
    public string[] StringArrayProperty { get; set; } = [];
}