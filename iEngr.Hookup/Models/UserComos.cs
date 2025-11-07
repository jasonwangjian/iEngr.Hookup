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
        public UserComos(IComosDUser comosDUser, IComosBaseObject objUser = null)
        {
            ComosDUser = comosDUser;
            ObjUser = objUser;
            RealName = ObjUser?.spec("Y00T00026.Y00A01154").DisplayValue();
            HaisumId = ObjUser?.spec("Y00T00026.Y00A02688").DisplayValue();
        }
        public IComosDUser ComosDUser { get; set; } 
        public IComosBaseObject ObjUser { get; set; }
        public string RealName { get; set; }
        public string HaisumId { get; set; }
        public int Roles{ get; set; }
}
}
