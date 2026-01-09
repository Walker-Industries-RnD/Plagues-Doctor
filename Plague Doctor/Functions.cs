using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Plague_Doctor
{
    public static class Functions
    {

        public class Core
        {
            public static bool CoreProgramExistsInDirectory(string filePath)
            {
                try
                {
                    var directories = Directory.GetDirectories(filePath);
                    foreach (var dir in directories)
                    {
                        var dirName = Path.GetFileName(dir);
                        if (dirName.Equals("Plague", StringComparison.OrdinalIgnoreCase) ||
                            dir.Contains("\\Plague") || dir.Contains("/Plague"))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    // Ignore access errors in subdirs
                }
                return false;
            }


            public static void CreatePlague(string filePath, string projectName)
            {
                projectName = projectName.Trim();

                #region Scripts To Generate

                //CORE

                //Create the .csproj

                var CoreCSProj = $"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n    <TargetFramework>net9.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n\r\n  <ItemGroup>\r\n    <ProjectReference Include=\"..\\{projectName}.Interfaces\\{projectName}.Interfaces.csproj\" />\r\n    <ProjectReference Include=\"..\\{projectName}.Linux\\{projectName}.Linux.csproj\" />\r\n    <ProjectReference Include=\"..\\{projectName}.Windows\\{projectName}.Windows.csproj\" />\r\n  </ItemGroup>\r\n\r\n  <ItemGroup>\r\n    <Reference Include=\"PariahCybersecurity\">\r\n      <HintPath>..\\PariahCybersecurity.dll</HintPath>\r\n    </Reference>\r\n  </ItemGroup>\r\n\r\n</Project>\r\n";

                //Create the C#

                var CoreC = $"using System.Runtime.InteropServices;\r\nusing {projectName}.Interfaces;\r\n#if WINDOWS\r\nusing {projectName}.Windows;\r\n#elif LINUX\r\nusing {projectName}.Linux;\r\n#endif\r\nnamespace {projectName}.Core\r\n{{\r\n    public static class AccountsProvider\r\n    {{\r\n        public static async Task<PublicAccount?> GetPublicAcc(string Username)\r\n        {{\r\n            PublicAccount? publicAcc = null;\r\n\r\n            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))\r\n            {{\r\n                var ts = new Windows.Accounts();\r\n                publicAcc = await ts.GetAccData(Username);\r\n            }}\r\n            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))\r\n            {{\r\n                var ts = new Linux.Accounts();\r\n                publicAcc = await ts.GetAccData(Username);\r\n            }}\r\n            else\r\n            {{\r\n                throw new PlatformNotSupportedException(\"Unsupported OS for Accounts\");\r\n            }}\r\n\r\n            return publicAcc ?? throw new Exception(\"Not found\");\r\n        }}\r\n    }}\r\n}}\r\n";


                //Interfacces

                var InterfaceCSProj = $"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n    <TargetFramework>net9.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n\r\n  <ItemGroup>\r\n      </ItemGroup>\r\n\r\n  <ItemGroup>\r\n    <Reference Include=\"PariahCybersecurity\">\r\n      <HintPath>..\\PariahCybersecurity.dll</HintPath>\r\n    </Reference>\r\n  </ItemGroup>\r\n\r\n</Project>\r\n";

                var InterfaceUtil = $"using System;\r\nusing System.Collections.Generic;\r\nusing System.Linq;\r\nusing System.Security.AccessControl;\r\nusing System.Security.Principal;\r\nusing System.Text;\r\nusing System.Text.Json;\r\nusing System.Threading.Tasks;\r\n\r\nnamespace {projectName}.Interfaces\r\n{{\r\n    public class Utils\r\n    {{\r\n        public static class SecureStore\r\n        {{\r\n            private static string BasePath\r\n            {{\r\n                get\r\n                {{\r\n                    // Cross‑platform per‑session location\r\n                    string? runtimeDir = Environment.GetEnvironmentVariable(\"XDG_RUNTIME_DIR\");\r\n                    if (!string.IsNullOrEmpty(runtimeDir))\r\n                        return runtimeDir; // Linux, auto-clears on logout\r\n\r\n                    if (OperatingSystem.IsWindows())\r\n                        return Path.Combine(\r\n                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),\r\n                            \"{projectName}_RUNTIME\"\r\n                        );\r\n\r\n                    // macOS fallback\r\n                    return \"/tmp\"; // cleared on restart/logout\r\n                }}\r\n            }}\r\n\r\n            private static string GetPath(string key) =>\r\n                Path.Combine(BasePath, $\"xr_{{key}}.dat\");\r\n\r\n            public static void Set<T>(string key, T value)\r\n            {{\r\n                Directory.CreateDirectory(BasePath);\r\n\r\n                string path = GetPath(key);\r\n                string json = JsonSerializer.Serialize(value);\r\n\r\n                File.WriteAllText(path, json);\r\n\r\n                if (OperatingSystem.IsWindows())\r\n                    ApplyWindowsAcl(path);\r\n                else\r\n                    ApplyUnixPermissions(path);\r\n            }}\r\n\r\n            public static T? Get<T>(string key)\r\n            {{\r\n                string path = GetPath(key);\r\n                if (!File.Exists(path))\r\n                    return default;\r\n\r\n                string json = File.ReadAllText(path);\r\n                return JsonSerializer.Deserialize<T>(json);\r\n            }}\r\n\r\n            private static void ApplyWindowsAcl(string path)\r\n            {{\r\n                var fileInfo = new FileInfo(path);\r\n                var security = fileInfo.GetAccessControl();\r\n\r\n                // Remove inherited permissions\r\n                security.SetAccessRuleProtection(true, false);\r\n\r\n                // Current user\r\n                var currentUser = WindowsIdentity.GetCurrent().User!;\r\n                var userRule = new FileSystemAccessRule(\r\n                    currentUser,\r\n                    FileSystemRights.FullControl,\r\n                    AccessControlType.Allow\r\n                );\r\n                security.AddAccessRule(userRule);\r\n\r\n                fileInfo.SetAccessControl(security);\r\n            }}\r\n\r\n            private static void ApplyUnixPermissions(string path)\r\n            {{\r\n                try\r\n                {{\r\n                    var chmod = new System.Diagnostics.ProcessStartInfo\r\n                    {{\r\n                        FileName = \"/bin/chmod\",\r\n                        Arguments = $\"600 \\\"{{path}}\\\"\",\r\n                        RedirectStandardOutput = true,\r\n                        RedirectStandardError = true,\r\n                        UseShellExecute = false,\r\n                        CreateNoWindow = true\r\n                    }};\r\n                    using var proc = System.Diagnostics.Process.Start(chmod);\r\n                    proc?.WaitForExit();\r\n                }}\r\n                catch\r\n                {{\r\n                    // Fallback: hide file (less secure)\r\n                    File.SetAttributes(path, FileAttributes.Hidden);\r\n                }}\r\n            }}\r\n\r\n\r\n\r\n        }}\r\n\r\n\r\n    }}\r\n}}\r\n";

                var InterfaceExample = $"using MagicOnion;\r\nusing System.Runtime.Serialization;\r\n\r\nnamespace {projectName}.Interfaces\r\n{{\r\n    [DataContract]\r\n    public struct PublicAccount\r\n    {{\r\n        [DataMember] public string Name;\r\n        [DataMember] public string LastCheck;\r\n        [DataMember] public string OSFolder;\r\n\r\n        public PublicAccount(string name, string lastCheck, string oSFolder)\r\n        {{\r\n            Name = name;\r\n            LastCheck = lastCheck;\r\n            OSFolder = oSFolder;\r\n        }}\r\n    }}\r\n\r\n    public interface IPublicAcc : IService<IPublicAcc>\r\n    {{\r\n        UnaryResult<PublicAccount> GetAccInfo(string Acc);\r\n    }}\r\n\r\n\r\n\r\n}}\r\n";

                //.Linux

                var LinuxCSProj = $"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n    <TargetFramework>net9.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n\r\n  <ItemGroup>\r\n      </ItemGroup>\r\n\r\n  <ItemGroup>\r\n    <ProjectReference Include=\"..\\{projectName}.Interfaces\\{projectName}.Interfaces.csproj\" />\r\n  </ItemGroup>\r\n\r\n</Project>\r\n";

                var LinuxExample = $"using Grpc.Net.Client;\r\nusing MagicOnion.Client;\r\nusing {projectName}.Interfaces;\r\n\r\nnamespace {projectName}.Linux\r\n{{\r\n    public class Accounts\r\n    {{\r\n        public async Task<PublicAccount?> GetAccData(string accountName)\r\n        {{\r\n            try\r\n            {{\r\n                // Get the dynamically assigned worker address\r\n                var serviceAddr = Environment.GetEnvironmentVariable(\"{projectName}_WORKER_ADDR\");\r\n\r\n                // Create gRPC channel\r\n                using var channel = GrpcChannel.ForAddress(serviceAddr);\r\n\r\n                // Create a proxy client\r\n                var client = MagicOnionClient.Create<IPublicAcc>(channel);\r\n\r\n                // Call the server method\r\n                var account = await client.GetAccInfo(accountName);\r\n\r\n                return account;\r\n            }}\r\n\r\n            catch (Exception ex)\r\n            {{\r\n                throw new Exception(\"An error occured while getting Account Data: \" + ex.Message);\r\n            }}\r\n        }}\r\n    }}\r\n}}\r\n";

                //.Windows

                var WindowsCSProj = $"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n    <TargetFramework>net9.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n\r\n  <ItemGroup>\r\n    <Compile Remove=\"NewFolder\\**\" />\r\n    <EmbeddedResource Remove=\"NewFolder\\**\" />\r\n    <None Remove=\"NewFolder\\**\" />\r\n  </ItemGroup>\r\n\r\n  <ItemGroup>\r\n    <Folder Include=\"Services\\\" />\r\n  </ItemGroup>\r\n\r\n  <ItemGroup>\r\n    <ProjectReference Include=\"..\\{projectName}.Interfaces\\{projectName}.Interfaces.csproj\" />\r\n  </ItemGroup>\r\n\r\n  <ItemGroup>\r\n    <Reference Include=\"PariahCybersecurity\">\r\n      <HintPath>..\\PariahCybersecurity.dll</HintPath>\r\n    </Reference>\r\n  </ItemGroup>\r\n\r\n</Project>\r\n";

                var WindowsExample = $"using Grpc.Net.Client;\r\nusing MagicOnion.Client;\r\nusing {projectName}.Interfaces;\r\n\r\nnamespace {projectName}.Windows\r\n{{\r\n    public class Accounts\r\n    {{\r\n        public async Task<PublicAccount?> GetAccData(string accountName)\r\n        {{\r\n            try\r\n            {{\r\n                // Get the dynamically assigned worker address\r\n                var serviceAddr = Environment.GetEnvironmentVariable(\"{projectName}_WORKER_ADDR\") ?? \"https://localhost:5001\";\r\n\r\n                // Create gRPC channel\r\n                using var channel = GrpcChannel.ForAddress(serviceAddr);\r\n\r\n                // Create a proxy client\r\n                var client = MagicOnionClient.Create<IPublicAcc>(channel);\r\n\r\n                // Call the server method\r\n                var account = await client.GetAccInfo(accountName);\r\n\r\n                return account;\r\n            }}\r\n\r\n            catch (Exception ex)\r\n            {{\r\n                throw new Exception(\"An error occured while getting Account Data: \" + ex.Message);\r\n            }}\r\n        }}\r\n    }}\r\n}}\r\n";

                #endregion

                #region Plague 

                var projectPath = filePath + $"/{projectName}/Plague";

                var corePath = Path.Combine(projectPath, $"{projectName}.Core");
                var interfacePath = Path.Combine(projectPath, $"{projectName}.Interfaces");
                var windowsPath = Path.Combine(projectPath, $"{projectName}.Windows");
                var linuxPath = Path.Combine(projectPath, $"{projectName}.Linux");


                var coreProjPath = Path.Combine(corePath, $"{projectName}.Core.csproj");
                var coreCSPath = Path.Combine(corePath, $"{projectName}.Core.cs");


                var interfaceProjPath = Path.Combine(interfacePath, $"{projectName}.Interfaces.csproj");
                var interfaceUtilsPath = Path.Combine(interfacePath, $"{projectName}.Utils.cs");
                var interfaceExamplePath = Path.Combine(interfacePath, $"{projectName}.PublicAcc.cs");

                var windowsProjPath = Path.Combine(windowsPath, $"{projectName}.Windows.csproj");
                var windowsExamplePath = Path.Combine(windowsPath, $"{projectName}.Accounts.cs");

                var linuxProjPath = Path.Combine(linuxPath, $"{projectName}.Linux.csproj");
                var linuxExamplePath = Path.Combine(linuxPath, $"{projectName}.Accounts.cs");

                TextElements.TypeText("Creating Directories...");


                Directory.CreateDirectory(projectPath);
                Directory.CreateDirectory(corePath);
                Directory.CreateDirectory(interfacePath);
                Directory.CreateDirectory(windowsPath);
                Directory.CreateDirectory(linuxPath);

                TextElements.TypeText("Creating Core Files...");


                //Core

                File.WriteAllText(coreProjPath, CoreCSProj);
                File.WriteAllText(coreCSPath, CoreC);

                TextElements.TypeText("Creating Interface Files...");


                //Interfaces

                File.WriteAllText(interfaceProjPath, InterfaceCSProj);
                File.WriteAllText(interfaceUtilsPath, InterfaceUtil);
                File.WriteAllText(interfaceExamplePath, InterfaceExample);

                TextElements.TypeText("Creating Window Files...");


                //Windows

                File.WriteAllText(windowsProjPath, WindowsCSProj);
                File.WriteAllText(windowsExamplePath, WindowsExample);

                TextElements.TypeText("Creating Linux Files...");


                //Linux

                File.WriteAllText(linuxProjPath, LinuxCSProj);
                File.WriteAllText(linuxExamplePath, LinuxExample);

                TextElements.TypeText("Adding Pariah Cybersecurity...");


                //Add DLLs
                string executableDir = AppDomain.CurrentDomain.BaseDirectory; // bin\Debug\net8.0\
                string projectRoot = Path.GetFullPath(Path.Combine(executableDir, @"..\..\..\"));
                string sourceFile = Path.Combine(projectRoot, "PariahCybersecurity.dll");

                string targetFile = Path.Combine(projectPath, "PariahCybersecurity.dll");

                // Optional: ensure the target directory exists
                Directory.CreateDirectory(projectPath); // No-op if it already exists

                TextElements.TypeText("Adding DLLs to Projects...");


                File.Copy(sourceFile, targetFile, true);
                RunDotnetAddPackage(interfaceProjPath, "MagicOnion");
                RunDotnetAddPackage(linuxProjPath, "MagicOnion");
                RunDotnetAddPackage(windowsProjPath, "MagicOnion");


                TextElements.TypeText("Building Plague DLLs...");



                RunDotnetBuild(interfaceProjPath);           // No dependencies
                RunDotnetBuild(linuxProjPath);               // Depends on Interfaces
                RunDotnetBuild(windowsProjPath);             // Depends on Interfaces
                RunDotnetBuild(coreProjPath);                // Depends on Interfaces, Linux, Windows

                #endregion


            }



            private static void RunDotnetAddPackage(string projectPath, string packageArgs)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"add \"{projectPath}\" package {packageArgs}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Failed to add package to {projectPath}. Error: {error}. Output: {output}");
                }

                Console.WriteLine($"Successfully added package to {Path.GetFileName(projectPath)}");
            }

            private static void RunDotnetBuild(string projectPath, string configuration = "Debug", bool noRestore = false)
            {
                if (string.IsNullOrWhiteSpace(projectPath))
                    throw new ArgumentException("Project path is required.");

                if (!File.Exists(projectPath))
                    throw new FileNotFoundException($"Project file not found: {projectPath}");

                var arguments = new List<string> { "build", $"\"{projectPath}\"", "-c", configuration };

                if (noRestore)
                    arguments.Add("--no-restore");  // Use if you've already restored

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = string.Join(" ", arguments),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Build failed for {projectPath}.\nError: {error}");
                }

                Console.WriteLine($"Successfully built {Path.GetFileNameWithoutExtension(projectPath)} ({configuration})");

            }


            public static void CreatePlagueWorker(
    string projectPath,
    string plagueName,
    string workerName)
            {
                string plagueRoot = Path.Combine(projectPath, "Plague");
                Directory.CreateDirectory(plagueRoot);

                string linuxDir = Path.Combine(
                    plagueRoot,
                    $"{plagueName}.Linux.{workerName}"
                );

                string linuxWorkerPath = Path.Combine(linuxDir, "Worker.cs");
                string linuxProgramPath = Path.Combine(linuxDir, "Program.cs");
                string linuxCsprojPath = Path.Combine(
                    linuxDir,
                    $"{plagueName}.Linux.{workerName}.csproj"
                );

                Directory.CreateDirectory(linuxDir);

                string linuxWorkerCs = $@"
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Pariah_Cybersecurity;
using System.Diagnostics;
using System.IO;
using {plagueName}.Interfaces;
using static Pariah_Cybersecurity.EasyPQC;

namespace {plagueName}.Linux.{workerName}
{{
    public class PublicAccService : ServiceBase<IPublicAcc>, IPublicAcc
    {{
        private readonly Worker _worker;

        public PublicAccService(Worker worker)
            => _worker = worker;

        public async UnaryResult<PublicAccount> GetAccInfo(string accountName)
            => await _worker.GetAccInfo(accountName);
    }}

    public class Worker : BackgroundService
    {{
        private readonly ILogger<Worker> _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IServer _server;

        public Worker(
            ILogger<Worker> logger,
            IHttpContextAccessor http,
            IServer server)
        {{
            _logger = logger;
            _httpContext = http;
            _server = server;

            var addresses = _server.Features.Get<IServerAddressesFeature>();
            if (addresses != null && addresses.Addresses.Any())
            {{
                var address = addresses.Addresses.First();
                Environment.SetEnvironmentVariable(
                    ""{plagueName}_WORKER_{workerName.ToUpper()}"",
                    address
                );
                _logger.LogInformation(
                    ""{workerName} bound at {{address}}"",
                    address
                );
            }}
            else
            {{
                _logger.LogWarning(""Could not find server addresses."");
            }}
        }}

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {{
            await Start();

            while (!stoppingToken.IsCancellationRequested)
            {{
                _logger.LogInformation(
                    ""Linux Worker '{workerName}' running at: {{time}}"",
                    DateTimeOffset.Now
                );

                await Task.Delay(1000, stoppingToken);
            }}
        }}

        public async Task<bool> VerifyIntegrity()
        {{
            try
            {{
                string assemblyLoc =
                    typeof(PublicAccount).Assembly.Location;

                _logger.LogInformation(
                    ""Verifying assembly: {{path}}"",
                    assemblyLoc
                );

                byte[] hashBytes;
                using (var stream = new FileStream(
                    assemblyLoc,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {{
                    hashBytes = await FileOperations.HashFile(stream);
                }}

                using (var trustedStream = new FileStream(
                    assemblyLoc,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {{
                    bool result =
                        await FileOperations.VerifyHash(
                            trustedStream,
                            hashBytes
                        );

                    if (!result)
                    {{
                        _logger.LogWarning(
                            ""VerifyHash failed for {{path}}"",
                            assemblyLoc
                        );
                    }}

                    return result;
                }}
            }}
            catch (Exception ex)
            {{
                _logger.LogError(
                    ""VerifyIntegrity Exception: {{msg}}"",
                    ex.Message
                );
                return false;
            }}
        }}

        public async Task<bool> VerifyIntegrity2()
        {{
            try
            {{
                string exePath =
                    Process.GetCurrentProcess().MainModule?.FileName
                    ?? ""/proc/self/exe"";

                if (!File.Exists(exePath))
                {{
                    throw new FileNotFoundException(
                        $""Could not determine current executable path: {{exePath}}""
                    );
                }}

                byte[] exeHash;
                using (var fs = new FileStream(
                    exePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {{
                    exeHash = await FileOperations.HashFile(fs);
                }}

                using (var fs = new FileStream(
                    exePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {{
                    bool exeValid =
                        await FileOperations.VerifyHash(fs, exeHash);

                    if (!exeValid)
                    {{
                        throw new Exception(
                            $""EXE integrity verification failed: {{exePath}}""
                        );
                    }}
                }}

                return true;
            }}
            catch (Exception ex)
            {{
                _logger.LogError(
                    ""VerifyIntegrity2 failed: {{msg}}"",
                    ex.Message
                );

                throw new Exception(
                    ""VerifyIntegrity2 failed: "" + ex.Message,
                    ex
                );
            }}
        }}

        private async Task Start()
        {{
            try
            {{
                if (!await VerifyIntegrity())
                {{
                    throw new Exception(""Integrity Check 1 Failed"");
                }}
            }}
            catch (Exception ex)
            {{
                _logger.LogError(
                    ex,
                    ""Startup integrity check failed.""
                );
            }}
        }}

        public async UnaryResult<PublicAccount> GetAccInfo(
            string accountName)
        {{
            if (!await VerifyIntegrity2())
            {{
                throw new Exception(""Integrity Check 2 Failed"");
            }}

            var folder =
                $""@""""/home/{{accountName}}/{plagueName}"""";

            var lastCheck =
                DateTime.Now.ToString(""yyyy-MM-dd HH:mm:ss"");

            _logger.LogInformation(
                ""[{workerName}] Requested info for account: {{account}}"",
                accountName
            );

            return new PublicAccount(
                accountName,
                lastCheck,
                folder
            );
        }}
    }}
}}
";

                string linuxProgramCs = $@"
using MagicOnion.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using {plagueName}.Linux.{workerName};

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{{
    options.ListenUnixSocket(
        ""/var/run/{plagueName.ToLower()}/{workerName.ToLower()}.sock"",
        o => o.Protocols = HttpProtocols.Http2
    );
}});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<Worker>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddMagicOnion();

var app = builder.Build();
app.MapMagicOnionService<PublicAccService>();
app.Run();
";

                string linuxCsproj = $@"
<Project Sdk=""Microsoft.NET.Sdk.Worker"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""MagicOnion"" Version=""7.0.7"" />
    <PackageReference Include=""Microsoft.Extensions.Hosting"" Version=""9.0.8"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\{plagueName}.Interfaces\{plagueName}.Interfaces.csproj"" />
    <ProjectReference Include=""..\{plagueName}.Linux\{plagueName}.Linux.csproj"" />
  </ItemGroup>

  <ItemGroup>
    <Reference Include=""PariahCybersecurity"">
      <HintPath>..\PariahCybersecurity.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
";


                string windowsDir = Path.Combine(
                    plagueRoot,
                    $"{plagueName}.Windows.{workerName}"
                );

                string windowsWorkerPath = Path.Combine(windowsDir, "Worker.cs");
                string windowsProgramPath = Path.Combine(windowsDir, "Program.cs");
                string windowsCsprojPath = Path.Combine(
                    windowsDir,
                    $"{plagueName}.Windows.{workerName}.csproj"
                );

                Directory.CreateDirectory(windowsDir);

                string windowsWorkerCs = $@"
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Pariah_Cybersecurity;
using System.Diagnostics;
using System.IO;
using {plagueName}.Interfaces;
using static Pariah_Cybersecurity.EasyPQC;

namespace {plagueName}.Windows.{workerName}
{{
    public class PublicAccService : ServiceBase<IPublicAcc>, IPublicAcc
    {{
        private readonly Worker _worker;

        public PublicAccService(Worker worker)
            => _worker = worker;

        public async UnaryResult<PublicAccount> GetAccInfo(string accountName)
            => await _worker.GetAccInfo(accountName);
    }}

    public class Worker : BackgroundService
    {{
        private readonly ILogger<Worker> _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IServer _server;

        public Worker(
            ILogger<Worker> logger,
            IHttpContextAccessor http,
            IServer server)
        {{
            _logger = logger;
            _httpContext = http;
            _server = server;
        }}

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {{
            await Start();

            var addressesFeature =
                _server.Features.Get<IServerAddressesFeature>();

            if (addressesFeature != null)
            {{
                while (!addressesFeature.Addresses.Any()
                       && !stoppingToken.IsCancellationRequested)
                {{
                    await Task.Delay(50, stoppingToken);
                }}

                if (addressesFeature.Addresses.Any())
                {{
                    var address = addressesFeature.Addresses.First();
                    {plagueName}.Interfaces.Utils.SecureStore.Set(
                        ""worker_addr_{workerName}"",
                        address
                    );

                    _logger.LogInformation(
                        ""{workerName} bound at {{address}}"",
                        address
                    );
                }}
            }}

            while (!stoppingToken.IsCancellationRequested)
            {{
                _logger.LogInformation(
                    ""Windows Worker '{workerName}' running at: {{time}}"",
                    DateTimeOffset.Now
                );

                await Task.Delay(1000, stoppingToken);
            }}
        }}

        public async Task<bool> VerifyIntegrity()
        {{
            try
            {{
                string assemblyLoc =
                    typeof(PublicAccount).Assembly.Location;

                byte[] hashBytes;
                using (var stream = new FileStream(
                    assemblyLoc,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {{
                    hashBytes = await FileOperations.HashFile(stream);
                }}

                using (var trustedStream = new FileStream(
                    assemblyLoc,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {{
                    return await FileOperations.VerifyHash(
                        trustedStream,
                        hashBytes
                    );
                }}
            }}
            catch (Exception ex)
            {{
                _logger.LogError(ex, ""VerifyIntegrity failed"");
                return false;
            }}
        }}

        public async Task<bool> VerifyIntegrity2()
        {{
            try
            {{
                int pid = Process.GetCurrentProcess().Id;
                string exePath =
                    Process.GetProcessById(pid).MainModule.FileName;

                byte[] exeHash;
                using (var fs = new FileStream(
                    exePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {{
                    exeHash = await FileOperations.HashFile(fs);
                }}

                using (var fs = new FileStream(
                    exePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {{
                    bool valid =
                        await FileOperations.VerifyHash(fs, exeHash);

                    if (!valid)
                    {{
                        throw new Exception(
                            $""EXE integrity failed: {{exePath}}""
                        );
                    }}
                }}

                return true;
            }}
            catch (Exception ex)
            {{
                throw new Exception(
                    ""VerifyIntegrity2 failed: "" + ex.Message,
                    ex
                );
            }}
        }}

        private async Task Start()
        {{
            if (!await VerifyIntegrity())
            {{
                _logger.LogError(""Integrity Check 1 Failed"");
            }}
        }}

        public async UnaryResult<PublicAccount> GetAccInfo(
            string accountName)
        {{
            if (!await VerifyIntegrity2())
            {{
                throw new Exception(""Integrity Check 2 Failed"");
            }}

            _logger.LogInformation(
                ""[{workerName}] Requested info for account: {{account}}"",
                accountName
            );

            var folder =
                $""C:\\Users\\{{accountName}}\\{plagueName}"";

            var lastCheck =
                DateTime.Now.ToString(""yyyy-MM-dd HH:mm:ss"");

            return new PublicAccount(
                accountName,
                lastCheck,
                folder
            );
        }}
    }}
}}
";

                string windowsProgramCs = $@"
using MagicOnion.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using {plagueName}.Windows.{workerName};

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{{
    options.ListenLocalhost(
        5000,
        o => o.Protocols = HttpProtocols.Http2
    );
}});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<Worker>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddMagicOnion();

var app = builder.Build();
app.MapMagicOnionService<PublicAccService>();
app.Run();
";

                string windowsCsproj = $@"
<Project Sdk=""Microsoft.NET.Sdk.Worker"">
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""MagicOnion"" Version=""7.0.7"" />
    <PackageReference Include=""Microsoft.Extensions.Hosting"" Version=""9.0.8"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\{plagueName}.Interfaces\{plagueName}.Interfaces.csproj"" />
    <ProjectReference Include=""..\{plagueName}.Windows\{plagueName}.Windows.csproj"" />
  </ItemGroup>

  <ItemGroup>
    <Reference Include=""PariahCybersecurity"">
      <HintPath>..\PariahCybersecurity.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
";

                TextElements.TypeText("Creating Linux Template...");

                File.WriteAllText(linuxWorkerPath, linuxWorkerCs);
                File.WriteAllText(linuxProgramPath, linuxProgramCs);
                File.WriteAllText(linuxCsprojPath, linuxCsproj);

                TextElements.TypeText("Creating Windows Template...");

                File.WriteAllText(windowsWorkerPath, windowsWorkerCs);
                File.WriteAllText(windowsProgramPath, windowsProgramCs);
                File.WriteAllText(windowsCsprojPath, windowsCsproj);

                TextElements.TypeText("Adding DLLs to Linux Side...");
                RunDotnetAddPackage(linuxCsprojPath, "MagicOnion");
                RunDotnetAddPackage(linuxCsprojPath, "Microsoft.Extensions.Hosting");

                TextElements.TypeText("Adding DLLs to Windows Side...");
                RunDotnetAddPackage(windowsCsprojPath, "MagicOnion");
                RunDotnetAddPackage(windowsCsprojPath, "Microsoft.Extensions.Hosting");
            }









        }

        public static class TextElements
        {
            public static void TypeText(string messsge, bool newLine = true, int delay = 3)
            {
                foreach (char c in messsge)
                {
                    Console.Write(c);
                    Thread.Sleep(delay);
                }

                if (newLine)
                {
                    Console.WriteLine();
                }
            }

            public static void TypeTextArt(List<string> messsge, bool newLine = true, int delay = 50)
            {
                foreach (var letter in messsge)
                {
                    Console.Write(letter);
                    Thread.Sleep(delay);
                }

                if (newLine)
                {
                    Console.WriteLine();
                }
            }

            public static void Delay (int delay = 200)
            {
                Thread.Sleep(delay);
            }

        }

        public static class Art
        {
            public static void ArtDummy()
            {

            }
            public static string Plagues = @"
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░  ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░            ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒▒░                    ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒                       ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒░                        ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒                          ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒▒▒░░░░░░░▒▒▒▒▒▒░               ░░░░         ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒░▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒       ░▒▒░    ░██████▒░      ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒░      ▒████░   ░▓███████▓      ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░░░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒░    ░██████░     ████████▒       ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒░▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒
 ▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒     ███████░ ░█▓░ ▓██████░       ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒
 ▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒░░▒░░░▒▒▒▒▒▒▒     ▓██████  ████░ ░████░        ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒░░░░░░░▒▒▒▒▒▒▒░░▒▒▒▒▒▒░     ░█████░  ████▓░      ░▒      ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒
 ▒▒▒▒▒▒▒▒░░░░░░▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒     ░         ████▓░░░░░▒██░      ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒░░░▒▒▒▒░     ░██▒    ▓       ░██▒░░       ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒░░░▒▒▒▒░      ░░▒▓█▒░█▒░   ░░▓█▓          ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░░░▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒░            ▓█▒▒░░░░▒██░         ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░░▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒            ░███▒▓███▓░         ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒░░▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒░            ░░▒▒▒░░                    ░░▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒░▒▒
 ▒▒▒▒▒▒░░░▒▒▒▒▒░░░▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒                                                   ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒░▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒░░▒▒▒▒▒▒░                                                   ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒░░░▒▒▒▒░░░▒▒▒▒▒▒▒▒░░░▒▒▒▒▒░                                                    ▒▒▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒░░░▒▒▒▒░░▒▒▒▒▒▒▒▒░░░▒▒▒                                                        ▒▒▒▒▒▒▒▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒░░▒▒▒▒░░░▒░░▒▒▒▒░░░░                                                          ░▒▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒░░▒▒▒░░░▒▒▒▒▒▒▒▒                                                              ▒▒▒▒▒▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒░░░░░░░░▒▒▒▒▒░                                                                ░▒▒▒▒░░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒░░░░░░▒▒░░▒▒▒                                                                    ▒▒▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒░░░▒▒░░                                                                       ░▒░░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒░▒▒▒▒▒▒▒▒░░░░                                                                           ░░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒▒▒░                                                                               ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒▒▒░                                                                                  ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒░                                                                                      ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒░                                                                                        ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒▒                                                                                           ▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒░                                                                                            ░▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒                                                                                               ░▒▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒▒                                                                                                 ▒▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒░                                                                                                 ░▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒                                                                                                   ▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒▒                                                                                                   ▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒                                                                                                    ▒▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒▒                                                                                                    ░▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒░                                                                                                     ▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒░                                                                                                     ▒▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒                                                                                                       ▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒▒                                                                                                       ▒▒▒▒▒▒▒▒▒
 ▒▒▒▒▒░                                                                                                       ░▒▒▒▒▒▒▒▒
 ▒▒▒▒▒                                                                                                         ▒▒▒▒▒▒▒▒
 ▒▒▒▒▒                                                                                                         ░▒▒▒▒▒▒▒
 ▒▒▒▒                                                                                                           ▒▒▒▒▒▒▒
 ▒▒▒▒                                                                                                           ▒▒▒▒▒▒▒
 ▒▒▒░                                                                                                           ▒▒▒▒▒▒▒
 ▒▒▒                                                                                                            ░▒▒▒▒▒▒
 ▒▒▒                                                                                                             ▒▒▒▒▒▒
 ▒▒░                                                                                                             ░▒▒▒▒▒
 ▒▒░                           ▓████▓░▒██▒       ▒█░    ░██▓▓██░▒██▒░░▒█▓░██████░░▓█▓██▒                          ▒▒▒▒▒
 ▒▒                            ░█▓ ▒█▓ ██░      ▒███░  ▓█▒    ▒░ ██░  ░█░ ▓█░ ░░░░██  ▓░                          ▒▒▒▒▒
 ▒░                            ░█████░ ██░     ░█░▒█▓ ░██░  ░██▓░██░  ░█░ ▓██▓█▓░ ░███▓░                          ░▒▒▒▒
 ▒                             ░█▓     ██░  ░░░▓▓▒▒██░░██░   ▒█▒ ██░  ░█░ ▓█░  ░░░▒  ░██                           ▒▒▒▒
                               ▒██░   ░██▓▒▓█░▓█░  ░██░░▒██▒▒▓█▒ ░██▓▓█▒░░▓█▓▒▒█▓░█▓▒▓█▒                           ░▒▒▒
                                                                                                                   ░▒▒▒
                                                                                                                    ▒▒▒
                                                                                                                    ▒▒▒";

            public static string PlaguesText = @"                                                                                                                                     
                                                                                                                                     
________  ___                                                     ________                                                       ___ 
`MMMMMMMb.`MM                                                     `MMMMMMMb.                                                     `MM 
 MM    `Mb MM                                                      MM    `Mb                    /                                 MM 
 MM     MM MM    ___     __     ___   ___   ____     ____          MM     MM ___  __   _____   /M      _____     ____     _____   MM 
 MM     MM MM  6MMMMb   6MMbMMM `MM    MM  6MMMMb   6MMMMb\        MM     MM `MM 6MM  6MMMMMb /MMMMM  6MMMMMb   6MMMMb.  6MMMMMb  MM 
 MM    .M9 MM 8M'  `Mb 6M'`Mb    MM    MM 6M'  `Mb MM'    `        MM    .M9  MM69 "" 6M'   `Mb MM    6M'   `Mb 6M'   Mb 6M'   `Mb MM 
 MMMMMMM9' MM     ,oMM MM  MM    MM    MM MM    MM YM.             MMMMMMM9'  MM'    MM     MM MM    MM     MM MM    `' MM     MM MM 
 MM        MM ,6MM9'MM YM.,M9    MM    MM MMMMMMMM  YMMMMb         MM         MM     MM     MM MM    MM     MM MM       MM     MM MM 
 MM        MM MM'   MM  YMM9     MM    MM MM            `Mb        MM         MM     MM     MM MM    MM     MM MM       MM     MM MM 
 MM        MM MM.  ,MM (M        YM.   MM YM    d9 L    ,MM        MM         MM     YM.   ,M9 YM.  ,YM.   ,M9 YM.   d9 YM.   ,M9 MM 
_MM_      _MM_`YMMM9'Yb.YMMMMb.   YMMM9MM_ YMMMM9  MYMMMM9        _MM_       _MM_     YMMMMM9   YMMM9 YMMMMM9   YMMMM9   YMMMMM9 _MM_
                       6M    Yb                                                                                                      
                       YM.   d9                                                                                                      
                        YMMMM9                                                                                                       ";

            public static string PlagueLogo =
                @"                                                                                                                            
                                                                                                                            
                   
                                                           .:-=+#%%%%%%%%%%@@%*=-.                                          
                                                           +%%%%%%%%%%%%%%%%%%%@@@@%-.                                      
                                                           .%%%%%%%%%%%%%%%%%%%%@@@@@@%:                                    
                                                            :%%%%%%%%%%%#%###%%%%@@@@@@@@-                                  
                                                            .+%%%############%%%%@@@@@@@@@%.                                
                                                             .*##********#####%%%%%@@@@@%%@%-                               
                                                              :**********######%%%%@@@@@@%%@@=                              
                                       .-=======-:.   .=======-=**+++++++*######%%%%%@%%@@@@@@=                             
                                          %@@. .=@@@:    *@@.   -+++++****##%%%##%%%%%%%%@@@@@@#.                           
                                          *@@.    %@@.   *@%    .++++**++*##%%%%%%%%%%%%%%@@@@%%+                           
                                          *@@.    *@@=   *@%     -**++***#%%@@@@#---==+###%%%%@%%-                          
                                          *@@.   .@@%.   *@%      +#****###*#%=-----==+**#*##%*=+%:                         
                                          *@@###%@%-.    *@%      .###**+***==-----===+**++*%#====*                         
                                          *@@.           *@%       :**+++**+=-=------===***==*#%@#+                         
                                          *@@.           *@%       .-=++*+**-----:----==++*+=++#===                         
                                          *@@.           *@%        :=-=+++#+---=====-=-+**==+#+-=+                         
                                         .%@@-.         .#@@=:::::-@@.---++*%#========-=##***#++===                         
                                                                      :::***##%@%*+***%%%%%%##-=-=#.                        
                                                                      .***++*#####%%##%#%##**=---=#%-                       
                                                                       :********#%####++##***-::-=*##.                      
                                                  +%.             :#@%+=+******##*%%%%#*#*++*==::-#%%-                      
                                                 :@@+           =@%:     +********####%%#*+++*--=*#%%=                      
                                                :@#@@-        .#@%.      .::::=******#%%**#++#%%*=+%%-                      
                                                #= *@@:       +@@.        ::::-++*+**+*#%#%%##%@%#%%-                       
                                              .#*. .#@%.     .@@%.       .:::--==+++++*+*###%*#@@@%@%#.                     
                                              -%    :@@*     :@@%.         *==:=-==+++++++*+#%**%@%%%%%*                    
                                             :@@@@@@@@@@-     %@@.         **.-=+++*+++**+**+--:##:#-%+#.                   
                                            .%-      .*@@:    :@@@.        *@==::****-:++:++=*#=#%##%*%%#                   
                                           .%+         %@%     .#@%:       *@%=..==*#**##=+#-*#+*%#+##%%#                   
                                         .=%@#=.     .=%@@@+-    .+@@*=-=+#@#-.  ==-======+- ::.-=-::..                     
                                                                      ...        -==-==-==+:                                
                                                                                 -=----=++*                                 
                                                     ........     ................+-=-=***=                                 
                                                     ..-@@@:..    ..*@=....+@@+---++++**##+                                 
                                                       .@@#         -%.    -@@=   =***+**=+++++*=-.                         
                                                       .@@#         -%.    -@@=   :==+**-+%**%+##*+                         
                                                       .@@#         -%.    -@@=    =***=-#+-#=+++*:                         
                                                       .@@#         -%.    -@@%%%%%@#*###%#%%%%##.                          
                                                       .@@#         -%.    -@@=     :####%%%%%%*.                           
                                                       .@@%         =%.    -@@=      -+*##%%%@#.                            
                                                        #@@.       .**     -@@=      .#%%###%%*.                            
                                                        .#@%.     :%*.     -@@=      .#%#%####*.                            
                                                          .=%@@@@%=.     -=************=*##++++.                            
                                                                                       .*****#-.                            

                                                                                                                            ";
  
        }







    }
}
