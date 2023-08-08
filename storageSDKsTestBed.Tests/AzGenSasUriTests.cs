using System.Net;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace storageSDKsTestBed.Tests;

// references:
// https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-user-delegation-sas-create-dotnet?tabs=packages-dotnetcli
// https://learn.microsoft.com/en-us/azure/storage/blobs/sas-service-create-dotnet

public class AzGenSasUriTests : SDKsTestsBase
{
    private readonly string? connectionString;
    private readonly HttpClient httpClient;
    private readonly string storageAcctKey;

    public AzGenSasUriTests()
    {        
        storageAcctName = _configuration["AzGenSasUriTest:storageAcctName"];
        storageAcctKey = _configuration["AzGenSasUriTest:storageAcctKey"];
        containerName = _configuration["AzGenSasUriTest:containerName"];
    }

    [Fact]
    public override void Authenticate()
    {
        // create a blobServiceClient using StorageSharedKeyCredential
        StorageSharedKeyCredential storageSharedKeyCredential = new(storageAcctName, storageAcctKey);
        BlobServiceClient blobServiceClient = new BlobServiceClient(
            new Uri($"https://{storageAcctName}.blob.core.windows.net"),
            storageSharedKeyCredential);


        // create a blobclient
        BlobClient blobClient = blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(sampleImage);

        
        // Verify 
        Assert.Equal(storageAcctName, blobClient?.AccountName);
        Assert.Equal(containerName, blobClient?.BlobContainerName);
    }
    
    [Fact]
    public override async Task Upload()
    {
        
        // create a blobServiceClient using StorageSharedKeyCredential
        StorageSharedKeyCredential storageSharedKeyCredential = new(storageAcctName, storageAcctKey);
        BlobServiceClient blobServiceClient = new BlobServiceClient(
            new Uri($"https://{storageAcctName}.blob.core.windows.net"),
            storageSharedKeyCredential);


        // create a blobclient
        BlobClient blobClient = blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(sampleImage);

        try 
        {
            // generate the sasUri for the given blob, i.e. sampleImage
            Uri blobSASURI = await CreateServiceSASBlob(blobClient, null, BlobContainerSasPermissions.Create);

            // Create a blob client object representing 'sample-blob.txt' with SAS authorization
            BlobClient blobClientSAS = new BlobClient(blobSASURI);

            // Upload data to the blob
            await blobClientSAS.UploadAsync(path, overwrite: true);

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
        // create a blobServiceClient using StorageSharedKeyCredential
        StorageSharedKeyCredential storageSharedKeyCredential = new(storageAcctName, storageAcctKey);
        BlobServiceClient blobServiceClient = new BlobServiceClient(
            new Uri($"https://{storageAcctName}.blob.core.windows.net"),
            storageSharedKeyCredential);


        // create a blobclient
        BlobClient blobClient = blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(sampleImage);

        // first Upload the image so it can be downloaded 
        await blobClient.UploadAsync(path, overwrite: true);

        try 
        {
            // generate the sasUri for the given blob, i.e. sampleImage
            Uri blobSASURI = await CreateServiceSASBlob(blobClient, null, BlobContainerSasPermissions.Read);

            // Create a blob client object representing 'sample-blob.txt' with SAS authorization
            BlobClient blobClientSAS = new BlobClient(blobSASURI);

            // Download the blob's contents and save it to a file
            await blobClientSAS.DownloadToAsync(downloadPath);

            // Verify the downloaded image
            Assert.Equal(fi.Length, File.ReadAllBytes(downloadPath).Length);    
        } 
        finally
        {
            // Clean up after the test when we're finished
            await blobClient.DeleteAsync();
        }
    }

    public static async Task<Uri> CreateServiceSASBlob(
        BlobClient blobClient,
        string storedPolicyName = null,
        BlobContainerSasPermissions sasPermission = BlobContainerSasPermissions.Read )
    {
        // Check if BlobContainerClient object has been authorized with Shared Key
        if (blobClient.CanGenerateSasUri)
        {
            // Create a SAS token that's valid for one hr
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            if (storedPolicyName == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(sasPermission);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

            return sasURI;
        }
        else
        {
            // Client object is not authorized via Shared Key
            return null;
        }
    }        
}