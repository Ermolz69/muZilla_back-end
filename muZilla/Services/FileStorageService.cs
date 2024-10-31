using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace muZilla.Services
{
    public class FileStorageService
    {
        static string connectionString = string.Empty;
        static string fileSharesName = string.Empty;
        private const int MB = 1048576;
        static ShareClient shareClient;
        public FileStorageService()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            connectionString = configuration["StorageConnectionString"];
            fileSharesName = configuration["FileStorageName"];

            shareClient = new ShareClient(connectionString, fileSharesName);
            shareClient.CreateIfNotExistsAsync();
        }

        public async Task CreateUserDirectoryIfNotExistsAsync(string login)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            await directoryClient.CreateIfNotExistsAsync();
        }

        public async Task CreateFileInDirectoryAsync(string login, string filename, byte[] fileBytes)
        {
            if (fileBytes.Length > MB)
            {
                throw new InvalidOperationException("File size exceeds 1 MB limit.");
            }

            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            await directoryClient.CreateIfNotExistsAsync();

            ShareFileClient fileClient = directoryClient.GetFileClient(filename);

            await fileClient.CreateAsync(fileBytes.Length);

            using (MemoryStream stream = new MemoryStream(fileBytes))
            {
                await fileClient.UploadRangeAsync(new HttpRange(0, fileBytes.Length), stream);
            }
        }

        public async Task<byte[]> ReadFileAsync(string login, string filename)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            ShareFileClient fileClient = directoryClient.GetFileClient(filename);

            ShareFileDownloadInfo download = await fileClient.DownloadAsync();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await download.Content.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }

        public async Task<byte[]?> ReadFileFromSongAsync(string login, int songId, string filename)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            ShareDirectoryClient subdirClient = directoryClient.GetSubdirectoryClient(songId.ToString());

            ShareFileClient fileClient = subdirClient.GetFileClient(filename);

            try
            {
                ShareFileDownloadInfo download = await fileClient.DownloadAsync();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await download.Content.CopyToAsync(memoryStream);

                    return memoryStream.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task CreateSongDirectoryInDirectoryAsync(string login, int songId)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            await directoryClient.CreateIfNotExistsAsync();

            ShareDirectoryClient subdirClient = directoryClient.GetSubdirectoryClient(songId.ToString());

            await subdirClient.CreateIfNotExistsAsync();
        }

        public async Task CreateFileInSongDirectoryInDirectoryAsync(string login, int songId, string filename, byte[] fileBytes)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            await directoryClient.CreateIfNotExistsAsync();

            ShareDirectoryClient subdirClient = directoryClient.GetSubdirectoryClient(songId.ToString());

            await subdirClient.CreateIfNotExistsAsync();

            ShareFileClient fileClient = subdirClient.GetFileClient(filename);

            await fileClient.CreateAsync(fileBytes.Length);

            using (MemoryStream stream = new MemoryStream(fileBytes))
            {
                await fileClient.UploadRangeAsync(new HttpRange(0, fileBytes.Length), stream);
            }
        }
    }
}
