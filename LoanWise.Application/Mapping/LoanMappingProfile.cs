using AutoMapper;
using LoanWise.Application.DTOs.Repayments;
using LoanWise.Application.Features.Fundings.DTOs;
using LoanWise.Application.Features.Loans.DTOs;
using LoanWise.Domain.Entities;

namespace LoanWise.Application.Mapping
{
    public class LoanMappingProfile : Profile
    {
        public LoanMappingProfile()
        {
            CreateMap<Loan, BorrowerLoanDto>()
                .ForMember(dest => dest.LoanId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Value))
                .ForMember(dest => dest.FundedAmount, opt => opt.MapFrom(src => src.Fundings.Sum(f => f.Amount.Value)))
                .ForMember(dest => dest.DurationInMonths, opt => opt.MapFrom(src => src.DurationInMonths))
                .ForMember(dest => dest.Purpose, opt => opt.MapFrom(src => src.Purpose))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.RiskLevel, opt => opt.MapFrom(src => src.RiskLevel));

            CreateMap<Loan, LoanSummaryDto>()
                .ForMember(dest => dest.LoanId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Value))
                .ForMember(dest => dest.FundedAmount, opt => opt.MapFrom(src => src.Fundings.Sum(f => f.Amount.Value)))
                .ForMember(dest => dest.BorrowerId, opt => opt.MapFrom(src => src.BorrowerId))
                .ForMember(dest => dest.DurationInMonths, opt => opt.MapFrom(src => src.DurationInMonths))
                .ForMember(dest => dest.Purpose, opt => opt.MapFrom(src => src.Purpose))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<Loan, LenderFundingDto>()
                 .ForCtorParam("LoanId", opt => opt.MapFrom(src => src.Id))
                 .ForCtorParam("LoanAmount", opt => opt.MapFrom(src => src.Amount.Value))
                 .ForCtorParam("TotalFunded", opt => opt.MapFrom(src => src.Fundings.Sum(f => f.Amount.Value)))
                 .ForCtorParam("Purpose", opt => opt.MapFrom(src => src.Purpose.ToString()))
                 .ForCtorParam("Status", opt => opt.MapFrom(src => src.Status.ToString()))
                 .ForCtorParam("AmountFundedByYou", opt => opt.MapFrom((src, ctx) =>
                 {
                     var lenderId = ctx.Items["LenderId"] as Guid?;
                     return lenderId.HasValue
                         ? src.Fundings.Where(f => f.LenderId == lenderId.Value).Sum(f => f.Amount.Value)
                         : 0m;
                 }));


            CreateMap<Repayment, RepaymentDto>()
           .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.RepaymentAmount));

        }
    }
}
