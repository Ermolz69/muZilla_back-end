using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Mvc;
using muZilla.Models;
using System.Drawing;

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
        /// <param name="filename">The name of the file to read.</param>
        /// <param name="ac">The access level of the user for validation purposes.</param>
        /// <returns>The file content as a byte array, or null if the file cannot be read.</returns>
        /// <exception cref="Exception">Thrown if the user lacks permissions to download .mp3 files.</exception>
        public async Task<byte[]?> ReadFileFromSongAsync(string login, int songId, string filename, AccessLevel? ac)
        {
            ShareDirectoryClient directoryClient = shareClient.GetDirectoryClient(login);

            ShareDirectoryClient subdirClient = directoryClient.GetSubdirectoryClient(songId.ToString());

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
        /// Finds the dominant color from a list of pixels using the k-means clustering algorithm.
        /// </summary>
        /// <param name="pixels">A list of colors representing the pixels to analyze.</param>
        /// <param name="k">The number of clusters to use for k-means. Defaults to 5.</param>
        /// <param name="maxIterations">The maximum number of iterations for the k-means algorithm. Defaults to 100.</param>
        /// <returns>The dominant color as a <see cref="Color"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if k is less than or equal to 0.</exception>
        public Color GetDominantColor(List<Color> pixels, int k = 5, int maxIterations = 100)
        {
            if (pixels == null || pixels.Count == 0)
                return Color.Black;

            if (k <= 0)
                throw new ArgumentException("k must be greater than 0");

            Random rand = new Random();
            List<Color> centroids = new List<Color>();
            for (int i = 0; i < k; i++)
            {
                centroids.Add(pixels[rand.Next(pixels.Count)]);
            }

            int[] assignments = new int[pixels.Count];
            bool changed = true;
            int iteration = 0;

            while (changed && iteration < maxIterations)
            {
                changed = false;
                iteration++;

                for (int i = 0; i < pixels.Count; i++)
                {
                    int nearestCluster = FindNearestCluster(pixels[i], centroids);
                    if (assignments[i] != nearestCluster)
                    {
                        assignments[i] = nearestCluster;
                        changed = true;
                    }
                }

                if (changed)
                {
                    centroids = RecalculateCentroids(pixels, assignments, k);
                }
            }

            var clusterCounts = new Dictionary<int, int>();
            for (int i = 0; i < assignments.Length; i++)
            {
                int c = assignments[i];
                if (!clusterCounts.ContainsKey(c))
                    clusterCounts[c] = 0;
                clusterCounts[c]++;
            }

            int dominantCluster = clusterCounts.OrderByDescending(x => x.Value).First().Key;
            return centroids[dominantCluster];
        }

        /// <summary>
        /// Finds the nearest cluster for a given pixel by calculating the distance to each centroid.
        /// </summary>
        /// <param name="pixel">The pixel whose nearest cluster is being determined.</param>
        /// <param name="centroids">The list of current centroids.</param>
        /// <returns>The index of the nearest cluster.</returns>
        private static int FindNearestCluster(Color pixel, List<Color> centroids)
        {
            int nearestCluster = 0;
            double minDist = double.MaxValue;
            for (int i = 0; i < centroids.Count; i++)
            {
                double dist = ColorDistance(pixel, centroids[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestCluster = i;
                }
            }
            return nearestCluster;
        }

        /// <summary>
        /// Calculates the Euclidean distance between two colors in RGB space.
        /// </summary>
        /// <param name="c1">The first color.</param>
        /// <param name="c2">The second color.</param>
        /// <returns>The Euclidean distance between the two colors.</returns>
        private static double ColorDistance(Color c1, Color c2)
        {
            int dr = c1.R - c2.R;
            int dg = c1.G - c2.G;
            int db = c1.B - c2.B;
            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        /// <summary>
        /// Recalculates the centroids for all clusters based on the assigned pixels.
        /// </summary>
        /// <param name="pixels">The list of pixels to analyze.</param>
        /// <param name="assignments">An array mapping each pixel to a cluster index.</param>
        /// <param name="k">The number of clusters.</param>
        /// <returns>A new list of centroids representing the updated cluster centers.</returns>
        private static List<Color> RecalculateCentroids(List<Color> pixels, int[] assignments, int k)
        {
            int[] counts = new int[k];
            long[] sumR = new long[k];
            long[] sumG = new long[k];
            long[] sumB = new long[k];

            for (int i = 0; i < pixels.Count; i++)
            {
                int cluster = assignments[i];
                counts[cluster]++;
                sumR[cluster] += pixels[i].R;
                sumG[cluster] += pixels[i].G;
                sumB[cluster] += pixels[i].B;
            }

            List<Color> newCentroids = new List<Color>(k);
            for (int i = 0; i < k; i++)
            {
                if (counts[i] == 0)
                {
                    newCentroids.Add(Color.FromArgb(0, 0, 0));
                }
                else
                {
                    int r = (int)(sumR[i] / counts[i]);
                    int g = (int)(sumG[i] / counts[i]);
                    int b = (int)(sumB[i] / counts[i]);
                    newCentroids.Add(Color.FromArgb(r, g, b));
                }
            }

            return newCentroids;
        }

        #endregion
    }
}
