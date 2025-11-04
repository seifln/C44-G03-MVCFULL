using Microsoft.AspNetCore.Http;

namespace GymManagementBLL.Services.AttachmentService
{
	public interface IAttachmentService
	{
		string? Upload(IFormFile file, string FolderName);
		bool Delete(string fileName, string folderName);
	}
}
