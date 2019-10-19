using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Search_Invoice.Data.Domain
{
    [Table("inv_InvoiceAuth")]
    public class Inv_InvoiceAuth
    {
        [Key]
        [Column("inv_InvoiceAuth_id")]
        public Guid Inv_InvoiceAuth_id { get; set; }

        [Column("mau_hd")]
        public string Mau_hd { get; set; }

        [Column("inv_invoiceSeries")]
        public string Inv_invoiceSeries { get; set; }

        [Column("inv_invoiceNumber")]
        public string Inv_invoiceNumber { get; set; }

        [Column("inv_invoiceNumber1")]
        public string Inv_invoiceNumber1 { get; set; }

        [Column("inv_invoiceIssuedDate")]
        public DateTime Inv_invoiceIssuedDate { get; set; }

        [Column("inv_currencyCode")]
        public string Inv_currencyCode { get; set; }

        [Column("inv_exchangeRate")]
        public decimal? Inv_exchangeRate { get; set; }

        [Column("inv_adjustmentType")]
        public int Inv_adjustmentType { get; set; }

        [Column("inv_buyerDisplayName")]
        public string Inv_buyerDisplayName { get; set; }

        [Column("ma_dt")]
        public string Ma_dt { get; set; }

        [Column("inv_buyerLegalName")]
        public string Inv_buyerLegalName { get; set; }

        [Column("inv_buyerTaxCode")]
        public string Inv_buyerTaxCode { get; set; }

        [Column("inv_buyerAddressLine")]
        public string Inv_buyerAddressLine { get; set; }

        [Column("inv_buyerEmail")]
        public string Inv_buyerEmail { get; set; }

        [Column("trang_thai")]
        public string Trang_thai { get; set; }

        [Column("ma_ct")]
        public string Ma_ct { get; set; }

        [Column("inv_InvoiceCode_id")]
        public Guid? Inv_InvoiceCode_id { get; set; }

        [Column("so_benh_an")]
        public string So_benh_an { get; set; }

        [Column("inv_invoiceType")]
        public string Inv_invoiceType { get; set; }

        [Column("inv_invoiceName")]
        public string Inv_invoiceName { get; set; }

        [Column("inv_paymentMethodName")]
        public string Inv_paymentMethodName { get; set; }

        [Column("ma_dvcs")]
        public string Ma_dvcs { get; set; }

        [Column("nguoi_ky")]
        public string Nguoi_ky { get; set; }

        [Column("sobaomat")]
        public string SoBaoMat { get; set; }

        [Column("trang_thai_hd")]
        public int? Trang_thai_hd { get; set; }

        [Column("inv_originalId")]
        public Guid? Inv_originalId { get; set; }

        [Column("in_chuyen_doi")]
        public Boolean? In_chuyen_doi { get; set; }

        [Column("ngay_in_cdoi")]
        public DateTime? Ngay_in_cdoi { get; set; }

        [Column("nguoi_in_cdoi")]
        public string Nguoi_in_cdoi { get; set; }

        [Column("sovb")]
        public string Sovb { get; set; }

        [Column("ngayvb")]
        public DateTime? Ngayvb { get; set; }

        [Column("so_hd_dc")]
        public string So_hd_dc { get; set; }

        public string inv_additionalReferenceDes { get; set; }
        public DateTime? inv_additionalReferenceDate { get; set; }
        public string inv_originalInvoiceId { get; set; }

        public string ghi_chu { get; set; }

        [Column("inv_deliveryOrderNumber")]
        public string Inv_deliveryOrderNumber { get; set; }
        [Column("inv_deliveryOrderDate")]
        public DateTime? Inv_deliveryOrderDate { get; set; }
        [Column("inv_deliveryBy")]
        public string Inv_deliveryBy { get; set; }
        [Column("inv_transportationMethod")]
        public string Inv_transportationMethod { get; set; }
        [Column("inv_fromWarehouseName")]
        public string Inv_fromWarehouseName { get; set; }
        [Column("inv_toWarehouseName")]
        public string Inv_toWarehouseName { get; set; }
        [Column("inv_invoiceNote")]
        public string Inv_invoiceNote { get; set; }
        [Column("inv_contractNumber")]
        public string Inv_contractNumber { get; set; }
        [Column("inv_contractDate")]
        public DateTime? Inv_contractDate { get; set; }
        [Column("inv_sobangke")]
        public Guid? Inv_sobangke { get; set; }
        [Column("inv_ngaybangke")]
        public DateTime? Inv_ngaybangke { get; set; }

        [NotMapped]
        public string Inv_sellerLegalName { get; set; }
        [NotMapped]
        public string Inv_sellerTaxCode { get; set; }
        [NotMapped]
        public string Inv_sellerAddressLine { get; set; }
        [NotMapped]
        public string Inv_sellerEmail { get; set; }

        public string user_new { get; set; }
        public DateTime? date_new { get; set; }
        public string user_edit { get; set; }
        public DateTime? date_edit { get; set; }
    }
}