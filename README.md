# Storage SDKs Test Bed
A dotnet test (xunit) project containing Azure storage account and  AWS S3 bucket test cases using the respective dotnet SDKs.  

## Prerequistes
1. Create an Azure app registration with a client secret (to run test locally).
2. Create an Azure Storage account in your home tenant with three separate containers and assign the above app with a 'Storage Blob Data Contributor' role to the account.

   a) Create a Shared access token for one of the containers with Read, Add, Write, Delete and List permissions.
    
3. Create another Azure app registration with multi-tenancy (to test cross tenant access).
4. Access to a separate Azure tenant with a storage account with one container provisioned.
      
   a) To establish cross tenant access, issue the request using the following link given the multi-tenant app registrion created above and the other tenant's id.
       
    ```
    https://login.microsoftonline.com/<your-other-azure-tenentId>/oauth2/authorize?client_id=<your-multiTenant-clientId>&response_type=code&redirect_uri=<your-replyUrl, e.g. localhost>`
    ```

    b) Once consent is provided and service principal created on the other Azure tenant, assign a RBAC role of 'Storage Blob Data Contributor' to the target storage account. 

5. Access to AWS tenant with an existing S3 Bucket and setup an `accessKeyId` and `accessKeySecret`.

6. Update the `appsettings.Tests.json` with the values above.
    - For `AwsS3Test` > `awsRegion` provide an  official S3 region system names, e.g. `ca-central-1`.  

## How to include as a submodule to another dotnet solution

1. Recommended pathing of the target solution (example):

    ```
    / storageAdapterClient
        storageAdapterClient.sln
        /src
            storageAdapterClient.cs
            storageAdapterClient.csproj
        /test
            /storageAdapterClient.Tests
                storageAdapterClient.Tests.csproj
                aStorageAdapterClientTest.cs
                ...
            /storage-sdks-testbed <-- added as a submodule!
                storageSDKsTestBed.sln
                ...
                ...
                /storageSDKsTestBed.Tests 
                    storageSDKsTestBed.Tests.csproj
                    ...
                    ...
                
    ```
2. Create the solution structure above and all its solution references  Examples:
    ```
    dotnet new sln -n storageAdapterClient -o .
    ...
    dotnet new classlib -n storageAdapterClient -o ./src
    dotnet sln add ./src/storageAdapterClient.csproj
    ...
    dotnet new classlib -n storageAdapterClient.Tests -o ./test/storageAdapterClient.Tests
    dotnet sln add ./test/storageAdapterClient.Tests/storageAdapterClient.Tests.csproj
    ```
 
3. To add the testbed as a submodule to your target solution, in your test folder, run the following:
    ```
    git submodule add https://github.com/becheng/storage-sdks-testbed.git
    git submodule init
    git submodule update
    ```
    A new `.gitmodules` file will be created in the root with the submodule info.  Example:
    ```
    [submodule "test/storage-sdks-testbed"]
	path = test/storage-sdks-testbed
	url = https://github.com/becheng/storage-sdks-testbed.git
    ```

4. Go to the root folder where your .sln is located and add the testbed project to your solution 
    ```
    dotnet sln add ./test/storageSDKsTestBed.Tests/storageSDKsTestBed.Tests.csproj
    ```

