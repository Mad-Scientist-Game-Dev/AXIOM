public sealed class ContextBundle
{
    public Meta meta { get; set; } = new();
    public Intent intent { get; set; } = new();

    // Code domain
    public FileEntry[] files { get; set; } = Array.Empty<FileEntry>();

    // System domain
    public SystemInfo? system { get; set; }
}

public sealed class Meta
{
    public long? run_id { get; set; }
    public string? project_root { get; set; }
    public string created_utc { get; set; } = "";
}

public sealed class Intent
{
    public string intent_type { get; set; } = "";
    public string task_domain { get; set; } = "";
}

public sealed class FileEntry
{
    public long file_id { get; set; }
    public string path { get; set; } = "";
    public string sha256 { get; set; } = "";
    public long? size_bytes { get; set; }
    public string? last_modified_utc { get; set; }
}

public sealed class SystemInfo
{
    public string machine_name { get; set; } = "";
    public string os_version { get; set; } = "";
    public int process_id { get; set; }
    public string dotnet_version { get; set; } = "";
    public int processor_count { get; set; }
    public long working_set_bytes { get; set; }
}
