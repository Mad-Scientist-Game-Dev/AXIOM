namespace Router;

public sealed record DependencyDecision(
    string Reason,
    string[] Files,
    bool BundleAll = false
);
