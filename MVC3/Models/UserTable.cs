namespace MVC3.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserTable")]
    public partial class UserTable
    {
        [Key]
        public int UserId { get; set; }

        [StringLength(50)]
        public string UserName { get; set; }

        [StringLength(1)]
        public string UserSex { get; set; }

        public DateTime? UserBirthDay { get; set; }

        [StringLength(15)]
        public string UserMobilePhone { get; set; }
    }
}
