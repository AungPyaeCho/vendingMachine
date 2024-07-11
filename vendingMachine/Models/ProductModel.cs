using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace vendingMachine.Models
{
    [Table("tblProduct")]
    public class ProductModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string productName { get; set; }

        [Required(ErrorMessage = "Product price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be a positive and decimal number")]
        public decimal productPrice { get; set; }

        [Required(ErrorMessage = "Product quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Product quantity must be a integer! - , + signs are not allowed")]
        public int productQuantity { get; set; }
    }

    public class ProductResponseModel
    {
        public int pageNo { get; set; }
        public int pageSize { get; set; }
        public int pageCount { get; set; }
        public string sortField { get; set; }
        public string sortOrder { get; set; }
        public bool isEndOfPage => pageNo >= pageCount;
        public List<ProductModel> productData { get; set; }
    }
}
