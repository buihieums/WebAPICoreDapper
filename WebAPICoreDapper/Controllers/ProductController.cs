using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebAPICoreDapper.Dtos;
using WebAPICoreDapper.Filter;
using WebAPICoreDapper.Models;


namespace WebAPICoreDapper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly string _connectionString;
        public ProductController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DbConnectionString");
        }
        // GET: api/<ProductController>
        [HttpGet]
        public async Task<IEnumerable<Product>>  Get()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if(conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                var result = await conn.QueryAsync<Product>("Get_All_Product", null, null,null,System.Data.CommandType.StoredProcedure);
                return result;
            }
        }
        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public async Task<Product> Get(int id)
        {
            using(var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@id", id);
                var result = await conn.QueryAsync<Product>("Get_Product_ById", paramaters, null, null, System.Data.CommandType.StoredProcedure);
                return result.Single();
            }
        }
        [HttpGet("paging")]
        public async Task<PagedResult<Product>> Get(string keyword, int pageIndex, int pageSize)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@keyword", keyword);
                paramaters.Add("@pageIndex", pageIndex);
                paramaters.Add("@pageSize", pageSize);
                paramaters.Add("@totalRow", dbType:System.Data.DbType.Int32,direction:System.Data.ParameterDirection.Output);
                var result = await conn.QueryAsync<Product>("Get_AllProduct_Paging", paramaters, null, null, System.Data.CommandType.StoredProcedure);
                return new PagedResult<Product>
                {
                    Items = result.ToList(),
                    TotalRow = paramaters.Get<int>("@totalRow"),
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
            }
        }
        // POST api/<ProductController>
        [ModelValidation]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            using(var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@sku", product.Sku);
                paramaters.Add("@price", product.Price);
                paramaters.Add("@discountPrice", product.DiscountPrice);
                paramaters.Add("@isActive", product.IsActive);
                paramaters.Add("@imageUrl", product.ImageUrl);
                paramaters.Add("@viewCount", product.ViewCount);
                paramaters.Add("@createdAt", product.CreatedAt);
                paramaters.Add("@id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
                await conn.ExecuteAsync("Create_New_Product", paramaters, null, null, System.Data.CommandType.StoredProcedure);
                var newId = paramaters.Get<int>("@id");
                return Ok(newId);
            }

        }
        // PUT api/<ProductController>/5
        [ModelValidation]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            using(var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@id", id);
                paramaters.Add("@sku", product.Sku);
                paramaters.Add("@price", product.Price);
                paramaters.Add("@discountPrice", product.DiscountPrice);
                paramaters.Add("@isActive", product.IsActive);
                paramaters.Add("@imageUrl", product.ImageUrl);
                paramaters.Add("@viewCount", product.ViewCount);
                paramaters.Add("@createdAt", product.CreatedAt);
                await conn.ExecuteAsync("Update_product", paramaters, null, null, System.Data.CommandType.StoredProcedure);
                return Ok();
            }
        }
        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            using(var conn = new SqlConnection(_connectionString))
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                var paramaters = new DynamicParameters();
                paramaters.Add("@id", id);
                await conn.ExecuteAsync("Delete_Product", paramaters, null, null, System.Data.CommandType.StoredProcedure);
            }
        }
    }
}
