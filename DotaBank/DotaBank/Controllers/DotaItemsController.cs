using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotaBank.Models;
using DotaBank.Helpers;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace DotaBank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DotaItemsController : ControllerBase
    {
        private readonly DotaBankContext _context;
        private IConfiguration _configuration;

        public DotaItemsController(DotaBankContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/DotaItems
        [HttpGet]
        public IEnumerable<DotaItem> GetDotaItem()
        {
            return _context.DotaItem;
        }

        // GET: api/DotaItems/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDotaItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dotaItem = await _context.DotaItem.FindAsync(id);

            if (dotaItem == null)
            {
                return NotFound();
            }

            return Ok(dotaItem);
        }

        // PUT: api/DotaItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDotaItem([FromRoute] int id, [FromBody] DotaItem dotaItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dotaItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(dotaItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DotaItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DotaItems
        [HttpPost]
        public async Task<IActionResult> PostDotaItem([FromBody] DotaItem dotaItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.DotaItem.Add(dotaItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDotaItem", new { id = dotaItem.Id }, dotaItem);
        }

        // DELETE: api/DotaItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDotaItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dotaItem = await _context.DotaItem.FindAsync(id);
            if (dotaItem == null)
            {
                return NotFound();
            }

            _context.DotaItem.Remove(dotaItem);
            await _context.SaveChangesAsync();

            return Ok(dotaItem);
        }

        private bool DotaItemExists(int id)
        {
            return _context.DotaItem.Any(e => e.Id == id);
        }

        // GET: api/Meme/Tags
        [Route("tags")]
        [HttpGet]
        public async Task<List<string>> GetTags()
        {
            var dota = (from m in _context.DotaItem
                         select m.Tags).Distinct();

            var returned = await dota.ToListAsync();

            return returned;
        }

        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadFile([FromForm]DotaImageItem meme)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                using (var stream = meme.Image.OpenReadStream())
                {
                    var cloudBlock = await UploadToBlob(meme.Image.FileName, null, stream);
                    //// Retrieve the filename of the file you have uploaded
                    //var filename = provider.FileData.FirstOrDefault()?.LocalFileName;
                    if (string.IsNullOrEmpty(cloudBlock.StorageUri.ToString()))
                    {
                        return BadRequest("An error has occured while uploading your file. Please try again.");
                    }

                    DotaItem memeItem = new DotaItem();
                    memeItem.Title = meme.Title;
                    memeItem.Tags = meme.Tags;

                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    memeItem.Height = image.Height.ToString();
                    memeItem.Width = image.Width.ToString();
                    memeItem.Url = cloudBlock.SnapshotQualifiedUri.AbsoluteUri;
                    memeItem.Uploaded = DateTime.Now.ToString();

                    _context.DotaItem.Add(memeItem);
                    await _context.SaveChangesAsync();

                    return Ok($"File: {meme.Title} has successfully uploaded");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error has occured. Details: {ex.Message}");
            }


        }

        private async Task<CloudBlockBlob> UploadToBlob(string filename, byte[] imageBuffer = null, System.IO.Stream stream = null)
        {

            var accountName = _configuration["AzureBlob:name"];
            var accountKey = _configuration["AzureBlob:key"]; ;
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer imagesContainer = blobClient.GetContainerReference("images");

            string storageConnectionString = _configuration["AzureBlob:connectionString"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Generate a new filename for every new blob
                    var fileName = Guid.NewGuid().ToString();
                    fileName += GetFileExtention(filename);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = imagesContainer.GetBlockBlobReference(fileName);

                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        return new CloudBlockBlob(new Uri(""));
                    }

                    return cloudBlockBlob;
                }
                catch (StorageException ex)
                {
                    return new CloudBlockBlob(new Uri(""));
                }
            }
            else
            {
                return new CloudBlockBlob(new Uri(""));
            }

        }

        private string GetFileExtention(string fileName)
        {
            if (!fileName.Contains("."))
                return ""; //no extension
            else
            {
                var extentionList = fileName.Split('.');
                return "." + extentionList.Last(); //assumes last item is the extension 
            }
        }
    }
}