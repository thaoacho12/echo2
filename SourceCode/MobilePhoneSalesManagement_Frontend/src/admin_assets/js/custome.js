// Lấy URL hiện tại
const currentPath = window.location.pathname;

// Tìm tất cả các liên kết
const menuItems = document.querySelectorAll("#menu a");

menuItems.forEach((item) => {
  if (item.getAttribute("href") === currentPath) {
    // Thêm class 'active' vào liên kết hiện tại
    item.classList.add("active");
  } else {
    // Xóa class 'active' khỏi các liên kết không phù hợp
    item.classList.remove("active");
  }
});
