using Microsoft.Extensions.Configuration;

namespace storageSDKsTestBed.Tests;

public abstract class SDKsTestsBase
{
    // common readonly fields, set in base constructor
    protected readonly IConfiguration _configuration;
    protected readonly string? sampleImage;
    protected readonly string path;
    protected readonly FileInfo fi;
    protected readonly string downloadPath;
    
    // derived readonly fields, set in derived constructor
    protected string? storageAcctName;
    protected string? containerName;

    public SDKsTestsBase()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"appsettings.Tests.json", optional: false, reloadOnChange: true)
            .Build();

        sampleImage = _configuration["SampleImage"];
        path = Path.Combine(Directory.GetCurrentDirectory(), sampleImage);
        fi = new FileInfo(path);
        downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "downloaded.jpg");           

        // TODO: there is a probably a better way to do this.
        // Background: xUnit test runner does not leverage the launch.json for environment vars so we need to set them here.
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", _configuration["AZURE_CLIENT_ID"]);
        Environment.SetEnvironmentVariable("AZURE_TENANT_ID", _configuration["AZURE_TENANT_ID"]);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", _configuration["AZURE_CLIENT_SECRET"]);
    }

    public abstract void Authenticate();
    public abstract Task Upload(); 
    public abstract Task Download(); 
}
