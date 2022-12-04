﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDC.Domain.Web.Setting;
using xDC.Infrastructure.Application;

namespace xDC.Services.Application
{
    public interface ISettingService
    {
        IQueryable<Config_Dropdown> DropdownConfig();
        bool DropdownConfig_Add(Config_Dropdown item, string currentUser);

        EmailConfigSetting EmailConfiguration();
        bool EmailConfigurationUpdate(EmailConfigSetting req, string currentUser);


    }
}
