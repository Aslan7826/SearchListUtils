using Microsoft.AspNetCore.Mvc;
using SearchListUtils.Models.ListSearch;
using SearchListUtils.Models.ViewModels;
using SearchListUtils.Utils;

namespace SearchListUtils.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ListSearchDataController : ControllerBase
    {

        List<TestDataViewModel> _datas;
        public ListSearchDataController()
        {
            _datas = new List<TestDataViewModel>() 
            {
                new TestDataViewModel(){ Id = 1 , Name = "A", CreatDate = new DateTime(2022,1,1)},
                new TestDataViewModel(){ Id = 2 , Name = "B", CreatDate = new DateTime(2022,1,2)},
                new TestDataViewModel(){ Id = 3 , Name = "C", CreatDate = new DateTime(2022,1,3)},
                new TestDataViewModel(){ Id = 4 , Name = "Aslan", CreatDate = new DateTime(2022,2,1)},
                new TestDataViewModel(){ Id = 5 , Name = "Test", CreatDate = new DateTime(2022,2,1)},
            };
        }
                                                                             
        [HttpGet]
        public ResponseListPageResult SearchBar([FromQuery] ReqPageObj ReqPageObj
                                               ,[FromQuery] InSearchObj inSearchObj
                                               ,[FromQuery] InOrderByObj inOrderByObj)
        {
            var result = _datas.ToSearchBar(inSearchObj)
                               .ToOrderBy(inOrderByObj)
                               .ToPage(ReqPageObj);
            return result;
        }

        [HttpGet]
        public ResponseListPageResult ClassSerach([FromQuery] ReqPageObj ReqPageObj
                                                , [FromQuery] TestDataViewModel filter
                                                , [FromQuery] InSearchClassObj<TestDataViewModel> inSearchClassObj
                                                , [FromQuery] InOrderByObj inOrderByObj
                                                ) 
        {
            var result = _datas.ToClassSearch(filter,inSearchClassObj)
                               .ToOrderBy(inOrderByObj)
                               .ToPage(ReqPageObj);
            return result;
        }
    }
}