using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FulbankApp.ViewModels
{
    public class DepthZone
    {
        public string Name { get; set; }
        public Rect Area { get; set; }
        public int BaseZIndex { get; set; }
        public double DepthThreshold { get; set; } // Position Y où l'objet commence
    }
}
