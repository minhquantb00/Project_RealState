using CMS_Design.Handler.HandlePagination;
using CMS_Design.Payloads.DTOs.DataResponsePhieuXemNha;
using CMS_Design.Payloads.Requests.InputRequests;
using CMS_Design.Payloads.Requests.PhieuXemNhaRequest;
using CMS_Design.Payloads.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.IService
{
    public interface IPhieuXemNhaService
    {
        Task<ResponseObject<PhieuXemNhaDTO>> CreatePhieuXemNha(int nhaId, int nhanVienId, Request_CreatePhieuXemNha request);
        Task<ResponseObject<PhieuXemNhaDTO>> UpdatePhieuXemNha(Request_UpdatePhieuXemNha request);
        Task<string> DeletePhieuXemNha(int phieuXemNhaId);
        Task<PageResult<PhieuXemNhaDTO>> GetPhieuXemNhaByCustomerName(string? name, int pageSize = 10, int pageNumber = 1);
        Task<PageResult<PhieuXemNhaDTO>> GetPhieuXemNhaByBanThanhCong(bool? banThanhCong, int pageSize = 10, int pageNumber = 1);
        Task<PageResult<PhieuXemNhaDTO>> GetAllPhieuXemNha(FilterDataPhieuXemNha filterData, int pageSize = 10, int pageNumber = 1);
        Task<PageResult<PhieuXemNhaDTO>> FilterPhieuXemNhaByKeyword(FilterDataPhieuXemNha filterData, int pageSize, int pageNumber);
    }
}


