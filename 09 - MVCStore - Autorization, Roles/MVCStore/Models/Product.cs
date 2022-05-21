﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCStore.Models
{
	public class Product
	{
		public int ProductID { get; set; }
		[Required(ErrorMessage = "Please enter a product name")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Please enter a description")]
		public string Description { get; set; }
		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Please enter a positive price")]
		[Column(TypeName = "decimal(8, 2)")]
		public decimal Price { get; set; }
	}
}
