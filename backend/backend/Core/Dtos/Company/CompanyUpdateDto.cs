using backend.Core.Enums;

namespace backend.Core.Dtos.Company
{
    public class CompanyUpdateDto
    {
        public string Name { get; set; }
        public CompanySize Size { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
