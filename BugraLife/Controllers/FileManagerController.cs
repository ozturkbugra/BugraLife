using Microsoft.AspNetCore.Mvc;
using BugraLife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features; // FormOptions için gerekli olabilir

namespace BugraLife.Controllers
{
    [Authorize]
    public class FileManagerController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public FileManagerController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // =========================================================================
        // 1. ANA SAYFA (LİSTELEME)
        // =========================================================================
        public IActionResult Index(string path = "")
        {
            // wwwroot/paylasim klasörü kontrolü
            if (string.IsNullOrEmpty(_env.WebRootPath))
            {
                return Content("Hata: Sunucu kök dizini (wwwroot) bulunamadı.");
            }

            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

            // Path null ise boş string yap
            var currentFullPath = Path.Combine(rootPath, path ?? "");

            // GÜVENLİK: Path Traversal (../../) Koruması
            // Kullanıcı ".." yazarak sistem dosyalarına erişmeye çalışırsa kök dizine at.
            if (!Path.GetFullPath(currentFullPath).StartsWith(rootPath))
            {
                currentFullPath = rootPath;
                path = "";
            }

            // Klasör fiziksel olarak yoksa (Silinmişse vb.) kök dizine dön
            if (!Directory.Exists(currentFullPath))
            {
                currentFullPath = rootPath;
                path = "";
            }

            var model = new FileManagerViewModel
            {
                CurrentPath = path,
                // ParentPath: Eğer ana dizindeysek null, değilse bir üst klasör
                ParentPath = string.IsNullOrEmpty(path) ? null : Path.GetDirectoryName(path)?.Replace("\\", "/")
            };

            try
            {
                // A. KLASÖRLERİ ÇEK
                var dirs = Directory.GetDirectories(currentFullPath);
                foreach (var dir in dirs)
                {
                    var dirName = new DirectoryInfo(dir).Name;
                    model.Directories.Add(new FileItem
                    {
                        Name = dirName,
                        Path = Path.Combine(path ?? "", dirName).Replace("\\", "/"),
                        IsDirectory = true,
                        Icon = "bi-folder-fill text-warning"
                    });
                }

                // B. DOSYALARI ÇEK
                var files = Directory.GetFiles(currentFullPath);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    model.Files.Add(new FileItem
                    {
                        Name = fileInfo.Name,
                        Path = Path.Combine(path ?? "", fileInfo.Name).Replace("\\", "/"),
                        IsDirectory = false,
                        Size = FormatSize(fileInfo.Length),
                        Extension = fileInfo.Extension.ToLower(),
                        Icon = GetFileIcon(fileInfo.Extension)
                    });
                }
            }
            catch (Exception ex)
            {
                // Erişim hatası vb. olursa kullanıcıya göster (View'da ViewBag.Error kontrolü yapabilirsin)
                ViewBag.Error = "Dosyalar listelenirken hata oluştu: " + ex.Message;
            }

