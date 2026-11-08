using AutoMapper;
using WCPS.Data.Model;
using BO = WCPS.Service.Model;
namespace WCPS.Repository.Mapper
{
    public class RepositoryProfile:Profile
    {
        public RepositoryProfile()
        {
            CreateMap<BO.Product, Product>().ReverseMap();
        }
    }
}
