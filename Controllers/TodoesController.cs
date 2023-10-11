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
            var user = await _userManager.GetUserAsync(User);
            // Récupérer les todos de l'utilisateur connecté
            var todos = _context.Todos
                .Include(t => t.Category)
                .Where(t => t.UserId == user.Id);
            return View(await todos.ToListAsync());
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
                .Include(t => t.User)
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Label");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Todoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TodoId,Title,Description,CreatedDate,CategoryId")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                todo.UserId = user.Id;
                todo.CreatedDate = DateTime.Now;

                _context.Add(todo);

                await _context.SaveChangesAsync();
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Label", todo.CategoryId);
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
                .Where(t => t.UserId == user.Id)
                .FirstOrDefaultAsync(t => t.TodoId == id);

            if (todo == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Label", todo.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", todo.UserId);
            return View(todo);
        }

        // POST: Todoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TodoId,Title,Description,CreatedDate,CategoryId,UserId")] Todo todo)
        {
            if (id != todo.TodoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    todo.CreatedDate = DateTime.Now;
                    _context.Update(todo);
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

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Label", todo.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", todo.UserId);

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
                _context.Todos.Remove(todo);
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
