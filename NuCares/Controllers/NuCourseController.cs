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
    [OpenApiTag("Course", Description = "課程")]
    public class NuCourseController : ApiController
    {
        private readonly NuCaresDBContext db = new NuCaresDBContext();

        #region "查看學員課程菜單"
        /// <summary>
        /// 查看學員課程菜單
        /// </summary>
        /// <param name="courseId">課程 Id</param>
        /// <param name="dailyCourseMenuId">課程菜單 ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("course/{courseId}/menu/{dailyCourseMenuId}")]
        [JwtAuthFilter]
        public IHttpActionResult GetDailyCourseMeal(int courseId, int dailyCourseMenuId)
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
                    Message = new { Auth = "您沒有營養師權限" }
                });
            }

            var courseMenu = db.DailyCourseMenus.FirstOrDefault(m => m.CourseId == courseId && m.Id == dailyCourseMenuId);
            if (courseMenu == null)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 400,
                    Status = "Error",
                    Message = new { CourseMenu = "查無此菜單" }
                });
            }
            if (courseMenu.MyCourse.Order.Plan.Nutritionist.UserId != id)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 403,
                    Status = "Error",
                    Message = new { CourseMenu = "您無權限" }
                });
            }
            string formattedDate = courseMenu.MenuDate.ToString("yyyy/MM/dd");
            var result = new
            {
                StatusCode = 200,
                Status = "Success",
                Message = "取得學員菜單資料成功",
                Data = new
                {
                    courseMenu.Id,
                    courseMenu.CourseId,
                    MenuDate = formattedDate,
                    courseMenu.Starch,
                    courseMenu.Protein,
                    courseMenu.Vegetable,
                    courseMenu.Fruit,
                    courseMenu.Oil,
                    courseMenu.Water
                }
            };
            return Ok(result);
        }
        #endregion

        #region "編輯學員課程菜單"
        /// <summary>
        /// 編輯學員課程菜單
        /// </summary>
        /// <param name="courseId">課程 Id</param>
        /// <param name="dailyCourseMenuId">課程菜單 ID</param>
        /// <param name="viewDailyCourseMenu">課程菜單</param>
        /// <returns></returns>
        [HttpPut]
        [Route("course/{courseId}/menu/{dailyCourseMenuId}")]
        [JwtAuthFilter]
        public IHttpActionResult EditDailyCourseMeal(int courseId, int dailyCourseMenuId, [FromBody] ViewDailyCourseMenu viewDailyCourseMenu)
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
                    Message = new { Auth = "您沒有營養師權限" }
                });
            }

            var courseMenu = db.DailyCourseMenus.FirstOrDefault(m => m.CourseId == courseId && m.Id == dailyCourseMenuId);
            if (courseMenu == null)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 400,
                    Status = "Error",
                    Message = new { CourseMenu = "查無此菜單" }
                });
            }
            if (courseMenu.MyCourse.Order.Plan.Nutritionist.UserId != id)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    StatusCode = 403,
                    Status = "Error",
                    Message = new { CourseMenu = "您無權限" }
                });
            }
            courseMenu.Starch = !string.IsNullOrEmpty(viewDailyCourseMenu.Starch) ? viewDailyCourseMenu.Starch : courseMenu.Starch;
            courseMenu.Protein = !string.IsNullOrEmpty(viewDailyCourseMenu.Protein) ? viewDailyCourseMenu.Protein : courseMenu.Protein;
            courseMenu.Vegetable = !string.IsNullOrEmpty(viewDailyCourseMenu.Vegetable) ? viewDailyCourseMenu.Vegetable : courseMenu.Vegetable;
            courseMenu.Fruit = (viewDailyCourseMenu.Fruit != 0) ? viewDailyCourseMenu.Fruit : courseMenu.Fruit;
            courseMenu.Oil = (viewDailyCourseMenu.Oil != 0) ? viewDailyCourseMenu.Oil : courseMenu.Oil;
            courseMenu.Water = (viewDailyCourseMenu.Water != 0) ? viewDailyCourseMenu.Water : courseMenu.Water;

            try
            {
                db.SaveChanges();
                var result = new
                {
                    StatusCode = 200,
                    Status = "Success",
                    Message = "更新學員菜單資料成功",
                    Data = new
                    {
                        CourseId = courseMenu.CourseId,
                        courseMenu.Id,
                        courseMenu.Starch,
                        courseMenu.Protein,
                        courseMenu.Vegetable,
                        courseMenu.Fruit,
                        courseMenu.Oil,
                        courseMenu.Water
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion "編輯學員課程菜單"
    }
}
