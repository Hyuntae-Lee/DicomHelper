using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomHelper
{
    class PatientInfo_t
    {
        public string Name { set; get; }
        public string ID { set; get; }
        public int Age { set; get; }
        public string Gender { set; get; }
        public string BirthDate { set; get; }
        public bool Pediatric { set; get; }
        public string TargetPart { set; get; }
    }
}
