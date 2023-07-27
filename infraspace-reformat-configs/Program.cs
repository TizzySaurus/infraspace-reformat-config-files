using Newtonsoft.Json;

string executionDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? throw new Exception("Failed to get execution directory");
string configFilesDirectory = Path.Join(executionDirectory, "/InfraSpaceConfigs");
if (!Directory.Exists(configFilesDirectory))
{
    throw new Exception("Failed to find the config directory");
}

string[] configFiles = Directory.GetFiles(configFilesDirectory);
Console.WriteLine($"Found {configFiles.Length} config files:\n{string.Join('\n', configFiles)}");

// This will get the current PROJECT directory
string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName ?? throw new Exception("Failed to get project directory");

// NB: This automatically checks that it doesn't exist before creating
DirectoryInfo newConfigDirectory = Directory.CreateDirectory(Path.Join(projectDirectory, "/InfraSpaceConfigsNew"));

foreach (string fullConfigFilePath in configFiles)
{
    Console.WriteLine(new string('=', 50));
    string configFileName = fullConfigFilePath.Split("/").Last();

    // Parse the config file
    Console.WriteLine($"Parsing {configFileName} from {fullConfigFilePath}");
    dynamic jsonContents = LoadJson(fullConfigFilePath);
    Console.WriteLine($"Successfully parsed {configFileName}.");
    Console.WriteLine(new string('-', 50));

    string newConfigFilePath = Path.Join(newConfigDirectory.FullName, configFileName);

    Console.WriteLine($"Writing new {configFileName} to ${newConfigFilePath}");
    using StreamWriter file = File.CreateText(newConfigFilePath);

    // Re-write the config file, in formatted valid JSON (no trailing commas or comments)
    JsonSerializer serializer = new()
    {
        Formatting = Formatting.Indented
    };

    serializer.Serialize(file, jsonContents);
    Console.WriteLine($"Successfully written new {configFileName}.");
}
Console.WriteLine(new string('=', 50));
Console.WriteLine("Finished rewriting config files.");

static dynamic LoadJson(string jsonFilePath)
{
    using StreamReader r = new(jsonFilePath);

    string json = r.ReadToEnd();
    dynamic? items = JsonConvert.DeserializeObject(json);
    return items ?? throw new Exception($"Couldn't parse anything from the ${jsonFilePath} file");
}
