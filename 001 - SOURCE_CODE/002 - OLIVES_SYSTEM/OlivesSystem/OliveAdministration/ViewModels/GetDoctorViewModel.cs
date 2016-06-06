﻿using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace DotnetSignalR.ViewModels
{
    public class GetDoctorViewModel
    {
        [Required(ErrorMessage = ErrorCodes.DoctorIdIsRequired)]
        public string Id { get; set; }

        public string IdentityCardNo { get; set; }
    }
}