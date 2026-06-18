using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Accounting;

public class CashTransactionService : ICashTransactionService
{
    private readonly IRepository<CashTransaction> _repository;
    private readonly IRepository<CashRegister> _registerRepository;

    public CashTransactionService(
        IRepository<CashTransaction> repository,
        IRepository<CashRegister> registerRepository)
    {
        _repository = repository;
        _registerRepository = registerRepository;
    }

    public async Task<IEnumerable<CashTransactionDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(t => t.CashRegister)
            .Select(t => new CashTransactionDto
            {
                Id = t.Id,
                CashRegisterId = t.CashRegisterId,
                CashRegisterName = t.CashRegister.Name,
                Type = t.Type,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                PaymentType = t.PaymentType,
                Description = t.Description,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId
            })
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CashTransactionDto>> GetByRegisterAsync(int cashRegisterId)
    {
        var transactions = await _repository.Query()
            .Where(t => t.CashRegisterId == cashRegisterId)
            .Include(t => t.CashRegister)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .Select(t => new CashTransactionDto
            {
                Id = t.Id,
                CashRegisterId = t.CashRegisterId,
                CashRegisterName = t.CashRegister.Name,
                Type = t.Type,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                PaymentType = t.PaymentType,
                Description = t.Description,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId
            })
            .ToListAsync();

        // Running balance hesapla
        var register = await _registerRepository.GetByIdAsync(cashRegisterId);
        decimal runningBalance = 0;
        // İlk bakiye: register açılış bakiyesi - tüm hareketlerin toplamı = açılış bakiyesi
        // Ama biz opening balance'ı ayrı tutmuyoruz, doğrudan hareketlerden hesaplayalım
        foreach (var t in transactions)
        {
            runningBalance += t.Type == CashTransactionType.Giris ? t.Amount : -t.Amount;
            t.RunningBalance = runningBalance;
        }

        transactions.Reverse(); // En yeni en üstte
        return transactions;
    }

    public async Task<CashTransactionDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(t => t.Id == id)
            .Include(t => t.CashRegister)
            .Select(t => new CashTransactionDto
            {
                Id = t.Id,
                CashRegisterId = t.CashRegisterId,
                CashRegisterName = t.CashRegister.Name,
                Type = t.Type,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                PaymentType = t.PaymentType,
                Description = t.Description,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CashTransactionDto> CreateAsync(CashTransactionCreateDto dto)
    {
        return await CreateWithReferenceAsync(dto, "Manual", 0);
    }

    public async Task<CashTransactionDto> CreateWithReferenceAsync(CashTransactionCreateDto dto, string referenceType, int referenceId)
    {
        var register = await _registerRepository.GetByIdAsync(dto.CashRegisterId)
            ?? throw new KeyNotFoundException($"Kasa bulunamadı: {dto.CashRegisterId}");

        var entity = new CashTransaction
        {
            CashRegisterId = dto.CashRegisterId,
            Type = dto.Type,
            Amount = dto.Amount,
            TransactionDate = TurkeyTime.WithCurrentTime(dto.TransactionDate),
            PaymentType = dto.PaymentType,
            Description = dto.Description,
            ReferenceType = referenceType,
            ReferenceId = referenceId > 0 ? referenceId : null
        };

        await _repository.AddAsync(entity);

        // Kasa bakiyesini güncelle
        if (dto.Type == CashTransactionType.Giris)
            register.Balance += dto.Amount;
        else
            register.Balance -= dto.Amount;

        await _registerRepository.UpdateAsync(register);

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.Query()
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Kasa hareketi bulunamadı: {id}");

        // Kasa bakiyesini geri al
        var register = await _registerRepository.GetByIdAsync(entity.CashRegisterId)
            ?? throw new KeyNotFoundException($"Kasa bulunamadı: {entity.CashRegisterId}");

        if (entity.Type == CashTransactionType.Giris)
            register.Balance -= entity.Amount;
        else
            register.Balance += entity.Amount;

        await _registerRepository.UpdateAsync(register);
        await _repository.DeleteAsync(entity);
    }
}
