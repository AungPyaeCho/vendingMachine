using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendingMachine.Models
{
    [Table("tblTransaction")]
    public class TransactionModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int productId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [ForeignKey("productId")]
        public ProductModel Product { get; set; }
    }

    public class TransactionResponseModel
    {
        public int pageNo { get; set; }
        public int pageSize { get; set; }
        public int pageCount { get; set; }
        public string sortField { get; set; }
        public string sortOrder { get; set; }
        public bool isEndOfPage => pageNo >= pageCount;
        public List<TransactionModel> transactionData { get; set; }
    }
}
