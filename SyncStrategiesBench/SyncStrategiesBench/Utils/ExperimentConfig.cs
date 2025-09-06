using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIO.Utils
{
    public class ExperimentConfig
    {
        public string Name { get; set; }
        public int AnzahlTasks { get; set; }
        public int DmSize { get; set; }
        public int TestZyklen { get; set; }
        public bool MesswertvorläufeIgnorieren { get; set; }
        public int Geraete { get; set; }
        public int DarstellungsObjekte { get; set; }
        public int DatenBasisObjekte { get; set; }
        public int Aufzeichner { get; set; }

    }

    public class ExperimentConfigWrapper
    {
        public List<ExperimentConfig> Experimente { get; set; }
    }
}
