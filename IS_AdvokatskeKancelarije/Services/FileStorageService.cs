namespace IS_AdvokatskeKancelarije.Services
{
    public class FileStorageService
    {
        public const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

        private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".pdf",  "application/pdf" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".txt",  "text/plain" }
        };

        private readonly string _root;

        public FileStorageService(IWebHostEnvironment env)
        {
            _root = Path.Combine(env.ContentRootPath, "Uploads");
            Directory.CreateDirectory(_root);
        }

        
        public async Task<string?> ValidateAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "File is empty";

            if (file.Length > MaxFileSizeBytes)
                return "File is too large (max 20 MB)";

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedTypes.ContainsKey(ext))
                return "Unsupported file type. Allowed: PDF, DOCX, TXT";

            
            var header = new byte[4];
            await using var stream = file.OpenReadStream();
            var read = await stream.ReadAsync(header, 0, header.Length);

            if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) &&
                !(read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46)) // %PDF
                return "File content is not a valid PDF";

            if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase) &&
                !(read >= 2 && header[0] == 0x50 && header[1] == 0x4B)) 
                return "File content is not a valid DOCX";

            return null;
        }

       
        public async Task<string> SaveAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storedName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(_root, storedName);

            await using var target = new FileStream(fullPath, FileMode.CreateNew);
            await file.CopyToAsync(target);

            return storedName;
        }

        public Stream? Open(string storedName)
        {
            var fullPath = GetSafePath(storedName);
            return File.Exists(fullPath)
                ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                : null;
        }

        public void Delete(string? storedName)
        {
            if (string.IsNullOrEmpty(storedName)) return;
            var fullPath = GetSafePath(storedName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public static string GetContentType(string fileName) =>
            AllowedTypes.TryGetValue(Path.GetExtension(fileName), out var ct) ? ct : "application/octet-stream";

       
        private string GetSafePath(string storedName)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_root, Path.GetFileName(storedName)));
            if (!fullPath.StartsWith(Path.GetFullPath(_root), StringComparison.Ordinal))
                throw new InvalidOperationException("Invalid file path");
            return fullPath;
        }
    }
}
