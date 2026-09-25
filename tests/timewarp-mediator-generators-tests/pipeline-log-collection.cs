#region Purpose
// Serializes test classes whose unscoped sends run the global tracking behaviors that write to the static PipelineLog.
#endregion

[CollectionDefinition(Name)]
public sealed class PipelineLogCollection
{
    public const string Name = "PipelineLog";
}
