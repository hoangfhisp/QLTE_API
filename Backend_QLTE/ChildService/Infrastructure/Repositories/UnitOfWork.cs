using Backend_QLTE.ChildService.Application.Interfaces.Repositories;
using Backend_QLTE.ChildService.Infrastructure.Data;

namespace Backend_QLTE.ChildService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ChildDbContext _context;

        public UnitOfWork
        (
            ChildDbContext context,
            IStudyPeriodRepository studyPeriod)
        {
            _context = context;
            StudyPeriods = studyPeriod;
        }

        public IStudyPeriodRepository StudyPeriods { get; }

        public async Task<IEfTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return new EfTransaction(transaction);
        }
    }

}
