using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        private HttpClient _httpClient;
        private Options _options;

        public IndexModel(HttpClient httpClient, Options options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        [BindProperty]
        public List<string> ImageList { get; private set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task OnGetAsync()
        {
            var imagesUrl = _options.ApiUrl;

            string imagesJson = await _httpClient.GetStringAsync(imagesUrl);

            IEnumerable<string> imagesList = JsonConvert.DeserializeObject<IEnumerable<string>>(imagesJson);

            this.ImageList = imagesList.ToList<string>();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            const long MaxFileBytes = 10 * 1024 * 1024; // 10 MB

            if (Upload != null && Upload.Length > 0)
            {
                if (Upload.Length > MaxFileBytes)
                {
                    ModelState.AddModelError("Upload", "ไฟล์ขนาดใหญ่กว่าที่กำหนด (สูงสุด 10 MB)");
                    return Page();
                }
                var imagesUrl = _options.ApiUrl;

                // Basic checks: content-type and extension whitelist
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp", "image/x-icon" };
                var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".ico" };

                if (!allowedTypes.Contains(Upload.ContentType?.ToLowerInvariant()))
                {
                    ModelState.AddModelError("Upload", "ไฟล์ที่อัปโหลดต้องเป็นรูปภาพ (jpg, png, gif, webp, bmp)");
                    return Page();
                }

                var ext = Path.GetExtension(Upload.FileName)?.ToLowerInvariant();
                if (!allowedExt.Contains(ext))
                {
                    ModelState.AddModelError("Upload", "นามสกุลไฟล์ไม่รองรับ");
                    return Page();
                }

                // Read into memory to perform header (magic bytes) check and then upload
                using (var ms = new MemoryStream())
                {
                    await Upload.CopyToAsync(ms);
                    ms.Position = 0;

                    if (!IsImageByHeader(ms))
                    {
                        ModelState.AddModelError("Upload", "ไฟล์ไม่ใช่รูปภาพที่ถูกต้อง");
                        return Page();
                    }

                    ms.Position = 0;
                    using (var content = new StreamContent(ms))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue(Upload.ContentType);
                        var response = await _httpClient.PostAsync(imagesUrl, content);
                    }
                }
            }
            return RedirectToPage("/Index");
        }

        private static bool IsImageByHeader(Stream stream)
        {
            if (!stream.CanSeek) return false;

            long originalPos = stream.Position;
            try
            {
                byte[] header = new byte[12];
                int read = stream.Read(header, 0, header.Length);
                if (read < 4) return false;

                // JPEG: FF D8 FF
                if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
                // PNG: 89 50 4E 47
                if (read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
                // GIF: 'G''I''F'
                if (header[0] == (byte)'G' && header[1] == (byte)'I' && header[2] == (byte)'F') return true;
                // BMP: 'B''M'
                if (header[0] == (byte)'B' && header[1] == (byte)'M') return true;
                // ICO: 00 00 01 00
                if (header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && header[3] == 0x00) return true;
                // WEBP: "RIFF" .... "WEBP"
                if (read >= 12 && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
                    && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P') return true;

                return false;
            }
            finally
            {
                stream.Position = originalPos;
            }
        }
    }
}