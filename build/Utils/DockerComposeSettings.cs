using System;
using System.Collections.Generic;
using Nuke.Common.Tooling;

namespace Utils;

[Serializable]
public class DockerComposeSettings : ToolSettings
{
    public override string ProcessToolPath => DockerComposeTasks.DockerPath;

    internal List<string> FileInternal = default;
    public IReadOnlyCollection<string> File => FileInternal.AsReadOnly();
}