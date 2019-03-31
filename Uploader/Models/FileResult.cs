namespace Uploader.Models
{
    public class FileResult
    {
        public bool uploaded { get; set; }
        public string fileUid { get; set; }
        public string hash { get; set; }
    }
}