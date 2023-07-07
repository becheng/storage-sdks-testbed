using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace storageSDKsTestBed.Tests;

public class AwsS3Tests : SDKsTestsBase
{
    private readonly string? awsAccessKeyId;
    private readonly string? awsSecretAccessKey;
    private readonly RegionEndpoint regionEndpoint;
    private readonly string? awsBucketName;
    private const double timeoutDuration = 1;
    private readonly string? exisitingObjectKey;
    private readonly HttpClient httpClient;
        

    public AwsS3Tests()
    {   
        awsAccessKeyId = _configuration["AwsS3Test:awsAccessKeyId"];
        awsSecretAccessKey = _configuration["AwsS3Test:awsSecretAccessKey"];
        awsBucketName = _configuration["AwsS3Test:awsBucketName"];
        regionEndpoint = RegionEndpoint.GetBySystemName(_configuration["AwsS3Test:awsRegion"]);
        exisitingObjectKey = _configuration["AwsS3Test:exisitingObjectKey"]; 
        httpClient = new HttpClient();
    }

    [Fact]
    public override async void Authenticate()
    {
        IAmazonS3 s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, regionEndpoint);
        
        // generate a pre-signed URL against the object in the bucket
        string preSignedUrl = GeneratePreSignedURL(s3Client, awsBucketName, exisitingObjectKey, HttpVerb.GET, timeoutDuration);

        // send the request
        HttpResponseMessage response = await httpClient.GetAsync(preSignedUrl);

        // verify the response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public override async Task Upload()
    {
        // create client
        IAmazonS3 s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, regionEndpoint);

        try 
        {            
            // generate a pre-signed URL against the object in the bucket
            string preSignedUrl = GeneratePreSignedURL(s3Client, awsBucketName, sampleImage, HttpVerb.PUT, timeoutDuration);

            // send the request / upload the file
            HttpResponseMessage response = await httpClient.PutAsync(preSignedUrl, new ByteArrayContent(File.ReadAllBytes(path)));

            // verify the response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            // clean up after the test when we're finished
            await s3Client.DeleteObjectAsync(awsBucketName, sampleImage);
        }
    }

    [Fact]
    public override async Task Download()
    {
        // create client
        IAmazonS3 s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, regionEndpoint);

        try
        {
            // first run upload an image to the bucket so it can downloaded
            await s3Client.UploadObjectFromFilePathAsync(awsBucketName, sampleImage, path, null);
        
            // generate a pre-signed URL against the object in the bucket
            string preSignedUrl = GeneratePreSignedURL(s3Client, awsBucketName, sampleImage, HttpVerb.GET, timeoutDuration);
            
            // send the request / recieved the download stream 
            using (Stream respStream = await httpClient.GetStreamAsync(preSignedUrl))
            {
                var buffer = new byte[8000];
                using (FileStream fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
                {
                    int bytesRead = 0;
                    while ((bytesRead = respStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                }
            }

            // Verify the downloaded image
            Assert.Equal(fi.Length, File.ReadAllBytes(downloadPath).Length);
        }
        finally
        {
            // clean up after the test when we're finished
            await s3Client.DeleteObjectAsync(awsBucketName, sampleImage);
        }                
    }    
    private static string GeneratePreSignedURL(
            IAmazonS3 client,
            string? bucketName,
            string? objectKey,
            HttpVerb verb,
            double duration){
        
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Verb = verb, 
            Expires = DateTime.UtcNow.AddHours(duration),
        };

        string url = client.GetPreSignedURL(request);
        return url;
    }
}