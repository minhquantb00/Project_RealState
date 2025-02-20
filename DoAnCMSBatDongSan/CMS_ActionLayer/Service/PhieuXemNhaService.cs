﻿using CMS_Design.Entities;
using CMS_Design.Handler.HandleImage;
using CMS_Design.Handler.HandlePagination;
using CMS_Design.IService;
using CMS_Design.Payloads.Converters;
using CMS_Design.Payloads.DTOs.DataResponsePhieuXemNha;
using CMS_Design.Payloads.Requests.InputRequests;
using CMS_Design.Payloads.Requests.PhieuXemNhaRequest;
using CMS_Design.Payloads.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CMS_ActionLayer.Service
{
    public class PhieuXemNhaService : BaseService, IPhieuXemNhaService
    {
        private readonly ResponseObject<PhieuXemNhaDTO> _responseObject;
        private readonly PhieuXemNhaConverter _phieuXemNhaConverter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PhieuXemNhaService(ResponseObject<PhieuXemNhaDTO> responseObject, PhieuXemNhaConverter phieuXemNhaConverter, IHttpContextAccessor httpContextAccessor)
        {
            _responseObject = responseObject;
            _phieuXemNhaConverter = phieuXemNhaConverter;
            _httpContextAccessor = httpContextAccessor;
        }
        #region CRUD phiếu xem nhà

        public async Task<ResponseObject<PhieuXemNhaDTO>> CreatePhieuXemNha(int nhaId, int nhanVienId, Request_CreatePhieuXemNha request)
        {
            var product = await _context.products.SingleOrDefaultAsync(x => x.Id == nhaId);
            var nhanVien = await _context.users.SingleOrDefaultAsync(x => x.Id == nhanVienId);
            if (product == null)
            {
                return _responseObject.ResponseError(StatusCodes.Status404NotFound, "Sản phẩm không tồn tại", null);
            }
            if (nhanVien == null)
            {
                return _responseObject.ResponseError(StatusCodes.Status404NotFound, "Nhân viên không tồn tại", null);
            }
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Staff"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            PhieuXemNha phieuXemNha = new PhieuXemNha();
            phieuXemNha.BanThanhCong = false;
            phieuXemNha.NhanVienId = nhanVienId;
            phieuXemNha.NhaId = nhaId;
            phieuXemNha.CreateTime = DateTime.Now;
            phieuXemNha.CustumerId = request.CustumerId;
            phieuXemNha.CustumerIdImg1 = await HandleUploadImage.Upfile(request.CustumerIdImg1);
            phieuXemNha.CustumerIdImg2 = await HandleUploadImage.Upfile(request.CustumerIdImg2);
            phieuXemNha.CustumerPhoneNumber = request.CustumerPhoneNumber;
            phieuXemNha.Desciption = request.Desciption;
            phieuXemNha.CustumerName = request.CustumerName;
            await _context.phieuXemNhas.AddAsync(phieuXemNha);
            await _context.SaveChangesAsync();
            return _responseObject.ResponseSuccess("Tạo phiếu xem nhà thành công", _phieuXemNhaConverter.EntityToDTO(phieuXemNha));
        }
        public async Task<ResponseObject<PhieuXemNhaDTO>> UpdatePhieuXemNha(Request_UpdatePhieuXemNha request)
        {
            var phieuXemNha = await _context.phieuXemNhas.SingleOrDefaultAsync(x => x.Id == request.PhieuXemNhaId);
            if (phieuXemNha is null)
            {
                return _responseObject.ResponseError(StatusCodes.Status404NotFound, "Phiếu xem nhà không tồn tại", null);
            }
            if (_context.phieuXemNhas.Any(x => x.NhaId == phieuXemNha.NhaId && x.BanThanhCong))
            {
                return _responseObject.ResponseError(StatusCodes.Status404NotFound, "Bất động sản đã được bán", null);
            }
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Staff"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }

            var product = _context.products.FirstOrDefault(x => x.Id == phieuXemNha.NhaId);

            phieuXemNha.Desciption = request.Desciption;
            phieuXemNha.CustumerPhoneNumber = request.CustumerPhoneNumber;
            phieuXemNha.CustumerId = request.CustumerId;
            phieuXemNha.CustumerName = request.CustumerName;
            phieuXemNha.BanThanhCong = request.BanThanhCong;
            _context.phieuXemNhas.Update(phieuXemNha);

            if (request.BanThanhCong)
            {
                product.StatusId = 1;
                _context.products.Update(product);

                var lstPhieuXemNhaUpdate = _context.phieuXemNhas.Where(x => !x.BanThanhCong && x.Id != phieuXemNha.Id && x.NhaId == phieuXemNha.NhaId).ToList();
                lstPhieuXemNhaUpdate.ForEach(x => x.IsActive = false);
                _context.phieuXemNhas.UpdateRange(lstPhieuXemNhaUpdate);
            }

            await _context.SaveChangesAsync();
            return _responseObject.ResponseSuccess("Cập nhật thông tin phiếu xem nhà thành công", _phieuXemNhaConverter.EntityToDTO(phieuXemNha));
        }
        public async Task<string> DeletePhieuXemNha(int phieuXemNhaId)
        {
            var phieuXemNha = await _context.phieuXemNhas.SingleOrDefaultAsync(x => x.Id == phieuXemNhaId);
            if (phieuXemNha is null)
            {
                return "Phiếu xem nhà không tồn tại";
            }
            var currentUser = _httpContextAccessor.HttpContext.User;


            if (!currentUser.Identity.IsAuthenticated)
            {
                return "Người dùng không được xác thực hoặc không được xác định";
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Staff"))
            {
                return "Người dùng không có quyền sử dụng chức năng này";
            }
            _context.phieuXemNhas.Remove(phieuXemNha);
            await _context.SaveChangesAsync();
            return "Xóa phiếu xem nhà thành công";
        }

        public async Task<PageResult<PhieuXemNhaDTO>> GetPhieuXemNhaByBanThanhCong(bool? banThanhCong, int pageSize = 10, int pageNumber = 1)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new NotImplementedException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Owner"))
            {
                throw new NotImplementedException("Người dùng không có quyền sử dụng chức năng này");
            }
            var phieuXemNhaQuery = _context.phieuXemNhas.Where(x => x.BanThanhCong == banThanhCong).Select(x => _phieuXemNhaConverter.EntityToDTO(x));
            var result = Pagination.GetPagedData<PhieuXemNhaDTO>(phieuXemNhaQuery, pageSize, pageNumber);
            return result;
        }

        public async Task<PageResult<PhieuXemNhaDTO>> GetPhieuXemNhaByCustomerName(string? name, int pageSize = 10, int pageNumber = 1)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new NotImplementedException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Owner"))
            {
                throw new NotImplementedException("Người dùng không có quyền sử dụng chức năng này");
            }
            var phieuXemNhaQuery = _context.phieuXemNhas.Where(x => ChuanHoaChuoi(x.CustumerName).Contains(ChuanHoaChuoi(name))).Select(x => _phieuXemNhaConverter.EntityToDTO(x));
            var result = Pagination.GetPagedData<PhieuXemNhaDTO>(phieuXemNhaQuery, pageSize, pageNumber);
            return result;
        }
        private string ChuanHoaChuoi(string str)
        {
            str = str.ToLower().Trim();
            while (str.Contains("  "))
            {
                str = str.Replace("  ", " ");
            }
            return str;
        }
        public async Task<PageResult<PhieuXemNhaDTO>> GetAllPhieuXemNha(FilterDataPhieuXemNha filterData, int pageSize = 10, int pageNumber = 1)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new NotImplementedException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Owner"))
            {
                throw new NotImplementedException("Người dùng không có quyền sử dụng chức năng này");
            }
            var phieuXemNhaQuery = _context.phieuXemNhas.Include(x => x.Nha).Where(x => x.IsActive).AsQueryable();
            if (filterData.FromDate.HasValue)
            {
                phieuXemNhaQuery = phieuXemNhaQuery.Where(x => x.CreateTime >= filterData.FromDate);
            }
            if (filterData.ToDate.HasValue)
            {
                phieuXemNhaQuery = phieuXemNhaQuery.Where(x => x.CreateTime <= filterData.ToDate);
            }
            if (filterData.IsSold.HasValue)
            {
                phieuXemNhaQuery = phieuXemNhaQuery.Where(x => x.BanThanhCong == filterData.IsSold);
            }
            if (!string.IsNullOrEmpty(filterData.Keyword))
            {
                phieuXemNhaQuery = phieuXemNhaQuery.Where(x => x.CustumerName.Trim().ToLower().Contains(filterData.Keyword.Trim().ToLower()));
            }

            var dsPhieuXemNha = phieuXemNhaQuery.Select(x => _phieuXemNhaConverter.EntityToDTO(x));
            var result = Pagination.GetPagedData<PhieuXemNhaDTO>(dsPhieuXemNha, pageSize, pageNumber);
            return result;
        }

        public async Task<PageResult<PhieuXemNhaDTO>> FilterPhieuXemNhaByKeyword(FilterDataPhieuXemNha filterData, int pageSize, int pageNumber)
        {
            var query = await _context.phieuXemNhas.Include(x => x.Nha).AsNoTracking().OrderByDescending(x => x.CreateTime).ToListAsync();
            if (filterData.FromDate.HasValue && filterData.ToDate.HasValue)
            {
                query = query.Where(x => x.CreateTime >= filterData.FromDate && x.CreateTime <= filterData.ToDate).ToList();
            }
            if (!string.IsNullOrEmpty(filterData.Keyword))
            {
                query = query.Where(x => x.CustumerName.Trim().ToLower().Contains(filterData.Keyword.Trim().ToLower())).ToList();
            }

            var data = query.Select(x => _phieuXemNhaConverter.EntityToDTO(x)).AsQueryable();
            var result = Pagination.GetPagedData(data, pageSize, pageNumber);
            return result;
        }
        #endregion
    }
}
