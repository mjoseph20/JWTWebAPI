using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JWTResourceServer.Models;

namespace JWTResourceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private static readonly List<Product> Products = new List<Product> 
        {
            new Product { Id = 1, Name = "Product A", Price = 10.0M, Description = "Test Product A" },
            new Product { Id = 2, Name = "Product B", Price = 20.0M, Description = "Test Product B" },
            new Product { Id = 3, Name = "Product C", Price = 30.0M, Description = "Test Product C" }
        };

        private static int _nextId = 4;

        [HttpGet("GetAll")]
        public ActionResult<List<Product>> GetAllProduct()
        { 
            return Ok(Products);
        }

        [HttpGet("GetById/{id}", Name = "GetProductById")]
        public ActionResult<Product> GetProductById(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            { 
                return NotFound(new { message = $"Product with ID {id} not found." });
            }
            return Ok(product);
        }

        [HttpPost("Add")]
        public IActionResult AddProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            product.Id = _nextId++;
            Products.Add(product);

            return CreatedAtRoute("GetProductById", new { id = product.Id }, product);
        }

        [HttpPut("Update/{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProduct = Products.FirstOrDefault(p => p.Id == id);

            if (existingProduct == null)
            { 
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            existingProduct.Name = updatedProduct.Name;
            existingProduct.Description = updatedProduct.Description;
            existingProduct.Price = updatedProduct.Price;

            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            Products.Remove(product);

            return NoContent();
        }
    }
}
