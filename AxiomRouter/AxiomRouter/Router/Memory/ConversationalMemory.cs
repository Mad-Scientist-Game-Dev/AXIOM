using System.Text.Json;

namespace Router.Memory;

public sealed class ConversationalMemory
{
    private readonly string _filePath;

    private MemoryStore _store = new();

    public ConversationalMemory(string filePath = "memory.json")
    {
        _filePath = filePath;

        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _store = JsonSerializer.Deserialize<MemoryStore>(json)
                     ?? new MemoryStore();
        }
        else
        {
            Save();
        }
    }

    public string? GetPreference(string key)
    {
        if (_store.User.Preferences.TryGetValue(key, out var value))
            return value;

        return null;
    }

    public void SetPreference(string key, string value)
    {
        _store.User.Preferences[key] = value;
        Save();
    }

    public MemoryStore Snapshot() => _store;

    private void Save()
    {
        var json = JsonSerializer.Serialize(
            _store,
            new JsonSerializerOptions { WriteIndented = true }
        );

        File.WriteAllText(_filePath, json);
    }
}

public sealed class MemoryStore
{
    public UserProfile User { get; set; } = new();
}

public sealed class UserProfile
{
    public string? Name { get; set; }

    public Dictionary<string, string> Preferences { get; set; }
        = new();
}
