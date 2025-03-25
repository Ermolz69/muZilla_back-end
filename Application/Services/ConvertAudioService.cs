using System.Diagnostics;

namespace Application.Services
{
    public class ConvertAudioService
    {
        public async Task<Stream?> ConvertToAac(MemoryStream rawFile)
        {
            if (rawFile == null || rawFile.Length == 0)
                return null;

            string tempInputPath = Path.GetTempFileName();

            rawFile.Position = 0;
            using (FileStream stream = new FileStream(tempInputPath, FileMode.Create))
            {
                await rawFile.CopyToAsync(stream);
            }

            rawFile.Dispose();

            var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "ffmpeg.exe");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-i \"{tempInputPath}\" -f adts -c:a aac -b:a 192k pipe:1",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start(); 

            process.Exited += (sender, args) =>
            {
                File.Delete(tempInputPath);
            };

            return process.StandardOutput.BaseStream;
        }
    }
}
