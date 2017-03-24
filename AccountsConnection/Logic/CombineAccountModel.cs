using xxx.Backoffice.BusinessLogic.Model;
using System;
using System.Collections.Generic;

namespace xxx.Backoffice.BusinessLogic.ViewModel
{
    public class CombineAccountModel
    {
        ///EX:wife ID
        public Guid Id { get; set; }

        ///EX:wife
        public Guid UserId { get; set; }

        ///EX:husband 
        public Guid CombinedId { get; set; }
        public string UserCode { get; set; }
        public string CombinedCode { get; set; }
        public string UserName { get; set; }
        public string CombinedName { get; set; }

        public string Email { get; set; }
        public string CellPhone { get; set; }
        public FormStatus Status { get; set; }
        public string File { get; set; }
        public string FileName { get; set; }

        /// T:upward, F:downward
        public bool Position { get; set; }
        public DateTime CreatedTime { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }             
        public string ModifiedName { get; set; }
        public string ModifiedDateTime { get; set; }

        public List<OrganizationModel> Organizations { get; set; }




}
}
