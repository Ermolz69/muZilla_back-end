using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using muZilla.Models;
using System.Drawing;
using 

namespace muZilla.Services
{
    public class MusicStreamResult
    {
        private FileStream fileStream;
        private string v1;
        private bool v2;

        public MusicStreamResult(FileStream fileStream, string v1, bool v2)
        {
            this.fileStream = fileStream;
            this.v1 = v1;
            this.v2 = v2;
        }

        public void SetAll(out Stream stream, out string _v1, out bool _v2)
        {
            stream = fileStream;
            _v1 = v1;
            _v2 = v2;
        }
    }

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

        public async Task<byte[]?> ReadFileFromSongAsync(string login, int songId, string filename, AccessLevel ac)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            ShareDirectoryClient subdirClient = directoryClient.GetSubdirectoryClient(songId.ToString());

            ShareFileClient fileClient = subdirClient.GetFileClient(filename);

            if (filename.EndsWith(".mp3")) if (!ac.CanDownload) throw new Exception("No vip no chip");

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

        public MusicStreamResult? GetMusicStream(string login, int songId, string filename, string rangeHeader)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);
            ShareDirectoryClient subdirClient = directoryClient.GetSubdirectoryClient(songId.ToString());
            ShareFileClient fileClient = subdirClient.GetFileClient(filename);

            HttpRange? range = null;

            if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
            {
                var rangeParts = rangeHeader.Substring("bytes=".Length).Split('-');
                if (rangeParts.Length > 0 && long.TryParse(rangeParts[0], out long startBytes))
                {
                    if (rangeParts.Length > 1 && !string.IsNullOrEmpty(rangeParts[1]) && long.TryParse(rangeParts[1], out long endBytes))
                    {
                        long length = endBytes - startBytes + 1;
                        if (length > 0)
                        {
                            range = new HttpRange(startBytes, length);
                        }
                    }
                    else
                    {
                        range = new HttpRange(startBytes);
                    }
                }
            }

            ShareFileDownloadOptions downloadOptions = null;
            if (range.HasValue)
            {
                downloadOptions = new ShareFileDownloadOptions
                {
                    Range = range.Value
                };
            }

            Response<ShareFileDownloadInfo> downloadResponse =
                downloadOptions != null ? fileClient.Download(downloadOptions) : fileClient.Download();

            string tempFilePath = Path.GetTempFileName();
            using (var fileStream = File.OpenWrite(tempFilePath))
            {
                downloadResponse.Value.Content.CopyTo(fileStream);
            }

            var finalStream = File.OpenRead(tempFilePath);
            return new MusicStreamResult(finalStream, "audio/mpeg", true);
        }

        public string GetDomainColor(byte[] pic)
        {
            
        }
    }
}
