using CMS_ActionLayer.DAO;
using CMS_Design.Entities;
using CMS_Design.Handler.HandleImage;
using CMS_Design.Handler.HandlePagination;
using CMS_Design.IService;
using CMS_Design.Payloads.Converters;
using CMS_Design.Payloads.DTOs.DataProductInformation;
using CMS_Design.Payloads.DTOs.DataResponseGeocoding;
using CMS_Design.Payloads.DTOs.DataResponseListProductSoldAndPrice;
using CMS_Design.Payloads.DTOs.DataResponseOfSalesAndProfitOfCompany;
using CMS_Design.Payloads.DTOs.DataResponseOfSalesAndProfitOfCompany.StatisticsByMonth;
using CMS_Design.Payloads.DTOs.DataResponseOfSalesAndProfitOfCompany.StatisticsByWeek;
using CMS_Design.Payloads.DTOs.DataResponseProduct;
using CMS_Design.Payloads.DTOs.DataResponseProductSoldStaffCommission;
using CMS_Design.Payloads.DTOs.DataResponseSalesAndNumberOfPassengers;
using CMS_Design.Payloads.DTOs.DataResponseSalesAndProfitOfTeam;
using CMS_Design.Payloads.DTOs.DataResponseSalesAndProfitOfTeam.StatisticsByMonth;
using CMS_Design.Payloads.DTOs.DataResponseSalesAndProfitOfTeam.StatisticsByWeek;
using CMS_Design.Payloads.DTOs.SalesRatio;
using CMS_Design.Payloads.DTOs.Statistics;
using CMS_Design.Payloads.DTOs.StatisticsSalesByTime.ByDay;
using CMS_Design.Payloads.DTOs.StatisticsSalesByTime.ByMonth;
using CMS_Design.Payloads.DTOs.StatisticsSalesByTime.ByWeek;
using CMS_Design.Payloads.Requests.InputRequests;
using CMS_Design.Payloads.Requests.ProductRequests;
using CMS_Design.Payloads.Responses;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CMS_ActionLayer.Service
{
    public class ProductService : BaseService, IProductService
    {
        private readonly ProductConverter _productConverter;
        private readonly ResponseObject<SalesStatisticsDTO> _responseObject;
        private readonly ResponseObject<ProductDTO> _responObjectProduct;
        private readonly PhieuXemNhaConverter _phieuXemNhaConverter;
        private readonly ProductImgConverter _productImgConverter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ResponseObject<IQueryable<ProductImg>> _responseObjectImage;
        private readonly ResponseObject<ProductImg> _responseObjectImg;
        public static string apiKey = "AIzaSyDWCoeuHmZroAazHpXfo_pEt2FAB4YF2Ss";
        public ProductService(ProductConverter productConverter, ResponseObject<SalesStatisticsDTO> responseObject, ResponseObject<ProductDTO> responObjectProduct, PhieuXemNhaConverter phieuXemNhaConverter, ProductImgConverter productImgConverter, IHttpContextAccessor httpContextAccessor, ResponseObject<IQueryable<ProductImg>> responseObjectImage, ResponseObject<ProductImg> responseObjectImg)
        {
            _productConverter = productConverter;
            _responseObject = responseObject;
            _responObjectProduct = responObjectProduct;
            _phieuXemNhaConverter = phieuXemNhaConverter;
            _productImgConverter = productImgConverter;
            _httpContextAccessor = httpContextAccessor;
            _responseObjectImage = responseObjectImage;
            _responseObjectImg = responseObjectImg;
        }
        #region Xem thống kê bất động sản của các đầu chủ phòng ban

        public async Task<IQueryable<ProductStatisticsDTO>> GetStatisticalsProductOfManager(int pageSize, int pageNumber)
        {
            var productQuery = await _context.products.Where(x => x.DauChu.RoleId == 3 && x.StatusId == 2)
                                                    .Include(x => x.ProductImgs)
                                                    .Include(x => x.DauChu)
                                                    .Include(x => x.PhieuXemNhas).AsNoTracking()
                                                    .ToListAsync();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod") && !currentUser.IsInRole("Owner"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            var productStatistics = new List<ProductStatisticsDTO>();
            foreach (var product in productQuery)
            {
                var owner = product.DauChu;
                int numOfProduct = productQuery.Count(x => x.DauChuId == owner.Id && x.StatusId == 2 && x.Id == product.Id);
                var productStatisticsItem = new ProductStatisticsDTO
                {
                    OwnerId = owner.Id,
                    NumberOfProduct = numOfProduct
                };
                var productImgs = product.ProductImgs.ToList();
                var productDTOs = productQuery.Where(x => x.Id == product.Id).Select(x => _productConverter.EntityToDTO(x)).ToList();

                foreach (var productImg in productImgs)
                {
                    var productImgDTO = _productImgConverter.EntityToDTO(productImg);
                    if (productImgDTO != null)
                    {
                        var listProductImgDTO = product.ProductImgs.Where(x => x.ProductId == product.Id).Select(x => _productImgConverter.EntityToDTO(x)).ToList();
                        productDTOs.ForEach(x =>
                        {
                            x.ProductImgDTOs = listProductImgDTO.AsQueryable();
                        });
                    }
                }
                productStatisticsItem.ProductDTOs = productDTOs.AsQueryable();
                productStatistics.Add(productStatisticsItem);
            }

            var groupStatistics = productStatistics.GroupBy(x => x.OwnerId).Select(x => new ProductStatisticsDTO
            {
                NumberOfProduct = x.Sum(y => y.NumberOfProduct),
                OwnerId = x.Key,
                ProductDTOs = x.SelectMany(p => p.ProductDTOs).AsQueryable()
            }).AsQueryable().AsNoTracking();

            return groupStatistics.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        #endregion
        #region Doanh số và lợi nhuận của công ty theo tuần và tháng
        public async Task<IQueryable<SalesAndProfitOfCompanyByMonth>> SalesStatisticsOfCompanyDuringMonth()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();

            var monthlyTeamStatistics = new List<SalesAndProfitOfCompanyByMonth>();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Mod"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }

            foreach (var product in productQuery)
            {
                var user = product.DauChu;

                bool isSold = product.StatusId == 1 || product.PhieuXemNhas.Any(x => x.BanThanhCong == true && x.IsActive);

                if (isSold)
                {
                    double productPrice = product.GiaBan;
                    double commission = (productPrice * product.PhanTramChiaNV) * 1.0 / 100;

                    int monthNumber = product.BatDauBan.Month;

                    var monthlyTeamStat = monthlyTeamStatistics.FirstOrDefault(mts => mts.MonthNumber == monthNumber);

                    if (monthlyTeamStat == null)
                    {
                        monthlyTeamStat = new SalesAndProfitOfCompanyByMonth
                        {
                            MonthNumber = monthNumber,
                            Sales = productPrice,
                            Profit = productPrice - commission,
                        };

                        monthlyTeamStatistics.Add(monthlyTeamStat);
                    }
                    else
                    {
                        monthlyTeamStat.Sales += productPrice;
                        monthlyTeamStat.Profit += productPrice - commission;
                    }
                }
            }

            return monthlyTeamStatistics.OrderByDescending(x => x.MonthNumber).AsQueryable();
        }

        public async Task<IQueryable<SalesAndProfitOfCompanyByWeek>> SalesStatisticsOfCompanyDuringWeek() // doanh số và lợi nhuận của công ty theo tuần nhé
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();

            var weeklyStatistics = new List<SalesAndProfitOfCompanyByWeek>();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Mod"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            foreach (var product in productQuery)
            {
                bool isSold = product.StatusId == 1 || product.PhieuXemNhas.Any(x => x.BanThanhCong == true && x.IsActive);

                if (isSold)
                {
                    int weekNumber = ConvertWeekOfYear(product.BatDauBan);
                    var weeklyStat = weeklyStatistics.FirstOrDefault(w => w.WeekNumber == weekNumber);

                    if (weeklyStat == null)
                    {
                        weeklyStat = new SalesAndProfitOfCompanyByWeek
                        {
                            WeekNumber = weekNumber,
                            Sales = product.GiaBan,
                            Profit = product.GiaBan - ((product.GiaBan * product.PhanTramChiaNV) * 1.0 / 100)
                        };

                        weeklyStatistics.Add(weeklyStat);
                    }
                    else
                    {
                        weeklyStat.Sales += product.GiaBan;
                        weeklyStat.Profit += product.GiaBan - (product.GiaBan * product.PhanTramChiaNV);
                    }
                }
            }

            return weeklyStatistics.OrderBy(x => x.WeekNumber).AsQueryable();
        }
        #endregion
        #region Convert sang tuần dựa trên kiểu dữ liệu datetime
        public static int ConvertWeekOfYear(DateTime date)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            return cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }
        #endregion
        #region Lấy ra doanh số và lợi nhuận của cả team
        public async Task<IQueryable<SalesStatisticsOfTeamDTO>> SalesStatisticsOfTeam()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();

            var teamStatistics = new List<SalesStatisticsOfTeamDTO>();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Mod") && !currentUser.IsInRole("Manager"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            foreach (var product in productQuery)
            {
                var user = product.DauChu;

                bool isSold = product.StatusId == 1 || product.PhieuXemNhas.Any(x => x.BanThanhCong == true && x.IsActive);

                if (isSold)
                {
                    double productPrice = product.GiaBan;
                    double commission = (productPrice * product.PhanTramChiaNV) * 1.0 / 100;

                    var team = _context.teams.FirstOrDefault(t => t.Users.Any(u => u.Id == user.Id));

                    var teamStat = teamStatistics.FirstOrDefault(ts => ts.TeamId == team.Id);

                    if (teamStat == null)
                    {
                        teamStat = new SalesStatisticsOfTeamDTO
                        {
                            TeamId = team.Id,
                            Sales = productPrice,
                            Profit = productPrice - commission,
                        };

                        teamStatistics.Add(teamStat);
                    }
                    else
                    {
                        teamStat.Sales += productPrice;
                        teamStat.Profit += productPrice - commission;
                    }
                }
            }

            return teamStatistics.AsQueryable();
        }
        #endregion
        #region Doanh số của team theo tuần và tháng
        public async Task<IQueryable<SalesAndProfitOfTeamByWeek>> SalesStatisticsOfTeamDuringWeek()
        {
            var productQuery = await _context.products.OrderByDescending(x => x.BatDauBan)
                                    .Include(x => x.DauChu)
                                    .Include(x => x.PhieuXemNhas)
                                    .ToListAsync();
            var weeklyTeamStatistics = new List<SalesAndProfitOfTeamByWeek>();
            foreach (var product in productQuery)
            {
                var user = product.DauChu;
                bool isSold = product.StatusId == 1 || product.PhieuXemNhas.Any(x => x.BanThanhCong == true && x.IsActive);
                if (isSold)
                {
                    double productPrice = product.GiaBan;
                    double commission = (productPrice * product.PhanTramChiaNV) * 1.0 / 100;
                    int weekNumber = ConvertWeekOfYear(product.BatDauBan);
                    var team = _context.teams.FirstOrDefault(x => x.Users.Any(y => y.Id == user.Id));
                    var weeklyTeamStat = weeklyTeamStatistics.FirstOrDefault(x => x.TeamId == team.Id && x.SatisticsAndWeekNumber.WeekNumber == weekNumber);
                    if (weeklyTeamStat is null)
                    {
                        weeklyTeamStat = new SalesAndProfitOfTeamByWeek
                        {
                            TeamId = team.Id,
                            SatisticsAndWeekNumber = new StatisticsAndWeekNumber
                            {
                                WeekNumber = weekNumber,
                                Profit = productPrice - commission,
                                Sales = productPrice
                            }
                        };
                        weeklyTeamStatistics.Add(weeklyTeamStat);
                    }
                    else
                    {
                        weeklyTeamStat.SatisticsAndWeekNumber = new StatisticsAndWeekNumber
                        {
                            WeekNumber = weekNumber,
                            Sales = productPrice,
                            Profit = productPrice - commission
                        };
                    }
                }
            }
            return weeklyTeamStatistics.OrderBy(x => x.SatisticsAndWeekNumber.WeekNumber).AsQueryable();
        }

        public async Task<IQueryable<SalesAndProfitOfTeamByMonth>> SalesStatisticsOfTeamDuringMonth()
        {
            var productQuery = await _context.products.OrderByDescending(x => x.BatDauBan)
                                    .Include(x => x.DauChu)
                                    .Include(x => x.PhieuXemNhas)
                                    .ToListAsync();
            var monthTeamStatistics = new List<SalesAndProfitOfTeamByMonth>();
            foreach (var product in productQuery)
            {
                var user = product.DauChu;
                bool isSold = product.StatusId == 1 || product.PhieuXemNhas.Any(x => x.BanThanhCong == true && x.IsActive);
                if (isSold)
                {
                    double productPrice = product.GiaBan;
                    double commission = (productPrice * (product.PhanTramChiaNV * 1.0 / 100));
                    int monthNumber = product.BatDauBan.Month;
                    var team = _context.teams.FirstOrDefault(t => t.Users.Any(u => u.Id == user.Id));
                    var monthlyTeamStat = monthTeamStatistics.FirstOrDefault(mts => mts.TeamId == team.Id && mts.StatisticsAndMonthNumber.MonthNumber == monthNumber);
                    if (monthlyTeamStat is null)
                    {
                        monthlyTeamStat = new SalesAndProfitOfTeamByMonth
                        {
                            TeamId = team.Id,
                            StatisticsAndMonthNumber = new StatisticsAndMonthNumber
                            {
                                MonthNumber = monthNumber,
                                Profit = productPrice - commission,
                                Sales = productPrice
                            }
                        };
                    }
                    else
                    {
                        monthlyTeamStat.StatisticsAndMonthNumber = new StatisticsAndMonthNumber
                        {
                            MonthNumber = monthNumber,
                            Sales = productPrice,
                            Profit = productPrice - commission
                        };
                    }
                }
            }
            return monthTeamStatistics.OrderByDescending(x => x.StatisticsAndMonthNumber.MonthNumber).AsQueryable();
        }
        #endregion
        #region Doanh số và số lượt số lượt dẫn khách
        public async Task<IQueryable<SalesAndNumberOfPassengersByDayDTO>> ViewSalesStatisticsAndNumberOfPassengersByDay()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();

            var dailyViewSalesStatistics = new List<SalesAndNumberOfPassengersByDayDTO>();

            foreach (var product in productQuery)
            {
                var user = product.DauChu;
                bool isSold = product.StatusId == 1 || product.PhieuXemNhas.Any(x => x.BanThanhCong == true && x.IsActive);

                if (isSold)
                {
                    double productPrice = product.GiaBan;
                    int dayNumber = product.BatDauBan.Day;
                    int numberOfPassengers = product.PhieuXemNhas.Count();

                    var team = _context.teams.FirstOrDefault(t => t.Users.Any(u => u.Id == user.Id));
                    var dayTeamStat = dailyViewSalesStatistics.FirstOrDefault(mts => mts.DayNumber == dayNumber);

                    if (dayTeamStat == null)
                    {
                        dayTeamStat = new SalesAndNumberOfPassengersByDayDTO
                        {
                            DayNumber = dayNumber,
                            EmployeeId = user.Id,
                            NumberOfPassengers = numberOfPassengers,
                            TeamId = team.Id,
                            Sales = productPrice
                        };
                        dailyViewSalesStatistics.Add(dayTeamStat);
                    }
                    else
                    {
                        dayTeamStat.DayNumber = dayNumber;
                        dayTeamStat.NumberOfPassengers = numberOfPassengers;
                        dayTeamStat.TeamId = team.Id;
                        dayTeamStat.EmployeeId = user.Id;
                        dayTeamStat.Sales = productPrice;
                    }
                }
            }

            return dailyViewSalesStatistics.AsQueryable();
        }


        public async Task<IQueryable<SalesAndNumberOfPassengersByMonthDTO>> ViewSalesStatisticsAndNumberOfPassengersByMonth()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();

            var monthlyViewSalesStatistics = new List<SalesAndNumberOfPassengersByMonthDTO>();

            foreach (var product in productQuery)
            {
                var user = product.DauChu;
                bool isSold = product.StatusId == 1 || product.PhieuXemNhas.Any(x => x.BanThanhCong == true && x.IsActive);

                if (isSold)
                {
                    double productPrice = product.GiaBan;
                    int monthNumber = product.BatDauBan.Month;
                    int numberOfPassengers = product.PhieuXemNhas.Count();

                    var team = _context.teams.FirstOrDefault(t => t.Users.Any(u => u.Id == user.Id));
                    var monthTeamStat = monthlyViewSalesStatistics.FirstOrDefault(mts => mts.MonthNumber == monthNumber);

                    if (monthTeamStat == null)
                    {
                        monthTeamStat = new SalesAndNumberOfPassengersByMonthDTO
                        {
                            MonthNumber = monthNumber,
                            EmployeeId = user.Id,
                            NumberOfPassengers = numberOfPassengers,
                            TeamId = team.Id,
                            Sales = productPrice
                        };
                        monthlyViewSalesStatistics.Add(monthTeamStat);
                    }
                    else
                    {
                        monthTeamStat.MonthNumber = monthNumber;
                        monthTeamStat.NumberOfPassengers = numberOfPassengers;
                        monthTeamStat.TeamId = team.Id;
                        monthTeamStat.EmployeeId = user.Id;
                        monthTeamStat.Sales = productPrice;
                    }
                }
            }

            return monthlyViewSalesStatistics.AsQueryable();
        }

        #endregion
        #region Tỉ lệ doanh số và lợi nhuận giữa team và công ty theo tuần
        public async Task<IQueryable<SalesAndProfitOfCompanyAndTeam>> SalesRatioBetweenTeamAndCompany()
        {
            var salesAndProfitOfCompany = await SalesStatisticsOfCompanyDuringWeek();
            var salesAndProfitOfTeam = await SalesStatisticsOfTeamDuringWeek();

            var salesAndProfitOfCompanyAndTeam = new List<SalesAndProfitOfCompanyAndTeam>();

            foreach (var salesOfCompany in salesAndProfitOfCompany)
            {
                var matchingSalesOfTeam = salesAndProfitOfTeam.FirstOrDefault(s => s.SatisticsAndWeekNumber.WeekNumber == salesOfCompany.WeekNumber);
                if (matchingSalesOfTeam != null)
                {
                    var item = new SalesAndProfitOfCompanyAndTeam
                    {
                        ProfitOfCompany = salesOfCompany.Profit,
                        SalesOfCompany = salesOfCompany.Sales,
                        StatisticsOfTeam = new StatisticsOfTeam
                        {
                            TeamId = matchingSalesOfTeam.TeamId,
                            Profit = matchingSalesOfTeam.SatisticsAndWeekNumber.Profit,
                            WeekNumber = matchingSalesOfTeam.SatisticsAndWeekNumber.WeekNumber,
                            ProfitRatio = (matchingSalesOfTeam.SatisticsAndWeekNumber.Profit / salesOfCompany.Profit) * 100,
                            SalesRatio = (matchingSalesOfTeam.SatisticsAndWeekNumber.Sales / salesOfCompany.Sales) * 100,
                            Sales = matchingSalesOfTeam.SatisticsAndWeekNumber.Sales
                        }
                    };
                    salesAndProfitOfCompanyAndTeam.Add(item);
                }
            }

            return salesAndProfitOfCompanyAndTeam.AsQueryable();
        }
        #endregion
        #region Thêm và sửa thông tin sản phẩm
        public async Task<ResponseObject<ProductDTO>> AddNewProduct(int dauChuId, Request_AddNewProduct request)
        {
            var dauChu = await _context.users.FirstOrDefaultAsync(x => x.Id == dauChuId);
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Manager"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            var createProduct = new Product
            {
                BatDauBan = request.BatDauBan,
                Build = request.Build,
                CertificateOfLand1 = request.CertificateOfLand1,
                CertificateOfLand2 = request.CertificateOfLand2,
                DauChuId = dauChu.Id,
                GiaBan = request.GiaBan,
                Address = request.Address,
                HoaHong = request.GiaBan - (request.GiaBan * request.PhanTramChiaNV),
                PhanTramChiaNV = request.PhanTramChiaNV * 0.01,
                HostName = request.HostName,
                HostPhoneNumber = request.HostPhoneNumber,
                StatusId = 2
            };
            await _context.products.AddAsync(createProduct);
            await _context.SaveChangesAsync();
            return _responObjectProduct.ResponseSuccess("Thêm sản phẩm thành công", _productConverter.EntityToDTO(createProduct));
        }


        public async Task<ResponseObject<ProductDTO>> UpdateProduct(int productId, int dauChuId, Request_UpdateProduct request)
        {
            var product = await _context.products.FirstOrDefaultAsync(x => x.Id == productId);
            var dauChu = await _context.users.FirstOrDefaultAsync(x => x.Id == dauChuId);
            if (product == null)
            {
                return _responObjectProduct.ResponseError(StatusCodes.Status404NotFound, "Không tìm thấy sản phẩm", null);
            }
            else
            {
                product.BatDauBan = request.BatDauBan;
                product.Build = request.Build;
                product.GiaBan = request.GiaBan;
                product.PhanTramChiaNV = request.PhanTramChiaNV * 0.01;
                product.HostName = request.HostName;
                product.HostPhoneNumber = request.HostPhoneNumber;
                product.HoaHong = request.HoaHong;
                product.Address = request.Address;
                product.CertificateOfLand1 = request.CertificateOfLand1;
                product.CertificateOfLand2 = request.CertificateOfLand2;
                product.DauChu = dauChu;
                _context.products.Update(product);
                await _context.SaveChangesAsync();
                return _responObjectProduct.ResponseSuccess("Cập nhật thông tin sản phẩm thành công", _productConverter.EntityToDTO(product));
            }
        }
        public async Task<ResponseObject<ProductDTO>> AddProductImage(int productId, Request_CreateProductImg request)
        {
            var product = await _context.products.FirstOrDefaultAsync(x => x.Id == productId);

            if (product is null)
            {
                return _responObjectProduct.ResponseError(StatusCodes.Status404NotFound, "Không tìm thấy sản phẩm", null);
            }

            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                return _responObjectProduct.ResponseError(StatusCodes.Status401Unauthorized, "Người dùng không được xác thực hoặc không được xác định", null);
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Owner"))
            {
                return _responObjectProduct.ResponseError(StatusCodes.Status403Forbidden, "Người dùng không có quyền sử dụng chức năng này", null);
            }

            var productImage = new ProductImg
            {
                ProductId = product.Id,
                LinkImg = await HandleUploadImage.Upfile(request.LinkImg)
            };

            await _context.productsImg.AddAsync(productImage);
            _context.products.Update(product);
            await _context.SaveChangesAsync();
            var productImgs = _context.productsImg.Where(x => x.Id == product.Id).ToList();
            var productDTO = _productConverter.EntityToDTO(product);
            if (productDTO != null)
            {
                productDTO.ProductImgDTOs = productImgs.Select(productImg => _productImgConverter.EntityToDTO(productImg)).AsQueryable();
            }

            return _responObjectProduct.ResponseSuccess("Thêm ảnh cho sản phẩm thành công", _productConverter.EntityToDTO(product));
        }
        public async Task<ResponseObject<ProductDTO>> UpdateImageProduct(Request_UpdateImageProduct request)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Mod"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            var productImg = await _context.productsImg.SingleOrDefaultAsync(x => x.Id == request.ProductImgId);
            try
            {
                if (productImg is null)
                {
                    return _responObjectProduct.ResponseError(StatusCodes.Status404NotFound, "Ảnh này không tồn tại", null);
                }
                if (request.LinkImg != null && !HandleCheckImage.IsImage(request.LinkImg))
                {
                    return _responObjectProduct.ResponseError(StatusCodes.Status400BadRequest, "Định dạng ảnh không hợp lệ", null);
                }
            }
            catch (Exception ex)
            {
                return _responObjectProduct.ResponseError(StatusCodes.Status401Unauthorized, ex.Message, null);
            }
            if (request.LinkImg != null)
            {
                productImg.LinkImg = await HandleUploadImage.UpdateFile(productImg.LinkImg, request.LinkImg);
                _context.productsImg.Update(productImg);
            }

            await _context.SaveChangesAsync();
            return _responObjectProduct.ResponseSuccess("Thay đổi ảnh sản phẩm thành công", null);
        }
        public async Task<string> DeleteProduct(int productId)
        {
            Product product = await _context.products.SingleOrDefaultAsync(x => x.Id == productId);
            try
            {
                if (product == null)
                {
                    return "Không tìm thấy sản phẩm cần tìm";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            var listImage = _context.productsImg.Where(x => x.ProductId == productId).ToList();
            if (listImage.Count > 0)
            {
                _context.RemoveRange(listImage);
                await _context.SaveChangesAsync();
            }
            product.IsActive = false;
            _context.products.Update(product);
            await _context.SaveChangesAsync();
            return "Xóa sản phẩm thành công";

        }
        public async Task<ResponseObject<IQueryable<ProductImg>>> AddListImageForProduct(List<IFormFile> images, int productId)
        {
            Product product = await _context.products.SingleOrDefaultAsync(x => x.Id == productId);
            try
            {
                if (product == null)
                {
                    return _responseObjectImage.ResponseError(StatusCodes.Status404NotFound, "Sản phẩm không tồn tại", null);
                }
                if (_context.productsImg.Where(x => x.ProductId == productId).Count() > 0)
                {
                    return _responseObjectImage.ResponseError(StatusCodes.Status400BadRequest, "Đã tồn tại sản phẩm trước đó", null);
                }
                if (images.Count == 0)
                {
                    return _responseObjectImage.ResponseError(StatusCodes.Status400BadRequest, "Không tồn tại ảnh để thêm", null);
                }
                if (images.Count > 5)
                {
                    return _responseObjectImage.ResponseError(StatusCodes.Status400BadRequest, "Bạn không thể thêm quá 5 ảnh cùng lúc cho sản phẩm", null);
                }
                foreach (var image in images)
                {
                    if (!HandleCheckImage.IsImage(image))
                    {
                        throw new Exception("Xảy ra lỗi trong quá trình xử lý ảnh");
                    }
                }
            }
            catch (Exception ex)
            {
                return _responseObjectImage.ResponseError(StatusCodes.Status401Unauthorized, ex.Message, null);
            }
            List<ProductImg> productImageCreate = new List<ProductImg>();
            int index = 0;
            foreach (var image in images)
            {
                index++;
                ProductImg productImg = new ProductImg();
                string url = await HandleUploadImage.Upfile(image);
                productImg.ProductId = productId;
                productImg.LinkImg = url;
                productImageCreate.Add(productImg);
            }
            await _context.productsImg.AddRangeAsync(productImageCreate);
            await _context.SaveChangesAsync();
            return _responseObjectImage.ResponseSuccess("Thêm danh sách ảnh cho sản phẩm thành công", productImageCreate.AsQueryable());
        }
        public async Task<ResponseObject<ProductImg>> DeleteImage(int productImgId)
        {
            var productImage = await _context.productsImg.SingleOrDefaultAsync(x => x.Id == productImgId);
            if (productImage == null)
            {
                return _responseObjectImg.ResponseError(StatusCodes.Status404NotFound, "Ảnh không tồn tại để xóa", null);
            }
            await HandleUploadImage.DeleteFile(productImage.LinkImg);
            _context.Remove(productImage);
            await _context.SaveChangesAsync();
            return _responseObjectImg.ResponseSuccess("Xóa ảnh thành công", productImage);
        }


        #endregion
        #region Xem thống kê bất động sản - giá bán - địa chỉ - hoa hồng - lịch sử dẫn khách
        public async Task<IQueryable<StatisticsProductInformation>> GetStatisticsProductInformation()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();

            var statisticsProductInformation = productQuery.Select(product =>
            {
                var address = product.PhieuXemNhas.Where(x => !x.BanThanhCong && x.NhaId == product.Id).Select(x => _phieuXemNhaConverter.EntityToDTO(x)).ToList();
                var user = product.DauChu;
                var commission = product.GiaBan;

                return new StatisticsProductInformation
                {
                    Address = product.Address,
                    Commission = product.GiaBan - (product.GiaBan * product.PhanTramChiaNV),
                    ProductId = product.Id,
                    Price = product.GiaBan,
                    PhieuXemNhas = address.AsQueryable()
                };
            }).AsQueryable();

            return statisticsProductInformation;
        }

        #endregion
        #region Tìm kiếm bất động sản
        public async Task<PageResult<ProductDTO>> GetProductById(int? productId, int pageSize = 10, int pageNumber = 1)
        {
            var productQuery = await _context.products
                .Where(product => ((product.StatusId == 2) || (product.PhieuXemNhas != null && product.PhieuXemNhas.Any(x => !x.BanThanhCong)))
                        && product.Id == productId)
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .AsNoTracking()
                .ToListAsync();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new NotImplementedException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Mod"))
            {
                throw new NotImplementedException("Người dùng không có quyền sử dụng chức năng này");
            }


            var listProductDTO = new List<ProductDTO>();

            foreach (var product in productQuery)
            {
                var productImgs = _context.productsImg.Where(x => x.ProductId == product.Id).ToList();

                var productDTO = _productConverter.EntityToDTO(product);

                if (productDTO != null)
                {
                    productDTO.ProductImgDTOs = productImgs.Select(productImg => _productImgConverter.EntityToDTO(productImg)).AsQueryable();
                    listProductDTO.Add(productDTO);
                }
            }

            var result = Pagination.GetPagedData(listProductDTO.AsQueryable(), pageSize, pageNumber);
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

        public async Task<IQueryable<ProductDTO>> GetAll(int pageSize = 10, int pageNumber = 1)
        {
            return _context.products.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => _productConverter.EntityToDTO(x)).AsQueryable();
        }

        public async Task<PageResult<ProductDTO>> GetAllProducts(FilterData filterData, int pageSize = 10, int pageNumber = 1)
        {
            var productQuery = await _context.products
                .Where(product => product.IsActive.Value && (product.StatusId == 2 || !(product.PhieuXemNhas != null && product.PhieuXemNhas.Any(x => x.BanThanhCong))))
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .AsNoTracking()
                .ToListAsync();

            if (filterData.MinPrice.HasValue)
            {
                productQuery = productQuery.Where(x => x.GiaBan >= filterData.MinPrice).ToList();
            }
            if (filterData.MaxPrice.HasValue)
            {
                productQuery = productQuery.Where(x => x.GiaBan <= filterData.MaxPrice).ToList();
            }
            if (filterData.FromDate.HasValue)
            {
                productQuery = productQuery.Where(x => x.BatDauBan >= filterData.FromDate).ToList();
            }
            if (filterData.ToDate.HasValue)
            {
                productQuery = productQuery.Where(x => x.BatDauBan <= filterData.ToDate).ToList();
            }
            if (filterData.BuildStart.HasValue)
            {
                productQuery = productQuery.Where(x => x.Build >= filterData.BuildStart).ToList();
            }
            if (filterData.BuildEnd.HasValue)
            {
                productQuery = productQuery.Where(x => x.Build <= filterData.BuildEnd).ToList();
            }
            if (!string.IsNullOrWhiteSpace(filterData.Keyword))
            {
                productQuery = productQuery.Where(x => x.HostName.Trim().ToLower().Contains(filterData.Keyword.Trim().ToLower()) || x.Address.Trim().ToLower().Contains(filterData.Keyword.Trim().ToLower())).ToList();
            }
            switch (filterData.SortType)
            {
                case "giaTangDan":
                    productQuery = productQuery.OrderBy(x => x.GiaBan).ToList();
                    break;
                case "giaGiamDan":
                    productQuery = productQuery.OrderByDescending(x => x.GiaBan).ToList();
                    break;
                case "bdsMoiNhat":
                    productQuery = productQuery.OrderByDescending(x => x.BatDauBan).ToList();
                    break;
                case "bdsCuNhat":
                    productQuery = productQuery.OrderBy(x => x.GiaBan).ToList();
                    break;
                default:
                    break;
            }
            var listProductDTO = new List<ProductDTO>();

            foreach (var product in productQuery)
            {
                var productImgs = _context.productsImg.Where(x => x.ProductId == product.Id).ToList();

                var productDTO = _productConverter.EntityToDTO(product);

                if (productDTO != null)
                {
                    productDTO.ProductImgDTOs = productImgs.Select(productImg => _productImgConverter.EntityToDTO(productImg)).AsQueryable();
                    listProductDTO.Add(productDTO);
                }
            }

            var result = Pagination.GetPagedData(listProductDTO.AsQueryable(), pageSize, pageNumber);
            return result;
        }
        public async Task<PageResult<ProductDTO>> FilterDataByKeyword(FilterData filterData, int pageSize, int pageNumber)
        {
            var query = await _context.products.Include(x => x.DauChu).Include(x => x.PhieuXemNhas).Include(x => x.ProductImgs).AsNoTracking().OrderByDescending(x => x.BatDauBan).ToListAsync();
            if (filterData.MinPrice.HasValue && filterData.MaxPrice.HasValue)
            {
                query = query.Where(x => x.GiaBan >= filterData.MinPrice && x.GiaBan <= filterData.MaxPrice).ToList();
            }
            if (filterData.FromDate.HasValue && filterData.ToDate.HasValue)
            {
                query = query.Where(x => x.Build >= filterData.FromDate && x.Build <= filterData.ToDate).ToList();
            }
            if (filterData.BuildStart.HasValue && filterData.BuildEnd.HasValue)
            {
                query = query.Where(x => x.Build >= filterData.BuildStart && x.Build <= filterData.BuildEnd).ToList();
            }
            if (!string.IsNullOrWhiteSpace(filterData.Keyword))
            {
                query = query.Where(x => x.HostName.Trim().ToLower().Contains(filterData.Keyword.Trim().ToLower()) || x.Address.Trim().ToLower().Contains(filterData.Keyword.Trim().ToLower())).ToList();
            }

            var data = query.OrderByDescending(x => x.GiaBan).Select(x => _productConverter.EntityToDTO(x)).AsQueryable();
            var result = Pagination.GetPagedData(data, pageSize, pageNumber);
            return result;
        }


        public async Task<PageResult<ProductDTO>> GetProductByName(string? name, int pageSize = 10, int pageNumber = 1)
        {
            var productQuery = await _context.products
                .Where(product => product.StatusId == 2 || (product.PhieuXemNhas != null && product.PhieuXemNhas.Any(x => !x.BanThanhCong))
                        && ChuanHoaChuoi(product.HostName).Contains(ChuanHoaChuoi(name)))
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .AsNoTracking()
                .ToListAsync();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new NotImplementedException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Mod"))
            {
                throw new NotImplementedException("Người dùng không có quyền sử dụng chức năng này");
            }


            var listProductDTO = new List<ProductDTO>();

            foreach (var product in productQuery)
            {
                var productImgs = _context.productsImg.Where(x => x.ProductId == product.Id).ToList();

                var productDTO = _productConverter.EntityToDTO(product);

                if (productDTO != null)
                {
                    productDTO.ProductImgDTOs = productImgs.Select(productImg => _productImgConverter.EntityToDTO(productImg)).AsQueryable();
                    listProductDTO.Add(productDTO);
                }
            }

            var result = Pagination.GetPagedData(listProductDTO.AsQueryable(), pageSize, pageNumber);
            return result;
        }
        public async Task<PageResult<ProductDTO>> GetProductByAddress(string? address, int pageSize = 10, int pageNumber = 1)
        {
            var productQuery = await _context.products
                .Where(product => product.StatusId == 2 || (product.PhieuXemNhas != null && product.PhieuXemNhas.Any(x => !x.BanThanhCong))
                        && ChuanHoaChuoi(product.Address).Contains(ChuanHoaChuoi(address)))
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .AsNoTracking()
                .ToListAsync();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new NotImplementedException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Mod"))
            {
                throw new NotImplementedException("Người dùng không có quyền sử dụng chức năng này");
            }


            var listProductDTO = new List<ProductDTO>();

            foreach (var product in productQuery)
            {
                var productImgs = _context.productsImg.Where(x => x.ProductId == product.Id).ToList();

                var productDTO = _productConverter.EntityToDTO(product);

                if (productDTO != null)
                {
                    productDTO.ProductImgDTOs = productImgs.Select(productImg => _productImgConverter.EntityToDTO(productImg)).AsQueryable();
                    listProductDTO.Add(productDTO);
                }
            }

            var result = Pagination.GetPagedData(listProductDTO.AsQueryable(), pageSize, pageNumber);
            return result;
        }
        public async Task<PageResult<ProductDTO>> GetProductByOwner(int? ownerId, int pageSize = 10, int pageNumber = 1)
        {
            var productQuery = await _context.products
                .Where(product => product.StatusId == 2 || (product.PhieuXemNhas != null && product.PhieuXemNhas.Any(x => !x.BanThanhCong))
                        && product.DauChuId == ownerId)
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .AsNoTracking()
                .ToListAsync();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new NotImplementedException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Owner") && !currentUser.IsInRole("Mod"))
            {
                throw new NotImplementedException("Người dùng không có quyền sử dụng chức năng này");
            }


            var listProductDTO = new List<ProductDTO>();

            foreach (var product in productQuery)
            {
                var productImgs = _context.productsImg.Where(x => x.ProductId == product.Id).ToList();

                var productDTO = _productConverter.EntityToDTO(product);

                if (productDTO != null)
                {
                    productDTO.ProductImgDTOs = productImgs.Select(productImg => _productImgConverter.EntityToDTO(productImg)).AsQueryable();
                    listProductDTO.Add(productDTO);
                }
            }

            var result = Pagination.GetPagedData(listProductDTO.AsQueryable(), pageSize, pageNumber);
            return result;
        }
        #endregion
        #region Thống kê về các bất động sản đã bán, chuyên viên chốt giá, giá, hoa hồng
        public async Task<IQueryable<StatisticsAboutProductSoldStaffCommission>> StatisticsProductOfOwnerAndOrtherInfomation()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas).Include(x => x.ProductImgs).AsNoTracking()
                .ToListAsync();

            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }

            var list = new List<StatisticsAboutProductSoldStaffCommission>();

            foreach (var product in productQuery)
            {
                if (product.DauChu != null && _productImgConverter != null)
                {
                    var statisticsItem = new StatisticsAboutProductSoldStaffCommission
                    {
                        Price = product.GiaBan,
                        Commission = product.GiaBan * product.PhanTramChiaNV,
                        OwnerId = product.DauChu.Id
                    };

                    var productImgs = product.ProductImgs.Where(x => x.ProductId == product.Id).ToList();
                    var productDTOs = productQuery.Select(x => _productConverter.EntityToDTO(x)).ToList();

                    foreach (var productImg in productImgs)
                    {
                        var productImgDTO = _productImgConverter.EntityToDTO(productImg);
                        if (productImgDTO != null)
                        {
                            var listProductImgDTO = product.ProductImgs.Select(x => _productImgConverter.EntityToDTO(x)).ToList();
                            productDTOs.ForEach(x =>
                            {
                                x.ProductImgDTOs = listProductImgDTO.AsQueryable();
                            });
                        }
                    }

                    statisticsItem.ProductDTOs = productDTOs.AsQueryable();
                    list.Add(statisticsItem);
                }
            }

            return list.AsQueryable();
        }


        #endregion
        #region Thống kê số lượng bán theo ngày - tuần - tháng 
        public async Task<IQueryable<StatisticsSalesByDay>> GetStatisticsSalesByDay()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();
            var statisticsSalesByDay = new List<StatisticsSalesByDay>();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            foreach (var product in productQuery)
            {
                int sellNumber = product.PhieuXemNhas.Count(x => x.BanThanhCong && x.IsActive);
                int dayNumber = product.BatDauBan.Day;
                var dayValue = statisticsSalesByDay.FirstOrDefault(x => x.DayNumber == dayNumber);
                if (dayValue == null)
                {
                    dayValue = new StatisticsSalesByDay
                    {
                        DayNumber = dayNumber,
                        SellNumber = sellNumber
                    };
                    statisticsSalesByDay.Add(dayValue);
                }
                else
                {
                    dayValue.DayNumber = dayNumber;
                    dayValue.SellNumber = sellNumber;
                }
            }
            return statisticsSalesByDay.AsQueryable();
        }
        public async Task<IQueryable<StatisticsSalesByWeek>> GetStatisticsSalesByWeek()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();
            var statisticsSalesByWeek = new List<StatisticsSalesByWeek>();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            foreach (var product in productQuery)
            {
                int sellNumber = product.PhieuXemNhas.Count(x => x.BanThanhCong && x.IsActive);
                int weekNumber = ConvertWeekOfYear(product.BatDauBan);
                var statisticsByWeek = statisticsSalesByWeek.FirstOrDefault(x => x.WeekNumber == weekNumber);
                if (statisticsByWeek == null)
                {
                    statisticsByWeek = new StatisticsSalesByWeek
                    {
                        WeekNumber = weekNumber,
                        SellNumber = sellNumber
                    };
                    statisticsSalesByWeek.Add(statisticsByWeek);
                }
                else
                {
                    statisticsByWeek.WeekNumber = weekNumber;
                    statisticsByWeek.SellNumber = sellNumber;
                }
            }
            return statisticsSalesByWeek.AsQueryable();
        }
        public async Task<IQueryable<StatisticsSalesByMonth>> GetStatisticsSalesByMonth()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();
            var statisticsSalesByMonth = new List<StatisticsSalesByMonth>();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            foreach (var product in productQuery)
            {
                int sellNumber = product.PhieuXemNhas.Count(x => x.BanThanhCong && x.IsActive);
                int monthNumber = product.BatDauBan.Month;
                var monthValue = statisticsSalesByMonth.FirstOrDefault(x => x.MonthNumber == monthNumber);
                if (monthValue == null)
                {
                    monthValue = new StatisticsSalesByMonth
                    {
                        MonthNumber = monthNumber,
                        SellNumber = sellNumber
                    };
                    statisticsSalesByMonth.Add(monthValue);
                }
                else
                {
                    monthValue.MonthNumber = monthNumber;
                    monthValue.SellNumber = sellNumber;
                }
            }
            return statisticsSalesByMonth.AsQueryable();
        }
        #endregion
        #region Xem danh sách nhà đang chờ bán - giá - hoa hồng (Google Map API)
        public async Task<IQueryable<ProductNotYetSoldAndPrice>> GetProductSoldAndPrices()
        {
            var productQuery = await _context.products
                .OrderByDescending(x => x.BatDauBan)
                .Include(x => x.DauChu)
                .Include(x => x.PhieuXemNhas)
                .ToListAsync();
            var productSoldAndPrice = new List<ProductNotYetSoldAndPrice>();
            var currentUser = _httpContextAccessor.HttpContext.User;

            if (!currentUser.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Người dùng không được xác thực hoặc không được xác định");
            }

            if (!currentUser.IsInRole("Admin") && !currentUser.IsInRole("Manager") && !currentUser.IsInRole("Mod"))
            {
                throw new UnauthorizedAccessException("Người dùng không có quyền sử dụng chức năng này");
            }
            foreach (var product in productQuery)
            {
                bool isSold = product.StatusId == 2 || product.PhieuXemNhas.Any(x => !x.BanThanhCong);
                if (isSold)
                {
                    double price = product.GiaBan;
                    double commission = product.GiaBan * product.PhanTramChiaNV;
                    var productNotYetSold = new ProductNotYetSoldAndPrice
                    {
                        Price = price,
                        Commission = commission
                    };
                    var productImgs = product.ProductImgs.Where(x => x.ProductId == product.Id).ToList();
                    var productDTOs = productQuery.Select(x => _productConverter.EntityToDTO(x)).ToList();

                    foreach (var productImg in productImgs)
                    {
                        var productImgDTO = _productImgConverter.EntityToDTO(productImg);
                        if (productImgDTO != null)
                        {
                            var listProductImgDTO = product.ProductImgs.Select(x => _productImgConverter.EntityToDTO(x)).ToList();
                            productDTOs.ForEach(x =>
                            {
                                x.ProductImgDTOs = listProductImgDTO.AsQueryable();
                            });
                        }
                    }
                    productNotYetSold.ProductDTOs = productDTOs.AsQueryable();
                    productSoldAndPrice.Add(productNotYetSold);
                }
            }
            return productSoldAndPrice.AsQueryable();
        }
        public async Task<GeocodingResult> GetCoordinatesAsync(string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace("AIzaSyDWCoeuHmZroAazHpXfo_pEt2FAB4YF2Ss"))
                {
                    throw new ArgumentException("API Key is required.");
                }

                var apiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key=AIzaSyDWCoeuHmZroAazHpXfo_pEt2FAB4YF2Ss";

                using (var httpClient = _httpClientFactory.CreateClient("YourHttpClientName"))
                {
                    var response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var googleMapsResponse = JsonConvert.DeserializeObject<GoogleMapsResponse>(content);

                        if (googleMapsResponse.Results.Count > 0)
                        {
                            var location = googleMapsResponse.Results[0].Geometry.Location;
                            return new GeocodingResult
                            {
                                Latitude = location.Lat,
                                Longitude = location.Lng
                            };
                        }
                        else
                        {
                            throw new InvalidOperationException("Không tìm thấy địa chỉ.");
                        }

                    }
                    else
                    {
                        throw new HttpRequestException($"Lỗi khi gửi yêu cầu lấy tọa độ từ Google Maps: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi: {ex.Message}");
            }
        }

        #endregion
        #region Tạo yêu cầu thay đổi trạng thái khi bán bất động sản thành công
        public async Task<string> CreateRequestChangeStatusWhenClosingSuccessfully(int productId)
        {
            var product = await _context.products.SingleOrDefaultAsync(x => x.Id == productId);
            if (product == null)
            {
                return "Không tìm thấy sản phẩm mà bạn cần";
            }

            var phieuXemNha = _context.phieuXemNhas.FirstOrDefault(x => x.NhaId == productId && x.BanThanhCong);
            if (phieuXemNha == null)
            {
                return "Bất động sản vẫn chưa được bán";
            }

            List<PhieuXemNha> phieuCanCapNhat = new List<PhieuXemNha>(); // Danh sách tạm thời

            foreach (var phieu in product.PhieuXemNhas)
            {
                if (phieu == null)
                {
                    // Thêm kiểm tra cho trường hợp phiếu null
                    continue;
                }

                if (product.StatusId == 1)
                {
                    break;
                }

                if (phieu.Id == phieuXemNha.Id)
                {
                    product.StatusId = 1;
                    phieu.BanThanhCong = true;
                    phieuCanCapNhat.Add(phieu);
                }
            }

            foreach (var phieuItem in _context.phieuXemNhas.ToList())
            {
                if (phieuItem.NhaId == phieuXemNha.NhaId && !phieuCanCapNhat.Contains(phieuItem))
                {
                    phieuItem.BanThanhCong = false;
                }
            }

            _context.products.Update(product);
            await _context.SaveChangesAsync();

            return "Thay đổi status thành công";
        }



        #endregion
        #region Thống kê phòng ban

        public async Task<IQueryable<DataResponseStatistics>> TeamStatistic(InputStatistics input)
        {
            var productList = await _context.products.OrderByDescending(x => x.BatDauBan)
                                        .Include(x => x.DauChu)
                                            .ThenInclude(x=>x.Team)
                                        .Include(x => x.PhieuXemNhas)
                                        .AsNoTracking()
                                        .Where(x => x.DauChu.TeamId != null && x.StatusId == 1)
                                        .ToListAsync();

            if (input.StartTime.HasValue)
            {
                productList = productList.Where(x => x.BatDauBan.Date >= input.StartTime.Value.Date).ToList();
            }

            if (input.EndTime.HasValue)
            {
                productList = productList.Where(x => x.BatDauBan.Date <= input.EndTime.Value.Date).ToList();
            }

            if (input.TeamId.HasValue)
            {
                productList = productList.Where(x => x.DauChu.TeamId == input.TeamId).ToList();
            }

            var teamStats = productList
                .GroupBy(p => new { p.DauChu.TeamId , p.DauChu.Team.Name })
                .Select(g =>
                {
                    var totalSales = g.Sum(p => p.GiaBan);
                    var totalCommission = g.Sum(p => (p.GiaBan * (p.PhanTramChiaNV * 1.0 / 100)));
                    var profit = totalSales - totalCommission;

                    return new DataResponseStatistics
                    {
                        teamId = g.Key.TeamId,
                        teamName = g.Key.Name,
                        StatisticSalesAndProfit = new StatisticSalesAndProfit
                        {
                            Profit = profit,
                            Sales = totalSales
                        }
                    };
                });

            return teamStats.AsQueryable();
        }

        #endregion
        #region Tỉ lệ giữa phòng ban và công ty
        public async Task<IQueryable<RaitoSalesAndProfit>> RatioBetweenTeamAndCompany(InputStatistics input)
        {
            var productList = await _context.products.OrderByDescending(x => x.BatDauBan)
                                        .Include(x => x.DauChu)
                                            .ThenInclude(x=>x.Team)
                                        .Include(x => x.PhieuXemNhas)
                                        .AsNoTracking()
                                        .Where(x => x.DauChu.TeamId != null && x.StatusId == 1)
                                        .ToListAsync();

            if (input.StartTime.HasValue)
            {
                productList = productList.Where(x => x.BatDauBan.Date >= input.StartTime.Value.Date).ToList();
            }

            if (input.EndTime.HasValue)
            {
                productList = productList.Where(x => x.BatDauBan.Date <= input.EndTime.Value.Date).ToList();
            }

            if (input.TeamId.HasValue)
            {
                productList = productList.Where(x => x.DauChu.TeamId == input.TeamId).ToList();
            }

            List<RaitoSalesAndProfit> list = new List<RaitoSalesAndProfit>();
            double totalSalesOfCompany = productList.Sum(x => x.GiaBan);
            double totalProfitOfCompany = productList.Sum(x => x.GiaBan - (x.GiaBan * x.PhanTramChiaNV));
            var teamStats = productList
                .GroupBy(p => new { p.DauChu.TeamId , p.DauChu.Team.Name })
                .Select(g =>
                {
                    var totalSales = g.Sum(p => p.GiaBan);
                    var totalCommission = g.Sum(p => (p.GiaBan * (p.PhanTramChiaNV)));
                    var profit = totalSales - totalCommission;

                    return new RaitoSalesAndProfit
                    {
                        TeamId = g.Key.TeamId,
                        TeamName = g.Key.Name,
                        RatioProfit = Math.Round(profit * 100.0 / totalProfitOfCompany, 2),
                        TotalProfitOfCompany = totalProfitOfCompany.ToString(),
                        TotalSalesOfCompany = totalSalesOfCompany.ToString(),
                        RatioSales = Math.Round(totalSales * 100.0 / totalSalesOfCompany, 2),
                    };
                });

            return teamStats.AsQueryable();
        }
        #endregion
    }
}
