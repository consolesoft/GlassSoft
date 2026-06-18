using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GlassSoft.Application.Services.Accounting;

public class AccountTransactionService : IAccountTransactionService
{
    private readonly IRepository<AccountTransaction> _repository;
    private readonly IRepository<CashTransaction> _cashTransactionRepository;

    public AccountTransactionService(IRepository<AccountTransaction> repository, IRepository<CashTransaction> cashTransactionRepository)
    {
        _repository = repository;
        _cashTransactionRepository = cashTransactionRepository;
    }

    public async Task<IEnumerable<AccountTransactionDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(t => t.Customer)
            .Select(t => new AccountTransactionDto
            {
                Id = t.Id,
                CustomerId = t.CustomerId,
                CustomerTitle = t.Customer.Title,
                Type = t.Type,
                PaymentType = t.PaymentType,
                Amount = t.Amount,
                Currency = t.Currency,
                ExchangeRate = t.ExchangeRate,
                AmountTRY = t.AmountTRY,
                TransactionDate = t.TransactionDate,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Description = t.Description
            })
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AccountTransactionDto>> GetByCustomerAsync(int customerId)
    {
        return await _repository.Query()
            .Where(t => t.CustomerId == customerId)
            .Include(t => t.Customer)
            .Select(t => new AccountTransactionDto
            {
                Id = t.Id,
                CustomerId = t.CustomerId,
                CustomerTitle = t.Customer.Title,
                Type = t.Type,
                PaymentType = t.PaymentType,
                Amount = t.Amount,
                Currency = t.Currency,
                ExchangeRate = t.ExchangeRate,
                AmountTRY = t.AmountTRY,
                TransactionDate = t.TransactionDate,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Description = t.Description
            })
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<AccountTransactionDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(t => t.Id == id)
            .Include(t => t.Customer)
            .Select(t => new AccountTransactionDto
            {
                Id = t.Id,
                CustomerId = t.CustomerId,
                CustomerTitle = t.Customer.Title,
                Type = t.Type,
                PaymentType = t.PaymentType,
                Amount = t.Amount,
                Currency = t.Currency,
                ExchangeRate = t.ExchangeRate,
                AmountTRY = t.AmountTRY,
                TransactionDate = t.TransactionDate,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Description = t.Description
            })
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(AccountTransactionUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Cari hareket bulunamadı: {dto.Id}");

        var amountTRY = dto.Currency == "TRY" ? dto.Amount : dto.Amount * dto.ExchangeRate;

        entity.CustomerId = dto.CustomerId;
        entity.Type = dto.Type;
        entity.PaymentType = dto.PaymentType;
        entity.Amount = dto.Amount;
        entity.Currency = dto.Currency;
        entity.ExchangeRate = dto.ExchangeRate;
        entity.AmountTRY = amountTRY;
        entity.TransactionDate = TurkeyTime.WithCurrentTime(dto.TransactionDate);
        entity.Description = dto.Description;

        await _repository.UpdateAsync(entity);

        // İlgili kasa hareketi varsa onu da güncelle
        var cashTx = await _cashTransactionRepository.Query()
            .FirstOrDefaultAsync(ct => ct.ReferenceType == "AccountTransaction" && ct.ReferenceId == entity.Id);
        if (cashTx != null)
        {
            cashTx.Amount = amountTRY;
            cashTx.TransactionDate = TurkeyTime.WithCurrentTime(dto.TransactionDate);
            cashTx.PaymentType = dto.PaymentType;
            cashTx.Type = dto.Type == TransactionType.Alacak ? CashTransactionType.Giris : CashTransactionType.Cikis;
            await _cashTransactionRepository.UpdateAsync(cashTx);
        }
    }

    public async Task<AccountTransactionDto> CreateAsync(AccountTransactionCreateDto dto)
    {
        var amountTRY = dto.Currency == "TRY" ? dto.Amount : dto.Amount * dto.ExchangeRate;

        var entity = new AccountTransaction
        {
            CustomerId = dto.CustomerId,
            Type = dto.Type,
            PaymentType = dto.PaymentType,
            Amount = dto.Amount,
            Currency = dto.Currency,
            ExchangeRate = dto.ExchangeRate,
            AmountTRY = amountTRY,
            TransactionDate = TurkeyTime.WithCurrentTime(dto.TransactionDate),
            ReferenceType = "Manual",
            Description = dto.Description
        };

        await _repository.AddAsync(entity);

        return await _repository.Query()
            .Where(t => t.Id == entity.Id)
            .Include(t => t.Customer)
            .Select(t => new AccountTransactionDto
            {
                Id = t.Id,
                CustomerId = t.CustomerId,
                CustomerTitle = t.Customer.Title,
                Type = t.Type,
                PaymentType = t.PaymentType,
                Amount = t.Amount,
                Currency = t.Currency,
                ExchangeRate = t.ExchangeRate,
                AmountTRY = t.AmountTRY,
                TransactionDate = t.TransactionDate,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Description = t.Description
            })
            .FirstAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return;

        // İlgili kasa hareketi varsa onu da sil
        var cashTx = await _cashTransactionRepository.Query()
            .FirstOrDefaultAsync(ct => ct.ReferenceType == "AccountTransaction" && ct.ReferenceId == entity.Id);
        if (cashTx != null)
        {
            await _cashTransactionRepository.DeleteAsync(cashTx);
        }

        await _repository.DeleteAsync(entity);
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        var transactions = await _repository.Query()
            .Where(t => t.CustomerId == customerId)
            .ToListAsync();

        var alacak = transactions.Where(t => t.Type == TransactionType.Alacak).Sum(t => t.AmountTRY);
        var borc = transactions.Where(t => t.Type == TransactionType.Borc).Sum(t => t.AmountTRY);

        return borc - alacak;
    }
}
