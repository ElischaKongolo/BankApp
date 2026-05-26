using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BankApp.Core.Interfaces;
using BankApp.Core.Models;

namespace BankApp.Api.Controllers
{
    [Authorize]
    public class EWalletController : Controller
    {
        private readonly IEWalletService _eWalletService;

        public EWalletController(IEWalletService eWalletService)
        {
            _eWalletService = eWalletService;
        }

        // GET: EWallet
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var wallets = await _eWalletService.GetUserWalletsAsync(userId);
            var totalBalance = await _eWalletService.GetTotalWalletBalanceAsync(userId);
            
            ViewBag.TotalBalance = totalBalance;
            return View(wallets);
        }

        // GET: EWallet/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EWallet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WalletName,WalletType,Description,LinkedAccountId")] EWallet wallet)
        {
            if (ModelState.IsValid)
            {
                wallet.UserId = GetUserId();
                await _eWalletService.CreateWalletAsync(wallet);
                TempData["SuccessMessage"] = "E-Wallet created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(wallet);
        }

        // GET: EWallet/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var wallet = await _eWalletService.GetWalletByIdAsync(id);
            if (wallet == null || wallet.UserId != GetUserId())
            {
                return NotFound();
            }

            var transactions = await _eWalletService.GetWalletTransactionsAsync(id, 1, 10);
            ViewBag.Transactions = transactions;
            
            return View(wallet);
        }

        // GET: EWallet/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var wallet = await _eWalletService.GetWalletByIdAsync(id);
            if (wallet == null || wallet.UserId != GetUserId())
            {
                return NotFound();
            }
            return View(wallet);
        }

        // POST: EWallet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WalletName,Description")] EWallet wallet)
        {
            if (id != wallet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingWallet = await _eWalletService.GetWalletByIdAsync(id);
                if (existingWallet == null || existingWallet.UserId != GetUserId())
                {
                    return NotFound();
                }

                existingWallet.WalletName = wallet.WalletName;
                existingWallet.Description = wallet.Description;
                
                await _eWalletService.UpdateWalletAsync(existingWallet);
                TempData["SuccessMessage"] = "E-Wallet updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(wallet);
        }

        // GET: EWallet/Fund/5
        public async Task<IActionResult> Fund(int id)
        {
            var wallet = await _eWalletService.GetWalletByIdAsync(id);
            if (wallet == null || wallet.UserId != GetUserId())
            {
                return NotFound();
            }
            return View(wallet);
        }

        // POST: EWallet/Fund/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fund(int id, decimal amount, string description)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("", "Amount must be greater than zero");
                var wallet = await _eWalletService.GetWalletByIdAsync(id);
                return View(wallet);
            }

            try
            {
                await _eWalletService.FundWalletAsync(id, amount, description);
                TempData["SuccessMessage"] = $"Wallet funded with {amount:C} successfully!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var wallet = await _eWalletService.GetWalletByIdAsync(id);
                return View(wallet);
            }
        }

        // GET: EWallet/Transfer/5
        public async Task<IActionResult> Transfer(int id)
        {
            var wallet = await _eWalletService.GetWalletByIdAsync(id);
            if (wallet == null || wallet.UserId != GetUserId())
            {
                return NotFound();
            }

            var userId = GetUserId();
            var userWallets = await _eWalletService.GetUserWalletsAsync(userId);
            ViewBag.UserWallets = userWallets.Where(w => w.Id != id);
            
            return View(wallet);
        }

        // POST: EWallet/Transfer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, int toWalletId, decimal amount, string description)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("", "Amount must be greater than zero");
                return await TransferView(id);
            }

            try
            {
                await _eWalletService.TransferBetweenWalletsAsync(id, toWalletId, amount, description);
                TempData["SuccessMessage"] = $"Transfer of {amount:C} completed successfully!";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return await TransferView(id);
            }
        }

        // POST: EWallet/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var wallet = await _eWalletService.GetWalletByIdAsync(id);
            if (wallet == null || wallet.UserId != GetUserId())
            {
                return NotFound();
            }

            await _eWalletService.DeleteWalletAsync(id);
            TempData["SuccessMessage"] = "E-Wallet closed successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Helper method to get user ID from session
        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 1;
        }

        private async Task<IActionResult> TransferView(int id)
        {
            var wallet = await _eWalletService.GetWalletByIdAsync(id);
            if (wallet == null || wallet.UserId != GetUserId())
            {
                return NotFound();
            }

            var userId = GetUserId();
            var userWallets = await _eWalletService.GetUserWalletsAsync(userId);
            ViewBag.UserWallets = userWallets.Where(w => w.Id != id);
            
            return View("Transfer", wallet);
        }
    }
}
