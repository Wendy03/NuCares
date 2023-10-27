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
    public class CourseDashboardController : ApiController
    {
        private readonly NuCaresDBContext db = new NuCaresDBContext();
        #region "查看學員三餐總量"
        /// <summary>
        /// 查看學員三餐總量
        /// </summary>
        /// <param name="courseId">課程 ID</param>
        /// <param name="date">日期</param>
        /// <returns></returns>
        [HttpGet]
        [Route("course/{courseId}/daily")]
        [JwtAuthFilter]
        public IHttpActionResult GetDailyMealSum(int courseId, DateTime? date = null)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int id = (int)userToken["Id"];
            bool checkStudent = db.Courses.Any(s => s.Order.UserId == id);
            bool checkNu = db.Courses.Any(n => n.Order.Plan.Nutritionist.UserId == id);

            if (!checkStudent || !checkNu)
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    StatusCode = 403,
                    Status = "Error",
                    Message = new { Auth = "您沒有權限" }
                });
            }

            if (!date.HasValue)
            {
                date = DateTime.Today;
            }
            var menuData = db.DailyCourseMenus
                  .Where(m => m.MenuDate == date && m.CourseId == courseId)
                  .FirstOrDefault();
            if (menuData == null)
            {
                menuData = new DailyCourseMenu
                {
                    CourseId = courseId,
                    MenuDate = date.Value,
                    Starch = "0, 0, 0",
                    Protein = "0, 0, 0",
                    Vegetable = "0, 0, 0",
                    Oil = 0,
                    Fruit = 0,
                    Water = 0,

                };
                db.DailyCourseMenus.Add(menuData);
                db.SaveChanges();
            }

            var studentLog = GetStudentLogData(menuData.Id);

            var menuStarch = menuData.Starch.Split(',').Select(int.Parse).ToArray();
            var menuProtein = menuData.Protein.Split(',').Select(int.Parse).ToArray();
            var menuVegetable = menuData.Vegetable.Split(',').Select(int.Parse).ToArray();

            var totalStarch = menuStarch.Sum();
            var totalProtein = menuProtein.Sum();
            var totalVegetable = menuVegetable.Sum();

            var breakfastData = GetMealData(studentLog.Id, "早餐");
            var lunchData = GetMealData(studentLog.Id, "午餐");
            var dinnerData = GetMealData(studentLog.Id, "晚餐");

            var totalStudentStarch = breakfastData.Starch + lunchData.Starch + dinnerData.Starch;
            var totalStudentProtein = breakfastData.Protein + lunchData.Protein + dinnerData.Protein;
            var totalStudentVegetable = breakfastData.Vegetable + lunchData.Vegetable + dinnerData.Vegetable;

            var response = new
            {
                StatusCode = 200,
                Status = "Success",
                Message = "取得資料成功",
                Data = new
                {
                    Id = menuData.Id,
                    InsertDate = menuData.CreateDate.ToString("yyyy-MM-dd"),
                    MenuDate = menuData.MenuDate.ToString("yyyy-MM-dd"),
                    StarchSum = $"{totalStudentStarch}, {totalStarch}",
                    ProteinSum = $"{totalStudentProtein}, {totalProtein}",
                    VegetableSum = $"{totalStudentVegetable}, {totalVegetable}",
                    OilSum = $"{studentLog.Oil}, {menuData.Oil}",
                    FruitSum = $"{studentLog.Fruit}, {menuData.Fruit}",
                    WaterSum = $"{studentLog.Water}, {menuData.Water}",
                    StarchSumAchieved = totalStudentStarch >= totalStarch,
                    ProteinSumAchieved = totalStudentProtein >= totalProtein,
                    VegetableSumAchieved = totalStudentVegetable >= totalVegetable,
                    Breakfast = new
                    {
                        Id = breakfastData.Id,
                        DailyLogId = studentLog.Id,
                        MealTime = breakfastData.MealTime,
                        MealDescription = breakfastData.MealDescription,
                        Image = breakfastData.MealImgUrl,
                        Starch = $"{breakfastData.Starch}, {menuStarch[0]}",
                        Protein = $"{breakfastData.Protein}, {menuProtein[0]}",
                        Vegetable = $"{breakfastData.Vegetable}, {menuVegetable[0]}",
                        StarchAchieved = CalculateAchieved(breakfastData.Starch, menuStarch[0]),
                        ProteinAchieved = CalculateAchieved(breakfastData.Protein, menuProtein[0]),
                        VegetableAchieved = CalculateAchieved(breakfastData.Vegetable, menuVegetable[0])
                    },
                    Lunch = new
                    {
                        Id = lunchData.Id,
                        DailyLogId = studentLog.Id,
                        MealTime = lunchData.MealTime,
                        MealDescription = lunchData.MealDescription,
                        Image = lunchData.MealImgUrl,
                        Starch = $"{lunchData.Starch}, {menuStarch[1]}",
                        Protein = $"{lunchData.Protein}, {menuProtein[1]}",
                        Vegetable = $"{lunchData.Vegetable}, {menuVegetable[1]}",
                        StarchAchieved = CalculateAchieved(lunchData.Starch, menuStarch[1]),
                        ProteinAchieved = CalculateAchieved(lunchData.Protein, menuProtein[1]),
                        VegetableAchieved = CalculateAchieved(lunchData.Vegetable, menuVegetable[1])
                    },
                    Dinner = new
                    {
                        Id = dinnerData.Id,
                        DailyLogId = studentLog.Id,
                        MealTime = dinnerData.MealTime,
                        MealDescription = dinnerData.MealDescription,
                        Image = dinnerData.MealImgUrl,
                        Starch = $"{dinnerData.Starch}, {menuStarch[2]}",
                        Protein = $"{dinnerData.Protein}, {menuProtein[2]}",
                        Vegetable = $"{dinnerData.Vegetable}, {menuVegetable[2]}",
                        StarchAchieved = CalculateAchieved(dinnerData.Starch, menuStarch[2]),
                        ProteinAchieved = CalculateAchieved(dinnerData.Protein, menuProtein[2]),
                        VegetableAchieved = CalculateAchieved(dinnerData.Vegetable, menuVegetable[2])
                    },
                    Fruit = $"{studentLog.Fruit}, {menuData.Fruit}",
                    FruitAchieved = studentLog.Fruit >= menuData.Fruit && studentLog.Fruit > 0,
                    FruitDescription = studentLog.FruitDescription,
                    FruitImgUrl = studentLog.FruitImgUrl,
                    Oil = $"{studentLog.Oil}, {menuData.Oil}",
                    OilAchieved = studentLog.Oil >= menuData.Oil && studentLog.Oil > 0,
                    OilDescription = studentLog.OilDescription,
                    OilImgUrl = studentLog.OilImgUrl,
                    Water = $"{studentLog.Water}, {menuData.Water}",
                    WaterAchieved = studentLog.Water >= menuData.Water && studentLog.Water > 0,
                    WaterDescription = studentLog.WaterDescription,
                    WaterImgUrl = studentLog.WaterImgUrl
                }
            };
            return Ok(response);
        }
        private bool CalculateAchieved(int value, int sumValue)
        {
            return value >= sumValue && value > 0;
        }

        private DailyLog GetStudentLogData(int menuId)
        {
            var studentLog = db.DailyLogs
                .Where(log => log.DailyCourseMenuId == menuId)
                .FirstOrDefault();

            if (studentLog == null)
            {
                studentLog = new DailyLog
                {
                    DailyCourseMenuId = menuId,
                    Oil = 0,
                    OilDescription = "",
                    Fruit = 0,
                    FruitDescription = "",
                    Water = 0,
                    WaterDescription = "",
                    InsertDate = DateTime.Now,
                    CreateDate = DateTime.Now
                };
                db.DailyLogs.Add(studentLog);
                db.SaveChanges();
            }

            return studentLog;
        }

        private DailyMealTime GetMealData(int dailyLogId, string mealTime)
        {
            var mealData = db.DailyMealTimes
                .Where(meal => meal.DailyLogId == dailyLogId && meal.MealTime == mealTime)
                .FirstOrDefault();

            if (mealData == null)
            {
                mealData = new DailyMealTime
                {
                    DailyLogId = dailyLogId,
                    MealTime = mealTime,
                    MealDescription = "",
                    MealImgUrl = "",
                    Starch = 0,
                    Protein = 0,
                    Vegetable = 0
                };
                db.DailyMealTimes.Add(mealData);
                db.SaveChanges();
            }

            return mealData;
        }

        #endregion "查看學員三餐總量"

    }
}