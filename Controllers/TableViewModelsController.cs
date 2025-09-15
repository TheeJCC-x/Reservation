using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Reservation.Data;
using Reservation.Models;

namespace Reservation.Controllers
{
    public class TableViewModelsController : Controller
    {
        private readonly ReservationContext _context;

        public TableViewModelsController(ReservationContext context)
        {
            _context = context;
        }

        // GET: TableViewModels
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tables.ToListAsync());
        }

        // GET: TableViewModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tableViewModel = await _context.Tables
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tableViewModel == null)
            {
                return NotFound();
            }

            return View(tableViewModel);
        }

        // GET: TableViewModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TableViewModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TableNumber,Seats,Availability")] TableViewModel tableViewModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tableViewModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tableViewModel);
        }

        // GET: TableViewModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tableViewModel = await _context.Tables.FindAsync(id);
            if (tableViewModel == null)
            {
                return NotFound();
            }
            return View(tableViewModel);
        }

        // POST: TableViewModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TableNumber,Seats,Availability")] TableViewModel tableViewModel)
        {
            if (id != tableViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tableViewModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TableViewModelExists(tableViewModel.Id))
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
            return View(tableViewModel);
        }

        // GET: TableViewModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tableViewModel = await _context.Tables
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tableViewModel == null)
            {
                return NotFound();
            }

            return View(tableViewModel);
        }

        // POST: TableViewModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tableViewModel = await _context.Tables.FindAsync(id);
            if (tableViewModel != null)
            {
                _context.Tables.Remove(tableViewModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TableViewModelExists(int id)
        {
            return _context.Tables.Any(e => e.Id == id);
        }
    }
}
