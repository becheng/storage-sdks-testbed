using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;

namespace storageSDKsTestBed.Tests;

public class AzOauthTests : SDKsTestsBase
{
    private readonly string blobContainerUri;
    private readonly TokenCredential tokenCredential;
    public AzOauthTests()
    {        
        storageAcctName = _configuration["AzOauthTest:storageAcctName"];
        containerName = _configuration["AzOauthTest:containerName"];
        blobContainerUri = string.Format(_configuration["AzSasUriTest:blobContainerUri"], storageAcctName, containerName);
        tokenCredential = new DefaultAzureCredential();        
    }

    [Fact]
    public override void Authenticate()
    {
        // Get reference to the container
        BlobContainerClient containerClient = new BlobContainerClient(new Uri(blobContainerUri), tokenCredential);
        
        // Verify its the container exists and it's the correct one
        Assert.Equal(storageAcctName, containerClient?.AccountName);
        Assert.Equal(containerName, containerClient?.Name );
    }

    [Fact]
    public override async Task Upload()
    {
        // Get reference to the container
        BlobContainerClient containerClient = new BlobContainerClient(new Uri(blobContainerUri), tokenCredential);
        
        // Get reference to a blob 
        BlobClient blobClient = containerClient.GetBlobClient(sampleImage);

        try 
        {
            // Upload data to the blob
            await blobClient.UploadAsync(path, overwrite: true);

            // Verify the uploaded image
            BlobProperties properties = await blobClient.GetPropertiesAsync();
            Assert.Equal(fi.Length, properties.ContentLength);    

        } 
        finally
        {
            // Clean up after the test when we're finished
            await blobClient.DeleteAsync();
        }
    }

    [Fact]
    public override async Task Download()
    {
        // Get reference to the container
        BlobContainerClient containerClient = new BlobContainerClient(new Uri(blobContainerUri), tokenCredential);
        
        // Get reference to a blob 
        BlobClient blobClient = containerClient.GetBlobClient(sampleImage);

        try 
        {
            // Upload data to the blob
            await blobClient.UploadAsync(path, overwrite: true);

            // Download the blob
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            // Verify the downloaded image
            Assert.Equal(fi.Length, download.ContentLength);
        } 
        finally
        {
            // Clean up after the test when we're finished
            await blobClient.DeleteAsync();
        }
    }
}