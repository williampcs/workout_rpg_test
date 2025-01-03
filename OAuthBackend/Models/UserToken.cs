using System.ComponentModel.DataAnnotations;

public class UserToken
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]  // 添加適當的長度限制
    public string AccessToken { get; set; } = string.Empty;  // 添加默認值

    [Required]
    [StringLength(255)]
    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresAt { get; set; }
}