using System.Linq;
using System.Threading.Tasks;
using CouchDB.Driver.Example.Models;
using CouchDB.Driver.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CouchDB.Driver.Example.Controllers;

public class RebelsController(MyDeathStarContext context) : Controller
{
    // GET: RebelsController
    public async Task<ActionResult> Index()
    {
        var rebels = await context.Rebels
            .Take(20)
            .ToListAsync();
        // It is possible to use OrderBy on the IQueryable but the DB needs an index
        return View(rebels.OrderBy(c => c.Name));
    }

    // GET: RebelsController/Details/5
    public async Task<ActionResult> Details(string id)
    {
        var rebel = await context.Rebels.FindAsync(id);
        return View(rebel);
    }

    // GET: RebelsController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: RebelsController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(IFormCollection collection)
    {
        try
        {
            var rebel = new Rebel
            {
                Id = collection["Name"].ToString().Replace(" ", "-").ToLower(),
                Name = collection["Name"],
                Surname = collection["Surname"],
                Age = int.Parse(collection["Age"])
            };
            await context.Rebels.AddAsync(rebel);
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: RebelsController/Edit/5
    public async Task<ActionResult> Edit(string id)
    {
        var rebel = await context.Rebels.FindAsync(id);
        return View(rebel);
    }

    // POST: RebelsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(string id, IFormCollection collection)
    {
        try
        {
            var rebel = await context.Rebels.FindAsync(id);
            rebel.Name = collection["Name"];
            rebel.Surname = collection["Surname"];
            rebel.Age = int.Parse(collection["Age"]);
            await context.Rebels.AddAsync(rebel);
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: RebelsController/Delete/5
    public async Task<ActionResult> Delete(string id)
    {
        var rebel = await context.Rebels.FindAsync(id);
        return View(rebel);
    }

    // POST: RebelsController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(string id, IFormCollection collection)
    {
        try
        {
            var rebel = await context.Rebels.FindAsync(id);
            await context.Rebels.DeleteAsync(rebel!.Id, rebel.Rev);
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}