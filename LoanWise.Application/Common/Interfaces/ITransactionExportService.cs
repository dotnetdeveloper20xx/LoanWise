using LoanWise.Application.DTOs.Lenders;

namespace LoanWise.Application.Common.Interfaces
{
    public interface ITransactionExportService
    {
        byte[] BuildCsv(IEnumerable<LenderTransactionDto> items);
        byte[] BuildExcel(IEnumerable<LenderTransactionDto> items);
    }
}
