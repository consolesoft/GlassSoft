using GlassSoft.Application.DTOs.Sales;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Entities.Purchasing;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Sales;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repository;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderLine> _orderLineRepo;
    private readonly IRepository<OrderLineFeature> _orderLineFeatureRepo;
    private readonly IRepository<PurchaseOrder> _purchaseRepo;
    private readonly IRepository<PurchaseOrderLine> _purchaseLineRepo;
    private readonly IRepository<AccountTransaction> _transactionRepo;
    private readonly IRepository<ChequeNote> _chequeRepo;
    private readonly IRepository<CashTransaction> _cashTxRepo;

    public CustomerService(
        IRepository<Customer> repository,
        IRepository<Order> orderRepo,
        IRepository<OrderLine> orderLineRepo,
        IRepository<OrderLineFeature> orderLineFeatureRepo,
        IRepository<PurchaseOrder> purchaseRepo,
        IRepository<PurchaseOrderLine> purchaseLineRepo,
        IRepository<AccountTransaction> transactionRepo,
        IRepository<ChequeNote> chequeRepo,
        IRepository<CashTransaction> cashTxRepo)
    {
        _repository = repository;
        _orderRepo = orderRepo;
        _orderLineRepo = orderLineRepo;
        _orderLineFeatureRepo = orderLineFeatureRepo;
        _purchaseRepo = purchaseRepo;
        _purchaseLineRepo = purchaseLineRepo;
        _transactionRepo = transactionRepo;
        _chequeRepo = chequeRepo;
        _cashTxRepo = cashTxRepo;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(c => c.Orders)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                CustomerType = c.CustomerType,
                TaxNumber = c.TaxNumber,
                TaxOffice = c.TaxOffice,
                Address = c.Address,
                City = c.City,
                Phone = c.Phone,
                Email = c.Email,
                Currency = c.Currency,
                IsActive = c.IsActive,
                OrderCount = c.Orders.Count
            })
            .OrderBy(c => c.Title)
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(c => c.Id == id)
            .Include(c => c.Orders)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                CustomerType = c.CustomerType,
                TaxNumber = c.TaxNumber,
                TaxOffice = c.TaxOffice,
                Address = c.Address,
                City = c.City,
                Phone = c.Phone,
                Email = c.Email,
                Currency = c.Currency,
                IsActive = c.IsActive,
                OrderCount = c.Orders.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto)
    {
        var entity = new Customer
        {
            Code = dto.Code,
            Title = dto.Title,
            CustomerType = dto.CustomerType,
            TaxNumber = dto.TaxNumber,
            TaxOffice = dto.TaxOffice,
            Address = dto.Address,
            City = dto.City,
            Phone = dto.Phone,
            Email = dto.Email,
            Currency = dto.Currency,
            IsActive = dto.IsActive
        };
        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(CustomerUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Müşteri bulunamadı: {dto.Id}");
        entity.Code = dto.Code;
        entity.Title = dto.Title;
        entity.CustomerType = dto.CustomerType;
        entity.TaxNumber = dto.TaxNumber;
        entity.TaxOffice = dto.TaxOffice;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Currency = dto.Currency;
        entity.IsActive = dto.IsActive;
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Müşteri bulunamadı: {id}");

        // Bağlı kayıtları kontrol et - varsa silmeyi engelle
        var refs = await GetReferencesAsync(id);
        if (refs.HasAnyReference)
        {
            throw new InvalidOperationException(
                $"Bu cariye ait kayıtlar var ({refs.OrderCount} sipariş, " +
                $"{refs.PurchaseOrderCount} satın alma, " +
                $"{refs.AccountTransactionCount} cari hareket, " +
                $"{refs.ChequeNoteCount} çek/senet). " +
                "Önce bağlı kayıtları silin veya 'Zorla Sil' seçeneğini kullanın.");
        }

        await _repository.DeleteAsync(entity);
    }

    public async Task<CustomerReferencesDto> GetReferencesAsync(int id)
    {
        var orderCount = await _orderRepo.Query().CountAsync(o => o.CustomerId == id);
        var purchaseCount = await _purchaseRepo.Query().CountAsync(p => p.CustomerId == id);
        var transactionCount = await _transactionRepo.Query().CountAsync(t => t.CustomerId == id);
        var chequeCount = await _chequeRepo.Query().CountAsync(c => c.CustomerId == id);

        // Kasa hareketleri: müşteriye ait sipariş/cari hareketlere bağlı olanlar
        var orderIds = await _orderRepo.Query()
            .Where(o => o.CustomerId == id)
            .Select(o => o.Id).ToListAsync();
        var transactionIds = await _transactionRepo.Query()
            .Where(t => t.CustomerId == id)
            .Select(t => t.Id).ToListAsync();

        var cashCount = await _cashTxRepo.Query().CountAsync(ct =>
            (ct.ReferenceType == "Order" && orderIds.Contains(ct.ReferenceId!.Value)) ||
            (ct.ReferenceType == "AccountTransaction" && transactionIds.Contains(ct.ReferenceId!.Value)));

        return new CustomerReferencesDto
        {
            OrderCount = orderCount,
            PurchaseOrderCount = purchaseCount,
            AccountTransactionCount = transactionCount,
            ChequeNoteCount = chequeCount,
            CashTransactionCount = cashCount
        };
    }

    public async Task ForceDeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Müşteri bulunamadı: {id}");

        var now = TurkeyTime.Now;

        // 1) Bu cariye ait sipariş ID'leri
        var orderIds = await _orderRepo.Query()
            .Where(o => o.CustomerId == id)
            .Select(o => o.Id).ToListAsync();

        // 2) Bu cariye ait cari hareket ID'leri
        var transactionIds = await _transactionRepo.Query()
            .Where(t => t.CustomerId == id)
            .Select(t => t.Id).ToListAsync();

        // 3) Bu cariye ait satın alma ID'leri
        var purchaseIds = await _purchaseRepo.Query()
            .Where(p => p.CustomerId == id)
            .Select(p => p.Id).ToListAsync();

        // 4) Kasa hareketleri (Order, AccountTransaction, PurchaseOrder referansları)
        var cashTransactions = await _cashTxRepo.Query()
            .Where(ct =>
                (ct.ReferenceType == "Order" && orderIds.Contains(ct.ReferenceId!.Value)) ||
                (ct.ReferenceType == "AccountTransaction" && transactionIds.Contains(ct.ReferenceId!.Value)) ||
                (ct.ReferenceType == "PurchaseOrder" && purchaseIds.Contains(ct.ReferenceId!.Value)))
            .ToListAsync();
        foreach (var ct in cashTransactions)
        {
            ct.IsDeleted = true;
            ct.DeletedAt = now;
            await _cashTxRepo.UpdateAsync(ct);
        }

        // 5) Order Line Features
        var orderLineIds = await _orderLineRepo.Query()
            .Where(l => orderIds.Contains(l.OrderId))
            .Select(l => l.Id).ToListAsync();
        var features = await _orderLineFeatureRepo.Query()
            .Where(f => orderLineIds.Contains(f.OrderLineId))
            .ToListAsync();
        foreach (var f in features)
        {
            f.IsDeleted = true;
            f.DeletedAt = now;
            await _orderLineFeatureRepo.UpdateAsync(f);
        }

        // 6) Order Lines
        var orderLines = await _orderLineRepo.Query()
            .Where(l => orderIds.Contains(l.OrderId))
            .ToListAsync();
        foreach (var l in orderLines)
        {
            l.IsDeleted = true;
            l.DeletedAt = now;
            await _orderLineRepo.UpdateAsync(l);
        }

        // 7) Orders
        var orders = await _orderRepo.Query()
            .Where(o => o.CustomerId == id)
            .ToListAsync();
        foreach (var o in orders)
        {
            o.IsDeleted = true;
            o.DeletedAt = now;
            await _orderRepo.UpdateAsync(o);
        }

        // 8) Purchase Order Lines
        var purchaseLines = await _purchaseLineRepo.Query()
            .Where(l => purchaseIds.Contains(l.PurchaseOrderId))
            .ToListAsync();
        foreach (var l in purchaseLines)
        {
            l.IsDeleted = true;
            l.DeletedAt = now;
            await _purchaseLineRepo.UpdateAsync(l);
        }

        // 9) Purchase Orders
        var purchases = await _purchaseRepo.Query()
            .Where(p => p.CustomerId == id)
            .ToListAsync();
        foreach (var p in purchases)
        {
            p.IsDeleted = true;
            p.DeletedAt = now;
            await _purchaseRepo.UpdateAsync(p);
        }

        // 10) Account Transactions
        var transactions = await _transactionRepo.Query()
            .Where(t => t.CustomerId == id)
            .ToListAsync();
        foreach (var t in transactions)
        {
            t.IsDeleted = true;
            t.DeletedAt = now;
            await _transactionRepo.UpdateAsync(t);
        }

        // 11) Cheques/Notes
        var cheques = await _chequeRepo.Query()
            .Where(c => c.CustomerId == id)
            .ToListAsync();
        foreach (var c in cheques)
        {
            c.IsDeleted = true;
            c.DeletedAt = now;
            await _chequeRepo.UpdateAsync(c);
        }

        // 12) Finally: Customer
        await _repository.DeleteAsync(entity);
    }

    public async Task<string> GenerateCodeAsync(string prefix)
    {
        var lastCustomer = await _repository.Query()
            .Where(c => c.Code.StartsWith(prefix + "-"))
            .OrderByDescending(c => c.Code)
            .FirstOrDefaultAsync();

        if (lastCustomer == null)
            return prefix + "-001";

        var numPart = lastCustomer.Code[(prefix.Length + 1)..];
        if (int.TryParse(numPart, out var num))
            return prefix + "-" + (num + 1).ToString("D3");

        return prefix + "-001";
    }
}
