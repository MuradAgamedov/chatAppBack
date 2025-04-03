namespace chatApp.Interfaces
{
    public interface IStatusService
    {
        Task<Status> UploadStatusAsync(string userId, UploadStatusDto model, IWebHostEnvironment env);
        Task<List<Status>> GetUserStatusesAsync(string userId);
        Task<List<Status>> GetUserStatusesByEmailAsync(string email);
        Task<bool> DeleteStatusAsync(int id, string userId, IWebHostEnvironment env);
    }

}
