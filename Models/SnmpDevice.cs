using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class SnmpDevice
    {
        public Guid Id { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public List<OidMapping> OidList { get; set; }
    }

    public class OidMapping
    {
        [Key]
        public Guid ParameterId { get; set; }
        public string Oid { get; set; }
        public string ParameterName { get; set; }
    }
}