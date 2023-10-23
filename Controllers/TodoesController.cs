using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Todolist.Data;
using Todolist.Models;

namespace Todolist.Controllers
{
    [Authorize]
    public class TodoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TodoesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Todoes
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User); // Récupération de l'utilisateur.
            var todos = _context.Todos
                .Include(t => t.Category) // Inclusion de la catégorie associée à chaque todo.
                .Include(t => t.TodoThemes).ThenInclude(tt => tt.Theme) // Inclusion des thèmes associés à chaque todo.
                .Where(t => t.UserId == user.Id); // Filtrage des todos pour l'utilisateur actuel.

            return View(await todos.ToListAsync()); // Affichage de la vue Index avec la liste des todos.
        }

        // GET: Todoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Todos == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var todo = await _context.Todos
                .Include(t => t.Category)
                .Include(t => t.TodoThemes).ThenInclude(tt => tt.Theme)
                .Where(t => t.UserId == user.Id)
                .FirstOrDefaultAsync(t => t.TodoId == id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // GET: Todoes/Create
        public IActionResult Create()
        {
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "Label");
            ViewData["Themes"] = new SelectList(_context.Themes, "ThemeId", "Name");
            return View();
        }

        // POST: Todoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TodoId,Title,Description,CreatedDate,CategoryId")] Todo todo, List<int> SelectedThemes, IFormFile? fileName)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                todo.UserId = user.Id;
                todo.CreatedDate = DateTime.Now;

                // Vérifie si 'fileName' n'est pas null et que sa taille est supérieure à zéro
                if (fileName != null && fileName.Length > 0)
                {
                    // Construit le chemin complet du dossier de téléchargement
                    var uploadPath = Path.Combine("wwwroot/images");

                    // Génère un nom de fichier unique en combinant un nouveau GUID avec le nom de fichier d'origine
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileName.FileName);

                    // Combine le chemin de téléchargement avec le nom de fichier unique pour obtenir le chemin complet du fichier
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    // Crée un flux de fichier et copie le contenu de 'fileName' dans ce flux
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileName.CopyToAsync(fileStream);
                    }

                    // Enregistre le nom de fichier unique dans la propriété 'FileName' de l'objet 'todo'
                    todo.FileName = uniqueFileName;
                }

                _context.Add(todo);
                await _context.SaveChangesAsync(); // Sauvegardez d'abord le Todo pour obtenir un TodoId

                // Vérifie si la liste des thèmes sélectionnés n'est pas nulle
                if (SelectedThemes != null)
                {
                    // Parcours des IDs de thèmes sélectionnés
                    foreach (var themeId in SelectedThemes)
                    {
                        // Crée une nouvelle relation TodoTheme avec l'ID du Todo actuel et l'ID du thème sélectionné
                        var todoTheme = new TodoTheme
                        {
                            TodoId = todo.TodoId,
                            ThemeId = themeId
                        };

                        // Ajoute la relation TodoTheme à la base de données
                        _context.TodoThemes.Add(todoTheme);
                    }

                    // Enregistre les changements dans la base de données, y compris les nouvelles relations TodoTheme
                    await _context.SaveChangesAsync(); // Sauvegardez les TodoThemes après avoir ajouté le Todo
                }

                return RedirectToAction(nameof(Index));
            }

            /*// Récupérer les erreurs du ModelState
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);

            // Afficher les erreurs dans la console
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }*/
            ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "Label", todo.CategoryId);
            ViewData["Themes"] = new SelectList(_context.Themes, "ThemeId", "Name");
            return View(todo);
        }

        // GET: Todoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Todos == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var todo = await _context.Todos
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.TodoThemes) // Incluez les TodoThemes
                .Where(t => t.UserId == user.Id)
                .FirstOrDefaultAsync(t => t.TodoId == id);

            if (todo == null)
            {
                return NotFound();
            }

            ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "Label", todo.CategoryId);
            ViewData["Themes"] = new SelectList(_context.Themes, "ThemeId", "Name");
            return View(todo);
        }

        // POST: Todoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TodoId,Title,Description,CreatedDate,CategoryId")] Todo todo, List<int> SelectedThemes, IFormFile? fileName)
        {
            if (id != todo.TodoId)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTodo = await _context.Todos.FindAsync(id);

                    // Supprimer l'ancienne image si elle existe
                    if (!string.IsNullOrEmpty(existingTodo.FileName))
                    {
                        var imagePath = Path.Combine("wwwroot/images", existingTodo.FileName);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Traitement de la nouvelle image
                    if (fileName != null && fileName.Length > 0)
                    {
                        var uploadPath = Path.Combine("wwwroot/images");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileName.FileName);
                        var filePath = Path.Combine(uploadPath, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await fileName.CopyToAsync(fileStream);
                        }

                        todo.FileName = uniqueFileName;
                    }

                    // Mettez à jour les propriétés du Todo
                    todo.CreatedDate = DateTime.Now;
                    todo.UserId = user.Id;

                    // Détacher l'entité existante du contexte
                    _context.Entry(existingTodo).State = EntityState.Detached;

                    _context.Update(todo);

                    // Mettez à jour les TodoThemes
                    var existingTodoThemes = _context.TodoThemes.Where(tt => tt.TodoId == id);
                    _context.TodoThemes.RemoveRange(existingTodoThemes); // Supprimez les anciens TodoThemes

                    if (SelectedThemes != null)
                    {
                        foreach (var themeId in SelectedThemes)
                        {
                            var todoTheme = new TodoTheme
                            {
                                TodoId = id,
                                ThemeId = themeId
                            };
                            _context.TodoThemes.Add(todoTheme);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoExists(todo.TodoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Category"] = new SelectList(_context.Categories, "CategoryId", "Label", todo.CategoryId);
            return View(todo);
        }

        // GET: Todoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Todos == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var todo = await _context.Todos
                .Where(t => t.UserId == user.Id)
                .Include(t => t.TodoThemes).ThenInclude(tt => tt.Theme)
                .FirstOrDefaultAsync(t => t.TodoId == id);

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: Todoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Todos == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Todos'  is null.");
            }

            var user = await _userManager.GetUserAsync(User);
            var todo = await _context.Todos
                .Where(t => t.UserId == user.Id)
                .FirstOrDefaultAsync(t => t.TodoId == id);

            if (todo != null)
            {
                // Vérifier si une image est associée à l'objet Todo
                var existingTodo = await _context.Todos.FindAsync(id);
   
                if (!string.IsNullOrEmpty(todo.FileName))
                {
                    // Construire le chemin complet de l'image
                    var imagePath = Path.Combine("wwwroot/images", existingTodo.FileName);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TodoExists(int id)
        {
            return (_context.Todos?.Any(e => e.TodoId == id)).GetValueOrDefault();
        }
    }
}
