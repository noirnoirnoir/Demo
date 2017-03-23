using LitangOfficial.Backoffice.BusinessLogic.Model;
using LitangOfficial.Backoffice.BusinessLogic.ViewModel;
using LitangOfficial.Backoffice.Databases.Model;
using LitangOfficial.Backoffice.Databases.Respositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace LitangOfficial.Backoffice.BusinessLogic.Logistics
{
    public class CombineAccountLogic : ICombineAccountLogic
    {
        private ICombineAccountRepository m_combineAccountRepository = null;
        private IUserRepository m_userRepository = null;
        private IUserExtInfoRepository m_userExtInfoRepository = null;
        private IUserExtInfoLogic m_userExtInfoLogic = null;
        public CombineAccountLogic(ICombineAccountRepository combineAccountRepository, IUserRepository userRepository, IUserExtInfoRepository userExtInfoRepository, IUserExtInfoLogic userExtInfoLogic)
        {
            m_combineAccountRepository = combineAccountRepository;
            m_userRepository = userRepository;
            m_userExtInfoRepository = userExtInfoRepository;
            m_userExtInfoLogic = userExtInfoLogic;
        }


        public string CheckUserExist(string userCode)
        {
            User _user = m_userRepository.FindOne(x => x.UserCode == userCode);
            if (_user != null)
            { return ""; }
            else
            { return "Cant Find User"; }

        }
        public List<CombineAccountModel> GetAll(string userCode, int status)
        {
            List<CombineAccountModel> _newModel = new List<CombineAccountModel>();
            List<CombineAccount> _model = new List<CombineAccount>();

            if (userCode != "")
            {
                User _user = m_userRepository.FindOne(x => x.UserCode == userCode);

                //if (_user != null)
                //{
                    if ((FormStatus)status == FormStatus.AllStatus)
                    { _model = m_combineAccountRepository.Find(x => x.UserId == _user.Id).ToList(); }
                    else
                    {
                        _model = m_combineAccountRepository.Find(x => x.UserId == _user.Id && x.Status == status).ToList();
                    }
                //}
                //else
                //{ Exception("Cant Find User"); }
            }
            else
            {
                _model = m_combineAccountRepository.GetAll().ToList(); 
            }

            foreach (CombineAccount _item in _model)
            {
                CombineAccountModel _combineAccount = GetCombineAccountModel(_item.Id);                
                _newModel.Add(_combineAccount);
            }

            return _newModel;
        }

        /// <returns>first OrganizationModel: user, second OrganizationModel:combined user</returns>
        public CombineAccountModel GetDetail(Guid id)
        {
            CombineAccountModel _newModel = GetCombineAccountModel(id);
            if (_newModel.ModifiedBy == _newModel.UserId)
            { _newModel.ModifiedName = ""; }
            else
            { _newModel.ModifiedName = m_userRepository.Find(x => x.Id == _newModel.ModifiedBy).Select(x => x.UserName).SingleOrDefault(); }

            if (_newModel.ModifiedTime == _newModel.CreatedTime)
            { _newModel.ModifiedDateTime = ""; }
            else
            { _newModel.ModifiedDateTime = _newModel.ModifiedTime.ToString(); }
            List<OrganizationModel> _organizationModel = new List<OrganizationModel>();
            _organizationModel.Add(m_userExtInfoLogic.GetOrganizationChart(_newModel.UserId));
            _organizationModel.Add(m_userExtInfoLogic.GetOrganizationChart(_newModel.CombinedId));

            _newModel.Organizations = _organizationModel;
            return _newModel;
        }

        public void Update(CombineAccountModel model)
        {
            try
            {                
                CombineAccount _updateModel = m_combineAccountRepository.FindOne(x => x.Id == model.Id);
                if (_updateModel == null)
                {
                    Exception("Cant Find Data On CombineAccount");
                }

                if (model.Status == FormStatus.Reject)
                {
                    _updateModel.Status = Convert.ToInt32(model.Status);
                    _updateModel.ModifiedTime = DateTime.Now;
                    _updateModel.ModifiedBy = model.ModifiedBy;

                    m_combineAccountRepository.Update(_updateModel);
                    m_combineAccountRepository.SaveChanges();
                }
                else
                {
                    if (CheckUsersRelation(_updateModel.UserId, _updateModel.CombinedId, _updateModel.Position))
                    {
                        //update CombineAccount
                        _updateModel.Status = Convert.ToInt32(model.Status);
                        _updateModel.ModifiedTime = DateTime.Now;
                        _updateModel.ModifiedBy = model.ModifiedBy;

                        m_combineAccountRepository.Update(_updateModel);
                        m_combineAccountRepository.SaveChanges();

                        //update UserExtInfoes 
                        UpdateUsersRelation(_updateModel.UserId, _updateModel.CombinedId, _updateModel.Position);
                    }
                }

        
            }
            catch (Exception e)
            {
                Exception("Update CombinedAccount Fail");              
            }
        }

        public CombineAccountModel GetCombineAccountModel(Guid id)
        {
            CombineAccount _model = m_combineAccountRepository.FindOne(x =>x.Id == id);
            if (_model == null)
            {
                Exception("Cant Find Data On CombineAccount");

            }

            User _user = m_userRepository.FindOne(x => x.Id == _model.UserId);
            User _combined = m_userRepository.FindOne(x => x.Id == _model.CombinedId);

            CombineAccountModel _newModel = new CombineAccountModel
            {
                Id = _model.Id,
                UserId = _model.UserId,
                CombinedId = _model.CombinedId,
                UserCode = _user.UserCode,
                CombinedCode = _combined.UserCode,
                UserName = _user.UserName,
                CombinedName = _combined.UserName,
                Email = _user.Email,
                CellPhone = _user.CellPhone,
                Status = (FormStatus)_model.Status,
                File = _model.File,
                FileName = Path.GetFileName(_model.File),
                Position = _model.Position,
                CreatedTime = _model.CreatedTime,
                ModifiedTime = _model.ModifiedTime,
                ModifiedBy = _model.ModifiedBy
            };

            return _newModel;
        }
        private bool CheckUsersRelation(Guid userId, Guid combinedId, bool position)
        {
            //UserAccoount move to CombineAccount
            //--- checkUserAccount
            List<UserExtInfo> _userDownline = m_userExtInfoRepository.Find(x => x.UplineId == userId).ToList();
            UserExtInfo _userExtUnfo = m_userExtInfoRepository.FindOne(x => x.UserId == userId);
            if (userId == _userExtUnfo.UplineId)
            {
                Exception("Cant Combine Account-UserAccount is on the top");
                return false;
            }
            else if (_userDownline.Count > 0 && userId == _userExtUnfo.UplineId)
            {
                Exception("Cant Combine Account-UserAccount has no upline");
                return false;
            }

            //--- checkCombinedAccount  (combine to CombinedAccount)
            if (position)//upward
            {
                _userExtUnfo = new UserExtInfo();
                _userExtUnfo = m_userExtInfoRepository.FindOne(x => x.UserId == combinedId);
                if (combinedId != _userExtUnfo.UplineId)
                {
                    Exception("Cant Combine Account-CombinedAccount has upline");
                    return false;
                }
            }

            return true;
        }
        private void UpdateUsersRelation(Guid userId, Guid combinedId, bool position)
        {
            try
            {
                UserExtInfo _userExtInfo = m_userExtInfoRepository.FindOne(x => x.UserId == userId);               

                //---update downline of UserAccount              
                List<UserExtInfo> _userDownline = m_userExtInfoRepository.Find(x => x.UplineId == userId).ToList();
                foreach (UserExtInfo _item in _userDownline)
                {
                    _item.UplineId = _userExtInfo.UplineId;
                    m_userExtInfoRepository.Update(_item);
                }

                //---update relative of CombinedAccount
                if (position)//upward
                {
                    UserExtInfo _combinedExtInfo = m_userExtInfoRepository.FindOne(x => x.UserId == combinedId);
                    _userExtInfo.UplineId = userId;
                    _combinedExtInfo.UplineId = userId;
                    m_userExtInfoRepository.Update(_combinedExtInfo);
                }
                else//downward
                {
                    _userExtInfo.UplineId = combinedId;
                }
                m_userExtInfoRepository.Update(_userExtInfo);
                

                m_userExtInfoRepository.SaveChanges();

            }
            catch (Exception)
            {
                Exception("Update UserExtInfoes Fail");
            }
        }

        private LitangBaseException Exception(string message)
        {
            throw new LitangBaseException(message.Trim(), message, "en", null);
        }
      

    }
}
