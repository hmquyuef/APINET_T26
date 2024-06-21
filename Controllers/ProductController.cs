using APINET_T26.Models.Entities;
using APINET_T26.Models.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APINET_T26.Controllers
{
    //[Route("api/[controller]")] //api/Product
    [Route("api/products")] //api/products
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly APINETT26DbContext _context;
        public ProductController(APINETT26DbContext context)
        {
            _context = context;
        }

        #region GET: api/products
        [HttpGet]
        public IActionResult GetProductList(string filter = "")
        {
            try
            {
                var items = _context.Products
                    .Where(x => x.Filter.ToLower().Contains(filter.ToLower()))
                    .Select(x => new OutputProduct
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Price = x.Price,
                        Description = x.Description
                    })
                    .ToList();
                return StatusCode(StatusCodes.Status200OK, items);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.ToString());
            }
        }
        #endregion

        #region GET: api/products/{categoryId}/{id}
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
