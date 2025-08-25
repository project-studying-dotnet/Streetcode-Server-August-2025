using System.Reflection.Metadata;
using System.Reflection;

namespace Streetcode.BLL
{
    /// <summary>
    /// Marker class for the application assembly.
    /// Used for clean MediatR registration and assembly scanning.
    /// </summary>
    public class ApplicationAssembly
    {
        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}
