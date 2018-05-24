using System;

namespace FrameWork
{
    // Must be used as a hit
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
    public class DataElement : Attribute
    {
        public DataElement()
        {
            AllowDbNull = true;
            Unique = false;
            Index = false;
            Varchar = 0;
            Decimal = 10;
        }

        // indicate if the var can be null
        public bool AllowDbNull { get; set; }

        // Indicates if the var is unique
        public bool Unique { get; set; }

        // Indicates if it is an Index
        public bool Index { get; set; }

        // Indicates the size of the varchar, 0 = TEXT
        public int Varchar { get; set; }

        // Indicates the number of digits after the decimal point
        public byte Decimal { get; set; }
    }
}