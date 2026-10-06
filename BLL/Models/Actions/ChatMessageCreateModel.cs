using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Actions;

public class ChatMessageCreateModel
{
    public const int MaxLength = 1000;

    [Required(ErrorMessage = "Write a message first.")]
    [StringLength(MaxLength, ErrorMessage = "A message can be at most {1} characters long.")]
    public string Message { get; set; } = string.Empty;
}
