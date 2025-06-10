using BulkyWeb.Data;
using BulkyWeb.Models;
using BulkyWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db; 
        private readonly IWebHostEnvironment _webHostEnvironment;

        //aan de constructor geef je een ApplicationDbContext object mee 
        //we hebben dit geregistreerd in de services (Program.cs)
        //hier zit heel de configuratie mee in van de EF Core
        //die we eerder maakten (incl connections string)

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment) 
        { 
            _db = db;
            
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
           
            List<Product> objProductList = _db.Products.Include(p => p.Category).ToList();
            
            
            return View(objProductList);
        }

        /* Bij het aanmaken van een nieuw product willen we de gebruiker een dropdownlist met de mogelijke
             * categorieën tonen. Je kan per view maar gebruik maken van één View Model. We hebben om dit te 
             * realiseren echter zowel het model Product als Category nodig. Oplossing is een view model aanmaken
             * waarin deze twee samengebracht worden --> ProductVM.cs
             * 
             * Alternatieve werkwijzen: ViewBag, ViewData of TempData (maar mogelijk te verwarrend voor grote projecten)
             */

        //Voorbeeld werken met ViewBag: 
        //ViewBag.CategoryList = CategoryList;

        //lijst van SelectListItems (combinatie Text + Value, ideaal voor dropdownlist) aanmaken met alle categorieën:
        public IActionResult Upsert(int? id)
        {
             
            IEnumerable<SelectListItem> categoryList = 
                _db.Categories.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

            ProductVM productVM = new ProductVM();
            productVM.Product = new Product();
            productVM.CategoryList = categoryList;
            
            if (id == null || id == 0)
            {
                //insert functionality
                return View(productVM);
            }
            else
            {
                //update functionality
                productVM.Product = _db.Products.FirstOrDefault(u => u.Id == id);
                return View(productVM); 
            }
            
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file) 
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        //oude afbeelding verwijderen, hiervoor hebben we het pad naar afbeelding nodig
                        //in database staat er in het begin een backslash, die moeten we verwijderen
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                    
                }
                if (obj.Product.Id == 0)
                {
                    _db.Products.Add(obj.Product);
                    
                }
                else
                {
                    //Alles updaten:  
                    //_db.Products.Update(obj.Product);

                    //Zelf kiezen welke velden bijgewerkt moeten worden of wanneer
                    //hier kan je zelf extra logica aan toevoegen dus: 
                    var objFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Product.Id);
                    if (objFromDb != null)
                    {
                        objFromDb.Title = obj.Product.Title;
                        objFromDb.ISBN = obj.Product.ISBN;
                        objFromDb.Author = obj.Product.Author;
                        objFromDb.ListPrice = obj.Product.ListPrice;
                        objFromDb.Price = obj.Product.Price;
                        objFromDb.Price50 = obj.Product.Price50;
                        objFromDb.Price100 = obj.Product.Price100;
                        objFromDb.Description = obj.Product.Description;
                        objFromDb.CategoryId = obj.Product.CategoryId;

                        //custom logic: 
                        if (obj.Product.ImageUrl != null)
                        {
                            objFromDb.ImageUrl = obj.Product.ImageUrl; 
                        }

                    }
                    

                }


                _db.SaveChanges();
                TempData["success"] = "Product created successfully";

                return RedirectToAction("Index", "Product");
            }
            else
            {
                return View(obj);
            }
        }
        

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productFromDb = _db.Products.FirstOrDefault(u => u.Id == id);
            
            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _db.Products.Find(id);
            if (obj == null) 
            {
                return NotFound(); 
            }
            _db.Products.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index", "Product");

        }
    }
}
