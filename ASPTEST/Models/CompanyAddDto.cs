using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASPTEST.Models
{
    public class CompanyAddDto
    {
        // 验证数据方式之一，使用System.ComponentModel.DataAnnotations的属性进行验证
        //[Required]
        // 如果添加了参数，则会用参数内容进行反馈
        [Required(ErrorMessage = "{0}这个字段是必填的")] // 此时返回的信息应该是：Name这个字段是必填的
        [MaxLength(100)]
        public string Name { get; set; }

        [Display(Name = "公司简介")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "{0}的范围应该在{2}~{1}之间")]  // 公司简介的范围应该在10~500之间
        public string Introduction { get; set; }
        // 直接new一个List，赋初值，避免后面没有该参数时带来的空指针异常
        public ICollection<EmployeeAddDto> Employees { get; set; } = new List<EmployeeAddDto>();
    }
}
