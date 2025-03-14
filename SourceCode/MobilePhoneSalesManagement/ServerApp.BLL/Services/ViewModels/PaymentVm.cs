namespace ServerApp.BLL.Services.ViewModels
{
    public class PaymentVm
    {
        public int UserId { get; set; }  // ID của người dùng (có thể lấy từ Claims)
        public int OrderId { get; set; }  // ID của đơn hàng
        public string PaymentMethod { get; set; }  // Phương thức thanh toán (e.g., "Credit Card", "Paypal")

        // Thông tin thẻ tín dụng (nếu thanh toán qua thẻ tín dụng)
        public string CardNumber { get; set; }  // Số thẻ tín dụng
        public string CardExpiry { get; set; }  // Ngày hết hạn thẻ (MM/YY)
        public string CardCvc { get; set; }  // Mã CVV của thẻ

        // Thông tin ví điện tử (nếu thanh toán qua ví điện tử)
        public string PayPalEmail { get; set; }  // Email PayPal (nếu dùng PayPal)

        // Địa chỉ giao hàng
        public string ShippingAddress { get; set; }  // Địa chỉ giao hàng
        public string ShippingCity { get; set; }  // Thành phố giao hàng
        public string ShippingState { get; set; }  // Tỉnh/Thành phố
        public string ShippingZipCode { get; set; }  // Mã bưu điện
        public string ShippingCountry { get; set; }  // Quốc gia giao hàng

        // Các thông tin khác (nếu có)
        public decimal TotalAmount { get; set; }  // Tổng số tiền cần thanh toán
    }
}
