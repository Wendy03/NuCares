﻿using NSwag.Annotations;
using NuCares.Models;
using NuCares.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NuCares.Controllers
{
    [OpenApiTag("Nutritionist", Description = "營養師")]
    public class NutritionistController : ApiController
    {
        private readonly NuCaresDBContext db = new NuCaresDBContext();

        #region "取得營養師資料 API"
        /// <summary>
        /// 取得營養師資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("nu/info")]
        [JwtAuthFilter]
        public IHttpActionResult GetNutritionist()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int id = (int)userToken["Id"];
            bool isNutritionist = (bool)userToken["IsNutritionist"];
            bool checkUser = db.Nutritionists.Any(n => n.UserId == id);
            if (!isNutritionist || !checkUser)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 403,
                    Status = "Error",
                    Message = "您沒有營養師權限"
                });
            }
            var nu = db.Nutritionists.FirstOrDefault(n => n.UserId == id);
            string[] expertiseArray = nu.Expertise.Split(',');

            var result = new
            {
                StatusCode = 200,
                Status = "Success",
                Message = "取得營養師資料成功",
                Date = new
                {
                    nu.Id,
                    nu.IsPublic,
                    nu.PortraitImage,
                    nu.Title,
                    nu.City,
                    Expertise = expertiseArray,
                    nu.Education,
                    nu.Experience,
                    nu.AboutMe,
                    nu.CourseIntro,
                    nu.Option1,
                    nu.OptionId1,
                    nu.Option2,
                    nu.OptionId2,
                    nu.Option3,
                    nu.OptionId3
                }
            };
            return Ok(result);
        }
        #endregion "取得營養師資料 API"

        #region "編輯營養師資料 API"
        /// <summary>
        /// 編輯營養師資料
        /// </summary>
        /// /// <param name="nutritionistView">營養師資料</param>
        /// <returns></returns>
        [HttpPut]
        [Route("nu/info")]
        [JwtAuthFilter]
        public IHttpActionResult EditNutritionist([FromBody] NutritionistView nutritionistView)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int id = (int)userToken["Id"];
            bool isNutritionist = (bool)userToken["IsNutritionist"];
            bool checkUser = db.Nutritionists.Any(n => n.UserId == id);
            if (!isNutritionist || !checkUser)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 403,
                    Status = "Error",
                    Message = "您沒有營養師權限"
                });
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                   .SelectMany(v => v.Errors)
                   .Select(e => e.ErrorMessage)
                   .ToList();

                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 400,
                    Status = "Error",
                    Message = errors
                });
            }
            //找到營養師
            var nu = db.Nutritionists.FirstOrDefault(n => n.UserId == id);

            //變更資料比對
            nu.IsPublic = nutritionistView.IsPublic;
            if (nutritionistView.Expertise != null && nutritionistView.Expertise.Count > 0)
            {
                string expertiseString = string.Join(",", nutritionistView.Expertise);
                nu.Expertise = expertiseString;
            }

            var propertiesToCopy = new List<string>
            {
                "PortraitImage",
                "Title",
                "City",
                "Education",
                "Experience",
                "AboutMe",
                "CourseIntro",
                "Option1",
                "OptionId1",
                "Option2",
                "OptionId2",
                "Option3",
                "OptionId3"
            };

            foreach (var property in propertiesToCopy)
            {
                var propertyValue = nutritionistView.GetType().GetProperty(property).GetValue(nutritionistView, null);
                if (propertyValue != null)
                {
                    nu.GetType().GetProperty(property).SetValue(nu, propertyValue);
                }
            }

            //更新資料庫
            db.SaveChanges();

            var expertiseArray = nu.Expertise.Split(',');
            var result = new
            {
                StatusCode = 200,
                Status = "Success",
                Message = "營養師資料更新成功",
                Date = new
                {
                    nu.Id,
                    nu.IsPublic,
                    nu.PortraitImage,
                    nu.Title,
                    nu.City,
                    Expertise = expertiseArray,
                    nu.Education,
                    nu.Experience,
                    nu.AboutMe,
                    nu.CourseIntro,
                    nu.Option1,
                    nu.OptionId1,
                    nu.Option2,
                    nu.OptionId2,
                    nu.Option3,
                    nu.OptionId3
                }
            };
            return Ok(result);
        }

        #endregion "編輯營養師資料 API"
    }
}