using Plt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iEngr.Hookup.Models
{
    public class UserComos
    {
        public UserComos(dynamic comosDUser)
        {
            ComosDUser = comosDUser;
        }
        public IComosDUser ComosDUser { get; set; }
        public IComosBaseObject ObjUser { get; set; }
        public string RealName { get; set; }
        public string HaisumId { get; set; }
        public int Roles { get; set; }
    }
}
