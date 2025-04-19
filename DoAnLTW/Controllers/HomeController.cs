using System.Diagnostics;
using System.Linq;
using DoAnLTW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DoAnLTW.Models.Repositories;

namespace DoAnLTW.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IProductRepository productRepository)
        {
            _logger = logger;
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            SetCartCount();
            // Lấy danh sách danh mục
            var categories = _context.Categories
       .Include(c => c.Products) // Load danh sách sản phẩm của từng danh mục
       .ToList();


            // Lấy danh sách sản phẩm
            var products = _context.Products
                .Include(p => p.Images) // Lấy hình ảnh
                .Select(p => new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Brand = p.Brand,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    ImageUrl = _context.ProductImages
                        .Where(img => img.ProductId == p.Id)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                })
                .ToList();
            // Lấy 4 sản phẩm mới nhất (Id giảm dần)
            var recentProducts = _context.Products
                .Include(p => p.Images)
                .OrderByDescending(p => p.Id) // Sắp xếp theo ID giảm dần
                .Take(4) // Chỉ lấy 4 sản phẩm mới nhất
                .Select(p => new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Brand = p.Brand,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    ImageUrl = _context.ProductImages
                        .Where(img => img.ProductId == p.Id)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                })
                .ToList();
            // Tạo model ViewModel để truyền cả hai danh sách vào View
            var viewModel = new HomeViewModel
            {
                Categories = categories,
                Products = products,
                RecentProducts = recentProducts
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            SetCartCount();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
