using LitangOfficial.Backoffice.BusinessLogic.Model;
using LitangOfficial.Backoffice.BusinessLogic.ViewModel;
using System;
using System.Collections.Generic;

namespace LitangOfficial.Backoffice.BusinessLogic.Logistics
{
    public interface ICombineAccountLogic
    {        
        List<CombineAccountModel> GetAll(string UserCode,int Status);
        CombineAccountModel GetDetail(Guid Id);        
        CombineAccountModel GetCombineAccountModel(Guid id);
        void Update(CombineAccountModel model);

        string CheckUserExist(string UserCode);



    }
}
