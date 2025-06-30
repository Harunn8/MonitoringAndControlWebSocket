using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TcpDeviceV2
    {
        public Guid Id { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public List<string> TcpFormat { get; set; }
        public List<TcpDataV2> TcpData { get; set; }

    }

    public class TcpDataV2
    {
        [Key]
        public Guid ParameterId { get; set; }
        public string Request { get; set; }
        public string ParameterName { get; set; }
    }
}
