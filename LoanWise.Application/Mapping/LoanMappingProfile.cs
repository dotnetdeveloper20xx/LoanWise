using AutoMapper;
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



        }
    }
}
