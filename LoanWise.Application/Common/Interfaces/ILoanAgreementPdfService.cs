using LoanWise.Application.DTOs.Exports;

namespace LoanWise.Application.Common.Interfaces;

public interface ILoanAgreementPdfService
{
    byte[] Render(LoanAgreementDoc doc);
}
