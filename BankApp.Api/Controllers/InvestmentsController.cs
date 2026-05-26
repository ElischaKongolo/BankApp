using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankApp.Core.Interfaces;
using BankApp.Core.Models;

namespace BankApp.Api.Controllers
{
    [Authorize]
    public class InvestmentsController : Controller
    {
        private readonly IInvestmentService _investmentService;

        public InvestmentsController(IInvestmentService investmentService)
        {
            _investmentService = investmentService;
        }

        // GET: Investments
        public async Task<IActionResult> Index()
        {
            // For demo purposes, using hardcoded user ID
            // In real app, get from authenticated user
            var userId = 1;
            var investments = await _investmentService.GetUserInvestmentsAsync(userId);
            return View(investments);
        }

        // GET: Investments/News
        public async Task<IActionResult> News()
        {
            var featuredNews = await _investmentService.GetFeaturedNewsAsync(5);
            var allNews = await _investmentService.GetInvestmentNewsAsync(1, 20);
            
            ViewBag.FeaturedNews = featuredNews;
            return View(allNews);
        }

        // GET: Investments/News/5
        public async Task<IActionResult> NewsDetails(int id)
        {
            var news = await _investmentService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        // GET: Investments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Investments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Type,InitialAmount,Description,RiskLevel,AccountId")] Investment investment)
        {
            if (ModelState.IsValid)
            {
                investment.UserId = 1; // Hardcoded for demo
                investment.CurrentValue = investment.InitialAmount;
                investment.PurchaseDate = DateTime.UtcNow;
                
                await _investmentService.CreateInvestmentAsync(investment);
                return RedirectToAction(nameof(Index));
            }
            return View(investment);
        }

        // GET: Investments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var investment = await _investmentService.GetInvestmentByIdAsync(id);
            if (investment == null)
            {
                return NotFound();
            }
            return View(investment);
        }

        // GET: Investments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var investment = await _investmentService.GetInvestmentByIdAsync(id);
            if (investment == null)
            {
                return NotFound();
            }
            return View(investment);
        }

        // POST: Investments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Description,RiskLevel")] Investment investment)
        {
            if (id != investment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _investmentService.UpdateInvestmentAsync(investment);
                return RedirectToAction(nameof(Index));
            }
            return View(investment);
        }

        // POST: Investments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _investmentService.DeleteInvestmentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
