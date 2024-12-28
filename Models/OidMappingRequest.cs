using System;
using System.Collections.Generic;

namespace Models
{
    public class OidMappingRequest
    {
        public Device Device { get; set; }
        public List<OidParameter> Mappings { get; set; }
    }

    public class OidParameter
    {
        public string Oid { get; set; }
        public string ParameterName { get; set; }
    }
}
