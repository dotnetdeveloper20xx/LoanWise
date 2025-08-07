using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanWise.Domain.Common
{
    /// <summary>
    /// Marker interface for domain events.
    /// These events represent significant occurrences within the domain model (e.g., LoanFunded).
    /// They can be raised by aggregates and dispatched after unit of work completion.
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredOnUtc { get; }
    }
}
