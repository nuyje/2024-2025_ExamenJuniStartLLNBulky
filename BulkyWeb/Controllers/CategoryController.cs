using BulkyWeb.Data;
using BulkyWeb.Models; 
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db; 

        //aan de constructor geef je een ApplicationDbContext object mee 
        //we hebben dit geregistreerd in de services (Program.cs)
        //hier zit heel de configuratie mee in van de EF Core
        //die we eerder maakten (incl connections string)

        public CategoryController(ApplicationDbContext db) 
        { 
            _db = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _db.Categories.ToList();
            return View(objCategoryList);
        }
        public IActionResult Create()
        {
            return View(); 
        }

        
        [HttpPost]
        public IActionResult Create(Category obj)
        {

            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "DisplayOrder cannot exactly match the Name");
            }
            if (obj.Name != null && obj.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Test is an invalid value");
            }
            if (ModelState.IsValid)
            {

                _db.Categories.Add(obj);
                //enkel na volgende lijn ga je effectief wegschrijven naar de database. 
                //zo kan je bijvoorbeeld meerdere wijzingen bijhouden / klaarzetten en dan maar
                //1 keer naar de database gaan wanneer je klaar bent met alles
                _db.SaveChanges();
                //bericht toevoegen aan TempData om op volgende pagina melding te tonen
                TempData["success"] = "Category created successfully";
                //Je wilt een actie van de controller met de naam Index terug uitvoeren
                //hiermee worden alle categories terug geladen en getoond en ga je terug naar deze pagina
                return RedirectToAction("Index", "Category");
            }
            return View();

            
        }
        public IActionResult Edit(int? id)
        {
            //Controle op ID
            if (id == null || id == 0) 
            { 
                return NotFound();
            }

            //op basis van ID het juiste Category object uit _db ophalen
            Category? categoryFromDb = _db.Categories.Find(id); //.Find werkt enkel op primary key! 
            //andere methoden om dit te doen: 
            Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id == id);
            //met FirstOrDefault ook mogelijk om op andere velden dan de primary key te zoeken
            Category? categoryFromDb2 = _db.Categories.Where(u=>u.Id == id).FirstOrDefault();
            if (categoryFromDb == null)
            {
                return NotFound(); 
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Category edited!";
                //Je wilt een actie van de controller met de naam Index terug uitvoeren
                //hiermee worden alle categories terug geladen en getoond en ga je terug naar deze pagina
                return RedirectToAction("Index", "Category");
            }
            return View();


        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? categoryFromDb = _db.Categories.FirstOrDefault(u => u.Id == id);
            
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? obj = _db.Categories.Find(id);
            if (obj == null) 
            {
                return NotFound(); 
            }
            _db.Categories.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Category removed!";
            return RedirectToAction("Index", "Category");

        }
    }
}
