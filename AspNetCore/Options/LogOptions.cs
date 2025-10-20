using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Options
{
    public sealed class LogOptions
    {
        public bool Enabled { get; set; } = true;
        public int MaxBodyBytes { get; set; } = 64 * 1024; // preview cap
        public bool LogHeaders { get; set; } = true;
    }
}
