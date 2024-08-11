using System.Reflection;
using System.Text;

namespace CodeFlows.GenAi.Common.Util
{
    public static class EmbeddedResourceUtil
    {
        public static string ReadResource(Assembly assembly, string name)
        {
            var stream =
                assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Resource {name} does not exist.");
            using var reader = new StreamReader(stream, Encoding.UTF8);

            return reader.ReadToEnd();
        }
    }
}
