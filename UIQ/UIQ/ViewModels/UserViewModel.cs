﻿using System.Text.Json.Serialization;
using UIQ.Models;

namespace UIQ.ViewModels
{
    public class UserViewModel : UserModel
    {
        [JsonPropertyName("group_name")]
        public string Group_Name { get; set; }
    }
}