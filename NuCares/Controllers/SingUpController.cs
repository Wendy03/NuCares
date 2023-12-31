﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using NSwag.Annotations;
using NuCares.Models;
using NuCares.Security;

namespace NuCares.Controllers
{
    [OpenApiTag("SingUp", Description = "註冊")]
    public class SingUpController : ApiController
    {
        private readonly NuCaresDBContext db = new NuCaresDBContext();
        private readonly Argon2Verify ag2Verify = new Argon2Verify();

        #region "註冊API"

        /// <summary>
        /// 新會員註冊
        /// </summary>
        /// <param name="viewUser">註冊新會員</param>
        /// <returns></returns>
        [HttpPost]
        [Route("auth/signup")]
        public IHttpActionResult SignUp(ViewUser viewUser)
        {
            if (ModelState.IsValid)
            {
                #region "argon2加密"

                //// Hash 加鹽加密
                //var salt = ag2Verify.CreateSalt();
                //string saltStr = Convert.ToBase64String(salt);
                //var hash = ag2Verify.HashPassword(viewUser.Password, salt);
                //string hashPassword = Convert.ToBase64String(hash);

                #endregion "argon2加密"

                // 密碼hash加密
                var getHash = ag2Verify.PasswordHash(viewUser.Password);

                // 判斷Email是否有重複
                bool emailCheck = db.Users.Any(u => u.Email == viewUser.Email);
                if (emailCheck)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        StatusCode = 400,
                        Status = "Error",
                        Message = new { Email = "信箱重複" }
                    });
                }

                // 將性別轉成數字
                int gender = viewUser.Gender == "male" ? 0 : 1;

                // 創建一個新的 User 物件，紀錄傳入的數值
                var newUser = new User
                {
                    UserName = viewUser.UserName,
                    Password = getHash.hashPassword,
                    Salt = getHash.salt,
                    Email = viewUser.Email,
                    Birthday = viewUser.Birthday,
                    Gender = (EnumList.GenderType)gender,
                    Phone = viewUser.Phone
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                var result = new
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "註冊成功"
                };
                return Ok(result);
            }
            else
            {
                // 回傳錯誤的訊息
                var errors = ModelState.Keys
                    .Select(key =>
                    {
                        var propertyName = key.Split('.').Last(); // 取的屬性名稱
                        var errorMessage = ModelState[key].Errors.First().ErrorMessage; // 取得錯誤訊息
                        return new { PropertyName = propertyName, ErrorMessage = errorMessage };
                    })
                    .ToDictionary(e => e.PropertyName, e => e.ErrorMessage);

                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 400,
                    Status = "Error",
                    Message = errors
                });
            }
        }

        #endregion "註冊API"

        #region "判斷信箱是否重複"

        /// <summary>
        /// 信箱驗證
        /// </summary>
        /// <param name="viewEmailCheck">判斷信箱是否重複</param>
        /// <returns></returns>
        [HttpPost]
        [Route("auth/checkEmail")]
        public IHttpActionResult CheckEmail(ViewEmailCheck viewEmailCheck)
        {
            // 判斷Email是否有重複
            bool emailCheck = db.Users.Any(u => u.Email == viewEmailCheck.Email);
            if (emailCheck)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 400,
                    Status = "Error",
                    Message = new { Email = "用戶已存在" }
                });
            }

            // 判斷密碼是否相同
            if (!string.Equals(viewEmailCheck.Password, viewEmailCheck.RePassword))
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 400,
                    Status = "Error",
                    Message = new { Password = "密碼不相同" }
                });
            }

            // 判斷輸入的格式是否正確
            if (!ModelState.IsValid)
            {
                // 回傳錯誤的訊息
                var errors = ModelState.Keys
                    .Select(key =>
                    {
                        var propertyName = key.Split('.').Last(); // 取的屬性名稱
                        var errorMessage = ModelState[key].Errors.First().ErrorMessage; // 取得錯誤訊息
                        return new { PropertyName = propertyName, ErrorMessage = errorMessage };
                    })
                    .ToDictionary(e => e.PropertyName, e => e.ErrorMessage);

                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 400,
                    Status = "Error",
                    Message = errors
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Status = "Success",
                Message = "Email可以使用"
            });
        }

        #endregion "判斷信箱是否重複"
    }
}