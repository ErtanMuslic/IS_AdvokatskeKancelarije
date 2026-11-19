namespace IS_AdvokatskeKancelarije.DTOs
{
    public class DocumentUploadDTO : DocumentDTO
    {
        public IFormFile File { get; set; } = null!;
    }
}
