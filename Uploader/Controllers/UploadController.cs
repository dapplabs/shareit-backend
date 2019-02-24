using Ipfs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Angular5FileUpload.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;

        public UploadController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> UploadFileAsync()
        {
            try
            {
                if (Request.Form.Files.Any(x => x.Length > 500000000)) throw new Exception("Archivo muy grande...");
                Dictionary<string, string> fileHashes = new Dictionary<string, string>();
                foreach (var file in Request.Form.Files)
                {
                    Console.WriteLine($"New file is being upload type: {file.ContentType}, fileName: {file.FileName}, name: {file.Name}, length: {file.Length/1048576}Mb");
                    string folderName = "Upload";
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    string newPath = Path.Combine(webRootPath, folderName);
                    string fileHash = "";
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    if (file.Length > 0)
                    {
                        string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        string fullPath = Path.Combine(newPath, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        fileHash = await AddToIPFSAsync(fullPath);
                        fileHashes.Add(fileName, fileHash);
                        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                    }
                    Console.WriteLine($"New file is uploaded with hash {fileHash}. length: {file.Length / 1048576}Mb");
                }
                return new OkObjectResult(fileHashes);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new StatusCodeResult(500);
            }
        }

        private async Task<string> AddToIPFSAsync(string fullPath)
        {
            using (var ipfs = new IpfsClient("http://ipfs:5001"))
            {
                var stream = System.IO.File.OpenRead(fullPath);
                //Name of the file to add
                string fileName = Guid.NewGuid().ToString();

                //Wrap our stream in an IpfsStream, so it has a file name.
                IpfsStream inputStream = new IpfsStream(fileName, stream);

                MerkleNode node = await ipfs.Add(inputStream);

                return node.Hash.ToString();
            }
        }
    }
}