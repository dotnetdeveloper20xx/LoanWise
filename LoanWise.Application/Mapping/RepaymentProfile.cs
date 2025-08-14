using AutoMapper;
using LoanWise.Application.DTOs.Repayments;
using LoanWise.Domain.Entities;

namespace LoanWise.Application.Mapping
{
    public sealed class RepaymentProfile : Profile
    {
        public RepaymentProfile()
        {
            // We pass "now" via context.Items to compute IsOverdue/Status consistently.
            CreateMap<Repayment, RepaymentDto>()
                .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.RepaymentAmount))
                .ForMember(d => d.PaidAtUtc, opt => opt.MapFrom(s => s.PaidOn))
                .ForMember(d => d.IsOverdue, opt => opt.MapFrom((src, _, __, ctx) =>
                {
                    var now = ctx.Items.TryGetValue("now", out var v) && v is DateTime dt ? dt : DateTime.UtcNow;
                    return !src.IsPaid && now.Date > src.DueDate;
                }))
                // Status depends on IsPaid + computed IsOverdue; easiest in AfterMap
                .ForMember(d => d.Status, opt => opt.Ignore())
                .AfterMap((src, dest, ctx) =>
                {
                    var now = ctx.Items.TryGetValue("now", out var v) && v is DateTime dt ? dt : DateTime.UtcNow;
                    var overdue = !src.IsPaid && now.Date > src.DueDate;
                    dest.Status = src.IsPaid ? "Paid" : (overdue ? "Overdue" : "Scheduled");
                    dest.IsOverdue = overdue; // ensure aligned with Status
                });
        }
    }
}
