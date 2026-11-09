using RaceDay.API.Services;

namespace RaceDay.Tests.TestHelpers
{
    // Tests shouldn't need a real Azure Storage account just to prove a controller
    // action works, so this stands in for IBlobStorageService and hands back a
    // made-up URL without touching the network.
    public class FakeBlobStorageService : IBlobStorageService
    {
        public Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType, string folderName)
        {
            return Task.FromResult($"https://fake-storage.test/{folderName}/{Guid.NewGuid()}.jpg");
        }
    }
}
