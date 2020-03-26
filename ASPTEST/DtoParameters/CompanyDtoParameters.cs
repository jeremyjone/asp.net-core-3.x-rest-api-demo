using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPTEST.DtoParameters
{
    // 作为一个查询参数类，映射到Controller的参数中，这样就不需要每次修改Controller
    public class CompanyDtoParameters
    {
        public string CompanyName { get; set; }
        public string SearchTerm { get; set; }
    }
}
