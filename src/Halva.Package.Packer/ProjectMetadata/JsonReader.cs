using System.Runtime.InteropServices;
using System.Text.Json;

namespace Halva.Package.Packer.ProjectMetadata;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "For safety reasons, using simplified using should be avoided (unless noted otherwise).")]
internal class JsonReader
{
    public static string FindGameFolder(string projectLocation)
    {
        if (File.Exists(projectLocation))
        {
            string input = File.ReadAllText(projectLocation);
            using (JsonDocument inputJson = JsonDocument.Parse(input))
            {
                JsonElement tempstring = inputJson.RootElement.GetProperty("main");
                if (tempstring.GetString() != null)
                {
                    string[] dataPart = tempstring.GetString().Split('/');
                    string tempString2 = dataPart[0];
                    if (dataPart.Length >= 2)
                        for (int i = 1; i < dataPart.Length - 2; i++)
                            tempString2 += dataPart[i] + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/");
                    else
                        if (tempString2.Contains(".html")) tempString2 = "";
                    return Path.Combine(projectLocation.Replace(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\package.json" : "/package.json", "", StringComparison.Ordinal), tempString2);
                }
                else return "Null";
            }
        }
        else return "Unknown";
    }
}
