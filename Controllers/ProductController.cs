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
    public ActionResult<product> GetProducts([FromQuery] string pname, [FromQuery] decimal pprice)
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

        // ส่งข้อมูลกลับไปให้ผู้ใช้งาน
        return Ok(query);
    }

}