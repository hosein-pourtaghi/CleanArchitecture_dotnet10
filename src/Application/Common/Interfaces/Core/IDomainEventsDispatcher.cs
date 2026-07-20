using Application.Common.Messaging;
using SharedKernel.Messaging;

namespace Application.Common.Interfaces.Core;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
