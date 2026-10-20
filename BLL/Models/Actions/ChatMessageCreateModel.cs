using System.ComponentModel.DataAnnotations;
using DAL.Constraints;

namespace BLL.Models.Actions;

public class ChatMessageCreateModel
{
    public const int MaxLength = ChatMessageConstraints.MessageMaxLength;

    [StringLength(MaxLength, ErrorMessage = "A message can be at most {1} characters long.")]
    public string? Message { get; set; }

    [Display(Name = "Private")]
    public bool IsPrivate { get; set; }
}
