using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System;
using HotelLuxuryWeb.Models;
using Microsoft.EntityFrameworkCore; // Thêm dòng này

namespace HotelLuxuryWeb.Controllers
{
    public class AdminController : Controller
    {
        // 1. KẾT NỐI DATABASE THAY VÌ DÙNG LIST ẢO
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true") return RedirectToAction("Login");
            
            // Lấy dữ liệu thật từ SQL Server
            var bookings = _context.Bookings.ToList();

            ViewBag.TotalOrders = bookings.Count;
            ViewBag.TotalRevenue = bookings.Sum(c => c.Price);
            ViewBag.TotalCustomers = bookings.Select(c => c.CustomerName).Distinct().Count();

            int totalHotelRooms = 20;
            ViewBag.TotalRooms = totalHotelRooms;
            ViewBag.OccupiedRooms = bookings.Count; 
            ViewBag.AvailableRooms = totalHotelRooms - bookings.Count;

            return View(bookings);
        }

        // 2. XỬ LÝ ĐẶT PHÒNG (Lưu vào DB thật)
        [HttpPost]
        public IActionResult Confirm(string CustomerName, string Phone, string RoomName, string Price)
        {
            int roomPrice = 0;
            int.TryParse(Price, out roomPrice);

            if (roomPrice == 0) 
            {
                if (RoomName.Contains("Deluxe")) roomPrice = 1200000;
                else if (RoomName.Contains("President")) roomPrice = 5000000;
                else roomPrice = 850000;
            }

            string finalRoom = RoomName;
            if (!finalRoom.Contains("#"))
            {
                finalRoom += " #" + new Random().Next(100, 500);
            }

            // TẠO ĐỐI TƯỢNG MỚI
            var newBooking = new Booking { 
                CustomerName = CustomerName, 
                Phone = Phone, 
                RoomName = finalRoom, 
                Status = "Đã thanh toán", 
                Price = roomPrice 
            };

            // --- LỆNH LƯU VÀO DATABASE QUAN TRỌNG NHẤT ---
            _context.Bookings.Add(newBooking); 
            _context.SaveChanges(); // Không có dòng này là SQL không có gì đâu!
            // --------------------------------------------

            return RedirectToAction("Index", "Booking");
        }

        // 3. CẬP NHẬT (EDIT) - Lưu vào DB thật
        [HttpPost]
        public IActionResult Update(int id, string name, string phone, int price, string status)
        {
            var booking = _context.Bookings.Find(id);
            if (booking != null)
            {
                booking.CustomerName = name;
                booking.Phone = phone;
                booking.Price = price;
                booking.Status = status;

                _context.SaveChanges(); // Lưu thay đổi xuống SQL
            }
            return RedirectToAction("Dashboard");
        }

        // 4. XÓA (DELETE) - Xóa trong DB thật
        public IActionResult Delete(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                _context.SaveChanges(); // Lưu thay đổi xuống SQL
            }
            return RedirectToAction("Dashboard");
        }

        public IActionResult Edit(int id)
        {
            var customer = _context.Bookings.Find(id);
            if (customer == null) return RedirectToAction("Dashboard");
            return View(customer);
        }

        // --- GIỮ NGUYÊN PHẦN LOGIN/LOGOUT ---
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}