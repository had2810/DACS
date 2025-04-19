using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnLTW.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using DoAnLTW.Models.Repositories;

namespace DoAnLTW.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // 1. Danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                            .Include(p => p.Images)
                            .Include(p => p.Category)
                            .Include(p=>p.ProductSizes)
                                .ThenInclude(ps => ps.Size)
                            .Include(p=>p.Brand).ToListAsync();
            return View(products);
        }

        // 2. Xem chi tiết sản phẩm
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.Size)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return StatusCode(404, "Không tìm thấy sản phẩm"); // Trả về lỗi 404 kèm thông báo
            }


            return View(product);
        }


        // 3. Thêm sản phẩm - GET
        public async Task<IActionResult> Create()
        {
            //var products = await _context.Products.Include(p => p.Category).Include(p => p.Images).Include(p => p.Brand).ToListAsync();
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewBag.Brands = new SelectList(await _context.Brands.ToListAsync(), "Id", "Name");
            ViewBag.Sizes = _context.Sizes.ToList(); 
            return View();
        }

        // 4. Thêm sản phẩm - POST
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Product product, List<IFormFile> ImageFiles)
        //{

        //        // Truyền lại danh sách danh mục và thương hiệu nếu có lỗi
        //        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
        //        ViewBag.Brands = new SelectList(await _context.Brands.ToListAsync(), "Id", "Name");

        //    // 1️⃣ Thêm sản phẩm vào database
        //    _context.Products.Add(product);
        //    await _context.SaveChangesAsync();  // Lưu để lấy product.Id

        //    // 2️⃣ Xử lý lưu hình ảnh
        //    if (ImageFiles != null && ImageFiles.Count > 0)
        //    {
        //        var imageList = new List<Product_Images>();

        //        foreach (var image in ImageFiles)
        //        {
        //            string imageUrl = await SaveImage(image); // Hàm lưu ảnh và lấy URL

        //                imageList.Add(new Product_Images
        //                {
        //                    ProductId = product.Id,
        //                    ImageUrl = imageUrl
        //                });

        //        }

        //        // Nếu có ảnh, thêm vào database
        //        if (imageList.Count > 0)
        //        {
        //            _context.ProductImages.AddRange(imageList);
        //            await _context.SaveChangesAsync();
        //        }
        //    }

        //    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
        //    return RedirectToAction(nameof(Index));
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Product product)
        //{

        //        _context.Products.Add(product);
        //        await _context.SaveChangesAsync(); // Lưu để lấy product.Id

        //        // Chuyển hướng sang trang thêm ảnh, truyền theo ProductId
        //        return RedirectToAction("AddImages", new { id = product.Id });



        //}
        //public async Task<IActionResult> AddImages(int id)
        //{
        //    var product = await _context.Products.FindAsync(id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    ViewBag.ProductId = id; // Gửi ProductId sang view
        //    return View();
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<int> selectedSizes, List<int> sizeQuantities)
        {

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // Lưu sản phẩm vào database trước
            if (selectedSizes != null && sizeQuantities != null)
            {
                for (int i = 0; i < selectedSizes.Count; i++)
                {
                    var sizeId = selectedSizes[i];
                    var quantity = sizeQuantities[i];

                    _context.ProductSizes.Add(new ProductSize
                    {
                        ProductId = product.Id,
                        SizeId = sizeId,
                        Stock = quantity
                    });
                }
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công! Hãy thêm hình ảnh.";

            // Chuyển hướng đến trang AddImages và truyền ID của sản phẩm vừa tạo
            return RedirectToAction("AddImages", new { productId = product.Id });
        }
        public async Task<IActionResult> AddImages(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại!";
                return RedirectToAction("Index");
            }

            ViewBag.ProductId = productId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int ProductId, List<IFormFile> ImageFiles)
        {
            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                var imageList = new List<Product_Images>();

                foreach (var image in ImageFiles)
                {
                    string imageUrl = await SaveImage(image); // Lưu ảnh
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        imageList.Add(new Product_Images
                        {
                            ProductId = ProductId,
                            ImageUrl = imageUrl
                        });
                    }
                }

                if (imageList.Count > 0)
                {
                    _context.ProductImages.AddRange(imageList);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công!";
            return RedirectToAction("Index"); // Chuyển về danh sách sản phẩm
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/img/products/" + uniqueFileName; // Trả về đường dẫn ảnh
        }


        // 5. Sửa sản phẩm - GET
        public IActionResult Edit(int id)
        {
            var product = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.Size) 
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách thương hiệu & danh mục từ database
            ViewBag.BrandList = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.SizeList = new SelectList(_context.Sizes, "Id", "SizeName");
            return View(product);
        }


        // 6. Sửa sản phẩm - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> ImageFiles, List<int> selectedSizes, List<int> sizeQuantities)
        {
            if (id != product.Id)
                return NotFound();

           
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
                ViewBag.Brands = new SelectList(await _context.Brands.ToListAsync(), "Id", "Name", product.BrandId);
                ViewBag.SizeList = _context.Sizes.ToList();


            try
            {
                // Lấy dữ liệu sản phẩm cũ từ DB
                var existingProduct = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.ProductSizes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (existingProduct == null)
                    return NotFound();

                // Cập nhật thông tin sản phẩm (trừ ảnh và kích thước)
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.BrandId = product.BrandId;
                existingProduct.CategoryId = product.CategoryId;

                // 1️⃣ Xóa ảnh cũ nếu có ảnh mới
                if (ImageFiles != null && ImageFiles.Count > 0)
                {
                    // Xóa ảnh cũ trong thư mục
                    foreach (var img in existingProduct.Images)
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    // Xóa ảnh cũ trong DB
                    _context.ProductImages.RemoveRange(existingProduct.Images);

                    // Thêm ảnh mới
                    var imageList = new List<Product_Images>();
                    foreach (var image in ImageFiles)
                    {
                        string imageUrl = await SaveImage(image);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            imageList.Add(new Product_Images { ProductId = existingProduct.Id, ImageUrl = imageUrl });
                        }
                    }

                    _context.ProductImages.AddRange(imageList);
                }

                // 2️⃣ Cập nhật lại kích thước nếu có
                if (selectedSizes != null && sizeQuantities != null)
                {
                    // Xóa kích thước cũ
                    _context.ProductSizes.RemoveRange(existingProduct.ProductSizes);

                    // Thêm kích thước mới
                    for (int i = 0; i < selectedSizes.Count; i++)
                    {
                        _context.ProductSizes.Add(new ProductSize
                        {
                            ProductId = existingProduct.Id,
                            SizeId = selectedSizes[i],
                            Stock = sizeQuantities[i]
                        });
                    }
                }

                // 3️⃣ Lưu thay đổi
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật: " + ex.Message);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật sản phẩm!";
                return View(product);
            }
        }
        //xóa hình ảnh trong edit
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image != null)
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", image.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Edit", new { id = image.ProductId });
        }


        // 7. Xóa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Xóa ảnh liên quan
            foreach (var img in product.Images)
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }       
                _context.ProductImages.Remove(img);
            }

            // Xóa sản phẩm
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        
    }
}
