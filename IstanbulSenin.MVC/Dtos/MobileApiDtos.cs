namespace IstanbulSenin.MVC.Dtos
{
    // ==================== COMMON DTOs ====================
    
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int? StatusCode { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "İşlem başarılı")
            => new ApiResponse<T> { Success = true, Message = message, Data = data, StatusCode = 200 };

        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400)
            => new ApiResponse<T> { Success = false, Message = message, Data = default, StatusCode = statusCode };
    }

    public class ErrorResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? StatusCode { get; set; }
    }

    // ==================== AUTH DTOs ====================
    
    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class LoginResponseDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }

    public class RegisterRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UserProfileDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserProfileUpdateDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }

    // ==================== NOTIFICATION DTOs ====================
    
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string TargetAudience { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsTestMode { get; set; }
    }

    public class NotificationCreateRequestDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string TargetAudience { get; set; }
        public bool IsTestMode { get; set; }
    }

    // ==================== SECTION DTOs ====================
    
    public class SectionResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MiniAppResponseDto> MiniApps { get; set; } = new();
    }

    // ==================== MINI APP DTOs ====================
    
    public class MiniAppResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string AppUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int? SectionId { get; set; }
    }

    // ==================== DASHBOARD DTOs ====================
    
    public class DashboardStatisticsDto
    {
        public int TotalNotifications { get; set; }
        public int SentNotifications { get; set; }
        public int PendingNotifications { get; set; }
        public decimal SuccessRate { get; set; }
        public List<DailyStatisticsDto> DailyStatistics { get; set; } = new();
        public List<NotificationResponseDto> RecentNotifications { get; set; } = new();
        public Dictionary<string, int> AudienceDistribution { get; set; } = new();
    }

    public class DailyStatisticsDto
    {
        public DateTime Date { get; set; }
        public int SentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
