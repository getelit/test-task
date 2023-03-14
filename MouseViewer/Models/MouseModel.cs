using System;

namespace MouseViewer.Models
{
    internal class MouseModel
    {

        public string CreationDate { get; set; } = DateTime.Now.ToString();
        public string M_Event { get; set; }
        public string M_Coords { get; set; }

    }
}
