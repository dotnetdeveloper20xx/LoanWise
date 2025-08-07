using LoanWise.Application.Common.Interfaces;
using LoanWise.Domain.Entities;
using MediatR;
using StoreBoost.Application.Common.Models;
using System;

namespace LoanWise.Application.Features.Auth.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApiResponse<Guid>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
        }

        public async Task<ApiResponse<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Registration;

            // Check if user with this email already exists
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return ApiResponse<Guid>.FailureResult("A user with this email already exists.");

            // Simulate KYC: assign credit score (mocked)
            var creditScore = new Random().Next(600, 750);
            var creditTier = creditScore switch
            {
                < 620 => RiskTier.High,
                < 700 => RiskTier.Medium,
                _ => RiskTier.Low
            };

            var userId = Guid.NewGuid();

            // Create user with blank password
            var user = new User(
                id: userId,
                fullName: dto.FullName,
                email: dto.Email,
                passwordHash: "", // set later
                role: dto.Role
            );

            // Hash password and assign it
            var hashedPassword = _passwordService.HashPassword(dto.Password, user);
            user.SetPassword(hashedPassword);

            // Assign mock credit profile
            user.AssignCreditProfile(creditScore, creditTier);

            await _userRepository.AddAsync(user, cancellationToken);

            return ApiResponse<Guid>.SuccessResult(userId, "User registered successfully with mock KYC.");
        }
    }
}
