using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class ChequeNotesController : Controller
{
    private readonly IChequeNoteService _chequeNoteService;
    private readonly ICustomerService _customerService;

    public ChequeNotesController(
        IChequeNoteService chequeNoteService,
        ICustomerService customerService)
    {
        _chequeNoteService = chequeNoteService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index()
    {
        var chequeNotes = await _chequeNoteService.GetAllAsync();
        return View(chequeNotes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var chequeNote = await _chequeNoteService.GetByIdAsync(id);
        if (chequeNote == null) return NotFound();
        return View(chequeNote);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectLists();
        return View(new ChequeNoteCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChequeNoteCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _chequeNoteService.CreateAsync(dto);
        TempData["Success"] = "Çek/Senet başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var chequeNote = await _chequeNoteService.GetByIdAsync(id);
        if (chequeNote == null) return NotFound();
        await LoadSelectLists();
        return View(new ChequeNoteUpdateDto
        {
            Id = chequeNote.Id,
            Type = chequeNote.Type,
            DocumentNumber = chequeNote.DocumentNumber,
            CustomerId = chequeNote.CustomerId,
            BankName = chequeNote.BankName,
            BranchName = chequeNote.BranchName,
            Amount = chequeNote.Amount,
            Currency = chequeNote.Currency,
            IssueDate = chequeNote.IssueDate,
            DueDate = chequeNote.DueDate,
            Notes = chequeNote.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ChequeNoteUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _chequeNoteService.UpdateAsync(dto);
        TempData["Success"] = "Çek/Senet başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ChequeNoteStatus status)
    {
        await _chequeNoteService.UpdateStatusAsync(id, status);
        var statusText = status switch
        {
            ChequeNoteStatus.Tahsilde => "tahsile verildi",
            ChequeNoteStatus.TahsilEdildi => "tahsil edildi",
            ChequeNoteStatus.Karsiliksiz => "karşılıksız olarak işaretlendi",
            ChequeNoteStatus.IadeEdildi => "iade edildi",
            ChequeNoteStatus.Cirolandi => "cirolandı",
            _ => "güncellendi"
        };
        TempData["Success"] = $"Çek/Senet durumu {statusText}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _chequeNoteService.DeleteAsync(id);
        TempData["Success"] = "Çek/Senet başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectLists()
    {
        var customers = await _customerService.GetAllAsync();
        ViewBag.Customers = new SelectList(customers.Where(c => c.IsActive), "Id", "Title");
    }
}
