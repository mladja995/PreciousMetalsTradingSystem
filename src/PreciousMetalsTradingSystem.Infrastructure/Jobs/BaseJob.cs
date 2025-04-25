using MediatR;

namespace PreciousMetalsTradingSystem.Infrastructure.Jobs
{
    public abstract class BaseJob
    {
        protected readonly IMediator Mediator;

        protected BaseJob(IMediator mediator)
        {
            Mediator = mediator;
        }

        public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
