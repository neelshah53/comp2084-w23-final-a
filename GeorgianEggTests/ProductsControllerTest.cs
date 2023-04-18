using GeorgianEgg.Controllers;
using GeorgianEgg.Data;
using GeorgianEgg.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeorgianEggTests
{
    [TestClass]
    public class ProductsControllerTest
    {
        private ApplicationDbContext _context;
        private ProductsController _controller;

        private List<Product> _products = new List<Product>();
        private Category _category;
        private Brand _brand;

        [TestInitialize]
        public void TestInitialize()
        { 
            // Mock db
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(dbOptions);

            // Mock category
            _category = new Category
            {
                Id = 1000,
                Name = "Mock Category",
            };
            _context.Categories.Add(_category);

            // Mock brand
            _brand = new Brand
            {
                Id = 2000,
                Name = "Mock Brand",
            };
            _context.Brands.Add(_brand);

            // Mock products
            _products.Add(new Product
            {
                Id = 1,
                Name = "Mock Product 1",
                Price = 100,
                Rating = 1,
                
                CategoryId = _category.Id,
                Category = _category,

                BrandId = _brand.Id,
                Brand = _brand,
            });
            _products.Add(new Product
            {
                Id = 2,
                Name = "Mock Product 2",
                Price = 120,
                Rating = 3,
                
                CategoryId = _category.Id,
                Category = _category,

                BrandId = _brand.Id,
                Brand = _brand,
            });

            foreach (var product in _products)
            {
                _context.Products.Add(product);
	        }

            _context.SaveChanges();

            _controller = new ProductsController(_context);
	    }

        [TestMethod]
        public async Task IndexLoadsViewWithProducts()
        {
            var result = (ViewResult)await _controller.Index();

            CollectionAssert.AreEqual(
                _products.OrderBy(p => p.Name).ToList(),
                (List<Product>)result.Model
            );
        }


        // Tests for lab 4

        [TestMethod]
        public async Task CreateAddsToDb()
        {
            // Arrange
            var product = CreateSomeProduct();

            /*
            Assert.AreEqual(_products.Count(), _context.Products.Count());
            Assert.IsNull(_context.Products.Find(product.Id));
            */

            // Act
            var result = (RedirectToActionResult)await _controller.Create(product, null);

            // Assert
            Assert.AreEqual(_products.Count() + 1, _context.Products.Count());
            Assert.AreEqual(product, _context.Products.Find(product.Id));

            Assert.AreEqual(nameof(_controller.Index), result.ActionName);
        }

        [TestMethod]

        public void DeleteProduct_GET_RemoveIdFromProductController()
        {   var controller = new Product();

            var result = controller.Delete(ProductId);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CreateInvalidDoesntCreate()
        {
            // Arrange
            var product = CreateSomeProduct();

            // some error causes no creation
            _controller.ModelState.AddModelError("Name", "Invalid");

            // Act
            var result = (ViewResult) await _controller.Create(product, null);

            // Assert

            // checking that nothing was added to db
            Assert.AreEqual(_products.Count(), _context.Products.Count());
            Assert.IsNull(_context.Products.Find(product.Id));

            // check that select lists are filled out
            var brandIds = (SelectList) result.ViewData["BrandId"];
            var categoryIds = (SelectList) result.ViewData["CategoryId"];

            AssertSelectListAreEqual(
		        new SelectList(_context.Brands, "Id", "Name", product.BrandId),
		        brandIds
		    );
            AssertSelectListAreEqual(
		        new SelectList(_context.Categories, "Id", "Name", product.CategoryId),
		        categoryIds
		    );

            // check that we render default view
            Assert.IsNull(result.ViewName);
	    }

        [TestMethod]
        public void DeleteConfirmed_RemovesProductFromDatabase()
        {
            
        }

        [TestMethod]
        public async Task EditUpdatesProduct()
        {
            var productId = 1;

            var newProduct = CreateSomeProduct();
            newProduct.Id = productId;

            var result = (RedirectToActionResult) await _controller.Edit(
		        productId,
		        newProduct,
		        null,
		        null
		    );

            Assert.AreEqual(newProduct, _context.Products.Find(productId));
            Assert.AreEqual(nameof(_controller.Index), result.ActionName);
	    }
        
        [TestMethod]
        public async Task EditInvalidIdNotFound()
        {
            var productId = 100000000;

            var newProduct = CreateSomeProduct();
            newProduct.Id = productId;

            var result = (NotFoundResult) await _controller.Edit(
		        productId,
		        newProduct,
		        null,
		        null
		    );

            Assert.IsNotNull(result);
	    }
        
        [TestMethod]
        public async Task EditInvalidStateDoesntEdit()
        {
            var productId = 1;

            var newProduct = CreateSomeProduct();
            newProduct.Id = productId;

            _controller.ModelState.AddModelError("Name", "Invalid");

            var result = (ViewResult) await _controller.Edit(
		        productId,
		        newProduct,
		        null,
		        null
		    );

            var brandIds = (SelectList) result.ViewData["BrandId"];
            var categoryIds = (SelectList) result.ViewData["CategoryIds"];

            AssertSelectListAreEqual(new SelectList(_context.Brands, "Id", "Name", newProduct.BrandId), brandIds);
            AssertSelectListAreEqual(new SelectList(_context.Categories, "Id", "Name", newProduct.CategoryId), categoryIds);

            Assert.AreEqual(newProduct, result.Model);
            Assert.IsNull(result.ViewName);
	    }


        private Product CreateSomeProduct()
        { 
            return new Product()
            {
                Id = 10,
                Name = "Mock Product",

                Price = 1337,
                Rating = 5,
                
                CategoryId = _category.Id,
                Category = _category,

                BrandId = _brand.Id,
                Brand = _brand,
	        };
	    }

        // SelectList.Equal won't work on different instances of matching SelectLists
        // so we need our own assert method
        private void AssertSelectListAreEqual(SelectList expectedSelectList, SelectList actualSelectList)
        { 
            Assert.AreEqual(expectedSelectList.DataGroupField, actualSelectList.DataGroupField);
            Assert.AreEqual(expectedSelectList.DataTextField, actualSelectList.DataTextField);
            Assert.AreEqual(expectedSelectList.DataValueField, actualSelectList.DataValueField);
            Assert.AreEqual(expectedSelectList.Items, actualSelectList.Items);
            Assert.AreEqual(expectedSelectList.SelectedValue, actualSelectList.SelectedValue);
	    }
    }
}
