using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TEcommerceWebApi.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderPath);
        Task DeleteFileAsync(string fileUrl);
    }
}