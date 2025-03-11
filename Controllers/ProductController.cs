using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreIRPCAPI.Data;
using StoreIRPCAPI.Models;

namespace StoreIRPCAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // สร้าง Constructor รับค่า ApplicationDbContext
    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ทดสอบเขียนฟังก์ชันการเชื่อมต่อ database
    // GET: /api/Product/testconnectdb
    [HttpGet("testconnectdb")]
    public void TestConnection()
    {
        // ถ้าเชื่อมต่อได้จะแสดงข้อความ "Connected"
        if (_context.Database.CanConnect())
        {
            Response.WriteAsync("Connected");
        }
        // ถ้าเชื่อมต่อไม่ได้จะแสดงข้อความ "Not Connected"
        else
        {
            Response.WriteAsync("Not Connected");
        }
    }

    // ฟังก์ชันสำหรับการดึงข้อมูลสินค้าทั้งหมด
    // GET: /api/Product
    [HttpGet]
    public ActionResult<product> GetProducts([FromQuery] string? pname, [FromQuery] decimal? pprice)
    {
        // LINQ is a query language for C# and .NET
        // LINQ สำหรับการดึงข้อมูลจากตาราง Products ทั้งหมด
        // var query = _context.products;

        // แบบอ่านที่มีเงื่อนไข คือ ราคาสินค้ามากกว่า 45000
        // var query = _context.products.Where(p => p.unit_price > 45000);

        // มากกว่า 1 เงื่อนไข คือ ราคาสินค้ามากกว่า 45000 และน้อยกว่า 50000
        // var query = _context.products.Where(p => p.unit_price > 45000 && p.unit_price < 50000);

        // แบบเชื่อมกับตารางอื่น products เชื่อมกับ categories
        var query = _context.products
            .Join(
                _context.categories,
                p => p.category_id,
                c => c.category_id,
                (p, c) => new
                {
                    p.product_id,
                    p.product_name,
                    p.unit_price,
                    p.unit_in_stock,
                    c.category_name
                }
            );
        
        // กรณีมีการค้นหาข้อมูล มากกว่า 1 เงื่อนไข เช่น จากชื่อสินค้า (pname) หรือ ราคา (pprice)
        // ค้นหาแบบ OR (เจอชื่อหรือราคาอย่างใดอย่างหนึ่ง)

        if(!string.IsNullOrEmpty(pname) || pprice > 0)
        {
            query = query.Where(p => 
                (!string.IsNullOrEmpty(pname) && EF.Functions.ILike(p.product_name!, $"%{pname}%")) || 
                (pprice > 0 && p.unit_price == pprice)
            );
        }

        // เรียงมูลสินค้าจากไอดีสินค้ามากไปน้อย
        query = query.OrderByDescending(p => p.product_id);

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(query);
    }

    // ฟังก์ชันสำหรับการดึงข้อมูลสินค้าตาม id
    // GET: /api/Product/{id}
    [HttpGet("{id}")]
    public ActionResult<product> GetProduct(int id)
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Products ตาม id
        // FirstOrDefault คือการดึงข้อมูลที่เจอค่าแรกที่ตรงเงื่อนไข หรือถ้าไม่เจอจะคืนค่า null
        var product = _context.products.FirstOrDefault(p => p.product_id == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (product == null)
        {
            return NotFound();
        }

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(product);
    }

    // ฟังก์ชันสำหรับการเพิ่มข้อมูลสินค้า
    // POST: /api/Product
    [HttpPost]
    public ActionResult<product> CreateProduct(product product)
    {
        // เพิ่มข้อมูลลงในตาราง Products
        _context.products.Add(product);
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(product);
    }

    // ฟังก์ชันสำหรับการแก้ไขข้อมูลสินค้า
    // PUT: /api/Product/{id}
    [HttpPut("{id}")]
    public ActionResult<product> UpdateProduct(int id, product product)
    {
        // ดึงข้อมูลสินค้าตาม id
        var existingProduct = _context.products.FirstOrDefault(p => p.product_id == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (existingProduct == null)
        {
            return NotFound();
        }

        // แก้ไขข้อมูลสินค้า
        existingProduct.product_name = product.product_name;
        existingProduct.unit_price = product.unit_price;
        existingProduct.unit_in_stock = product.unit_in_stock;
        existingProduct.category_id = product.category_id;

        // บันทึกข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(existingProduct);
    }

    // ฟังก์ชันสำหรับการลบข้อมูลสินค้า
    // DELETE: /api/Product/{id}
    [HttpDelete("{id}")]
    public ActionResult<product> DeleteProduct(int id)
    {
        // ดึงข้อมูลสินค้าตาม id
        var product = _context.products.FirstOrDefault(p => p.product_id == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (product == null)
        {
            return NotFound();
        }

        // ลบข้อมูล
        _context.products.Remove(product);
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปให้ผู้ใช้
        return Ok(product);
    }

}