namespace RaceDay.API.Services
{
    // Everything Azure-related sits behind this interface. Controllers only ever
    // talk to this, never to the Azure SDK directly - keeps the door open to
    // swap storage providers later without touching a single controller.
    public interface IBlobStorageService
    {
        // Uploads a file and returns the public URL EF Core should save against
        // the Event/User row. folderName just keeps banners and profile pictures
        // in separate "folders" inside the same container (e.g. "banners", "profile-pictures").
        Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType, string folderName);
    }
}
