using Microsoft.AspNetCore.Mvc;
using BugraLife.Models;

namespace BugraLife.Controllers
{
    public class FileManagerController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public FileManagerController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ANA SAYFA (LİSTELEME)
        public IActionResult Index(string path = "")
        {
            // 1. Ana kök dizin (wwwroot/paylasim)
            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");

            // Klasör yoksa oluştur
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

            // 2. İstenen tam yol (Güvenlik kontrolü ile)
            var currentFullPath = Path.Combine(rootPath, path ?? "");

            // PATH TRAVERSAL KORUMASI: Kullanıcı "../" yapıp sistem dosyalarına gidemesin
            if (!Path.GetFullPath(currentFullPath).StartsWith(rootPath))
            {
                currentFullPath = rootPath;
                path = "";
            }

            // 3. Klasörleri ve Dosyaları Oku
            var model = new FileManagerViewModel
            {
                CurrentPath = path,
                ParentPath = string.IsNullOrEmpty(path) ? null : Path.GetDirectoryName(path)?.Replace("\\", "/")
            };

            // Klasörleri Çek
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

            // Dosyaları Çek
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

            return View(model);
        }

        // KLASÖR OLUŞTUR
        [HttpPost]
        public IActionResult CreateFolder(string currentPath, string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName)) return RedirectToAction("Index", new { path = currentPath });

            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
            var fullPath = Path.Combine(rootPath, currentPath ?? "", folderName);

            // Güvenlik kontrolü ve oluşturma
            if (Path.GetFullPath(fullPath).StartsWith(rootPath) && !Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return RedirectToAction("Index", new { path = currentPath });
        }

        // DOSYA YÜKLE
        [HttpPost]
        public async Task<IActionResult> UploadFile(string currentPath, List<IFormFile> files)
        {
            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
            var targetFolder = Path.Combine(rootPath, currentPath ?? "");

            if (Path.GetFullPath(targetFolder).StartsWith(rootPath))
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var filePath = Path.Combine(targetFolder, file.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
            }

            return RedirectToAction("Index", new { path = currentPath });
        }

        // SİLME İŞLEMİ (DOSYA VEYA KLASÖR)
        [HttpPost]
        public IActionResult Delete(string path)
        {
            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
            var fullPath = Path.Combine(rootPath, path);

            if (Path.GetFullPath(fullPath).StartsWith(rootPath))
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                else if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true); // true = içi doluysa da sil
                }
            }

            // Silince bir üst klasöre veya aynı yere dönelim
            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            return RedirectToAction("Index", new { path = parent });
        }

        // İNDİRME
        public IActionResult Download(string path)
        {
            var rootPath = Path.Combine(_env.WebRootPath, "paylasim");
            var fullPath = Path.Combine(rootPath, path);

            if (Path.GetFullPath(fullPath).StartsWith(rootPath) && System.IO.File.Exists(fullPath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                return File(fileBytes, "application/octet-stream", Path.GetFileName(fullPath));
            }

            return RedirectToAction("Index");
        }

        // YARDIMCI: Dosya Boyutu Formatlama (1024 -> 1 KB)
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

        // YARDIMCI: İkon Belirleme
        private string GetFileIcon(string ext)
        {
            return ext.ToLower() switch
            {
                ".pdf" => "bi-file-pdf-fill text-danger",
                ".jpg" or ".jpeg" or ".png" or ".gif" => "bi-file-image-fill text-info",
                ".zip" or ".rar" => "bi-file-zip-fill text-warning",
                ".txt" => "bi-file-text-fill text-secondary",
                ".xlsx" or ".xls" => "bi-file-excel-fill text-success",
                ".docx" or ".doc" => "bi-file-word-fill text-primary",
                _ => "bi-file-earmark-fill text-light"
            };
        }
    }
}