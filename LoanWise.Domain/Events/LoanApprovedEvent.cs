using LoanWise.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanWise.Domain.Events
{
    public sealed record LoanApprovedEvent(Guid LoanId, Guid BorrowerId) : IDomainEvent
    {
       
    }
}
