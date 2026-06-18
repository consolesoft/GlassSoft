using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Accounting;

public class ChequeNoteService : IChequeNoteService
{
    private readonly IRepository<ChequeNote> _repository;

    public ChequeNoteService(IRepository<ChequeNote> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ChequeNoteDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(cn => cn.Customer)
            .Select(cn => new ChequeNoteDto
            {
                Id = cn.Id,
                Type = cn.Type,
                DocumentNumber = cn.DocumentNumber,
                CustomerId = cn.CustomerId,
                CustomerTitle = cn.Customer.Title,
                BankName = cn.BankName,
                BranchName = cn.BranchName,
                Amount = cn.Amount,
                Currency = cn.Currency,
                IssueDate = cn.IssueDate,
                DueDate = cn.DueDate,
                Status = cn.Status,
                Notes = cn.Notes
            })
            .OrderByDescending(cn => cn.DueDate)
            .ToListAsync();
    }

    public async Task<ChequeNoteDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(cn => cn.Id == id)
            .Include(cn => cn.Customer)
            .Select(cn => new ChequeNoteDto
            {
                Id = cn.Id,
                Type = cn.Type,
                DocumentNumber = cn.DocumentNumber,
                CustomerId = cn.CustomerId,
                CustomerTitle = cn.Customer.Title,
                BankName = cn.BankName,
                BranchName = cn.BranchName,
                Amount = cn.Amount,
                Currency = cn.Currency,
                IssueDate = cn.IssueDate,
                DueDate = cn.DueDate,
                Status = cn.Status,
                Notes = cn.Notes
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ChequeNoteDto> CreateAsync(ChequeNoteCreateDto dto)
    {
        var entity = new ChequeNote
        {
            Type = dto.Type,
            DocumentNumber = dto.DocumentNumber,
            CustomerId = dto.CustomerId,
            BankName = dto.BankName,
            BranchName = dto.BranchName,
            Amount = dto.Amount,
            Currency = dto.Currency,
            IssueDate = TurkeyTime.WithCurrentTime(dto.IssueDate),
            DueDate = TurkeyTime.WithCurrentTime(dto.DueDate),
            Status = ChequeNoteStatus.Portfoyde,
            Notes = dto.Notes
        };

        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(ChequeNoteUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Çek/Senet bulunamadı: {dto.Id}");

        entity.Type = dto.Type;
        entity.DocumentNumber = dto.DocumentNumber;
        entity.CustomerId = dto.CustomerId;
        entity.BankName = dto.BankName;
        entity.BranchName = dto.BranchName;
        entity.Amount = dto.Amount;
        entity.Currency = dto.Currency;
        entity.IssueDate = TurkeyTime.WithCurrentTime(dto.IssueDate);
        entity.DueDate = TurkeyTime.WithCurrentTime(dto.DueDate);
        entity.Notes = dto.Notes;

        await _repository.UpdateAsync(entity);
    }

    public async Task UpdateStatusAsync(int id, ChequeNoteStatus status)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Çek/Senet bulunamadı: {id}");

        entity.Status = status;
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Çek/Senet bulunamadı: {id}");

        await _repository.DeleteAsync(entity);
    }
}
