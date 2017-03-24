using xxx.Backoffice.BusinessLogic.Logistics;
using xxx.Backoffice.BusinessLogic.Model;
using xxx.Backoffice.BusinessLogic.ViewModel;
using xxx.Backoffice.Web.Configurations;
using xxx.Backoffice.Web.Tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace xxx.Backoffice.Web.Controllers.MVC
{
    public class CombineAccountController : ControllerBase
    {
        private ICombineAccountLogic m_combineAccountLogic = null;
        private IUserExtInfoLogic m_userExtInfoLogic = null;     
        private IParameters m_parameters = null;
   
        public CombineAccountController(ICombineAccountLogic combineAccountLogic, IUserExtInfoLogic userExtInfoLogic,
            IParameters parameters)
        {
            this.m_combineAccountLogic = combineAccountLogic;
            this.m_userExtInfoLogic = userExtInfoLogic;           
            this.m_parameters = parameters;
        }

        public ActionResult Index()
        {
            //Guid id = m_parameters.UserId;
            ViewData["FormStatusSelector"] = _FormStatusList(true);
            List<CombineAccountModel> _model = null;

            return View("index", _model);
        }

        [Route("CombineAccount/CheckUserExist/{userCode}")]
        public JsonResult CheckUserExist(string userCode)
        {
            var result = new
            {
                message = m_combineAccountLogic.CheckUserExist(userCode)
            };

            return Json(result);
        }

        [Route("CombineAccount/ShowCombineAccountList/{fStatus}")]
        public ActionResult ShowCombineAccountList(int fStatus)
        {
            return ShowCombineAccountList("", fStatus);
        }

        [Route("CombineAccount/ShowCombineAccountList/{userCode}/{fStatus}")]
        public ActionResult ShowCombineAccountList(string userCode,int fStatus)
        {
            List<CombineAccountModel> _model = m_combineAccountLogic.GetAll(userCode, fStatus);
            return PartialView("_CombineAccountList", _model);
        }


        [Route("CombineAccount/ShowDetail/{id}")]
        public ActionResult ShowDetail(Guid id)
        {            
            CombineAccountModel _model = m_combineAccountLogic.GetDetail(id);
            //ViewData["FormStatusSelector"] = _FormStatusList(false, _model.Status);
            return PartialView("_Detail", _model);
        }


        public void Update(CombineAccountModel model)
        {
            model.ModifiedBy = m_parameters.UserId;
            model.Status = FormStatus.Confirmed;
            m_combineAccountLogic.Update(model);
        }

        public void Reject(CombineAccountModel model)
        {
            model.ModifiedBy = m_parameters.UserId;
            model.Status = FormStatus.Reject;
            m_combineAccountLogic.Update(model);
        }


        public ActionResult Download(Guid id)
        {
            CombineAccountModel _model = m_combineAccountLogic.GetCombineAccountModel(id);
            string _filename = _model.File;
            IBaseConfiguration _config = new BaseConfiguration();
            string _url = _config.CombineAccountPictureBaseRoute;           
            int _content = _filename.IndexOf('.');
            string _type = _filename.Substring(_content + 1);//抓出副檔名
                  
            WebClient WebClient = new WebClient();
            var byteArr = WebClient.DownloadData(_url + _filename);

            return File(byteArr, "image/" + _type);
            
        }

        private List<SelectListItem> _FormStatusList(bool showAll)
        {
            return _FormStatusList(showAll, FormStatus.AllStatus);
        }
        private List<SelectListItem> _FormStatusList(bool showAll, FormStatus selectValue)
        {
            List<SelectListItem> _list = new List<SelectListItem>();

            foreach (int _status in Enum.GetValues(typeof(FormStatus)))
            {
                FormStatus _value = (FormStatus)_status;
                if (!showAll && _status == 0)// _status= showAll
                { continue; }

                 _list.Add(new SelectListItem  { Value = Convert.ToString(_status), Text = Convert.ToString(_value), Selected = _value.Equals(selectValue) });       
            }

            return _list;
        }


       
    }
}
