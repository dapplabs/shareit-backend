using Ipfs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
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
                Dictionary<string, string> fileHashes = new Dictionary<string, string>();
                foreach (var file in Request.Form.Files)
                {
                    string folderName = "Upload";
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    string newPath = Path.Combine(webRootPath, folderName);
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
                        fileHashes.Add(fileName, await AddToIPFSAsync(fullPath));
                    }
                }
                return new OkObjectResult(fileHashes);
            }
            catch (System.Exception ex)
            {
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