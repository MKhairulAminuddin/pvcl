﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDC.Domain.Web.Application
{
    public class ExportedFile
    {
        public byte[] FileBytes { get; set; }
        public string FileExt { get; set; }
        public string FileName { get; set; }

    }
}
