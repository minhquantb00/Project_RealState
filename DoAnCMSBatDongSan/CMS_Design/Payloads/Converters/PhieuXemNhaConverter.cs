using CMS_Design.Entities;
using CMS_Design.Payloads.DTOs.DataResponsePhieuXemNha;
using CMS_Design.Payloads.DTOs.DataResponseProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Design.Payloads.Converters
{
    public class PhieuXemNhaConverter
    {
        public PhieuXemNhaDTO EntityToDTO(PhieuXemNha phieuXemNha)
        {
            return new PhieuXemNhaDTO
            {
                Id = phieuXemNha.Id,
                CreateTime = phieuXemNha.CreateTime,
                NhaId = phieuXemNha.NhaId,
                CustumerName = phieuXemNha.CustumerName,
                CustumerPhoneNumber = phieuXemNha.CustumerPhoneNumber,
                Desciption = phieuXemNha.Desciption,
                CustumerId = phieuXemNha.CustumerId,
                CustumerIdImg1 = phieuXemNha.CustumerIdImg1,
                CustumerIdImg2 = phieuXemNha.CustumerIdImg2,
                BanThanhCong = phieuXemNha.BanThanhCong,
                Product = new ProductDTO
                {
                    HostName = phieuXemNha.Nha.HostName,
                    HostPhoneNumber = phieuXemNha.Nha.HostPhoneNumber,
                    Build = phieuXemNha.Nha.Build,
                    CertificateOfLand1 = phieuXemNha.Nha.CertificateOfLand1,
                    CertificateOfLand2 = phieuXemNha.Nha.CertificateOfLand2,
                    Address = phieuXemNha.Nha.Address,
                    StatusId = phieuXemNha.Nha.StatusId,
                    DauChuId = phieuXemNha.Nha.DauChuId,
                    BatDauBan = phieuXemNha.Nha.BatDauBan,
                    GiaBan = phieuXemNha.Nha.GiaBan,
                }
            };
        }
    }
}
