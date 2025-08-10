// LoanWise.Application/Features/Borrowers/Commands/VerifyKyc/VerifyKycCommand.cs
using MediatR;
using StoreBoost.Application.Common.Models;

namespace LoanWise.Application.Features.Borrowers.Commands.VerifyKyc;

public sealed record VerifyKycCommand(Guid BorrowerId) : IRequest<ApiResponse<string>>;
