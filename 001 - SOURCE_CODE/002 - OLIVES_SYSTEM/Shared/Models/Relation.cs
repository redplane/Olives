//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shared.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Relation
    {
        public int Id { get; set; }
        public int Owner { get; set; }
        public int Relative { get; set; }
    
        public virtual Person Person { get; set; }
        public virtual Person Person1 { get; set; }
    }
}
