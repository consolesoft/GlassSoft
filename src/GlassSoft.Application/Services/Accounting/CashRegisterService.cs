using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Accounting;

public class CashRegisterService : ICashRegisterService
{
    private readonly IRepository<CashRegister> _repository;
    private readonly IRepository<CashTransaction> _transactionRepository;

    public CashRegisterService(IRepository<CashRegister> repository, IRepository<CashTransaction> transactionRepository)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IEnumerable<CashRegisterDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(c => c.Transactions)
            .Select(c => new CashRegisterDto
            {
                Id = c.Id,
                Name = c.Name,
                Currency = c.Currency,
                Balance = c.Balance,
                IsActive = c.IsActive,
                Description = c.Description,
                TransactionCount = c.Transactions.Count
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<CashRegisterDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(c => c.Id == id)
            .Include(c => c.Transactions)
            .Select(c => new CashRegisterDto
            {
                Id = c.Id,
                Name = c.Name,
                Currency = c.Currency,
                Balance = c.Balance,
                IsActive = c.IsActive,
                Description = c.Description,
                TransactionCount = c.Transactions.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CashRegisterDto> CreateAsync(CashRegisterCreateDto dto)
    {
        var entity = new CashRegister
        {
            Name = dto.Name,
            Currency = dto.Currency,
            Balance = dto.OpeningBalance,
            Description = dto.Description,
            IsActive = true
        };

        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(CashRegisterUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Kasa bulunamadı: {dto.Id}");

        entity.Name = dto.Name;
        entity.Currency = dto.Currency;
        entity.IsActive = dto.IsActive;
        entity.Description = dto.Description;

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Kasa bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }

    public async Task<(int RegisterCount, List<(string Name, decimal OldBalance, decimal NewBalance)> Details)> RecalculateAllBalancesAsync()
    {
        var registers = await _repository.Query().ToListAsync();
        var allTransactions = await _transactionRepository.Query().ToListAsync();
        var details = new List<(string Name, decimal OldBalance, decimal NewBalance)>();

        foreach (var register in registers)
        {
            var oldBalance = register.Balance;
            var txs = allTransactions.Where(t => t.CashRegisterId == register.Id).ToList();
            var giris = txs.Where(t => t.Type == GlassSoft.Domain.Enums.CashTransactionType.Giris).Sum(t => t.Amount);
            var cikis = txs.Where(t => t.Type == GlassSoft.Domain.Enums.CashTransactionType.Cikis).Sum(t => t.Amount);
            var newBalance = giris - cikis;

            if (oldBalance != newBalance)
            {
                register.Balance = newBalance;
                await _repository.UpdateAsync(register);
                details.Add((register.Name, oldBalance, newBalance));
            }
        }

        return (registers.Count, details);
    }
}
