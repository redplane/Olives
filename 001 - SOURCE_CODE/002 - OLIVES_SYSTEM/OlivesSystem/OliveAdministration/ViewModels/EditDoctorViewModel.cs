﻿using System.ComponentModel.DataAnnotations;
using Shared.Attributes;
using Shared.Constants;
using Shared.Models;

namespace DotnetSignalR.ViewModels
{
    public class EditDoctorViewModel : EditPersonViewModel
    {
        [CoordinateValidation(ErrorMessage = ErrorCodes.InvalidAddress)]
        public Coordinate Address { get; set; }

        [Required(ErrorMessage = ErrorCodes.SpecializationIsRequired)]
        [MaxLength(FieldLength.SpecializationMaxLength, ErrorMessage = ErrorCodes.InvalidSpecializationLength)]
        public string Specialization { get; set; }

        public string[] SpecializationAreas { get; set; }

        public double Rank { get; set; }

        public string IdentityCardNo { get; set; }
    }
}