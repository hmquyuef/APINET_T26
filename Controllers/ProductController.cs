using APINET_T26.Models;
using APINET_T26.Models.Entities;
using APINET_T26.Models.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace APINET_T26.Controllers
{
    //[Route("api/[controller]")] //api/Product
    [Route("api/v{version:apiVersion}/products")] //api/v1/products
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Product - Sản phẩm phiên bản 1.0")]
    //[Authorize]
    public class ProductController : ControllerBase
    {
        #region Constructor
        private readonly APINETT26DbContext _context;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private static string cacheGetProductsKey = "ProductController";
        private readonly ILogger<ProductController> _logger;
        public ProductController(APINETT26DbContext context, 
            IMemoryCache memory,
            ILogger<ProductController> logger)
        {
            _context = context;
            _cache = memory;
            _cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
            _logger = logger;
        }
        #endregion

        #region GET: api/products
        /// <summary>
        /// Lấy danh sách sản phẩm. Roles: Admin, User
        /// </summary>
        /// <remarks>
        /// Sử dụng Api với các tham số: IsActive, Filter, ...
        /// </remarks>
        /// <param name="filter"></param>
        /// <response code="200">Trả về danh sách sản phẩm</response>
        /// <response code="400">Thiếu thông tin</response>
        /// <response code="404">Không tim thấy thông tin</response>
        /// <response code="500">Lỗi hệ thống</response>
        [HttpGet]
        public ResponseDefault<List<OutputProduct>> GetProductList(string filter = "")
        {
            if (!_cache.TryGetValue(cacheGetProductsKey, out ResponseDefault<List<OutputProduct>> cacheResult))
            {
                var items = _context.Products
                    .Where(x => x.Filter.ToLower().Contains(filter.ToLower()))
                    .OrderBy(x => x.Name)
                    .Select(x => new OutputProduct
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Price = x.Price,
                        Description = x.Description
                    })
                    //.Skip(0).Take(10) //Phân trang dữ liệu
                    .ToList();

                var response = new ResponseDefault<List<OutputProduct>>
                {
                    Items = items,
                    Filter = filter
                };

                _cache.Set(cacheGetProductsKey, response, _cacheOptions);
                _logger.LogInformation("Get data from SQL SERVER: GetProducts");
                return response;
            }
            else
            {
                _logger.LogWarning("Get data from CACHE: GetProducts");
                return cacheResult;
            }
            //try
            //{
            //    //mô tả
            //    var items = _context.Products
            //        .Where(x => x.Filter.ToLower().Contains(filter.ToLower()))
            //        .Select(x => new OutputProduct
            //        {
            //            Id = x.Id,
            //            Name = x.Name,
            //            Price = x.Price,
            //            Description = x.Description
            //        })
            //        .ToList();
            //    return StatusCode(StatusCodes.Status200OK, items);
            //}
            //catch (Exception e)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError, e.ToString());
            //}
        }
        #endregion

        #region GET: api/products/{categoryId}/{id}
        /// <summary>
        /// Lấy danh sách sản phẩm. Roles: Admin, User
        /// </summary>
        /// <remarks>
        /// Sử dụng Api với các tham số: IsActive, Filter, ...
        /// </remarks>
        /// <param name="filter"></param>
        /// <response code="200">Trả về danh sách sản phẩm</response>
        /// <response code="400">Thiếu thông tin</response>
        /// <response code="404">Không tim thấy thông tin</response>
        /// <response code="500">Lỗi hệ thống</response>
        [HttpGet("/{categoryId}/{id}")]
        public IActionResult GetProduct(Guid categoryId, Guid id)
        {
            try
            {
                var item = _context.Products.FirstOrDefault(x => x.Id == id);
                if (item == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
                return StatusCode(StatusCodes.Status200OK, item);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.ToString());
            }
        }
        #endregion

        #region POST: api/products
        /// <summary>
        /// Lấy danh sách sản phẩm. Roles: Admin, User
        /// </summary>
        /// <remarks>
        /// Sử dụng Api với các tham số: IsActive, Filter, ...
        /// </remarks>
        /// <param name="filter"></param>
        /// <response code="200">Trả về danh sách sản phẩm</response>
        /// <response code="400">Thiếu thông tin</response>
        /// <response code="404">Không tim thấy thông tin</response>
        /// <response code="500">Lỗi hệ thống</response>
        [HttpPost]
        public IActionResult AddProduct(InputProduct input)
        {
            try
            {
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = input.Name,
                    Price = input.Price,
                    Description = input.Description ?? string.Empty,
                    Filter = input.Name + " " + input.Name.ToLower() + input.Description + " " + input.Description.ToLower()
                };
                _context.Products.Add(product);
                _context.SaveChanges();
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.ToString());
            }
        }
        #endregion

        #region PUT: api/products/{id}
        /// <summary>
        /// Lấy danh sách sản phẩm. Roles: Admin, User
        /// </summary>
        /// <remarks>
        /// Sử dụng Api với các tham số: IsActive, Filter, ...
        /// </remarks>
        /// <param name="filter"></param>
        /// <response code="200">Trả về danh sách sản phẩm</response>
        /// <response code="400">Thiếu thông tin</response>
        /// <response code="404">Không tim thấy thông tin</response>
        /// <response code="500">Lỗi hệ thống</response>
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(Guid id, UpdateProduct input)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(x => x.Id == id);
                if (product == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
                product.Price = input.Price;
                _context.Products.Update(product);
                _context.SaveChanges();
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.ToString());
            }
        }
        #endregion

        #region DELETE: api/products/{id}
        /// <summary>
        /// Lấy danh sách sản phẩm. Roles: Admin, User
        /// </summary>
        /// <remarks>
        /// Sử dụng Api với các tham số: IsActive, Filter, ...
        /// </remarks>
        /// <param name="filter"></param>
        /// <response code="200">Trả về danh sách sản phẩm</response>
        /// <response code="400">Thiếu thông tin</response>
        /// <response code="404">Không tim thấy thông tin</response>
        /// <response code="500">Lỗi hệ thống</response>
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(Guid id) {
            try
            {
                var product = _context.Products.FirstOrDefault(x => x.Id == id);
                if (product == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
                _context.Products.Remove(product);
                _context.SaveChanges();
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.ToString());
            }
        }
        #endregion
    }
}
