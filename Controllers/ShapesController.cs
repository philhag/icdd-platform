using IcddWebApp.PageModels.Error;
using IcddWebApp.PageModels.Shapes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IcddWebApp.Controllers
{
    public class ShapesController : Controller
    {
        public IActionResult Index()
        {
            return View(new ShapesPageModel());
        }

        [HttpPost]
        public async Task<IActionResult> UploadSHACLFileAsync(IFormFile file, string shapesFolderPath)
        {
            if (file != null && Path.GetExtension(file.FileName) == ".ttl")
            {
                var fileName = file.FileName;
                if (!fileName.Contains(".shapes.ttl"))
                {
                    fileName = file.FileName.Split(".")[0] + ".shapes.ttl";
                }

                try {
                    using (var fs = System.IO.File.Create(Path.Combine(shapesFolderPath, fileName)))
                    {
                        await file.CopyToAsync(fs);
                        fs.Flush();
                    }
                }
                catch
                {
                    return View("ExtendedError", new ErrorPageModel(500, "Internal Server Error",
                        "File cannot be saved.",
                        new FileLoadException(), $"~/Shapes"));
                }

                return RedirectToAction("Index");
            }
            else
            {
                return View("ExtendedError", new ErrorPageModel(400, "Bad request",
                    "File is not of type .ttl or doesn't exist",
                    new FormatException(), $"~/Shapes"));
            }

        }
    }
}
