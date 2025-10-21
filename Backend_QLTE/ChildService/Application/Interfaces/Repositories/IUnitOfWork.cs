namespace Backend_QLTE.ChildService.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IStudyPeriodRepository StudyPeriods { get; } // StudyPeriod Repository
        Task<IEfTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default); // Transaction
    }
}
