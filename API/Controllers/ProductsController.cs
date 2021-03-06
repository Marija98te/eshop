using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Data;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces;
using Core.Specifications;
using API.Dtos;
using AutoMapper;
using API.Errors;
using Microsoft.AspNetCore.Http;
using API.Helpers;

namespace API.Controllers 
{

    public class ProductsController : BaseApiController
    {


        private readonly IGenericRepository<Product> _productsRepo;
        private readonly IGenericRepository<ProductBrand> _productBrandRepo;
        private readonly IGenericRepository<ProductGender> _productGenderRepo;
        private readonly IMapper _mapper;
        public ProductsController(IGenericRepository<Product> productsRepo, 
        IGenericRepository<ProductBrand> productBrandRepo, IGenericRepository<ProductGender> productGenderRepo, IMapper mapper)  //This is called when is retrieved http request
        {
            _mapper = mapper;
            _productsRepo = productsRepo;
            _productBrandRepo = productBrandRepo;
            _productGenderRepo = productGenderRepo;
        }

        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
            [FromQuery]ProductSpecParams productParams)  //This is the response
        {
            var spec = new ProductsWithBrandsAndGendersSpecification(productParams);

            var countSpec = new ProductWithFiltersForCountSpecification(productParams);

            var totalItems = await _productsRepo.CountAsync(countSpec);

            var products = await _productsRepo.ListAsync(spec);

            var data = _mapper
                .Map<IReadOnlyList<Product> , IReadOnlyList<ProductToReturnDto>>(products);

                return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex, 
                productParams.PageSize, totalItems, data));

            return Ok(_mapper
            .Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var spec = new ProductsWithBrandsAndGendersSpecification(id);

            var product = await _productsRepo.GetEntityWithSpec(spec);

            if (product == null) return NotFound(new ApiResponse(404));

            return _mapper.Map<Product, ProductToReturnDto>(product);
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands() 
        {
            return Ok(await _productBrandRepo.ListAllAsync());     
        }

        [HttpGet("genders")]
        public async Task<ActionResult<IReadOnlyList<ProductGender>>> GetProductGenders() 
        {
            return Ok(await _productGenderRepo.ListAllAsync());     
        }
    }
}