//------------------------------------------------------------------------------
// <auto-generated>
//     Der Code wurde von einer Vorlage generiert.
//
//     Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten der Anwendung.
//     Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LfS.GestureDatabase
{
    using System;
    using System.Collections.Generic;
    
    public partial class Touch
    {
        public long Id { get; set; }
        public long Time { get; set; }
        public long FingerId { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public long Trace_Id { get; set; }
    
        public virtual Trace Trace { get; set; }
    }
}
