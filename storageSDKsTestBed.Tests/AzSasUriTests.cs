using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace storageSDKsTestBed.Tests;

public class AzSasUriTests : SDKsTestsBase
{
    private readonly string blobContainerUri;
    private readonly AzureSasCredential sasCredential;

    public AzSasUriTests()
    {        
        storageAcctName = _configuration["AzSasUriTest:storageAcctName"];
        containerName = _configuration["AzSasUriTest:containerName"];
        blobContainerUri = string.Format(_configuration["AzSasUriTest:blobContainerUri"], storageAcctName, containerName);
        sasCredential = new AzureSasCredential(_configuration["AzSasUriTest:sasKey"]);
    }

    [Fact]
    public override void Authenticate()
    {
        // Get reference to the container
        BlobContainerClient containerClient = new BlobContainerClient(new Uri(blobContainerUri), sasCredential);
        
        // Verify its the container exists and it's the correct one
        Assert.Equal(storageAcctName, containerClient?.AccountName);
        Assert.Equal(containerName, containerClient?.Name );
    }

    [Fact]
    public override async Task Upload()
    {
        // Get reference to the container
        BlobContainerClient containerClient = new BlobContainerClient(new Uri(blobContainerUri), sasCredential);
        
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
        BlobContainerClient containerClient = new BlobContainerClient(new Uri(blobContainerUri), sasCredential);
        
        // Get reference to a blob 
        BlobClient blobClient = containerClient.GetBlobClient(sampleImage);

        try 
        {
            // first Upload the image so it can be downloaded 
            await blobClient.UploadAsync(path, overwrite: true);

            // Download the blob's contents and save it to a file
            await blobClient.DownloadToAsync(downloadPath);

            // Verify the downloaded image
            Assert.Equal(fi.Length, File.ReadAllBytes(downloadPath).Length);    
        } 
        finally
        {
            // Clean up after the test when we're finished
            await blobClient.DeleteAsync();
        }
   }
}