            return View(model);
        }

        // =========================================================================
        // 2. YENİ KLASÖR OLUŞTUR
        // =========================================================================
        [HttpPost]
        public IActionResult CreateFolder(string currentPath, string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName)) return RedirectToAction("Index", new { path = currentPath });

            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
            var fullPath = Path.Combine(rootPath, currentPath ?? "", folderName);

            // Sadece "paylasim" klasörü altına izin ver
            if (Path.GetFullPath(fullPath).StartsWith(rootPath) && !Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return RedirectToAction("Index", new { path = currentPath });
        }

        // =========================================================================
        // 3. DOSYA YÜKLEME (4 GB DESTEKLİ & AJAX UYUMLU)
        // =========================================================================
        [HttpPost]
        [DisableRequestSizeLimit] // ASP.NET Core limitlerini kaldırır (IIS limiti web.config'den gelir)
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)] // Form veri limitini kaldırır
        public async Task<IActionResult> UploadFile(string currentPath, List<IFormFile> files)
        {
            // Eğer dosya çok büyükse veya IIS reddettiyse 'files' null gelebilir.
            if (files == null || files.Count == 0)
            {
                return Json(new { success = false, message = "Dosya seçilmedi veya sunucu limiti aşıldı." });
            }

            try
            {
                var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
                var targetFolder = Path.Combine(rootPath, currentPath ?? "");

                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                // Güvenlik kontrolü
                if (Path.GetFullPath(targetFolder).StartsWith(rootPath))
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var filePath = Path.Combine(targetFolder, file.FileName);

                            // Büyük dosyaları stream ile kopyala (RAM'i şişirmez)
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                        }
                    }
                    // Başarılı (Frontend AJAX beklediği için JSON dönüyoruz)
                    return Json(new { success = true, message = "Dosyalar başarıyla yüklendi." });
                }

                return Json(new { success = false, message = "Geçersiz hedef klasör." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Yükleme hatası: " + ex.Message });
            }
        }

        // =========================================================================
        // 4. SİLME İŞLEMİ (DOSYA VEYA KLASÖR)
        // =========================================================================
        [HttpPost]
        public IActionResult Delete(string path)
        {
            if (string.IsNullOrEmpty(path)) return RedirectToAction("Index");

            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
            var fullPath = Path.Combine(rootPath, path);

            // Sadece "paylasim" klasörü altındakileri silebilir
            if (Path.GetFullPath(fullPath).StartsWith(rootPath))
            {
                try
                {
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    else if (Directory.Exists(fullPath))
                    {
                        Directory.Delete(fullPath, true); // true = İçi doluysa da sil
                    }
                }
                catch
                {
                    // Silme hatası olursa (izin vb.) sessizce devam et veya logla
                }
            }

            // Silme işleminden sonra mevcut klasöre değil, silinen öğenin bulunduğu listeye dön
            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            return RedirectToAction("Index", new { path = parent });
        }

        // =========================================================================
        // 5. DOSYA İNDİRME
        // =========================================================================
        public IActionResult Download(string path)
        {
            if (string.IsNullOrEmpty(path)) return RedirectToAction("Index");

            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
            var fullPath = Path.Combine(rootPath, path);

            if (Path.GetFullPath(fullPath).StartsWith(rootPath) && System.IO.File.Exists(fullPath))
            {
                // Dosya türünü (MIME Type) otomatik bul
                var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fullPath, out string contentType))
                {
                    contentType = "application/octet-stream"; // Bilinmiyorsa genel dosya tipi
                }

                // Dosyayı byte array olarak belleğe alıp göndermek yerine Stream olarak gönder
                // Bu, büyük dosyalarda sunucu RAM'ini şişirmez.
                var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                return File(fileStream, contentType, Path.GetFileName(fullPath));
            }

            return RedirectToAction("Index");
        }

        // =========================================================================
        // YARDIMCI METOTLAR (PRIVATE)
        // =========================================================================

        // Byte'ı KB, MB, GB, TB formatına çevirir
        private string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }

        // Uzantıya göre Bootstrap ikonu belirler
        private string GetFileIcon(string ext)
        {
            return ext.ToLower() switch
            {
                ".pdf" => "bi-file-pdf-fill text-danger",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".svg" => "bi-file-image-fill text-info",
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "bi-file-zip-fill text-warning",
                ".txt" or ".md" or ".log" => "bi-file-text-fill text-secondary",
                ".xlsx" or ".xls" or ".csv" => "bi-file-excel-fill text-success",
                ".docx" or ".doc" => "bi-file-word-fill text-primary",
                ".pptx" or ".ppt" => "bi-file-ppt-fill text-danger",
                ".mp4" or ".avi" or ".mov" or ".mkv" => "bi-file-play-fill text-danger",
                ".mp3" or ".wav" or ".flac" => "bi-file-music-fill text-success",
                ".exe" or ".msi" or ".bat" => "bi-file-earmark-binary-fill text-secondary",
                ".html" or ".css" or ".js" or ".cs" or ".json" or ".xml" => "bi-file-earmark-code-fill text-warning",
                _ => "bi-file-earmark-fill text-light"
            };
        }
    }
}