using LoanWise.Application.DTOs.Exports;

namespace LoanWise.Application.Common.Interfaces
{
    public interface IRepaymentPlanPdfService
    {
        byte[] Render(RepaymentPlanDoc doc);
    }
}
