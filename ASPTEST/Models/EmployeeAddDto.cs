using ASPTEST.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASPTEST.Models
{
    public class EmployeeAddDto : IValidatableObject
    {
        [Required]
        public string EmployeeNo { get; set; }
        // 可以单写
        [Required]
        [MinLength(10)]
        [MaxLength(100)]
        public string FirstName { get; set; }
        // 也可以连写
        [Required, MinLength(10), MaxLength(100)]
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

        // 使用IValidatableObject接口实现跨属性的验证功能
        // 注意，该验证方法，是在属性自身验证之后，如果每个属性的验证不通过，不会执行这里
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FirstName == LastName)
            {
                yield return new ValidationResult("姓和名不可以一样", new[] { nameof(FirstName), nameof(LastName) });
            }
        }
    }
}
