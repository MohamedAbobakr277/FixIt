using FixIt.DAL.Data;
using FixIt.DAL.Entities;
using FixIt.DAL.Repositories;

namespace FixIt.DAL.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Issue> Issues { get; }
    IGenericRepository<Rating> Ratings { get; }
    IGenericRepository<MaintenanceSchedule> Schedules { get; }
    IGenericRepository<MaintenanceReport> Reports { get; }
    IGenericRepository<IssueStatusHistory> StatusHistories { get; }
    IGenericRepository<LoginHistory> LoginHistories { get; }
    IGenericRepository<Notification> Notifications { get; }
    IGenericRepository<AdminNotification> AdminNotifications { get; }
    IGenericRepository<Payment> Payments { get; }
    Task<int> CompleteAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly FixItDbContext _context;

    private IGenericRepository<Issue>? _issues;
    private IGenericRepository<Rating>? _ratings;
    private IGenericRepository<MaintenanceSchedule>? _schedules;
    private IGenericRepository<MaintenanceReport>? _reports;
    private IGenericRepository<IssueStatusHistory>? _statusHistories;
    private IGenericRepository<LoginHistory>? _loginHistories;
    private IGenericRepository<Notification>? _notifications;
    private IGenericRepository<AdminNotification>? _adminNotifications;
    private IGenericRepository<Payment>? _payments;

    public UnitOfWork(FixItDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<Issue> Issues
        => _issues ??= new GenericRepository<Issue>(_context);

    public IGenericRepository<Rating> Ratings
        => _ratings ??= new GenericRepository<Rating>(_context);

    public IGenericRepository<MaintenanceSchedule> Schedules
        => _schedules ??= new GenericRepository<MaintenanceSchedule>(_context);

    public IGenericRepository<MaintenanceReport> Reports
        => _reports ??= new GenericRepository<MaintenanceReport>(_context);

    public IGenericRepository<IssueStatusHistory> StatusHistories
        => _statusHistories ??= new GenericRepository<IssueStatusHistory>(_context);

    public IGenericRepository<LoginHistory> LoginHistories
        => _loginHistories ??= new GenericRepository<LoginHistory>(_context);

    public IGenericRepository<Notification> Notifications
        => _notifications ??= new GenericRepository<Notification>(_context);
        
    public IGenericRepository<AdminNotification> AdminNotifications
        => _adminNotifications ??= new GenericRepository<AdminNotification>(_context);

    public IGenericRepository<Payment> Payments
        => _payments ??= new GenericRepository<Payment>(_context);

    public async Task<int> CompleteAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}
