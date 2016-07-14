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
    
    public partial class Doctor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Doctor()
        {
            this.Ratings = new HashSet<Rating>();
        }
    
        public int Id { get; set; }
        public Nullable<double> Rank { get; set; }
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; }
        public int Voters { get; set; }
        public int Money { get; set; }
        public int PlaceId { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    
        public virtual Person Person { get; set; }
        public virtual Place Place { get; set; }
        public virtual Specialty Specialty { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rating> Ratings { get; set; }
    }
}
