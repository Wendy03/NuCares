﻿using NuCares.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NuCares.helper
{
    public class Notice
    {
        public static void AddNotice(NuCaresDBContext db, int userId, string message, string type)
        {
            var addNotice = new Notification
            {
                UserId = userId,
                NoticeMessage = message,
                NoticeType = type,
            };

            db.Notification.Add(addNotice);
            db.SaveChanges();
        }
    }
}