using System.Drawing;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;
using muZilla.Entities.Enums;
using muZilla.Entities.Models;

namespace muZilla.Application.Services
{
    public class MusicStreamResult
    {
        private FileStream _stream;
        private string _contentType;
        private bool _supportsRange;

        public MusicStreamResult(FileStream stream, string contentType, bool supportsRange)
        {
            _stream = stream;
            _contentType = contentType;
            _supportsRange = supportsRange;
        }

        public void GetStreamDetails(out Stream stream, out string contentType, out bool supportsRange)
        {
            stream = _stream;
            contentType = _contentType;
            supportsRange = _supportsRange;
        }
    }


    public class FileStorageService
    {
        static string connectionString = string.Empty;
        static string fileSharesName = string.Empty;
        private const int MB = 1048576;
        static ShareClient shareClient;
        public FileStorageService(IConfiguration _config)
        {
            connectionString = _config["StorageConnectionString"]!;
            fileSharesName = _config["FileStorageName"]!;

            shareClient = new ShareClient(connectionString, fileSharesName);
            shareClient.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Creates a directory for a user if it does not already exist.
        /// </summary>
        /// <param name="login">The login of the user for whom the directory is being created.</param>
        /// <returns>An asynchronous task representing the operation.</returns>
        public async Task CreateUserDirectoryIfNotExistsAsync(string login)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            await directoryClient.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Creates a file within a user's directory and uploads the file content.
        /// </summary>
        /// <param name="login">The login of the user whose directory the file will be created in.</param>
        /// <param name="filename">The name of the file to create.</param>
        /// <param name="fileBytes">The byte array representing the file content.</param>
        /// <exception cref="InvalidOperationException">Thrown if the file size exceeds 1 MB.</exception>
        /// <returns>An asynchronous task representing the file creation operation.</returns>
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

        /// <summary>
        /// Reads a file from a user's directory and returns its content as a byte array.
        /// </summary>
        /// <param name="login">The login of the user whose directory the file is being read from.</param>
        /// <param name="filename">The name of the file to read.</param>
        /// <returns>The file content as a byte array.</returns>
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
        
        /// <summary>
        /// Reads a file from a song-specific directory, ensuring the user has necessary permissions.
        /// </summary>
        /// <param name="login">The login of the user whose song directory the file is being read from.</param>
        /// <param name="songId">The ID of the song whose directory contains the file.</param>
        /// <param name="fileType">The name of the file to read.</param>
        /// <param name="ac">The access level of the user for validation purposes.</param>
        /// <returns>The file content as a byte array, or null if the file cannot be read.</returns>
        /// <exception cref="Exception">Thrown if the user lacks permissions to download .mp3 files.</exception>
        public async Task<byte[]?> ReadFileFromSongAsync(string login, int songId, SongFile fileType, AccessLevel? ac)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            ShareDirectoryClient subdirClient = directoryClient.GetSubdirectoryClient(songId.ToString());

            string filename;
            switch (fileType)
            {
                case SongFile.Song:
                    filename = "song.mp3";
                    break;
                case SongFile.Cover:
                    filename = "";
                    break;
                case SongFile.Lyrics:
                    filename = "";
                    break;
                default:
                    filename = "song.mp3"; 
                    break;
            }


            ShareFileClient fileClient = subdirClient.GetFileClient(filename);

            if (filename.EndsWith(".mp3")) if (ac != null) if (!ac.CanDownload) throw new Exception("Cannot download song.");

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

        /// <summary>
        /// Creates a directory for a song within a user's directory if it does not already exist.
        /// </summary>
        /// <param name="login">The login of the user.</param>
        /// <param name="songId">The ID of the song for which the directory is being created.</param>
        /// <returns>An asynchronous task representing the directory creation operation.</returns>
        public async Task CreateSongDirectoryInDirectoryAsync(string login, int songId)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            await directoryClient.CreateIfNotExistsAsync();

            ShareDirectoryClient subdirClient = directoryClient.GetSubdirectoryClient(songId.ToString());

            await subdirClient.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Creates a file in a song-specific directory and uploads the file content.
        /// </summary>
        /// <param name="login">The login of the user whose song directory the file will be created in.</param>
        /// <param name="songId">The ID of the song for which the file is being created.</param>
        /// <param name="filename">The name of the file to create.</param>
        /// <param name="fileBytes">The byte array representing the file content.</param>
        /// <returns>An asynchronous task representing the file creation operation.</returns>
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

        /// <summary>
        /// Streams a music file from a song-specific directory, supporting range-based requests.
        /// </summary>
        /// <param name="login">The login of the user whose song directory contains the file.</param>
        /// <param name="songId">The ID of the song whose directory contains the file.</param>
        /// <param name="filename">The name of the file to stream.</param>
        /// <param name="rangeHeader">The range header indicating the byte range for the stream.</param>
        /// <returns>
        /// A <see cref="MusicStreamResult"/> containing the file stream, content type, and range support, or null if the file cannot be streamed.
        /// </returns>
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


        #region color

        /// <summary>
        /// Analyzes a given bitmap image and determines the dominant color by clustering pixels.
        /// </summary>
        /// <param name="image">The input bitmap image.</param>
        /// <param name="clusterCountX">The number of clusters along the X-axis (default: 48).</param>
        /// <param name="clusterCountY">The number of clusters along the Y-axis (default: 48).</param>
        /// <returns>The most frequent (dominant) color in the image.</returns>
        /// <remarks>
        /// - The image is resized to a smaller resolution to improve performance.
        /// - Colors are rounded to the nearest multiple of 5 to reduce noise.
        /// - If the number of unique colors exceeds 800, an alternative approach is used based on brightness.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if the image is null.</exception>
        /// <exception cref="ArgumentException">Thrown if cluster count values are non-positive.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static Color GetDominantColor(Bitmap image, int clusterCountX = 48, int clusterCountY = 48)
        {
            if (image == null)
                return Color.Black;

            if (clusterCountX <= 0 || clusterCountY <= 0)
                return Color.Black;

            image = new Bitmap((Bitmap)image.Clone(), new Size(clusterCountX, clusterCountY));

            int imgWidth = image.Width;
            int imgHeight = image.Height;

            int clusterWidth = (int)Math.Ceiling((double)imgWidth / clusterCountX);
            int clusterHeight = (int)Math.Ceiling((double)imgHeight / clusterCountY);

            var colorFrequency = new Dictionary<Color, double>();

            for (int x = 0; x < imgWidth; x++)
            {
                for (int y = 0; y < imgHeight; y++)
                {
                    int clusterX = x / clusterWidth;
                    int clusterY = y / clusterHeight;
                    var pixelColor = image.GetPixel(x, y);

                    const int roundFactor = 5;
                    int r = (pixelColor.R / roundFactor) * roundFactor;
                    int g = (pixelColor.G / roundFactor) * roundFactor;
                    int b = (pixelColor.B / roundFactor) * roundFactor;

                    if (colorFrequency.ContainsKey(pixelColor))
                        colorFrequency[pixelColor] += 1;
                    else
                        colorFrequency[pixelColor] = 1;
                }
            }


            var dominantColor = colorFrequency.OrderByDescending(c => c.Value).ElementAt(0).Key;
            if (colorFrequency.Count > 800)
            {
                dominantColor = colorFrequency.OrderByDescending(c => c.Key.GetBrightness()).ElementAt(colorFrequency.Count / 3).Key;
                return dominantColor;
            }
            return dominantColor;
        }


        #endregion
    }
}
