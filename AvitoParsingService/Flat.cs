using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AvitoParsingService
{
    public class Flat
    {
        public Flat()
        {
            floor       = "N/A";
            floorInHome = "N/A";
            homeType    = "N/A";
            countRoom   = "N/A";
            totalArea   = "N/A";
            lifeArea    = "N/A";
            price       = "N/A";
            summary     = "N/A";
            link        = "N/A";
        }
        public string floor;
        public string floorInHome;
        public string homeType;
        public string countRoom;
        public string totalArea;
        public string lifeArea;
        public string price;
        public string summary;
        public string link;
    }
}