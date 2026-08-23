namespace CareerConnect.Application.DTOs.Users;

public record ChangePasswordRequest(
    string OldPassword,
    string NewPassword
);
